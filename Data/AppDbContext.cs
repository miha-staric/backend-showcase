using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        User user1 = new User { Id = new Guid(), FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
        User user2 = new User { Id = new Guid(), FirstName = "Ronald", LastName = "MacDonald", Email = "ronald@macdonalds.com" };
        User user3 = new User { Id = new Guid(), FirstName = "Colonel", LastName = "Sanders", Email = "colonel@kfc.com" };

        modelBuilder.Entity<User>().HasData(
         user1, user2, user3
        );

        TaskItem task1 = new TaskItem { Id = new Guid(), Title = "Learn ASP.NET Core", Status = TaskStatus.New, AssignedUserId = user2.Id, DueDate = null };
        TaskItem task2 = new TaskItem { Id = new Guid(), Title = "Build Web API", Status = TaskStatus.New, AssignedUserId = user3.Id, DueDate = null };

        modelBuilder.Entity<TaskItem>().HasData(
            task1, task2
        );

        modelBuilder.Entity<TaskItem>()
        .HasOne(t => t.AssignedUser)
        .WithMany(u => u.Tasks)
        .HasForeignKey(t => t.AssignedUserId);
    }
}
