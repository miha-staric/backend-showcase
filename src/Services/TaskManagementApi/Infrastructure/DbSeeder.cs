using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskStatus = Contracts.Enums.TaskStatus;
using UserRole = Contracts.Enums.UserRole;

namespace TaskManagementApi.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedTestData(AppDbContext db)
    {
        if (await db.Tenants.AnyAsync())
            return;

        // Tenants
        Tenant tenantA = new()
        {
            Id = Guid.Parse("4da30340-fda0-49b0-b564-f511c630d221"),
            Title = "Tenant-A",
            Enabled = true,
        };
        Tenant tenantB = new()
        {
            Id = Guid.Parse("2337e27f-58eb-4973-9b43-4b795dac1ad7"),
            Title = "Tenant-B",
            Enabled = true,
        };
        db.Tenants.AddRange(tenantA, tenantB);

        // Users
        User alice = new()
        {
            Id = Guid.Parse("bef81bfc-2cbb-4321-bd4a-cecb244dadcb"),
            Username = "alice",
            Email = "alice@tenant-a.example.com",
        };
        User bob = new()
        {
            Id = Guid.Parse("657ca4fa-fb2d-4180-80db-1403c6b8579e"),
            Username = "bob",
            Email = "bob@tenant-a.example.com",
        };
        User carol = new()
        {
            Id = Guid.Parse("1b33930d-4437-41ee-9b10-a864b40cec78"),
            Username = "carol",
            Email = "carol@tenant-b.example.com",
        };
        db.Users.AddRange(alice, bob, carol);

        _ = await db.SaveChangesAsync();

        // UserTenants
        List<UserTenant> userTenants =
        [
            new()
            {
                UserId = alice.Id,
                TenantId = tenantA.Id,
                Username = alice.Username,
                UserRole = UserRole.User,
            },
            new()
            {
                UserId = bob.Id,
                TenantId = tenantA.Id,
                Username = bob.Username,
                UserRole = UserRole.User,
            },
            new()
            {
                UserId = carol.Id,
                TenantId = tenantB.Id,
                Username = carol.Username,
                UserRole = UserRole.Admin,
            },
        ];
        db.UserTenant.AddRange(userTenants);
        _ = await db.SaveChangesAsync();

        // Tasks
        TaskItem task1 = new()
        {
            Id = Guid.Parse("d907410e-5860-4cc4-8800-2230895c001f"),
            TenantId = tenantB.Id,
            Title = "Learn ASP.NET Core",
            Status = TaskStatus.New,
            PrimaryAssigneeId = carol.Id,
        };
        TaskItem task2 = new()
        {
            Id = Guid.Parse("c534787f-dfb8-4269-8941-791efcb8c4e4"),
            TenantId = tenantA.Id,
            Title = "Build Web API",
            Status = TaskStatus.New,
            PrimaryAssigneeId = alice.Id,
        };
        db.Tasks.AddRange(task1, task2);
        _ = await db.SaveChangesAsync();

        // UserTasks
        List<UserTask> userTasks =
        [
            new()
            {
                UserId = alice.Id,
                TaskItemId = task2.Id,
                TenantId = tenantA.Id,
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new()
            {
                UserId = carol.Id,
                TaskItemId = task1.Id,
                TenantId = tenantB.Id,
                CreatedAt = DateTimeOffset.UtcNow,
            },
        ];
        db.UserTask.AddRange(userTasks);
        _ = await db.SaveChangesAsync();

        // Comments
        List<Comment> comments =
        [
            new()
            {
                TenantId = tenantA.Id,
                UserId = alice.Id,
                TaskId = task2.Id,
                Subject = "This is my comment",
                Content = "Blah blah. Blah blah blah blah.",
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new()
            {
                TenantId = tenantA.Id,
                UserId = alice.Id,
                TaskId = task2.Id,
                Subject = "This is my next comment",
                Content = "Oh, ho, ho ho!",
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new()
            {
                TenantId = tenantB.Id,
                UserId = carol.Id,
                TaskId = task1.Id,
                Subject = "The Others... They can't lie!",
                Content = "I noticed it just the other day, they cannot lie. Like, at all!",
                CreatedAt = DateTimeOffset.UtcNow,
            },
        ];
        db.Comments.AddRange(comments);
        _ = await db.SaveChangesAsync();
    }
}
