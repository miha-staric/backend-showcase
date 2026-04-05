using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Models;
using TaskManagementApi.Services.Tenancy;

namespace TaskManagementApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    public required DbSet<TaskItem> Tasks { get; set; }
    public required DbSet<Tenant> Tenants { get; set; }
    public required DbSet<User> Users { get; set; }
    public required DbSet<UserTask> UserTask { get; set; }
    public required DbSet<UserTenant> UserTenant { get; set; }
    public required DbSet<Comment> Comments { get; set; }
    public Guid? CurrentTenantId => _tenantContext?.TenantId;

    private readonly ITenantContext _tenantContext = tenantContext;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        AddQueryFilters(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureTenant(modelBuilder);
        ConfigureTaskItem(modelBuilder);
        ConfigureUserTenant(modelBuilder);
        ConfigureUserTask(modelBuilder);
        ConfigureComment(modelBuilder);
    }

    private void AddQueryFilters(ModelBuilder modelBuilder)
    {
        _ = modelBuilder
            .Entity<User>()
            .HasQueryFilter(u => u.UserTenants.Any(ut => ut.TenantId == CurrentTenantId));

        _ = modelBuilder.Entity<TaskItem>().HasQueryFilter(t => t.TenantId == CurrentTenantId);

        _ = modelBuilder.Entity<Comment>().HasQueryFilter(t => t.TenantId == CurrentTenantId);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<User>(static entity =>
        {
            _ = entity.HasKey(static u => u.Id);
            _ = entity.HasIndex(static u => u.Id).IsUnique();
            _ = entity.Property(static u => u.Username).HasMaxLength(200);
            _ = entity.Property(static u => u.Email).HasMaxLength(200);
        });
    }

    private static void ConfigureComment(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Comment>(static entity =>
        {
            _ = entity.HasKey(static c => c.Id);
            _ = entity.HasIndex(static c => c.TaskId);
            _ = entity.HasIndex(static c => new { c.TaskId, c.CreatedAt });
            _ = entity.Property(static c => c.Subject).HasMaxLength(200);
        });
    }

    private static void ConfigureUserTask(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<UserTask>(static entity =>
        {
            _ = entity.HasKey(static ut => new { ut.UserId, ut.TaskItemId });
            _ = entity
                .HasOne(static ut => ut.User)
                .WithMany(static u => u.UserTasks)
                .HasForeignKey(static ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            _ = entity
                .HasOne(static ut => ut.TaskItem)
                .WithMany(static t => t.UserTasks)
                .HasForeignKey(static ut => ut.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
            _ = entity.HasIndex(static ut => ut.TenantId);
        });
    }

    private static void ConfigureUserTenant(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<UserTenant>(static entity =>
        {
            _ = entity.HasKey(static ut => new { ut.UserId, ut.TenantId });
            _ = entity.HasIndex(static ut => new { ut.TenantId, ut.Username }).IsUnique();
            _ = entity
                .HasOne(static ut => ut.User)
                .WithMany(static u => u.UserTenants)
                .HasForeignKey(static ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            _ = entity.Property(static ut => ut.Username).IsRequired().HasMaxLength(100);
        });
    }

    private static void ConfigureTaskItem(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<TaskItem>(static entity =>
        {
            _ = entity.HasKey(static t => t.Id);
            _ = entity.HasIndex(static t => t.TenantId);
            _ = entity
                .HasOne(static t => t.PrimaryAssigneeUser)
                .WithMany()
                .HasForeignKey(static t => t.PrimaryAssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
            _ = entity
                .HasMany(static t => t.Comments)
                .WithOne(static c => c.Task)
                .HasForeignKey(static c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTenant(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Tenant>(static entity => _ = entity.HasKey(static t => t.Id));
    }
}
