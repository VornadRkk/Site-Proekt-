# Date and Time

MVP сайта творческой команды, которая создаёт праздники, театрализованные и игровые программы, фестивали и другие творческие проекты.

Стек: React, TypeScript, Vite, Bootstrap, ASP.NET Core Web API, Entity Framework Core и MySQL 8.4.

Требования и контракты находятся в [docs/requirements-and-acceptance.md](docs/requirements-and-acceptance.md), [docs/api-contracts.md](docs/api-contracts.md) и [docs/data-dictionary.md](docs/data-dictionary.md).

## Возможности

- адаптивные страницы: главная, услуги, портфолио, контакты и форма заявки;
- каталог услуг из API с ориентировочной ценой;
- анонимные заявки без указания количества гостей;
- регистрация Client, вход и личный кабинет с собственными заявками;
- закрытая административная панель с серверной проверкой роли Admin;
- health/readiness endpoints, JSON-логи и Prometheus-метрики;
- unit/API-тесты, Playwright E2E, k6 и security-проверки GitHub Actions.

## Локальный Frontend

Нужен Node.js 20 или новее.

```powershell
npm install
Copy-Item .env.example .env
npm run dev
```

По умолчанию локальный Frontend использует демо-данные. Для реального API задайте в `.env`:

```env
VITE_API_URL=http://localhost:5080
VITE_USE_MOCKS=false
VITE_ENABLE_CLIENT_AUTH=true
```

## Полный запуск через Docker Compose

```bash
docker compose up --build
```

- сайт: `http://localhost:8080`;
- API и Swagger: `http://localhost:5080`, `http://localhost:5080/swagger`;
- MySQL: `localhost:3306`.

Development-пароли в `compose.yaml` предназначены только для локальной машины.

Мониторинг запускается отдельным профилем:

```bash
docker compose --profile monitoring up --build
```

- Prometheus: `http://localhost:9090`;
- Grafana: `http://localhost:3000`, локальный вход `admin / grafana_dev`.

## API

Основные маршруты:

- `GET /api/services`;
- `POST /api/applications`;
- `POST /api/auth/register` и `POST /api/auth/login`;
- `GET /api/applications/mine` для Client;
- административные CRUD endpoints с ролью Admin;
- `GET /health`, `GET /health/ready`, `GET /metrics`.

Пример заявки:

```json
{
  "clientName": "Анна",
  "phone": "+7 999 123-45-67",
  "email": "anna@example.com",
  "eventType": "festival",
  "eventDate": "2027-06-20",
  "comment": "Нужна программа для городского фестиваля"
}
```

## Проверки

```bash
npm run lint
npm run build
dotnet test DateAndTime.sln --configuration Release
```

Для E2E сначала запустите Docker Compose и установите Chromium Playwright:

```bash
npx playwright install chromium
npm run test:e2e
```

Нагрузочная проверка при установленном k6:

```bash
k6 run -e BASE_URL=http://localhost:8080 tests/load/public-api.js
```

## Production

Рабочая версия размещается на `http://194.226.125.251`. Автоматическое развёртывание и первоначальная настройка сервера описаны в [deploy/README.md](deploy/README.md).

Production пока работает по HTTP на IP, поэтому клиентская регистрация и вход должны оставаться выключенными через `Security__EnableClientAuth=false` и `VITE_ENABLE_CLIENT_AUTH=false`. Их можно включить после подключения домена и HTTPS. Административные учётные данные и строки подключения передаются только через server environment/GitHub Secrets.
