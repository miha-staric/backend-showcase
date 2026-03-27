using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserTask> UserTask { get; set; }
    public DbSet<UserTenant> UserTenant { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public Guid? CurrentTenantId => _tenantContext?.TenantId;

    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

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
        modelBuilder
            .Entity<User>()
            .HasQueryFilter(u => u.UserTenants.Any(ut => ut.TenantId == CurrentTenantId));

        modelBuilder.Entity<TaskItem>().HasQueryFilter(t => t.TenantId == CurrentTenantId);

        modelBuilder.Entity<Comment>().HasQueryFilter(t => t.TenantId == CurrentTenantId);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Id).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(200);
            entity.Property(u => u.Email).HasMaxLength(200);
        });
    }

    private void ConfigureComment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.TaskId);
            entity.HasIndex(c => new { c.TaskId, c.CreatedAt });
            entity.Property(c => c.Subject).HasMaxLength(200);
        });
    }

    private void ConfigureUserTask(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserTask>(entity =>
        {
            entity.HasKey(ut => new { ut.UserId, ut.TaskItemId });
            entity
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTasks)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(ut => ut.TaskItem)
                .WithMany(t => t.UserTasks)
                .HasForeignKey(ut => ut.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(ut => ut.TenantId);
        });
    }

    private void ConfigureUserTenant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserTenant>(entity =>
        {
            entity.HasKey(ut => new { ut.UserId, ut.TenantId });
            entity.HasIndex(ut => new { ut.TenantId, ut.Username }).IsUnique();
            entity
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTenants)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(ut => ut.Username).IsRequired().HasMaxLength(100);
        });
    }

    private void ConfigureTaskItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.TenantId);
            entity
                .HasOne(t => t.PrimaryAssigneeUser)
                .WithMany()
                .HasForeignKey(t => t.PrimaryAssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureTenant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
        });
    }
}
