import http from 'k6/http'
import { check, sleep } from 'k6'

export const options = {
  scenarios: {
    public_api: { executor: 'constant-vus', vus: 5, duration: '30s' },
  },
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<750'],
  },
}

const baseUrl = __ENV.BASE_URL || 'http://localhost:8080'

export default function () {
  const services = http.get(`${baseUrl}/api/services`)
  check(services, { 'services returns 200': (response) => response.status === 200 })

  if (__VU === 1 && __ITER % 7 === 0) {
    const application = http.post(`${baseUrl}/api/applications`, JSON.stringify({
      clientName: 'k6 check',
      phone: '+79990000000',
      email: `k6-${__VU}-${__ITER}@example.test`,
      eventType: 'festival',
      eventDate: '2099-12-31',
      comment: 'Automated load check',
    }), { headers: { 'Content-Type': 'application/json' } })
    check(application, { 'application returns 201': (response) => response.status === 201 })
  }
  sleep(1)
}
