namespace CreditApi.Models;

public class Branch
{
    public int BranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
}
