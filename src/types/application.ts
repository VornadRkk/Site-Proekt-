export type EventType = 'wedding' | 'birthday' | 'corporate' | 'other'
export interface CreateApplicationRequest { clientName: string; phone: string; email: string; eventType: EventType; eventDate: string; guestCount: number; comment: string }
