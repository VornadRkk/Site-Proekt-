# REST API contracts

Все тела и ответы используют JSON camelCase. Ошибка валидации возвращает `400 ValidationProblemDetails`, неизвестный объект — `404`, отсутствующий или неверный JWT — `401`, недостаточная роль — `403`.

| Метод | Путь | Доступ | Назначение |
|---|---|---|---|
| POST | `/api/auth/register` | Публичный при включённой клиентской авторизации | `{displayName,email,password}` → `201 {accessToken,expiresAtUtc,displayName,role}`. Пароль не короче 12 символов; роль всегда назначает сервер (`Client`). |
| POST | `/api/auth/login` | Публичный | `{email,password}` → `200 {accessToken,expiresAtUtc,displayName,role}`; неверные данные всегда дают одинаковый `401`. |
| GET | `/api/services` | Публичный | Активные услуги по `displayOrder`: `{id,name,shortDescription,priceFrom,category}`. |
| POST/PUT/DELETE | `/api/services[/{id}]` | Admin | Создание, обновление и скрытие услуги. |
| GET | `/api/portfolio` | Публичный | Только опубликованные проекты. |
| POST/PUT/DELETE | `/api/portfolio[/{id}]` | Admin | Управление портфолио. |
| POST | `/api/applications` | Публичный или Client | Создать заявку → `201 {id,status,createdAtUtc}`; лимит 10 запросов в минуту на IP. Для Client email берётся из JWT. |
| GET | `/api/applications/mine` | Client | Только заявки, связанные с внутренним ID текущего аккаунта. |
| GET | `/api/applications` | Admin | Все заявки, новые первыми. |
| PATCH | `/api/applications/{id}/status` | Admin | `{status}` → `204`; статусы: `new`, `inProgress`, `completed`, `rejected`. |
| DELETE | `/api/applications/{id}` | Admin | Безвозвратно удалить заявку. |
| GET | `/health` | Публичный | Проверка процесса API. |
| GET | `/health/ready` | Публичный | Readiness API и соединения с MySQL. |
| GET | `/metrics` | Внутренняя сеть | Метрики Prometheus: запросы, статусы и длительность. Nginx production не публикует этот маршрут. |

Закрытые методы требуют `Authorization: Bearer <accessToken>`. JWT подписывается HS256; срок задаёт `Jwt__LifetimeHours`.

Пример заявки:

```json
{
  "clientName": "Анна",
  "phone": "+7 999 123-45-67",
  "email": "anna@example.com",
  "eventType": "festival",
  "eventDate": "2027-06-20",
  "comment": "Фестиваль на городской площадке"
}
```

Категории: `celebration`, `theatre`, `festival`, `birthday` (частный праздник), `wedding`, `corporate`, `other`.
