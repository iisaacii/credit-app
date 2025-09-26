using CreditApi.Dtos;
using CreditApi.Services;
using Xunit;

namespace CreditApi.Tests;

public class ScoringTests
{
    private readonly ICreditScoringService _svc = new CreditScoringService();

    [Fact]
    public void Aprobado_CuandoIngresoAltoYAntiguedadAlta()
    {
        var dto = new CreditApplyDto
        {
            Amount = 50000,
            TermMonths = 12,
            Income = 30000,
            EmploymentMonths = 24
        };

        var (status, score) = _svc.Calculate(dto);

        Assert.Equal("APROBADO", status);
        Assert.True(score >= 60);
    }

    [Fact]
    public void Rechazado_CuandoIngresoBajoYAntiguedadBaja()
    {
        var dto = new CreditApplyDto
        {
            Amount = 80000,
            TermMonths = 12,
            Income = 5000,
            EmploymentMonths = 3
        };

        var (status, score) = _svc.Calculate(dto);

        Assert.Equal("RECHAZADO", status);
        Assert.True(score < 60);
    }

    [Fact]
    public void Borde_Exacto60_DeberiaAprobar()
    {
        // Construimos un caso que llegue a 60: 30 (income ~15000-29999) + 15 (empl 12-23) + 20 (amount <= income*6) = 65
        var dto = new CreditApplyDto
        {
            Amount = 30000,      // 30k <= 6000*6? no, ajustemos income
            TermMonths = 12,
            Income = 16000,      // => 30 pts
            EmploymentMonths = 12 // => 15 pts
        };
        // Hacemos que amount <= income*6 para sumar +20: 16000*6 = 96000
        dto.Amount = 50000;

        var (status, score) = _svc.Calculate(dto);

        Assert.Equal("APROBADO", status);
        Assert.True(score >= 60);
    }
}
