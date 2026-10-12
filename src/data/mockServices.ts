import type { Service } from '../types/service'

export const mockServices: Service[] = [
  { id: 1, name: 'Театрализованная программа', shortDescription: 'Создадим авторский сценарий, образы и живое представление для вашей площадки и зрителей.', priceFrom: 60000, category: 'theatre' },
  { id: 2, name: 'Праздник под ключ', shortDescription: 'Соберём идею, оформление, артистов и программу в цельный праздник с вашим характером.', priceFrom: 80000, category: 'celebration' },
  { id: 3, name: 'Интерактивный спектакль', shortDescription: 'Вовлечём гостей в историю, где зрители становятся участниками происходящего.', priceFrom: 45000, category: 'theatre' },
  { id: 4, name: 'День рождения', shortDescription: 'Придумаем личный сценарий для семейного праздника, камерного вечера или большой вечеринки.', priceFrom: 75000, category: 'birthday' },
  { id: 5, name: 'Свадебная программа', shortDescription: 'Поможем наполнить важный день живой историей, творческими номерами и тёплыми деталями.', priceFrom: 120000, category: 'wedding' },
  { id: 6, name: 'Корпоративный праздник', shortDescription: 'Подготовим яркую программу для команды: от тематического вечера до большого представления.', priceFrom: 100000, category: 'corporate' },
]
