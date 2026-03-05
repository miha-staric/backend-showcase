// used for POST
public class CreateTaskDto
{
    public String Title { get; set; } = null!;
    public String Status { get; set; } = "New";
    public Guid AssignedUserId { get; set; }
    public DateTime? DueDate { get; set; }
}
