# Спринт 2: реализация и проверка

Требования взяты из Sprint_2_Date_and_Time.docx. MySQL сохранён; PostgreSQL и Figma исключены согласно документу.

| Пункт | Что добавлено | Проверка / оставшаяся работа |
|---|---|---|
| US-06 | Регистрация Client, нормализация email, уникальный индекс, PBKDF2, пароль от 12 символов, общая ошибка входа, rate limit | API integration tests проходят |
| US-07 | /account, проверка сессии, GET /api/applications/mine, UserId у новых авторизованных заявок | Изоляция двух аккаунтов проверена API-тестом; анонимные заявки сохранены |
| US-08 | Проверка сессии перед показом админки, перенаправление Client, серверные Admin policies | Отказы 401/403 проверены API-тестами |
| QA-01 | Unit и HTTP integration tests через WebApplicationFactory | 19 тестов проходят; HTTP-тесты используют InMemory, не MySQL |
| QA-02 | tests/e2e/client.spec.ts | E2E на настоящем API/MySQL проверяют регистрацию, заявки, доступы, согласия, cookies и навигацию |
| QA-03 | tests/load/public-api.js: услуги и создание заявки | Пороги p95 < 1000 мс и ошибки < 1% выполнены в CI; отчёт сохраняется как load-results.txt |
| SRE-01 | Метрики HTTP, MySQL exporter, Prometheus, Grafana dashboard, health-check БД, JSON logs | Проверены оба scrape-target, mysql_up, подключение Grafana и данные всех 8 панелей; production-установка мониторинга отдельно не выполнена |
| DX-01 | Compose: frontend/API/MySQL/мониторинг; миграция и seed | Compose, миграции, seed и создание заявки проверены на настоящем MySQL в CI; production-деплой успешно выполняется |
| SEC-01 | PR workflow: CodeQL, npm/NuGet audit, Gitleaks, Trivy обоих образов, passive ZAP | CodeQL, audits, Gitleaks, Trivy и passive ZAP проходят в GitHub Actions; у ZAP остаются предупреждения |
| SEC-02 | nginx.https.conf: TLS, redirect, HSTS/CSP/headers; запрет auth по HTTP в production | Внешний блокер: домен/DNS/сертификат и доступ к серверу |

## Локальный запуск

1. Запустите Docker Desktop (Linux containers).
2. Скопируйте `.env.example` в `.env` и задайте JWT_KEY (минимум 32 байта), ADMIN_PASSWORD (от 12 символов), GRAFANA_PASSWORD. Для реального API установите VITE_USE_MOCKS=false.
3. Выполните `docker compose up --build -d --wait`.
4. Откройте http://localhost:8080; API/Swagger — http://localhost:5080/swagger; Prometheus — http://localhost:9090; Grafana — http://localhost:3000 (admin и GRAFANA_PASSWORD).

Compose предназначен для локальной разработки: MySQL-пароли в нём демонстрационные, API работает в Development. Не используйте этот файл как production-конфигурацию. Порты опубликованы только на loopback.

Миграции и seed запускаются при старте API. Первый Admin создаётся из Admin__Email (по умолчанию admin@dateandtime.local) и Admin__Password. Публичная регистрация роль не принимает. Старые анонимные заявки не присваиваются по совпадению email.

Без Docker: задайте ConnectionStrings__MySql, Jwt__Key, Admin__Password, ASPNETCORE_ENVIRONMENT=Development и запустите `dotnet run --project backend/DateAndTime.Api`; frontend: `npm ci`, `npm run dev`. Для работы кабинета отключите mocks.

## Проверки

```sh
npm ci
npm run lint
npm run build
dotnet test DateAndTime.sln -c Release
npx playwright install chromium
npm run test:e2e
k6 run tests/load/public-api.js
```

Последние две команды требуют работающего реального стенда. E2E_BASE_URL меняет адрес frontend; BASE_URL меняет адрес API для k6. Нагрузка создаёт тестовые заявки; используйте отдельную тестовую БД. Интервал k6 учитывает лимит 10 заявок в минуту с одного IP.

Workflow `.github/workflows/quality.yml` поднимает отдельный стенд и сохраняет отчёты. Для CodeQL/Gitleaks могут потребоваться разрешения репозитория или лицензия организации. Назначьте проверки обязательными в branch protection перед слиянием; существующий production-deploy запускается отдельно после push в main.

## Production HTTPS

Не публикуйте авторизацию до настройки TLS. API в Production возвращает 503 на /api/auth по HTTP; frontend скрывает формы входа на HTTP вне localhost. Development — только локальный стенд.

После получения домена:

1. Настройте DNS на сервер и выпустите сертификат Let's Encrypt.
2. Замените __SERVER_NAME__ в deploy/nginx.https.conf на домен; проверьте пути сертификата.
3. Установите этот конфиг вместо HTTP-конфига, выполните `nginx -t`, затем reload.
4. Проверьте `curl -I http://DOMAIN` (301), `curl -I https://DOMAIN` (сертификат и security headers).
5. Убедитесь, что API доступен только через доверенный nginx на loopback и работает в Production; задайте уникальные секреты через окружение.

Этот шаблон не установлен на удалённом сервере. SEC-02 и полный Definition of Done пока не закрыты.

## Проверки зависимостей

Обновлены Vite, React Router, MySqlConnector; закреплён исправленный minimatch для typescript-eslint. После обновления `npm audit` сообщает 0 уязвимостей. NuGet-аудит также не обнаруживает уязвимых пакетов. Для frontend после обновления нужен Node.js 22.12+ или 24 (Docker/CI используют 24).

## Подтверждённые проверки и оставшиеся ограничения

Проверки Docker/E2E/k6/DAST/мониторинга выполнены в GitHub Actions; локальный Docker для этого не требуется. Рабочий сайт обновлён через production-deploy. Реальные HTTP-проверки подтвердили health и публичные списки, а также 401 у закрытых API. HTTPS и production-регистрация остаются заблокированы до настройки сертификата. Grafana/Prometheus на VPS отдельно не устанавливались.

Статусы пяти задач со скриншота и доказательства: [task-status.md](task-status.md).
