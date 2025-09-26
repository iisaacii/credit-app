using CreditApi.Data;
using CreditApi.Middlewares;
using CreditApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Detecta si corre dentro de contenedor
var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

// Elige la cadena adecuada (Docker → SqlServer_DockerNet, Local → SqlServer_Localhost)
var connectionString = builder.Configuration.GetConnectionString(
    isDocker ? "SqlServer_DockerNet" : "SqlServer_Localhost"
);

// EF Core con SQL Server (⚠️ antes estaba UseSqlite)
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Servicio de scoring
builder.Services.AddScoped<ICreditScoringService, CreditScoringService>();

// Swagger + ApiKey
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

// CORS
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
