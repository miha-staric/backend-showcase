using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Services.Caching;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Authentication - Keycloak
var keycloakSettings = builder.Configuration.GetSection("Keycloak");
var authority = keycloakSettings["Authority"];
var audience = keycloakSettings["Audience"];

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });

    options.AddSecurityDefinition("X-Tenant-Id", new OpenApiSecurityScheme
    {
        Name = "X-Tenant-Id",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("X-Tenant-Id", document)] = []
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddAuthentication("Bearer")
  .AddJwtBearer("Bearer", options =>
  {
      options.Authority = "http://keycloak:8080/realms/realm1";
      options.RequireHttpsMetadata = false;
      options.TokenValidationParameters.ValidateAudience = false;
  });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddFusionCache();
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddMassTransit(x =>
{
    // If you have consumers, register them here
    // x.AddConsumer<TaskCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddSingleton<UserCacheHelper>();
builder.Services.AddTransient<IValidator<CreateUserCommand>, UserValidator>();

// Build Application
WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
        });
    app.UseSwaggerUI();

    if (args.Contains("--seed"))
    {
        using IServiceScope scope = app.Services.CreateScope();

        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Boolean reset = args.Contains("--reset");

        if (reset)
        {
            Console.WriteLine("Resetting database...");
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
            Console.WriteLine("Database reset complete.");
        }

        Console.WriteLine("Seeding test data...");
        await DbSeeder.SeedTestData(db);
        Console.WriteLine("Seeding complete!");
        return;
    }
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();
app.MapControllers();

try
{
    Log.Information("Starting application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly" + ex.Message);
}
finally
{
    Log.CloseAndFlush();
}

