export type EventType = 'celebration' | 'theatre' | 'festival' | 'birthday' | 'wedding' | 'corporate' | 'other'
export interface CreateApplicationRequest { clientName: string; phone: string; email: string; eventType: EventType; eventDate: string; comment: string }
export interface Application { id: string; clientName: string; phone: string; email: string; eventType: EventType; eventDate: string; comment?: string; status: 'new' | 'inProgress' | 'completed' | 'rejected'; createdAtUtc: string }
