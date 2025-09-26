using CreditApi.Data;
using CreditApi.Dtos;
using CreditApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CreditApi.Services; // <-- AÑADIR

namespace CreditApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditController(AppDbContext db, ICreditScoringService scoring) : ControllerBase

{
    // POST /api/credit/apply
[HttpPost("apply")]
public async Task<IActionResult> Apply([FromBody] CreditApplyDto dto)
{
    try
    {
        // ✅ Validaciones de entrada
        if (dto is null)
            return BadRequest("Body requerido.");

        if (dto.Amount <= 0)
            return BadRequest("Amount debe ser mayor a 0.");

        if (dto.TermMonths <= 0)
            return BadRequest("TermMonths debe ser mayor a 0.");

        if (dto.Income is < 0)
            return BadRequest("Income no puede ser negativo.");

        if (dto.EmploymentMonths is < 0)
            return BadRequest("EmploymentMonths no puede ser negativo.");

        // 1) Cliente: usar existente o crear nuevo
        Client? client = null;
        if (dto.ClientId.HasValue)
        {
            client = await db.Clients.FindAsync(dto.ClientId.Value);
            if (client is null) return NotFound("ClientId no existe.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
                return BadRequest("FirstName y LastName son requeridos si no envías ClientId.");

            client = new Client
            {
                FirstName = dto.FirstName!,
                LastName = dto.LastName!,
                Email = dto.Email
            };
            db.Clients.Add(client);
            await db.SaveChangesAsync();
        }

        var (status, score) = scoring.Calculate(dto);


        // 3) Persistir solicitud
        var cr = new CreditRequest
        {
            ClientId = client.ClientId,
            BranchId = dto.BranchId,
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            Income = dto.Income,
            EmploymentMonths = dto.EmploymentMonths,
            Status = status,
            Score = score
        };
        db.CreditRequests.Add(cr);
        await db.SaveChangesAsync();

        return Ok(new
        {
            requestId = cr.CreditRequestId,
            status,
            score,
            client = new { client.ClientId, client.FirstName, client.LastName }
        });
    }
    catch (Exception)
    {
        // 🔒 No exponemos detalles técnicos al cliente
        return Problem("Ocurrió un error inesperado procesando la solicitud.");
    }
}


    // GET /api/credit/stats  -> para la pantalla de indicadores
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var approved = await db.CreditRequests.CountAsync(x => x.Status == "APROBADO");
        var rejected = await db.CreditRequests.CountAsync(x => x.Status == "RECHAZADO");
        var total = approved + rejected;
        return Ok(new { approved, rejected, total });
    }

    [HttpGet("all")]
    public async Task<IActionResult> All()
    {
        var items = await db.CreditRequests
            .OrderByDescending(x => x.RequestedAt)
            .Select(x => new
            {
                x.CreditRequestId,
                x.Amount,
                x.TermMonths,
                x.Income,
                x.EmploymentMonths,
                x.Status,
                x.Score,
                x.RequestedAt
            })
            .ToListAsync();

        return Ok(items);
    }


[HttpPost("simulate")]
public async Task<IActionResult> Simulate([FromQuery] int count = 50)
{
    var rnd = new Random();

    for (int i = 0; i < count; i++)
    {
        var income = rnd.Next(5000, 80000);
        var empl = rnd.Next(0, 120);
        var amount = rnd.Next(1000, 300000);

        var (status, score) = scoring.Calculate(new CreditApplyDto
{
    Amount = amount,
    TermMonths = 12,
    Income = income,
    EmploymentMonths = empl
});


        var client = new Client { FirstName = $"Demo{i}", LastName = "Test" };
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        db.CreditRequests.Add(new CreditRequest {
            ClientId = client.ClientId,
            Amount = amount,
            TermMonths = 12,
            Income = income,
            EmploymentMonths = empl,
            Status = status,
            Score = score
        });
    }

    await db.SaveChangesAsync();
    return Ok(new { inserted = count });
}

}
