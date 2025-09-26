using CreditApi.Dtos;

namespace CreditApi.Services;

public class CreditScoringService : ICreditScoringService
{
    public (string Status, decimal Score) Calculate(CreditApplyDto dto)
    {
        decimal score = 0;
        var income = dto.Income ?? 0;
        var empl = dto.EmploymentMonths ?? 0;

        score += income >= 30000 ? 50 : income >= 15000 ? 30 : income > 0 ? 10 : 0;
        score += empl >= 24 ? 30 : empl >= 12 ? 15 : empl > 0 ? 5 : 0;
        if (dto.Amount <= income * 6) score += 20;

        var status = score >= 60 ? "APROBADO" : "RECHAZADO";
        return (status, score);
    }
}
