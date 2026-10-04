import { useCallback, useEffect, useState } from 'react'
import { useMocks } from '../api/client'
import { getServices } from '../api/services'
import PageIntro from '../components/PageIntro'
import ServiceCard from '../components/ServiceCard'
import type { Service } from '../types/service'

export default function ServicesPage() {
  const [services, setServices] = useState<Service[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const loadServices = useCallback(async () => {
    setIsLoading(true); setError(null)
    try { setServices(await getServices()) } catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Не удалось загрузить услуги.') } finally { setIsLoading(false) }
  }, [])
  useEffect(() => { void loadServices() }, [loadServices])

  return <><PageIntro eyebrow="Форматы и возможности" title="События разного масштаба — с одинаковым вниманием" description="Выберите готовое направление или расскажите о своей идее. Финальная смета зависит от формата, площадки, числа гостей и состава команды." /><section className="pb-section"><div className="container">
    {useMocks && <div className="demo-note mb-4"><span>Демо</span> Сейчас показаны локальные данные. Подключение API описано в README.</div>}
    {isLoading && <div className="row g-4" aria-label="Загрузка услуг" aria-busy="true">{[1,2,3].map((item) => <div className="col-lg-4" key={item}><div className="service-skeleton" /></div>)}</div>}
    {error && <div className="error-state text-center"><span className="error-state__icon" aria-hidden="true">!</span><h2>Услуги пока не загрузились</h2><p>{error}</p><button className="btn btn-dark rounded-pill px-4" type="button" onClick={() => void loadServices()}>Попробовать ещё раз</button></div>}
    {!isLoading && !error && <div className="row g-4">{services.map((service, index) => <div className="col-md-6 col-xl-4" key={service.id}><ServiceCard service={service} index={index} /></div>)}</div>}
  </div></section></>
}
