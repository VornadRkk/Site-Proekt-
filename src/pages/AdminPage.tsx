import { FormEvent, useEffect, useState } from 'react'

type Service = { id: string; name: string; shortDescription: string; priceFrom: number; category: string; isActive: boolean; displayOrder: number }
type PortfolioItem = { id: string; title: string; type: string; description: string; imageUrl?: string; displayOrder: number; isPublished: boolean }
type Application = { id: string; clientName: string; phone: string; email: string; eventType: string; eventDate: string; guestCount: number; status: string }
const tokenKey = 'dateandtime.admin.token'

async function request<T>(path: string, token: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${import.meta.env.VITE_API_URL ?? ''}${path}`, { ...init, headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}`, ...init?.headers } })
  if (!response.ok) throw new Error(response.status === 401 ? 'Сессия истекла или недостаточно прав.' : `Ошибка API (${response.status})`)
  return response.status === 204 ? undefined as T : response.json() as Promise<T>
}

export default function AdminPage() {
  const [token, setToken] = useState(() => localStorage.getItem(tokenKey) ?? '')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [services, setServices] = useState<Service[]>([])
  const [portfolio, setPortfolio] = useState<PortfolioItem[]>([])
  const [applications, setApplications] = useState<Application[]>([])
  const [message, setMessage] = useState('')
  const [serviceName, setServiceName] = useState('')
  const [serviceDescription, setServiceDescription] = useState('')
  const [servicePrice, setServicePrice] = useState('')
  const [portfolioTitle, setPortfolioTitle] = useState('')
  const [portfolioType, setPortfolioType] = useState('')
  const [portfolioDescription, setPortfolioDescription] = useState('')

  const refresh = async (auth: string) => {
    try {
      const [serviceData, portfolioData, applicationData] = await Promise.all([
        request<Service[]>('/api/services/admin', auth), request<PortfolioItem[]>('/api/portfolio/admin', auth), request<Application[]>('/api/applications', auth),
      ])
      setServices(serviceData); setPortfolio(portfolioData); setApplications(applicationData); setMessage('')
    } catch (error) { setMessage(error instanceof Error ? error.message : 'Не удалось загрузить данные.') }
  }
  useEffect(() => { if (token) void refresh(token) }, [token])

  const login = async (event: FormEvent) => {
    event.preventDefault()
    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL ?? ''}/api/auth/login`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email, password }) })
      if (!response.ok) throw new Error('Неверный email или пароль.')
      const result = await response.json() as { accessToken: string }
      localStorage.setItem(tokenKey, result.accessToken); setToken(result.accessToken); setPassword(''); setMessage('')
    } catch (error) { setMessage(error instanceof Error ? error.message : 'Не удалось войти.') }
  }
  const createService = async (event: FormEvent) => {
    event.preventDefault()
    try { await request('/api/services', token, { method: 'POST', body: JSON.stringify({ name: serviceName, shortDescription: serviceDescription, priceFrom: Number(servicePrice), category: 'celebration', isActive: true, displayOrder: services.length + 1 }) }); setServiceName(''); setServiceDescription(''); setServicePrice(''); await refresh(token) }
    catch (error) { setMessage(error instanceof Error ? error.message : 'Не удалось создать услугу.') }
  }
  const createPortfolio = async (event: FormEvent) => {
    event.preventDefault()
    try { await request('/api/portfolio', token, { method: 'POST', body: JSON.stringify({ title: portfolioTitle, type: portfolioType, description: portfolioDescription, displayOrder: portfolio.length + 1, isPublished: true }) }); setPortfolioTitle(''); setPortfolioType(''); setPortfolioDescription(''); await refresh(token) }
    catch (error) { setMessage(error instanceof Error ? error.message : 'Не удалось создать проект.') }
  }
  const editService = async (item: Service) => {
    const name = window.prompt('Название услуги', item.name); if (name === null) return
    const shortDescription = window.prompt('Краткое описание', item.shortDescription); if (shortDescription === null) return
    const price = window.prompt('Цена от', String(item.priceFrom)); if (price === null || !Number.isFinite(Number(price))) return
    try { await request(`/api/services/${item.id}`, token, { method: 'PUT', body: JSON.stringify({ name, shortDescription, priceFrom: Number(price), category: item.category, isActive: item.isActive, displayOrder: item.displayOrder }) }); await refresh(token) }
    catch (error) { setMessage(error instanceof Error ? error.message : 'Не удалось изменить услугу.') }
  }
  const editPortfolio = async (item: PortfolioItem) => {
    const title = window.prompt('Название проекта', item.title); if (title === null) return
    const type = window.prompt('Тип мероприятия', item.type); if (type === null) return
    const description = window.prompt('Описание', item.description); if (description === null) return
    try { await request(`/api/portfolio/${item.id}`, token, { method: 'PUT', body: JSON.stringify({ title, type, description, imageUrl: item.imageUrl, displayOrder: item.displayOrder, isPublished: item.isPublished }) }); await refresh(token) }
    catch (error) { setMessage(error instanceof Error ? error.message : 'Не удалось изменить проект.') }
  }
  const remove = async (path: string) => { try { await request(path, token, { method: 'DELETE' }); await refresh(token) } catch (error) { setMessage(error instanceof Error ? error.message : 'Не удалось выполнить операцию.') } }

  if (!token) return <section className="form-page"><div className="container py-5"><form className="request-form mx-auto" style={{ maxWidth: 520 }} onSubmit={login}><span className="eyebrow">Управление сайтом</span><h1 className="display-title mt-3">Вход администратора</h1><label className="form-label mt-3" htmlFor="adminEmail">Email</label><input id="adminEmail" className="form-control" type="email" autoComplete="username" required value={email} onChange={(event) => setEmail(event.target.value)} /><label className="form-label mt-3" htmlFor="adminPassword">Пароль</label><input id="adminPassword" className="form-control" type="password" autoComplete="current-password" required value={password} onChange={(event) => setPassword(event.target.value)} />{message && <p className="alert alert-danger mt-3">{message}</p>}<button className="btn btn-dark rounded-pill mt-4">Войти</button></form></div></section>

  return <main className="section-padding"><div className="container"><div className="d-flex justify-content-between align-items-center mb-4"><div><span className="eyebrow">Панель управления</span><h1 className="display-title mt-2">Администрирование</h1></div><button className="btn btn-outline-dark rounded-pill" onClick={() => { localStorage.removeItem(tokenKey); setToken('') }}>Выйти</button></div>{message && <div className="alert alert-warning">{message}</div>}
    <section className="mb-5"><h2>Услуги</h2><form className="row g-2 mb-3" onSubmit={createService}><div className="col-md-3"><input className="form-control" placeholder="Название" required value={serviceName} onChange={(event) => setServiceName(event.target.value)} /></div><div className="col-md-5"><input className="form-control" placeholder="Краткое описание" required value={serviceDescription} onChange={(event) => setServiceDescription(event.target.value)} /></div><div className="col-md-2"><input className="form-control" type="number" min="0" placeholder="Цена от" required value={servicePrice} onChange={(event) => setServicePrice(event.target.value)} /></div><div className="col-md-2"><button className="btn btn-dark w-100">Добавить</button></div></form><ul className="list-group">{services.map((item) => <li className="list-group-item d-flex justify-content-between" key={item.id}><span>{item.name}{!item.isActive && ' (скрыта)'}</span><span className="d-flex gap-2"><button className="btn btn-sm btn-outline-secondary" onClick={() => void editService(item)}>Изменить</button><button className="btn btn-sm btn-outline-danger" onClick={() => void remove(`/api/services/${item.id}`)}>{item.isActive ? 'Скрыть' : 'Удалить'}</button></span></li>)}</ul></section>
    <section className="mb-5"><h2>Портфолио</h2><form className="row g-2 mb-3" onSubmit={createPortfolio}><div className="col-md-3"><input className="form-control" placeholder="Название проекта" required value={portfolioTitle} onChange={(event) => setPortfolioTitle(event.target.value)} /></div><div className="col-md-3"><input className="form-control" placeholder="Тип мероприятия" required value={portfolioType} onChange={(event) => setPortfolioType(event.target.value)} /></div><div className="col-md-4"><input className="form-control" placeholder="Описание" required value={portfolioDescription} onChange={(event) => setPortfolioDescription(event.target.value)} /></div><div className="col-md-2"><button className="btn btn-dark w-100">Добавить</button></div></form><ul className="list-group">{portfolio.map((item) => <li className="list-group-item d-flex justify-content-between" key={item.id}><span>{item.title}{!item.isPublished && ' (снята с публикации)'}</span><span className="d-flex gap-2"><button className="btn btn-sm btn-outline-secondary" onClick={() => void editPortfolio(item)}>Изменить</button><button className="btn btn-sm btn-outline-danger" onClick={() => void remove(`/api/portfolio/${item.id}`)}>Скрыть</button></span></li>)}</ul></section>
    <section><h2>Заявки</h2><div className="table-responsive"><table className="table"><thead><tr><th>Клиент</th><th>Контакты</th><th>Дата</th><th>Гостей</th><th>Статус</th><th>Действие</th></tr></thead><tbody>{applications.map((item) => <tr key={item.id}><td>{item.clientName}</td><td>{item.phone}<br />{item.email}</td><td>{item.eventDate}</td><td>{item.guestCount}</td><td><select className="form-select" value={item.status} onChange={async (event) => { try { await request(`/api/applications/${item.id}/status`, token, { method: 'PATCH', body: JSON.stringify({ status: event.target.value }) }); await refresh(token) } catch (error) { setMessage(error instanceof Error ? error.message : 'Не удалось изменить статус.') } }}><option value="new">Новая</option><option value="inProgress">В работе</option><option value="completed">Завершена</option><option value="rejected">Отклонена</option></select></td><td><button className="btn btn-sm btn-outline-danger" onClick={() => { if (window.confirm('Удалить эту заявку безвозвратно?')) void remove(`/api/applications/${item.id}`) }}>Удалить</button></td></tr>)}</tbody></table></div></section>
  </div></main>
}
