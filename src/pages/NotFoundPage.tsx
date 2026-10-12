import { Link } from 'react-router-dom'
export default function NotFoundPage() { return <section className="not-found d-flex align-items-center text-center"><div className="container py-5"><span>404</span><h1>Такой страницы нет</h1><p>Похоже, ссылка устарела или в адресе есть ошибка.</p><Link className="btn btn-dark rounded-pill px-4" to="/">Вернуться на главную</Link></div></section> }
