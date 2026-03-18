using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserTask> UserTask { get; set; }
    public DbSet<UserTenant> UserTenant { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Id).IsUnique();
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.TenantId);

            entity.HasOne(t => t.PrimaryAssigneeUser)
                .WithMany() // no back reference
                .HasForeignKey(t => t.PrimaryAssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserTenant>(entity =>
        {
            entity.HasKey(ut => new { ut.UserId, ut.TenantId });

            entity.HasOne(ut => ut.User)
                  .WithMany(u => u.UserTenants)
                  .HasForeignKey(ut => ut.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserTask>(entity =>
        {
            entity.HasKey(ut => new { ut.UserId, ut.TaskItemId });

            entity.HasOne(ut => ut.User)
                .WithMany(u => u.UserTasks)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ut => ut.TaskItem)
                .WithMany(t => t.UserTasks)
                .HasForeignKey(ut => ut.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ut => ut.TenantId);
        });
    }
}
