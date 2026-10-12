import { Link } from 'react-router-dom'
import PageIntro from '../components/PageIntro'

const projects = [
  { type: 'Театральная программа', title: 'Тихий сад', meta: '45 зрителей · загородная площадка', className: 'project-card--garden' },
  { type: 'Интерактивный спектакль', title: 'Новая глава', meta: '120 зрителей · городская сцена', className: 'project-card--night' },
  { type: 'Семейный праздник', title: 'Летний полдень', meta: '30 гостей · открытая веранда', className: 'project-card--summer' },
  { type: 'Городской проект', title: 'Город и мы', meta: 'Большая аудитория · открытая площадка', className: 'project-card--city' },
]

export default function PortfolioPage() {
  return <><PageIntro eyebrow="Наши проекты" title="У каждой истории свой ритм и характер" description="Здесь появятся реальные фотографии праздников, постановок и творческих программ. Пока подготовлена структура карточек, которую Backend сможет наполнять через API." /><section className="pb-section"><div className="container"><div className="portfolio-grid">{projects.map((project, index) => <article className={`project-card ${project.className}`} key={project.title}><span className="project-card__index">0{index + 1}</span><div className="project-card__content"><span>{project.type}</span><h2>{project.title}</h2><p>{project.meta}</p></div></article>)}</div><div className="portfolio-cta mt-5 text-center"><h2>Следующей может быть ваша история</h2><p>Расскажите нам о своей идее — ответим и предложим первые варианты воплощения.</p><Link className="btn btn-dark rounded-pill px-4" to="/request">Обсудить проект</Link></div></div></section></>
}
