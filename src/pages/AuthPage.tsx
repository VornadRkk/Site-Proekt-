import { FormEvent, useState } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { clientAuthEnabled, getClientSession, loginClient, registerClient } from '../api/auth'

export default function AuthPage() {
  const navigate = useNavigate()
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')

  if (!clientAuthEnabled) return <Navigate to="/" replace />
  if (getClientSession()) return <Navigate to="/account" replace />

  const submit = async (event: FormEvent) => {
    event.preventDefault(); setError(''); setIsSubmitting(true)
    try {
      if (mode === 'register') await registerClient(displayName.trim(), email.trim(), password)
      else await loginClient(email.trim(), password)
      navigate('/account', { replace: true })
    } catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Не удалось выполнить вход.') }
    finally { setIsSubmitting(false) }
  }

  return <section className="form-page auth-page"><div className="container py-5"><div className="row justify-content-center"><div className="col-lg-6 col-xl-5"><form className="request-form" onSubmit={submit} aria-busy={isSubmitting}>
    <span className="eyebrow">Личный кабинет</span><h1 className="display-title mt-3">{mode === 'login' ? 'Вход' : 'Регистрация'}</h1>
    <div className="auth-switch my-4" role="group" aria-label="Режим формы"><button type="button" className={mode === 'login' ? 'active' : ''} onClick={() => { setMode('login'); setError('') }}>Войти</button><button type="button" className={mode === 'register' ? 'active' : ''} onClick={() => { setMode('register'); setError('') }}>Создать аккаунт</button></div>
    {mode === 'register' && <><label className="form-label" htmlFor="displayName">Имя *</label><input className="form-control mb-3" id="displayName" autoComplete="name" minLength={2} maxLength={120} required value={displayName} onChange={(event) => setDisplayName(event.target.value)} /></>}
    <label className="form-label" htmlFor="clientEmail">Email *</label><input className="form-control mb-3" id="clientEmail" type="email" autoComplete="email" maxLength={254} required value={email} onChange={(event) => setEmail(event.target.value)} />
    <label className="form-label" htmlFor="clientPassword">Пароль *</label><input className="form-control" id="clientPassword" type="password" autoComplete={mode === 'login' ? 'current-password' : 'new-password'} minLength={mode === 'register' ? 12 : 1} maxLength={128} required value={password} onChange={(event) => setPassword(event.target.value)} />
    {mode === 'register' && <p className="form-consent mt-2">Используйте не менее 12 символов.</p>}
    {error && <div className="alert alert-danger mt-3" role="alert">{error}</div>}
    <button className="btn btn-dark btn-lg rounded-pill w-100 mt-4" disabled={isSubmitting}>{isSubmitting ? 'Подождите…' : mode === 'login' ? 'Войти' : 'Зарегистрироваться'}</button>
    <p className="form-consent text-center mt-3 mb-0">Администратор входит через <Link to="/admin">служебную страницу</Link>.</p>
  </form></div></div></div></section>
}
