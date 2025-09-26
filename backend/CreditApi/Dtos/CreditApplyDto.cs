namespace CreditApi.Dtos;

public class CreditApplyDto
{
    // Datos del cliente (si no existe, se crea)
    public int? ClientId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }

    // Datos de la solicitud
    public int? BranchId { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public decimal? Income { get; set; }
    public int? EmploymentMonths { get; set; }
}
