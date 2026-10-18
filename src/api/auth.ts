import { apiRequest } from './client'
import type { Application } from '../types/application'

export interface AuthSession {
  accessToken: string
  expiresAtUtc: string
  displayName: string
  email: string
  role: 'Client' | 'Admin'
}

interface LoginResponse {
  accessToken: string
  expiresAtUtc: string
  displayName: string
  role: 'Client' | 'Admin'
}

const sessionKey = 'dateandtime.client.session'
export const clientAuthEnabled = import.meta.env.VITE_ENABLE_CLIENT_AUTH === 'true'

export function getClientSession(): AuthSession | null {
  const value = sessionStorage.getItem(sessionKey)
  if (!value) return null
  try {
    const session = JSON.parse(value) as AuthSession
    if (session.role !== 'Client' || new Date(session.expiresAtUtc) <= new Date()) { clearClientSession(); return null }
    return session
  } catch { clearClientSession(); return null }
}

export function clearClientSession() { sessionStorage.removeItem(sessionKey) }

function saveSession(response: LoginResponse, email: string) {
  const session: AuthSession = { ...response, email: email.trim().toLowerCase() }
  sessionStorage.setItem(sessionKey, JSON.stringify(session))
  return session
}

export async function loginClient(email: string, password: string) {
  const response = await apiRequest<LoginResponse>('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) })
  if (response.role !== 'Client') throw new Error('Для администратора используйте страницу /admin.')
  return saveSession(response, email)
}

export async function registerClient(displayName: string, email: string, password: string) {
  const response = await apiRequest<LoginResponse>('/api/auth/register', { method: 'POST', body: JSON.stringify({ displayName, email, password }) })
  return saveSession(response, email)
}

export async function getMyApplications() {
  const session = getClientSession()
  if (!session) throw new Error('Войдите, чтобы увидеть свои заявки.')
  return apiRequest<Application[]>('/api/applications/mine', { headers: { Authorization: `Bearer ${session.accessToken}` } })
}

export function withClientAuthorization(init?: RequestInit): RequestInit {
  const session = getClientSession()
  if (!session) return init ?? {}
  return { ...init, headers: { ...init?.headers, Authorization: `Bearer ${session.accessToken}` } }
}
