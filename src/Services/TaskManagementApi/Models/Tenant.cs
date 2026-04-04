namespace TaskManagementApi.Models;

public class Tenant
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public bool Enabled { get; set; }
}
