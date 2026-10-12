import { useState } from 'react'
import { NavLink } from 'react-router-dom'

const navItems = [{ to: '/', label: 'Главная', end: true }, { to: '/services', label: 'Услуги' }, { to: '/portfolio', label: 'Портфолио' }]

export default function Header() {
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  return <header className="site-header sticky-top"><nav className="navbar navbar-expand-lg" aria-label="Главная навигация"><div className="container">
    <NavLink className="navbar-brand brand-mark" to="/" aria-label="Date and Time — главная"><span>DATE</span><i aria-hidden="true" /><span>TIME</span></NavLink>
    <button className="navbar-toggler" type="button" aria-controls="mainNavigation" aria-expanded={isMenuOpen} aria-label={isMenuOpen ? 'Закрыть меню' : 'Открыть меню'} onClick={() => setIsMenuOpen((current) => !current)}><span className="navbar-toggler-icon" /></button>
    <div className={`collapse navbar-collapse${isMenuOpen ? ' show' : ''}`} id="mainNavigation"><ul className="navbar-nav ms-auto align-items-lg-center gap-lg-2">
      {navItems.map((item) => <li className="nav-item" key={item.to}><NavLink className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`} to={item.to} end={item.end} onClick={() => setIsMenuOpen(false)}>{item.label}</NavLink></li>)}
      <li className="nav-item ms-lg-3"><NavLink className="btn btn-dark rounded-pill px-4" to="/request" onClick={() => setIsMenuOpen(false)}>Обсудить проект</NavLink></li>
    </ul></div>
  </div></nav></header>
}
