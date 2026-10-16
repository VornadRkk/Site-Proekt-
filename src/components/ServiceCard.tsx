import { Link } from 'react-router-dom'
import type { Service, ServiceCategory } from '../types/service'

const names: Record<ServiceCategory, string> = { celebration: 'Праздники', theatre: 'Театр', birthday: 'Дни рождения', wedding: 'Свадьбы', corporate: 'Корпоративы', other: 'Дополнительно' }
const formatter = new Intl.NumberFormat('ru-RU')

export default function ServiceCard({ service, index }: { service: Service; index: number }) {
  const requestParams = new URLSearchParams({ format: service.category ?? 'other', service: service.name })

  return <article className="service-card h-100"><div className="service-card__top"><span>{String(index + 1).padStart(2, '0')}</span><span>{service.category ? names[service.category] : 'Творческие проекты'}</span></div><div className="service-card__body"><h2>{service.name}</h2><p>{service.shortDescription}</p></div><div className="service-card__footer"><div><small>Стоимость от</small><strong>{formatter.format(service.priceFrom)} ₽</strong></div><Link to={`/request?${requestParams}`} aria-label={`Оставить заявку на услугу «${service.name}»`}><span aria-hidden="true">↗</span></Link></div></article>
}
