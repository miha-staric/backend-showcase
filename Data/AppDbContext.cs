using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // tenant-a
        User alice = new User { Id = Guid.Parse("bef81bfc-2cbb-4321-bd4a-cecb244dadcb"), Username = "alice", Email = "alice@tenant-a.example.com" };
        User bob = new User { Id = Guid.Parse("657ca4fa-fb2d-4180-80db-1403c6b8579e"), Username = "bob", Email = "bob@tenant-a.example.com" };

        // tenant-b
        User carol = new User { Id = Guid.Parse("1b33930d-4437-41ee-9b10-a864b40cec78"), Username = "carol", Email = "carol@tenant-b.example.com" };
        User dan = new User { Id = Guid.Parse("420f5b2e-ef55-4733-8bd9-053a07b9ed9c"), Username = "dan", Email = "dan@tenant-b.example.com" };

        // tenant-c
        User eve = new User { Id = Guid.Parse("eaa46a1d-7ace-4ba8-9b2e-a4e4f8d654d1"), Username = "eve", Email = "eve@tenant-c.example.com" };
        User frank = new User { Id = Guid.Parse("13bac98c-abd2-424b-bf2f-e1c26dee0e71"), Username = "frank", Email = "frank@tenant-c.example.com" };

        // no tenant
        User grace = new User { Id = Guid.Parse("e8f46251-0c7a-47c2-99b3-88c9496d8e9d"), Username = "grace", Email = "grace@no-tenant.example.com" };


        modelBuilder.Entity<User>().HasData(
         alice, bob, carol, dan, eve, frank, grace
        );

        Tenant tenantA = new Tenant { Id = Guid.Parse("4da30340-fda0-49b0-b564-f511c630d221"), Title = "Tenant-A", Enabled = true };
        Tenant tenantB = new Tenant { Id = Guid.Parse("2337e27f-58eb-4973-9b43-4b795dac1ad7"), Title = "Tenant-B", Enabled = true };
        Tenant tenantC = new Tenant { Id = Guid.Parse("4da16ab8-3f6b-4af6-9fba-82daa779aeb9"), Title = "Tenant-C", Enabled = true };

        modelBuilder.Entity<Tenant>().HasData(
            tenantA, tenantB, tenantC
        );

        TaskItem task1 = new TaskItem
        {
            Id = Guid.Parse("d907410e-5860-4cc4-8800-2230895c001f"),
            Title = "Learn ASP.NET Core",
            Status = TaskStatus.New,
            AssignedUserId = carol.Id,
            TenantId = carol.TenantId,
            DueDate = null
        };

        TaskItem task2 = new TaskItem
        {
            Id = Guid.Parse("c534787f-dfb8-4269-8941-791efcb8c4e4"),
            Title = "Build Web API",
            Status = TaskStatus.New,
            AssignedUserId = alice.Id,
            TenantId = alice.TenantId,
            DueDate = null
        };

        modelBuilder.Entity<TaskItem>().HasData(
            task1, task2
        );

        modelBuilder.Entity<TaskItem>()
          .HasOne(t => t.AssignedUser)
          .WithMany(u => u.Tasks);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.TenantId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Id).IsUnique();
        });

        modelBuilder.Entity<UserTenant>()
          .HasKey(x => new { x.UserId, x.TenantId });
    }
}
