import { FormEvent, useState } from 'react'
import { createApplication } from '../api/applications'
import { useMocks } from '../api/client'
import type { CreateApplicationRequest, EventType } from '../types/application'

const initialForm: CreateApplicationRequest = { clientName: '', phone: '', email: '', eventType: 'wedding', eventDate: '', guestCount: 20, comment: '' }

export default function RequestPage() {
  const [form, setForm] = useState(initialForm)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSubmitted, setIsSubmitted] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const updateField = <K extends keyof CreateApplicationRequest>(key: K, value: CreateApplicationRequest[K]) => setForm((current) => ({ ...current, [key]: value }))
  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => { event.preventDefault(); setError(null); setIsSubmitting(true); try { await createApplication(form); setIsSubmitted(true) } catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Не удалось отправить заявку.') } finally { setIsSubmitting(false) } }

  if (isSubmitted) return <section className="form-page d-flex align-items-center"><div className="container py-5"><div className="success-card mx-auto text-center"><div className="success-card__mark" aria-hidden="true">✓</div><span className="eyebrow">Заявка отправлена</span><h1>Спасибо! Начало положено.</h1><p>{useMocks ? 'Это демо-режим: запрос не ушёл на сервер. Интерфейс успешной отправки работает.' : 'Мы получили ваши данные и скоро свяжемся, чтобы обсудить детали.'}</p><button className="btn btn-outline-dark rounded-pill px-4" type="button" onClick={() => { setForm(initialForm); setIsSubmitted(false) }}>Отправить ещё одну</button></div></div></section>

  return <section className="form-page"><div className="container py-5"><div className="row g-5 py-lg-5"><div className="col-lg-5"><span className="eyebrow">Давайте знакомиться</span><h1 className="display-title mt-3">Расскажите о будущем событии</h1><p className="form-lead">Заполните короткую форму. Мы уточним детали, предложим формат и подготовим ориентировочный расчёт.</p><div className="contact-block mt-5"><span>Можно связаться напрямую</span><a href="tel:+79990000000">+7 999 000-00-00</a><a href="mailto:hello@dateandtime.ru">hello@dateandtime.ru</a></div></div><div className="col-lg-6 ms-lg-auto"><form className="request-form" onSubmit={handleSubmit}><div className="row g-4">
    <div className="col-12"><label className="form-label" htmlFor="clientName">Как к вам обращаться *</label><input className="form-control" id="clientName" value={form.clientName} onChange={(e) => updateField('clientName', e.target.value)} required maxLength={100} autoComplete="name" placeholder="Анна" /></div>
    <div className="col-md-6"><label className="form-label" htmlFor="phone">Телефон *</label><input className="form-control" id="phone" type="tel" value={form.phone} onChange={(e) => updateField('phone', e.target.value)} required maxLength={30} autoComplete="tel" placeholder="+7 999 000-00-00" /></div>
    <div className="col-md-6"><label className="form-label" htmlFor="email">Email *</label><input className="form-control" id="email" type="email" value={form.email} onChange={(e) => updateField('email', e.target.value)} required maxLength={254} autoComplete="email" placeholder="name@example.com" /></div>
    <div className="col-md-6"><label className="form-label" htmlFor="eventType">Тип события *</label><select className="form-select" id="eventType" value={form.eventType} onChange={(e) => updateField('eventType', e.target.value as EventType)} required><option value="wedding">Свадьба</option><option value="birthday">День рождения</option><option value="corporate">Корпоратив</option><option value="other">Другое</option></select></div>
    <div className="col-md-6"><label className="form-label" htmlFor="eventDate">Предполагаемая дата *</label><input className="form-control" id="eventDate" type="date" value={form.eventDate} min={new Date().toISOString().split('T')[0]} onChange={(e) => updateField('eventDate', e.target.value)} required /></div>
    <div className="col-12"><label className="form-label" htmlFor="guestCount">Количество гостей *</label><input className="form-control" id="guestCount" type="number" min="1" max="5000" value={form.guestCount} onChange={(e) => updateField('guestCount', Number(e.target.value))} required /></div>
    <div className="col-12"><label className="form-label" htmlFor="comment">Что для вас важно?</label><textarea className="form-control" id="comment" rows={4} value={form.comment} onChange={(e) => updateField('comment', e.target.value)} maxLength={2000} placeholder="Расскажите о пожеланиях, площадке или примерном бюджете" /></div>
    {error && <div className="col-12"><div className="alert alert-danger mb-0" role="alert">{error}</div></div>}
    <div className="col-12"><button className="btn btn-dark btn-lg rounded-pill w-100" type="submit" disabled={isSubmitting}>{isSubmitting ? 'Отправляем…' : 'Отправить заявку'}</button><p className="form-consent mt-3 mb-0">Нажимая кнопку, вы соглашаетесь на обработку указанных данных для связи по заявке.</p></div>
  </div></form></div></div></div></section>
}
