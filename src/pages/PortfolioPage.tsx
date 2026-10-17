import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import PageIntro from '../components/PageIntro'

type PortfolioItem = { id: string; type: string; title: string; description: string; imageUrl?: string }
const examples: PortfolioItem[] = [
  { id: 'garden', type: 'Театральная программа', title: 'Тихий сад', description: '45 зрителей · загородная площадка' },
  { id: 'night', type: 'Интерактивный спектакль', title: 'Новая глава', description: '120 зрителей · городская сцена' },
  { id: 'summer', type: 'Семейный праздник', title: 'Летний полдень', description: '30 гостей · открытая веранда' },
  { id: 'city', type: 'Городской проект', title: 'Город и мы', description: 'Большая аудитория · открытая площадка' },
]

export default function PortfolioPage() {
  const [projects, setProjects] = useState<PortfolioItem[]>(examples)
  useEffect(() => {
    fetch(`${import.meta.env.VITE_API_URL ?? ''}/api/portfolio`)
      .then((response) => response.ok ? response.json() as Promise<PortfolioItem[]> : Promise.reject())
      .then((items) => { if (items.length) setProjects(items) })
      .catch(() => undefined)
  }, [])
  return <><PageIntro eyebrow="Наши проекты" title="У каждой истории свой ритм и характер" description="Праздники, постановки и творческие программы Date and Time." /><section className="pb-section"><div className="container"><div className="portfolio-grid">{projects.map((project, index) => <article className={`project-card project-card--${['garden', 'night', 'summer', 'city'][index % 4]}`} key={project.id}><span className="project-card__index">{String(index + 1).padStart(2, '0')}</span>{project.imageUrl && <img src={project.imageUrl} alt="" loading="lazy" />}<div className="project-card__content"><span>{project.type}</span><h2>{project.title}</h2><p>{project.description}</p></div></article>)}</div><div className="portfolio-cta mt-5 text-center"><h2>Следующей может быть ваша история</h2><p>Расскажите нам о своей идее — обсудим варианты воплощения.</p><Link className="btn btn-dark rounded-pill px-4" to="/request">Обсудить проект</Link></div></div></section></>
}
