using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static async Task SeedTestData(AppDbContext db)
    {
        if (await db.Tenants.AnyAsync())
            return;

        // Tenants
        var tenantA = new Tenant { Id = Guid.Parse("4da30340-fda0-49b0-b564-f511c630d221"), Title = "Tenant-A", Enabled = true };
        var tenantB = new Tenant { Id = Guid.Parse("2337e27f-58eb-4973-9b43-4b795dac1ad7"), Title = "Tenant-B", Enabled = true };
        db.Tenants.AddRange(tenantA, tenantB);

        // Users
        var alice = new User { Id = Guid.Parse("bef81bfc-2cbb-4321-bd4a-cecb244dadcb"), Username = "alice", Email = "alice@tenant-a.example.com" };
        var bob = new User { Id = Guid.Parse("657ca4fa-fb2d-4180-80db-1403c6b8579e"), Username = "bob", Email = "bob@tenant-a.example.com" };
        var carol = new User { Id = Guid.Parse("1b33930d-4437-41ee-9b10-a864b40cec78"), Username = "carol", Email = "carol@tenant-b.example.com" };
        db.Users.AddRange(alice, bob, carol);

        await db.SaveChangesAsync();

        // UserTenants
        var userTenants = new List<UserTenant>
        {
            new() { UserId = alice.Id, TenantId = tenantA.Id },
            new() { UserId = bob.Id, TenantId = tenantA.Id },
            new() { UserId = carol.Id, TenantId = tenantB.Id }
        };
        db.UserTenant.AddRange(userTenants);
        await db.SaveChangesAsync();

        // Tasks
        var task1 = new TaskItem
        {
            Id = Guid.Parse("d907410e-5860-4cc4-8800-2230895c001f"),
            TenantId = tenantB.Id,
            Title = "Learn ASP.NET Core",
            Status = TaskStatus.New,
            PrimaryAssigneeId = carol.Id
        };
        var task2 = new TaskItem
        {
            Id = Guid.Parse("c534787f-dfb8-4269-8941-791efcb8c4e4"),
            TenantId = tenantA.Id,
            Title = "Build Web API",
            Status = TaskStatus.New,
            PrimaryAssigneeId = alice.Id
        };
        db.Tasks.AddRange(task1, task2);
        await db.SaveChangesAsync();

        // UserTasks
        var userTasks = new List<UserTask>
        {
            new() { UserId = alice.Id, TaskItemId = task2.Id, TenantId = tenantA.Id, Role = Roles.Assignee, CreatedAt = DateTimeOffset.UtcNow },
            new() { UserId = carol.Id, TaskItemId = task1.Id, TenantId = tenantB.Id, Role = Roles.Assignee, CreatedAt = DateTimeOffset.UtcNow }
        };
        db.UserTask.AddRange(userTasks);
        await db.SaveChangesAsync();
    }
}
