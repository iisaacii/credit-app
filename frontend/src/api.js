import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "http://localhost:5131/api",
});

// Interceptor: agrega la API Key a cada request
const API_KEY = import.meta.env.VITE_API_KEY || "123";

api.interceptors.request.use((config) => {
  config.headers["x-api-key"] = API_KEY;
  return config;
});

export default api;
