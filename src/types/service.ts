export type ServiceCategory = 'celebration' | 'theatre' | 'festival' | 'birthday' | 'wedding' | 'corporate' | 'other'
export interface Service { id: number | string; name: string; shortDescription: string; priceFrom: number; category?: ServiceCategory }
export interface ServicesResponse { items: Service[] }
