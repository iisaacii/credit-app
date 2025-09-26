import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import App from './App';

// Mockeamos el módulo api para controlar la respuesta del backend
vi.mock('./api', () => {
  return {
    default: {
      post: vi.fn().mockResolvedValue({
        data: { status: 'APROBADO', score: 70, requestId: 123 }
      }),
      get: vi.fn().mockResolvedValue({
        data: { approved: 1, rejected: 0, total: 1 }
      })
    }
  }
});

describe('Flujo básico de App', () => {
  beforeEach(() => {
    // limpiar DOM entre tests si hace falta
    document.body.innerHTML = '';
  });

  it('envía el formulario y muestra pantalla de resultado', async () => {
    render(<App />);

    // Inputs
    const nombre = screen.getByLabelText(/Nombre del cliente/i);
    const monto = screen.getByLabelText(/Monto solicitado/i);
    const btn = screen.getByRole('button', { name: /enviar solicitud/i });

    // Llenar y enviar
    fireEvent.change(nombre, { target: { value: 'Juan Pérez' } });
    fireEvent.change(monto, { target: { value: '4500' } });
    fireEvent.click(btn);

    // Esperar a que aparezca la vista de resultado (decisión aprobada)
    await waitFor(() => {
      expect(screen.getByText(/Resultado de Solicitud/i)).toBeInTheDocument();
      expect(screen.getByText(/Aprobado/i)).toBeInTheDocument();
      expect(screen.getByText(/Score/i)).toBeInTheDocument();
    });
  });
});
