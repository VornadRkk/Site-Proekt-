# Проверка мониторинга

Локальный Compose поднимает Prometheus, Grafana и MySQL exporter вместе с API и MySQL. Grafana автоматически подключает источник Prometheus и дашборд Date and Time.

1. Заполните JWT_KEY, ADMIN_PASSWORD и GRAFANA_PASSWORD в локальном .env.
2. Выполните `docker compose up --build -d --wait`.
3. Запустите `npm run test:monitoring` (Node.js 22.12+ или 24).
4. Откройте http://localhost:3000/d/dateandtime (admin и пароль из .env). Prometheus — http://localhost:9090/targets.

Утилита проверяет health API с БД, оба scrape-target, mysql_up, состояние Grafana, работоспособность источника Prometheus и реальные значения запросов всех восьми панелей. Показатели rate/latency требуют минимум двух сборов; утилита ждёт появления данных. Отчёт записывается в test-results/monitoring.json; в CI входит в artifact verification-results.

Для другого тестового стенда можно задать MONITORING_API_URL, PROMETHEUS_URL, GRAFANA_URL и GRAFANA_PASSWORD. Для локального стенда пароль читается из .env и не выводится в отчёт.

Восемь панелей: доступность API/MySQL, запросы API в секунду, p95 задержки, доля 5xx, соединения MySQL, запросы MySQL в секунду, время работы MySQL. У показателей доступности 0 отмечается красным, 1 — зелёным.

Compose предназначен для локальной разработки и CI: опубликованные порты ограничены loopback, демонстрационные MySQL-настройки не подходят для production. Мониторинг на рабочий VPS обычным workflow deploy не устанавливается; production-подключение требует отдельных серверных настроек и учётных данных.
