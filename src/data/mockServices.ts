import type { Service } from '../types/service'

export const mockServices: Service[] = [
  { id: 1, name: 'Свадьба под ключ', shortDescription: 'Концепция, площадка, подрядчики, декор и координация самого важного дня.', priceFrom: 180000, category: 'wedding' },
  { id: 2, name: 'День рождения', shortDescription: 'Личный сценарий, оформление и развлечения для камерного вечера или большой вечеринки.', priceFrom: 75000, category: 'birthday' },
  { id: 3, name: 'Корпоративное событие', shortDescription: 'Событие для команды: от деловой части и тимбилдинга до яркой вечерней программы.', priceFrom: 120000, category: 'corporate' },
  { id: 4, name: 'Координация мероприятия', shortDescription: 'Возьмём готовый план, проверим детали и проконтролируем работу команды в день события.', priceFrom: 35000, category: 'other' },
  { id: 5, name: 'Оформление и декор', shortDescription: 'Разработаем визуальную концепцию и соберём пространство, которое поддерживает вашу историю.', priceFrom: 50000, category: 'other' },
  { id: 6, name: 'Фото и видео', shortDescription: 'Подберём проверенную съёмочную команду и сохраним живые моменты вашего события.', priceFrom: 45000, category: 'other' },
]
