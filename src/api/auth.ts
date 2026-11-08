import { apiRequest } from './client'
export const authAllowed = window.location.protocol === 'https:' || ['localhost', '127.0.0.1', '[::1]'].includes(window.location.hostname)
export const tokenKey = 'dateandtime.session'
export type Session = { displayName: string; role: string }
export const session = () => apiRequest<Session>('/api/auth/session')
export async function authenticate(mode: string, email: string, password: string, displayName: string) {
 if (!authAllowed) throw new Error("Для входа требуется HTTPS")
 const result = await apiRequest<{accessToken: string}>(`/api/auth/${mode}`, { method: 'POST', body: JSON.stringify({email,password,displayName}) })
 sessionStorage.setItem(tokenKey,result.accessToken)
 return session()
}
