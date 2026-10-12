import type { CreateApplicationRequest } from '../types/application'
import { apiRequest, useMocks } from './client'

export async function createApplication(application: CreateApplicationRequest): Promise<void> {
  if (useMocks) { await new Promise((resolve) => window.setTimeout(resolve, 600)); return }
  await apiRequest<void>('/api/applications', { method: 'POST', body: JSON.stringify(application) })
}
