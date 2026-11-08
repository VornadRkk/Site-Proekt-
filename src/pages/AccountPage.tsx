import { FormEvent, useEffect, useState } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { authAllowed, authenticate, session, tokenKey, type Session } from '../api/auth'
import PrivacyConsent from '../components/PrivacyConsent'
import { apiRequest } from '../api/client'
type Application = { id: string; eventDate: string; status: string }
export default function AccountPage() {
 const [user,setUser]=useState<Session|null>(null), [loading,setLoading]=useState(true), [items,setItems]=useState<Application[]>([])
 const [mode,setMode]=useState('login'),[email,setEmail]=useState(''),[password,setPassword]=useState(''),[name,setName]=useState(''),[error,setError]=useState('')
 const load=async()=>{const u=await session(); const data=await apiRequest<Application[]>('/api/applications/mine');setUser(u);setItems(data)}
 useEffect(()=>{if (!authAllowed) return;void load().catch(()=>{sessionStorage.removeItem(tokenKey)}).finally(()=>setLoading(false))},[])
 const submit=async(e:FormEvent)=>{e.preventDefault();setError('');try{await authenticate(mode,email,password,name);setPassword('');await load()}catch{setError('Не удалось войти или зарегистрироваться. Проверьте данные; пароль при регистрации — от 12 символов.')}}
 if (!authAllowed) return <Navigate to="/" replace />
 if(loading)return <div className="container py-5" role="status">Проверяем сессию…</div>
 return <section className="container py-5"><h1>Личный кабинет</h1>{error&&<p role="alert">{error}</p>}{user?<><p>{user.displayName}</p><button className="btn btn-outline-dark" onClick={()=>{sessionStorage.removeItem(tokenKey);setUser(null);setItems([])}}>Выйти</button><p><Link to="/request">Оставить заявку</Link></p><h2>Мои заявки</h2>{items.length===0?<p>У вас пока нет заявок.</p>:<ul>{items.map(x=><li key={x.id}>{x.eventDate} — {({new:'Новая',inProgress:'В работе',completed:'Завершена',rejected:'Отклонена'} as Record<string,string>)[x.status]??x.status}</li>)}</ul>}</>:<form className="request-form" style={{maxWidth:520}} onSubmit={submit}><h2>{mode==='login'?'Вход':'Регистрация'}</h2>{mode==='register'&&<label className="d-block">Имя<input className="form-control" required maxLength={120} value={name} onChange={e=>setName(e.target.value)}/></label>}<label className="d-block">Email<input className="form-control" type="email" required autoComplete="username" value={email} onChange={e=>setEmail(e.target.value)}/></label><label className="d-block">Пароль<input className="form-control" type="password" required minLength={mode==='register'?12:8} maxLength={128} autoComplete={mode==='register'?'new-password':'current-password'} value={password} onChange={e=>setPassword(e.target.value)}/></label><PrivacyConsent key={mode} /><button className="btn btn-dark mt-3">{mode==='login'?'Войти':'Зарегистрироваться'}</button><button className="btn btn-link" type="button" onClick={()=>setMode(mode==='login'?'register':'login')}>{mode==='login'?'Создать аккаунт':'Уже есть аккаунт'}</button></form>}</section>
}

