import { useEffect } from 'react'
import { Route, Routes, useLocation } from 'react-router-dom'
import Layout from './components/Layout'
import HomePage from './pages/HomePage'
import ServicesPage from './pages/ServicesPage'
import PortfolioPage from './pages/PortfolioPage'
import RequestPage from './pages/RequestPage'
import NotFoundPage from './pages/NotFoundPage'

function ScrollToTop() {
  const { pathname } = useLocation()

  useEffect(() => {
    window.scrollTo({ top: 0, behavior: 'auto' })
  }, [pathname])

  return null
}

export default function App() {
  return <><ScrollToTop /><Routes><Route element={<Layout />}><Route index element={<HomePage />} /><Route path="services" element={<ServicesPage />} /><Route path="portfolio" element={<PortfolioPage />} /><Route path="request" element={<RequestPage />} /><Route path="*" element={<NotFoundPage />} /></Route></Routes></>
}
