using CreditApi.Data;
using CreditApi.Middlewares;
using CreditApi.Services;                 // 👈 NUEVO: using del servicio de scoring
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// EF Core con SQLite (archivo local credit.db)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=credit.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 👇 NUEVO: registra el servicio de scoring para inyección de dependencias
builder.Services.AddScoped<ICreditScoringService, CreditScoringService>(); // 👈 NUEVO

// Swagger con definición de ApiKey
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Credit API", Version = "v1" });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Ingresa tu API Key en el header x-api-key",
        Type = SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS: permitir el frontend de Vite
var allowLocal = "_allowLocal";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowLocal, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(allowLocal);

// Middleware de API Key
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();
