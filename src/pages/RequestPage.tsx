import { FormEvent, useEffect, useRef, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { createApplication } from '../api/applications'
import { useMocks } from '../api/client'
import type { CreateApplicationRequest, EventType } from '../types/application'

const initialForm: CreateApplicationRequest = { clientName: '', phone: '', email: '', eventType: 'celebration', eventDate: '', guestCount: 20, comment: '' }
const eventTypes: EventType[] = ['celebration', 'theatre', 'birthday', 'wedding', 'corporate', 'other']

function getInitialForm(params: URLSearchParams): CreateApplicationRequest {
  const format = params.get('format') as EventType | null
  const service = params.get('service')?.slice(0, 200)
  return {
    ...initialForm,
    eventType: format && eventTypes.includes(format) ? format : initialForm.eventType,
    comment: service ? `Интересует услуга «${service}».` : '',
  }
}

function getToday() {
  const now = new Date()
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
}

export default function RequestPage() {
  const [searchParams] = useSearchParams()
  const [form, setForm] = useState(() => getInitialForm(searchParams))
  const [guestCount, setGuestCount] = useState(String(initialForm.guestCount))
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSubmitted, setIsSubmitted] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const submittingRef = useRef(false)
  const successHeadingRef = useRef<HTMLHeadingElement>(null)
  const updateField = <K extends keyof CreateApplicationRequest>(key: K, value: CreateApplicationRequest[K]) => {
    setForm((current) => ({ ...current, [key]: value }))
    setError(null)
  }

  useEffect(() => {
    if (isSubmitted) {
      window.scrollTo({ top: 0, behavior: 'auto' })
      successHeadingRef.current?.focus({ preventScroll: true })
    }
  }, [isSubmitted])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (submittingRef.current) return
    setError(null)
    const application = { ...form, clientName: form.clientName.trim(), phone: form.phone.trim(), email: form.email.trim(), comment: form.comment.trim(), guestCount: Number(guestCount) }
    if (application.clientName.length < 2) { setError('Укажите имя: не меньше двух символов.'); return }
    if (application.phone.replace(/\D/g, '').length < 6) { setError('Проверьте номер телефона: укажите не меньше шести цифр.'); return }
    if (application.eventDate < getToday()) { setError('Выберите сегодняшнюю или будущую дату.'); return }
    submittingRef.current = true
    setIsSubmitting(true)
    try { await createApplication(application); setIsSubmitted(true) }
    catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Не удалось отправить заявку.') }
    finally { submittingRef.current = false; setIsSubmitting(false) }
  }

  if (isSubmitted) return <section className="form-page d-flex align-items-center"><div className="container py-5"><div className="success-card mx-auto text-center"><div className="success-card__mark" aria-hidden="true">✓</div><span className="eyebrow">{useMocks ? 'Демонстрация отправки' : 'Заявка отправлена'}</span><h1 ref={successHeadingRef} tabIndex={-1}>Спасибо! Начало положено.</h1><p>{useMocks ? 'Это демо-режим: запрос не ушёл на сервер. Интерфейс успешной отправки работает.' : 'Мы получили ваши данные и скоро свяжемся, чтобы обсудить детали.'}</p><button className="btn btn-outline-dark rounded-pill px-4" type="button" onClick={() => { setForm(getInitialForm(searchParams)); setGuestCount(String(initialForm.guestCount)); setError(null); setIsSubmitted(false) }}>Отправить ещё одну</button></div></div></section>

  return <section className="form-page"><div className="container py-5"><div className="row g-5 py-lg-5"><div className="col-lg-5"><span className="eyebrow">Давайте знакомиться</span><h1 className="display-title mt-3">Расскажите о своей идее</h1><p className="form-lead">Заполните короткую форму. Мы уточним детали, предложим творческий формат и подготовим ориентировочный расчёт.</p><div className="contact-block mt-5"><span>Можно связаться напрямую</span><a href="tel:+79990000000">+7 999 000-00-00</a><a href="mailto:hello@dateandtime.ru">hello@dateandtime.ru</a></div></div><div className="col-lg-6 ms-lg-auto"><form className="request-form" onSubmit={handleSubmit} aria-busy={isSubmitting}><fieldset disabled={isSubmitting}><legend className="visually-hidden">Данные для заявки</legend><div className="row g-4">
    <div className="col-12"><label className="form-label" htmlFor="clientName">Как к вам обращаться *</label><input className="form-control" id="clientName" name="clientName" value={form.clientName} onChange={(e) => updateField('clientName', e.target.value)} required minLength={2} maxLength={100} autoComplete="name" placeholder="Анна" /></div>
    <div className="col-md-6"><label className="form-label" htmlFor="phone">Телефон *</label><input className="form-control" id="phone" name="phone" type="tel" value={form.phone} onChange={(e) => updateField('phone', e.target.value)} required minLength={6} maxLength={30} pattern="[0-9+\s\(\)\-]+" title="Укажите номер телефона цифрами; можно использовать +, пробелы, скобки и дефисы." autoComplete="tel" placeholder="+7 999 000-00-00" /></div>
    <div className="col-md-6"><label className="form-label" htmlFor="email">Email *</label><input className="form-control" id="email" name="email" type="email" value={form.email} onChange={(e) => updateField('email', e.target.value)} required maxLength={254} autoComplete="email" placeholder="name@example.com" /></div>
    <div className="col-md-6"><label className="form-label" htmlFor="eventType">Формат проекта *</label><select className="form-select" id="eventType" name="eventType" value={form.eventType} onChange={(e) => updateField('eventType', e.target.value as EventType)} required><option value="celebration">Праздник</option><option value="theatre">Театральная программа</option><option value="birthday">День рождения</option><option value="wedding">Свадьба</option><option value="corporate">Корпоратив</option><option value="other">Другое</option></select></div>
    <div className="col-md-6"><label className="form-label" htmlFor="eventDate">Предполагаемая дата *</label><input className="form-control" id="eventDate" name="eventDate" type="date" value={form.eventDate} min={getToday()} onChange={(e) => updateField('eventDate', e.target.value)} required /></div>
    <div className="col-12"><label className="form-label" htmlFor="guestCount">Количество гостей / зрителей *</label><input className="form-control" id="guestCount" name="guestCount" type="number" min="1" max="5000" step="1" value={guestCount} onChange={(e) => { setGuestCount(e.target.value); setError(null) }} required /></div>
    <div className="col-12"><label className="form-label" htmlFor="comment">Что для вас важно?</label><textarea className="form-control" id="comment" name="comment" rows={4} value={form.comment} onChange={(e) => updateField('comment', e.target.value)} maxLength={2000} placeholder="Расскажите о пожеланиях, площадке или примерном бюджете" /></div>
    {useMocks && <div className="col-12"><p className="demo-note mb-0">Демо-режим: заявки пока не сохраняются.</p></div>}
    {error && <div className="col-12"><div className="alert alert-danger mb-0" role="alert">{error}</div></div>}
    <div className="col-12"><button className="btn btn-dark btn-lg rounded-pill w-100" type="submit" disabled={isSubmitting}>{isSubmitting ? 'Отправляем…' : 'Отправить заявку'}</button><p className="form-consent mt-3 mb-0">Нажимая кнопку, вы соглашаетесь на обработку указанных данных для связи по заявке.</p></div>
  </div></fieldset></form></div></div></div></section>
}
