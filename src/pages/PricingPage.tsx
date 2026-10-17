import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import PageIntro from '../components/PageIntro'
import { getServices } from '../api/services'
import type { Service } from '../types/service'

const formatter = new Intl.NumberFormat('ru-RU')

export default function PricingPage() {
  const [services, setServices] = useState<Service[]>([])
  const [error, setError] = useState('')
  useEffect(() => { getServices().then(setServices).catch(() => setError('Не удалось загрузить цены. Попробуйте открыть раздел услуг.')) }, [])
  return <><PageIntro eyebrow="Стоимость" title="Понятный ориентир — точная смета под ваше событие" description="Стоимость зависит от сценария, площадки, числа гостей и состава команды. Итоговую смету подготовим после обсуждения деталей." /><section className="pb-section"><div className="container">{error && <p className="alert alert-warning">{error}</p>}<div className="row g-4">{services.map((service) => <div className="col-md-6 col-xl-4" key={service.id}><article className="service-card h-100"><span className="eyebrow">{service.category ?? 'Мероприятие'}</span><h2 className="mt-4">{service.name}</h2><p>{service.shortDescription}</p><strong className="mt-auto">от {formatter.format(service.priceFrom)} ₽</strong><Link className="btn btn-dark rounded-pill mt-3" to={`/request?service=${encodeURIComponent(service.name)}`}>Запросить расчёт</Link></article></div>)}</div><div className="portfolio-cta text-center"><h2>Нужен индивидуальный формат?</h2><p>Расскажите о задаче — предложим сценарий и смету.</p><Link className="btn btn-dark rounded-pill px-4" to="/request">Оставить заявку</Link></div></div></section></>
}
