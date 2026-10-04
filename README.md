# Date and Time — frontend

Frontend MVP сайта event-агентства **Date and Time**. Проект создан на React, TypeScript и Vite, для сетки и компонентов используется Bootstrap 5.

## Что уже есть

- адаптивные страницы «Главная», «Услуги», «Портфолио» и «Оставить заявку»;
- загрузка услуг через Backend API с состояниями загрузки и ошибки;
- демо-режим для разработки без запущенного Backend;
- отправка заявки через API и клиентская валидация;
- единый API-клиент и TypeScript-типы для согласования контракта.

## Запуск

Нужен Node.js 20 или новее.

```bash
npm install
copy .env.example .env
npm run dev
```

## Подключение Backend

В `.env` укажите адрес ASP.NET Core API и отключите демо-режим:

```env
VITE_API_URL=https://localhost:7001
VITE_USE_MOCKS=false
```

Frontend ожидает:

- `GET /api/services` — массив объектов `{ id, name, shortDescription, priceFrom, category? }` или `{ items: [...] }`;
- `POST /api/applications` — создание заявки с полями `clientName`, `phone`, `email`, `eventType`, `eventDate`, `guestCount`, `comment`.

Категории: `wedding`, `birthday`, `corporate`, `other`. Успешным считается ответ со статусом `2xx`. CORS на Backend должен разрешать адрес Vite, обычно `http://localhost:5173`.

Пример услуги:

```json
{
  "id": 1,
  "name": "Свадьба под ключ",
  "shortDescription": "Организация свадьбы от идеи до финального тайминга.",
  "priceFrom": 180000,
  "category": "wedding"
}
```

Пример заявки:

```json
{
  "clientName": "Анна",
  "phone": "+7 999 123-45-67",
  "email": "anna@example.com",
  "eventType": "wedding",
  "eventDate": "2026-12-20",
  "guestCount": 50,
  "comment": "Хотим камерную свадьбу за городом"
}
```

## Команды

```bash
npm run dev
npm run build
npm run preview
npm run lint
```

Административную панель логично добавлять после согласования авторизации, ролей и CRUD endpoint с Backend-командой.
