# REST API contracts

Все тела и ответы — JSON camelCase. Валидация: `400 ValidationProblemDetails`; отсутствующий объект: `404`; нет/неверен JWT: `401`; недостаточная роль: `403`.

| Метод | Путь | Доступ | Ответ |
|---|---|---|---|
| POST | `/api/auth/login` | Публичный | `{email,password}` → `200 {accessToken,expiresAtUtc,displayName,role}`; ошибка учётных данных `401`. |
| GET | `/api/services` | Публичный | `200 Service[]`, только активные: `{id,name,shortDescription,priceFrom,category}`. |
| POST | `/api/services` | Admin | Создать услугу → `201 Service`. |
| PUT | `/api/services/{id}` | Admin | Заменить поля услуги → `204` или `404`. |
| DELETE | `/api/services/{id}` | Admin | Мягко деактивировать → `204` или `404`. |
| GET | `/api/portfolio` | Публичный | `200 PortfolioItem[]`, только опубликованные. |
| POST | `/api/portfolio` | Admin | Создать проект → `201 PortfolioItem`. |
| PUT | `/api/portfolio/{id}` | Admin | Обновить → `204` или `404`. |
| DELETE | `/api/portfolio/{id}` | Admin | Снять с публикации → `204` или `404`. |
| POST | `/api/applications` | Публичный | Создать заявку → `201 {id,status,createdAtUtc}`; лимит 10 запросов в минуту на IP. |
| GET | `/api/applications` | Admin | Список заявок, новые первыми. |
| PATCH | `/api/applications/{id}/status` | Admin | `{status}` → `204` или `404`. Допустимы `new`, `inProgress`, `completed`, `rejected`. |
| DELETE | `/api/applications/{id}` | Admin | Удалить заявку безвозвратно → `204` или `404`. |

Закрытые методы требуют `Authorization: Bearer <accessToken>`. Токен подписан HS256; срок действия задаётся `Jwt__LifetimeHours` (8 часов по умолчанию).

Создание услуги: `{"name":"Свадебная программа","shortDescription":"Сценарий и ведущие","priceFrom":120000,"category":"wedding","isActive":true,"displayOrder":1}`.

Заявка: `{"clientName":"Анна","phone":"+7 999 123-45-67","email":"anna@example.com","eventType":"wedding","eventDate":"2027-06-20","guestCount":50,"comment":"Семейный праздник"}`.

## Sprint 2: authentication and ownership

- POST /api/auth/register: { email, password (12–128 chars), displayName }; returns { accessToken, expiresAtUtc, displayName, role }. Public registration always assigns Client; duplicate email returns 409.
- POST /api/auth/login: { email, password }; same response. Invalid credentials: 401. Shared registration/login limit: 5 requests/minute/IP; rejection: 429.
- GET /api/auth/session: bearer JWT required; returns { displayName, role }; active account and current role verified against database.
- GET /api/applications/mine: bearer JWT required; only applications owned by JWT subject. Never matches ownership using submitted email.
- POST /api/applications: remains anonymous; valid bearer JWT links the application to its subject.
- Administrative endpoints require Admin; unauthenticated: 401, Client: 403.
- In Production, /api/auth over HTTP returns 503; HTTPS must terminate at a trusted loopback proxy.
