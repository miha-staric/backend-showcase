namespace TaskManagementApi.Dtos;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public bool Enabled { get; set; }
}
