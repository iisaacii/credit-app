using CreditApi.Dtos;

namespace CreditApi.Services;

public interface ICreditScoringService
{
    (string Status, decimal Score) Calculate(CreditApplyDto dto);
}
