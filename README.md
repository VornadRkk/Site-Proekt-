# Date and Time — frontend

MVP сайта творческой команды **Date and Time**, которая создаёт праздники, театрализованные программы и другие проекты. Frontend создан на React, TypeScript и Vite; Backend — на ASP.NET Core Web API и EF Core с MySQL.

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
VITE_API_URL=http://localhost:5080
VITE_USE_MOCKS=false
```

Frontend ожидает:

- `GET /api/services` — массив объектов `{ id, name, shortDescription, priceFrom, category? }` или `{ items: [...] }`;
- `POST /api/applications` — создание заявки с полями `clientName`, `phone`, `email`, `eventType`, `eventDate`, `guestCount`, `comment`.

Категории: `celebration`, `theatre`, `birthday`, `wedding`, `corporate`, `other`. Успешным считается ответ со статусом `2xx`. CORS на Backend должен разрешать адрес Vite, обычно `http://localhost:5173`.

Пример услуги:

```json
{
  "id": 1,
  "name": "Театрализованная программа",
  "shortDescription": "Авторский сценарий и живое представление для вашей площадки.",
  "priceFrom": 60000,
  "category": "theatre"
}
```

Пример заявки:

```json
{
  "clientName": "Анна",
  "phone": "+7 999 123-45-67",
  "email": "anna@example.com",
  "eventType": "theatre",
  "eventDate": "2026-12-20",
  "guestCount": 50,
  "comment": "Хотим интерактивную постановку для семейного праздника"
}
```

## Команды

```bash
npm run dev
npm run build
npm run preview
npm run lint
```

## Backend

API находится в `backend/DateAndTime.Api` и рассчитан на .NET 8 и MySQL 8.4. Реализованы:

- `GET /api/services` — активные услуги из MySQL;
- `POST /api/applications` — создание заявки с валидацией;
- миграция со схемой и начальными услугами;
- CORS для локального Frontend;
- ограничение отправки заявок до 10 запросов в минуту на IP;
- Swagger в Development и health-check `GET /health`.

Запуск API и MySQL через Docker:

```bash
docker compose up --build
```

API будет доступен на `http://localhost:5080`, Swagger — на `http://localhost:5080/swagger`, MySQL — на `localhost:3306`.

Для запуска без Docker нужны .NET 8 SDK и локальный MySQL 8.4:

```bash
dotnet tool restore
dotnet ef database update --project backend/DateAndTime.Api
dotnet run --project backend/DateAndTime.Api
```

Настройки подключения для разработки находятся в `backend/DateAndTime.Api/appsettings.json`. Для общего или production-сервера строку подключения следует передавать через переменную `ConnectionStrings__MySql` или хранилище секретов.

Проверки Backend:

```bash
dotnet build DateAndTime.sln --configuration Release
dotnet test DateAndTime.sln --configuration Release
```

Административную панель, JWT и защищённые CRUD endpoint следует добавлять после согласования ролей и сценария создания первого администратора.

## Развёртывание на сервере

Рабочий сайт: http://194.226.125.251. Production использует реальный API и MySQL; локальный `npm run dev` сохраняет демо-режим.

После push в `main` GitHub Actions собирает и проверяет проект, загружает релиз на сервер и переключает сайт на новую версию. При неуспешной проверке API возвращается предыдущая версия кода.

Настройка nginx, MySQL, systemd и автоматического обновления из ветки `main` описана в [deploy/README.md](deploy/README.md).
