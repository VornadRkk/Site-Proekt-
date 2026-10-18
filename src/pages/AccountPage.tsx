import { useEffect, useState } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { clearClientSession, clientAuthEnabled, getClientSession, getMyApplications } from '../api/auth'
import type { Application } from '../types/application'

const statuses: Record<Application['status'], string> = { new: 'Новая', inProgress: 'В работе', completed: 'Завершена', rejected: 'Отклонена' }
const eventTypes: Record<Application['eventType'], string> = { celebration: 'Праздник', theatre: 'Театрализованная программа', festival: 'Фестиваль', birthday: 'Частный праздник', wedding: 'Свадьба', corporate: 'Корпоратив', other: 'Другое' }

export default function AccountPage() {
  const navigate = useNavigate()
  const [session] = useState(getClientSession)
  const [applications, setApplications] = useState<Application[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!session) return
    getMyApplications().then(setApplications).catch((requestError) => setError(requestError instanceof Error ? requestError.message : 'Не удалось загрузить заявки.')).finally(() => setIsLoading(false))
  }, [session])

  if (!clientAuthEnabled) return <Navigate to="/" replace />
  if (!session) return <Navigate to="/login" replace />

  return <section className="section-padding account-page"><div className="container"><div className="d-flex flex-wrap justify-content-between align-items-end gap-3 mb-5"><div><span className="eyebrow">Личный кабинет</span><h1 className="display-title mt-2 mb-0">Здравствуйте, {session.displayName}</h1><p className="section-copy mt-3 mb-0">Здесь отображаются только заявки вашего аккаунта.</p></div><button className="btn btn-outline-dark rounded-pill" onClick={() => { clearClientSession(); navigate('/', { replace: true }) }}>Выйти</button></div>
    {isLoading && <p aria-live="polite">Загружаем заявки…</p>}
    {error && <div className="alert alert-danger">{error}</div>}
    {!isLoading && !error && applications.length === 0 && <div className="account-empty"><h2>Заявок пока нет</h2><p>Расскажите нам о своей идее, и заявка появится здесь.</p><Link className="btn btn-dark rounded-pill px-4" to="/request">Оставить заявку</Link></div>}
    <div className="row g-3">{applications.map((application) => <div className="col-lg-6" key={application.id}><article className="account-card"><div className="d-flex justify-content-between gap-3"><span className="eyebrow">{eventTypes[application.eventType]}</span><span className={`status-badge status-badge--${application.status}`}>{statuses[application.status]}</span></div><h2>{new Date(`${application.eventDate}T00:00:00`).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })}</h2>{application.comment && <p>{application.comment}</p>}<small>Создана {new Date(application.createdAtUtc).toLocaleDateString('ru-RU')}</small></article></div>)}</div>
  </div></section>
}
