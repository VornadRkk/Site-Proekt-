import { Link } from 'react-router-dom'
import PageIntro from '../components/PageIntro'

const projects = [
  { type: 'Свадьба', title: 'Тихий сад', meta: '45 гостей · загородный дом', className: 'project-card--garden' },
  { type: 'Корпоратив', title: 'Новая глава', meta: '120 гостей · лофт', className: 'project-card--night' },
  { type: 'День рождения', title: 'Летний полдень', meta: '30 гостей · веранда', className: 'project-card--summer' },
  { type: 'Свадьба', title: 'Город и мы', meta: '70 гостей · панорамный зал', className: 'project-card--city' },
]

export default function PortfolioPage() {
  return <><PageIntro eyebrow="Наши проекты" title="У каждого события свой ритм и характер" description="Здесь появятся реальные фотографии и истории проектов. Пока подготовлена структура карточек, которую Backend сможет наполнять через API." /><section className="pb-section"><div className="container"><div className="portfolio-grid">{projects.map((project, index) => <article className={`project-card ${project.className}`} key={project.title}><span className="project-card__index">0{index + 1}</span><div className="project-card__content"><span>{project.type}</span><h2>{project.title}</h2><p>{project.meta}</p></div></article>)}</div><div className="portfolio-cta mt-5 text-center"><h2>Следующей может быть ваша история</h2><p>Расскажите нам о будущем событии — ответим и предложим первые варианты.</p><Link className="btn btn-dark rounded-pill px-4" to="/request">Обсудить проект</Link></div></div></section></>
}
