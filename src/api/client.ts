const API_URL = (import.meta.env.VITE_API_URL ?? (import.meta.env.DEV ? 'http://localhost:5080' : '')).replace(/\/$/, '')
export const useMocks = import.meta.env.VITE_USE_MOCKS === 'true' || (import.meta.env.DEV && import.meta.env.VITE_USE_MOCKS !== 'false')

export class ApiError extends Error {
  constructor(message: string, public readonly status?: number) { super(message); this.name = 'ApiError' }
}

export async function apiRequest<T>(path: string, options?: RequestInit): Promise<T> {
  const controller = new AbortController()
  const timeout = window.setTimeout(() => controller.abort(), 15000)
  const abortRequest = () => controller.abort()
  if (options?.signal?.aborted) controller.abort()
  options?.signal?.addEventListener('abort', abortRequest, { once: true })
  try {
    const headers = new Headers(options?.headers)
    if (!headers.has('Accept')) headers.set('Accept', 'application/json')
    if (options?.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json')
    const response = await fetch(`${API_URL}${path}`, { ...options, headers, signal: controller.signal })
    if (!response.ok) {
      const message = response.status === 400
        ? 'Проверьте заполненные поля: имя, контакты, дату и количество гостей.'
        : response.status === 429
          ? 'Слишком много запросов. Подождите минуту и попробуйте снова.'
          : 'Сервер временно не может обработать запрос. Попробуйте немного позже.'
      throw new ApiError(message, response.status)
    }
    if (response.status === 204) return undefined as T
    const body = await response.text()
    if (!body) return undefined as T

    try { return JSON.parse(body) as T }
    catch { throw new ApiError('Не удалось получить данные. Попробуйте обновить страницу.', response.status) }
  } catch (error) {
    if (error instanceof ApiError) throw error
    if (controller.signal.aborted) throw new ApiError('Сервер долго не отвечает. Попробуйте немного позже.')
    throw new ApiError('Не удалось связаться с сервером. Проверьте подключение к интернету и попробуйте ещё раз.')
  } finally {
    window.clearTimeout(timeout)
    options?.signal?.removeEventListener('abort', abortRequest)
  }
}
