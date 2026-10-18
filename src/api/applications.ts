import type { CreateApplicationRequest } from '../types/application'
import { apiRequest, useMocks } from './client'
import { withClientAuthorization } from './auth'

export async function createApplication(application: CreateApplicationRequest): Promise<void> {
  if (useMocks) { await new Promise((resolve) => window.setTimeout(resolve, 600)); return }
  await apiRequest<void>('/api/applications', withClientAuthorization({ method: 'POST', body: JSON.stringify(application) }))
}
