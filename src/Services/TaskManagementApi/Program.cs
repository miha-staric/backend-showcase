using System.Text.Json.Serialization;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Services.Caching;
using ZiggyCreatures.Caching.Fusion;

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
    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme.",
        }
    );

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = [],
    });

    options.AddSecurityDefinition(
        "X-Tenant-Id",
        new OpenApiSecurityScheme
        {
            Name = "X-Tenant-Id",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
        }
    );

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("X-Tenant-Id", document)] = [],
    });
});
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = System
            .Text
            .Json
            .Serialization
            .ReferenceHandler
            .IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder
    .Services.AddAuthentication("Bearer")
    .AddJwtBearer(
        "Bearer",
        options =>
        {
            options.Authority = "http://keycloak:8080/realms/realm1";
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateAudience = false;
        }
    );
builder.Services.AddAuthorization();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddFusionCache();
FusionCacheEntryOptions defaultCacheOptions = new FusionCacheEntryOptions
{
    Duration = TimeSpan.FromMinutes(5),
    IsFailSafeEnabled = true,
};
builder.Services.AddSingleton(defaultCacheOptions);
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();

    busConfigurator.UsingRabbitMq(
        (context, configurator) =>
        {
            configurator.Host(
                "localhost",
                "/",
                h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                }
            );

            configurator.ConfigureEndpoints(context);
        }
    );
});
builder.Services.AddSingleton<UserCacheHelper>();
builder.Services.AddSingleton<TaskCacheHelper>();
builder.Services.AddSingleton<CommentCacheHelper>();
builder.Services.AddTransient<IValidator<CreateUserCommand>, UserValidator>();
builder.Services.AddHttpClient<GetAccessTokenCommandHandler>(client =>
{
    client.BaseAddress = new Uri("http://keycloak:8080");
});
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddSingleton<IEmailService, FakeEmailService>();
builder.Services.AddTransient<ILoggingService, LoggingService>();

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
        IServiceProvider? provider = scope.ServiceProvider;

        AppDbContext db = provider.GetRequiredService<AppDbContext>();
        IFusionCache? cache = provider.GetRequiredService<IFusionCache>();

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

        Console.WriteLine("Clearing cache...");
        if (cache is FusionCache fc)
            fc.Clear();
        Console.WriteLine("Clearing complete!");
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
