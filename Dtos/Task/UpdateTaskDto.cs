// used for PUT/PATCH
public class UpdateTaskDto
{
    public String? Title { get; set; }
    public String? Status { get; set; }
    public Guid? AssignedUserId { get; set; }
    public DateTime? DueDate { get; set; }
}
