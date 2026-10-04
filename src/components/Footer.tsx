import { Link } from 'react-router-dom'

export default function Footer() {
  return <footer className="site-footer"><div className="container py-5"><div className="row gy-4 align-items-end">
    <div className="col-lg-6"><div className="brand-mark brand-mark--light mb-3"><span>DATE</span><i aria-hidden="true" /><span>TIME</span></div><p className="footer-lead mb-0">Создаём события, к которым хочется возвращаться.</p></div>
    <div className="col-6 col-lg-3"><p className="footer-label">Навигация</p><Link to="/services">Услуги</Link><Link to="/portfolio">Портфолио</Link><Link to="/request">Оставить заявку</Link></div>
    <div className="col-6 col-lg-3"><p className="footer-label">Связаться</p><a href="tel:+79990000000">+7 999 000-00-00</a><a href="mailto:hello@dateandtime.ru">hello@dateandtime.ru</a><span>Самара</span></div>
  </div><div className="footer-bottom mt-5 pt-4 d-flex flex-wrap justify-content-between gap-2"><span>© {new Date().getFullYear()} Date and Time</span><span>Организация событий с вниманием к деталям</span></div></div></footer>
}
