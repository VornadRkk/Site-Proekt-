import { mockServices } from '../data/mockServices'
import type { Service, ServicesResponse } from '../types/service'
import { apiRequest, useMocks } from './client'

export async function getServices(): Promise<Service[]> {
  if (useMocks) { await new Promise((resolve) => window.setTimeout(resolve, 350)); return mockServices }
  const response = await apiRequest<Service[] | ServicesResponse>('/api/services')
  return Array.isArray(response) ? response : response.items
}
