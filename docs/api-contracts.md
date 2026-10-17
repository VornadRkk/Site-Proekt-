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
