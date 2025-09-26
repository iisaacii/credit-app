# Credit App — Simulador de Solicitudes de Crédito

## ✨ Resumen
Aplicación web que simula solicitudes de crédito:
- Frontend (React + Vite) — captura, resultado, e indicadores.
- Backend (ASP.NET Core + EF Core, SQLite) — persiste y evalúa solicitudes.
- Seguridad simple por **API Key** y CORS.
- **Docker Compose** para levantar todo.

## 🔧 Stack
- Frontend: React + Vite (puerto 5173)
- Backend: ASP.NET Core Web API (puerto 5131)
- BD: SQLite (archivo `credit.db`, ignorado en Git)
- Gráficas: Recharts
- Tests: xUnit (backend), Vitest + React Testing Library (frontend)
- Docker: Dockerfiles + docker-compose

## 🚀 Cómo correr (desarrollo)
1) **Backend**
```bash
cd backend/CreditApi
dotnet ef database update
dotnet run
# Swagger: http://localhost:5131/swagger
