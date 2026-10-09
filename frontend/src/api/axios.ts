import axios, { AxiosError } from 'axios'

export const TOKEN_KEY = 'finanzas_token'
export const USER_KEY = 'finanzas_user'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api',
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (r) => r,
  (error: AxiosError) => {
    const isLogin = error.config?.url?.includes('/auth/login')
    if (error.response?.status === 401 && !isLogin) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(USER_KEY)
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

/** Extracts a readable message from any API error. */
export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    if (!error.response) return 'No se pudo conectar con el servidor.'
    if (error.response.status === 429) return 'Demasiados intentos. Espera un minuto e inténtalo de nuevo.'
    if (error.response.status === 403) return 'No tienes permiso para realizar esta acción.'
    const data = error.response.data as {
      message?: string
      errors?: Record<string, string[]>
      title?: string
    }
    if (data?.message) return data.message
    if (data?.errors) {
      const first = Object.values(data.errors)[0]?.[0]
      if (first) return first
    }
    if (data?.title) return data.title
  }
  return 'Ocurrió un error inesperado.'
}

export default api
