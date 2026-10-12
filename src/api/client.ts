const API_URL = (import.meta.env.VITE_API_URL ?? 'http://localhost:5080').replace(/\/$/, '')
export const useMocks = import.meta.env.VITE_USE_MOCKS !== 'false'

export class ApiError extends Error {
  constructor(message: string, public readonly status?: number) { super(message); this.name = 'ApiError' }
}

export async function apiRequest<T>(path: string, options?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${API_URL}${path}`, { ...options, headers: { Accept: 'application/json', 'Content-Type': 'application/json', ...options?.headers } })
  } catch {
    throw new ApiError('Не удалось связаться с сервером. Проверьте адрес API и настройки CORS.')
  }
  if (!response.ok) throw new ApiError('Сервер не смог обработать запрос.', response.status)
  if (response.status === 204) return undefined as T
  const body = await response.text()
  if (!body) return undefined as T

  try {
    return JSON.parse(body) as T
  } catch {
    throw new ApiError('Сервер вернул ответ в неизвестном формате.', response.status)
  }
}
