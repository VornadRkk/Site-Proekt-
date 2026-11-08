import { readFile, mkdir, writeFile } from 'node:fs/promises'

const apiUrl = process.env.MONITORING_API_URL || 'http://localhost:5080'
const prometheusUrl = process.env.PROMETHEUS_URL || 'http://localhost:9090'
const grafanaUrl = process.env.GRAFANA_URL || 'http://localhost:3000'
const reportPath = 'test-results/monitoring.json'
const report = { checkedAt: new Date().toISOString(), checks: [] }

async function json(url, options) {
  const response = await fetch(url, { ...options, signal: AbortSignal.timeout(8000) })
  if (!response.ok) throw new Error(`HTTP ${response.status} at ${new URL(url).pathname}`)
  return response.json()
}
async function until(label, check) {
  const deadline = Date.now() + 150_000
  let lastError
  while (Date.now() < deadline) {
    try {
      const result = await check()
      report.checks.push({ name: label, passed: true, ...result })
      console.log(`OK: ${label}`)
      return result
    } catch (error) { lastError = error }
    await new Promise(resolve => setTimeout(resolve, 3000))
  }
  throw new Error(`${label}: ${lastError?.message || 'not ready'}`)
}
async function query(expression) {
  const body = await json(`${prometheusUrl}/api/v1/query?query=${encodeURIComponent(expression)}`)
  if (body.status !== 'success' || !body.data.result.length) throw new Error('Prometheus query returned no data')
  const values = body.data.result.map(series => Number(series.value[1]))
  if (!values.every(Number.isFinite)) throw new Error('Prometheus query returned a non-finite value')
  return values
}

try {
  let password = process.env.GRAFANA_PASSWORD
  if (!password) {
    const envFile = await readFile('.env', 'utf8')
    password = envFile.split(/\r?\n/).find(line => line.startsWith('GRAFANA_PASSWORD='))?.slice('GRAFANA_PASSWORD='.length)
  }
  if (!password) throw new Error('Set GRAFANA_PASSWORD before verifying monitoring')
  const auth = { headers: { Authorization: 'Basic ' + Buffer.from(`admin:${password}`).toString('base64') } }
  await until('Database-backed API health', async () => {
    const response = await fetch(`${apiUrl}/health`, { signal: AbortSignal.timeout(8000) })
    if (!response.ok || (await response.text()).trim() !== 'Healthy') throw new Error('API or database is unhealthy')
    return {}
  })
  await until('Prometheus scrapes API and MySQL', async () => {
    await json(`${apiUrl}/api/services`)
    for (const expr of ['up{job="api"}', 'up{job="mysql"}', 'mysql_up{job="mysql"}']) {
      if ((await query(expr)).some(value => value !== 1)) throw new Error('API or MySQL scrape is down')
    }
    return { apiUp: 1, mysqlUp: 1 }
  })
  await until('Grafana health', async () => {
    if ((await json(`${grafanaUrl}/api/health`)).database !== 'ok') throw new Error('Grafana database is unhealthy')
    return {}
  })
  await until('Grafana Prometheus datasource', async () => {
    const health = await json(`${grafanaUrl}/api/datasources/uid/prometheus/health`, auth)
    if (health.status !== 'OK') throw new Error('Grafana cannot query Prometheus')
    return {}
  })
  const { dashboard } = await json(`${grafanaUrl}/api/dashboards/uid/dateandtime`, auth)
  if (dashboard.panels.length < 8) throw new Error('Expected eight provisioned monitoring panels')
  for (const panel of dashboard.panels) {
    await until(`Dashboard: ${panel.title}`, async () => {
      await json(`${apiUrl}/api/services`)
      return { values: await query(panel.targets[0].expr) }
    })
  }
  report.passed = true
} catch (error) {
  report.passed = false
  report.error = error.message
  console.error(`Monitoring check failed: ${error.message}`)
  process.exitCode = 1
} finally {
  await mkdir('test-results', { recursive: true })
  await writeFile(reportPath, JSON.stringify(report, null, 2) + '\n')
  console.log(`Monitoring report: ${reportPath}`)
}
