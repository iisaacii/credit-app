namespace CreditApi.Models;

public class CreditRequest
{
    public int CreditRequestId { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public decimal? Income { get; set; }
    public int? EmploymentMonths { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "PENDING"; // APROBADO / RECHAZADO
    public decimal? Score { get; set; }
}
