import http from 'k6/http';
import { check, sleep } from 'k6';
export const options={vus:1,duration:'30s',thresholds:{http_req_duration:['p(95)<1000'],http_req_failed:['rate<0.01']}};
export default function(){const base=__ENV.BASE_URL||'http://localhost:5080';check(http.get(base+'/api/services'),{'services OK':r=>r.status===200});const date=new Date(Date.now()+86400000*30).toISOString().slice(0,10);check(http.post(base+'/api/applications',JSON.stringify({clientName:'Load Test',phone:'+79991234567',email:'load@example.test',eventType:'birthday',eventDate:date,guestCount:10}),{headers:{'Content-Type':'application/json'}}),{'created':r=>r.status===201});sleep(7);}
