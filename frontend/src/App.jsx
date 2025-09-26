import { useState, useEffect } from "react";
import "./App.css";
import api from "./api";
import { PieChart, Pie, Cell, Tooltip, Legend } from "recharts";

export default function App() {
  // --- Form state ---
  const [nombre, setNombre] = useState("");
  const [monto, setMonto] = useState("");
  const [sucursal, setSucursal] = useState("");
  const [ingreso, setIngreso] = useState("");
  const [antiguedad, setAntiguedad] = useState(""); // meses

  // --- UI state ---
  const [resultado, setResultado] = useState(null);
  const [verIndicadores, setVerIndicadores] = useState(false);

  // --- Historial ---
  const [verHistorial, setVerHistorial] = useState(false);
  const [historial, setHistorial] = useState([]);

  // --- Stats desde el backend ---
  const [stats, setStats] = useState({ approved: 0, rejected: 0, total: 0 });

  // --- Extras UX ---
  const [enviando, setEnviando] = useState(false);
  const [simulando, setSimulando] = useState(false);
  const [errorMsg, setErrorMsg] = useState("");

  async function loadStats() {
    try {
      const { data } = await api.get("/credit/stats");
      setStats(data);
    } catch (e) {
      console.error("Error cargando stats:", e);
      setErrorMsg("No se pudieron cargar los indicadores.");
    }
  }

  async function loadHistorial() {
    try {
      const { data } = await api.get("/credit/all");
      setHistorial(data);
    } catch (e) {
      console.error("Error cargando historial:", e);
      alert("No se pudo cargar el historial.");
    }
  }

  useEffect(() => {
    // carga inicial de indicadores
    loadStats();
  }, []);

  // Helpers
  function splitName(full) {
    const parts = full.trim().split(" ").filter(Boolean);
    const first = parts[0] || "N/A";
    const last = parts.slice(1).join(" ") || "N/A";
    return { first, last };
  }

  // Submit al backend
  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrorMsg("");
    setEnviando(true);
    try {
      const { first, last } = splitName(nombre);

      const payload = {
        firstName: first,
        lastName: last,
        email: `${first.toLowerCase()}@example.com`,
        // branchId opcional
        amount: parseFloat(monto),
        termMonths: 12,
        income: ingreso ? parseFloat(ingreso) : null,
        employmentMonths: antiguedad ? parseInt(antiguedad) : null,
      };

      const { data } = await api.post("/credit/apply", payload);

      setResultado({
        nombre,
        monto,
        sucursal,
        decision: data.status, // "APROBADO" | "RECHAZADO"
        score: data.score,
        requestId: data.requestId,
      });

      // refresca indicadores reales
      loadStats();
    } catch (err) {
      console.error(err);
      setErrorMsg("Error al enviar la solicitud.");
      alert("Error al enviar la solicitud. Revisa la consola.");
    } finally {
      setEnviando(false);
    }
  };

  const handleNuevaSolicitud = () => {
    setNombre("");
    setMonto("");
    setSucursal("");
    setIngreso("");
    setAntiguedad("");
    setResultado(null);
    setErrorMsg("");
  };

  // --- Vista: Historial ---
  if (verHistorial) {
    return (
      <div className="container">
        <h1>Historial de Solicitudes</h1>

        <button onClick={() => setVerHistorial(false)}>Volver</button>
        <button
          style={{ marginLeft: 8 }}
          onClick={async () => {
            await loadHistorial();
          }}
        >
          Recargar
        </button>

        <div style={{ overflowX: "auto", marginTop: "1rem" }}>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr>
                <th>ID</th>
                <th>Monto</th>
                <th>Ingreso</th>
                <th>Meses</th>
                <th>Status</th>
                <th>Score</th>
                <th>Fecha</th>
              </tr>
            </thead>
            <tbody>
              {historial.map((r) => (
                <tr key={r.creditRequestId}>
                  <td>{r.creditRequestId}</td>
                  <td>${r.amount}</td>
                  <td>{r.income ?? "—"}</td>
                  <td>{r.employmentMonths ?? "—"}</td>
                  <td>{r.status}</td>
                  <td>{r.score?.toFixed?.(2) ?? "—"}</td>
                  <td>{new Date(r.requestedAt).toLocaleString()}</td>
                </tr>
              ))}
              {historial.length === 0 && (
                <tr>
                  <td colSpan="7" style={{ padding: "12px" }}>No hay registros.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    );
  }

  // --- Vista: Indicadores ---
  if (verIndicadores) {
    const data = [
      { name: "Aprobados", value: stats.approved },
      { name: "Rechazados", value: stats.rejected },
    ];
    const COLORS = ["#28a745", "#dc3545"];
    const empty = stats.approved + stats.rejected === 0;

    return (
      <div className="container">
        <h1>Indicadores (backend)</h1>

        {empty ? (
          <p>No hay solicitudes registradas aún.</p>
        ) : (
          <PieChart width={320} height={320}>
            <Pie
              data={data}
              cx="50%"
              cy="50%"
              outerRadius={110}
              dataKey="value"
              label
            >
              {data.map((entry, index) => (
                <Cell key={index} fill={COLORS[index % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip />
            <Legend />
          </PieChart>
        )}

        <div style={{ marginTop: "1rem" }}>
          <p><strong>Total:</strong> {stats.total}</p>
          <p><strong>Aprobados:</strong> {stats.approved}</p>
          <p><strong>Rechazados:</strong> {stats.rejected}</p>
        </div>

        <div style={{ marginTop: 8, display: "flex", gap: 8, flexWrap: "wrap" }}>
          <button onClick={() => setVerIndicadores(false)}>Volver</button>
          <button onClick={async () => { await loadStats(); alert("Indicadores actualizados."); }}>
            Recargar
          </button>
          <button onClick={async () => { await loadHistorial(); setVerHistorial(true); }}>
            Ver Historial
          </button>
        </div>

        <div style={{ marginTop: 12 }}>
          <button
            disabled={simulando}
            onClick={async () => {
              setSimulando(true);
              try {
                await api.post("/credit/simulate?count=50");
                await loadStats();
                alert("Se generaron 50 registros de prueba.");
              } catch (e) {
                console.error(e);
                alert("No se pudo simular datos. Revisa la consola.");
              } finally {
                setSimulando(false);
              }
            }}
          >
            {simulando ? "Generando..." : "Generar datos demo"}
          </button>
        </div>
      </div>
    );
  }

  // --- Vista: Resultado ---
  if (resultado) {
    return (
      <div className="container">
        <h1>Resultado de Solicitud</h1>
        <p><strong>Cliente:</strong> {resultado.nombre}</p>
        <p><strong>Monto:</strong> ${resultado.monto}</p>
        <p><strong>Sucursal:</strong> {resultado.sucursal || "N/A"}</p>
        <p><strong>Score:</strong> {resultado.score?.toFixed(2)}</p>
        <h2>{resultado.decision === "APROBADO" ? "✅ Aprobado" : "❌ Rechazado"}</h2>

        <button onClick={handleNuevaSolicitud}>Nueva Solicitud</button>
        <button onClick={() => setVerIndicadores(true)} style={{ marginLeft: 8 }}>
          Ver Indicadores
        </button>
        <button style={{ marginLeft: 8 }} onClick={async () => { await loadHistorial(); setVerHistorial(true); }}>
          Ver Historial
        </button>
      </div>
    );
  }

  // --- Vista: Formulario ---
  return (
    <div className="container">
      <h1>Solicitud de Crédito</h1>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="nombre">Nombre del cliente: </label>
          <input
            id="nombre"
            type="text"
            value={nombre}
            onChange={(e) => setNombre(e.target.value)}
            placeholder="Ej. Juan Pérez"
            required
          />
        </div>

        <div>
          <label htmlFor="monto">Monto solicitado: </label>
          <input
            id="monto"
            type="number"
            value={monto}
            onChange={(e) => setMonto(e.target.value)}
            min="1"
            required
          />
        </div>

        <div>
          <label htmlFor="sucursal">Sucursal: </label>
          <select
            id="sucursal"
            value={sucursal}
            onChange={(e) => setSucursal(e.target.value)}
          >
            <option value="">Seleccione una sucursal</option>
            <option value="Norte">Sucursal Norte</option>
            <option value="Sur">Sucursal Sur</option>
            <option value="Centro">Sucursal Centro</option>
          </select>
        </div>

        <div>
          <label htmlFor="ingreso">Ingreso mensual (MXN): </label>
          <input
            id="ingreso"
            type="number"
            value={ingreso}
            onChange={(e) => setIngreso(e.target.value)}
            placeholder="Ej. 20000"
          />
        </div>

        <div>
          <label htmlFor="antiguedad">Antigüedad laboral (meses): </label>
          <input
            id="antiguedad"
            type="number"
            value={antiguedad}
            onChange={(e) => setAntiguedad(e.target.value)}
            placeholder="Ej. 18"
          />
        </div>

        {errorMsg && <p style={{ color: "crimson", marginTop: 4 }}>{errorMsg}</p>}

        <button type="submit" disabled={enviando}>
          {enviando ? "Enviando..." : "Enviar Solicitud"}
        </button>

        <div style={{ marginTop: 8 }}>
          <button type="button" onClick={() => setVerIndicadores(true)}>
            Ver Indicadores
          </button>
          <button
            type="button"
            style={{ marginLeft: 8 }}
            onClick={async () => { await loadHistorial(); setVerHistorial(true); }}
          >
            Ver Historial
          </button>
        </div>
      </form>
    </div>
  );
}
