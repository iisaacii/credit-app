namespace CreditApi.Models;

public class Client
{
    public int ClientId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CreditRequest> CreditRequests { get; set; } = new List<CreditRequest>();
}
