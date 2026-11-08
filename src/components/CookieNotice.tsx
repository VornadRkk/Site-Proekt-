import { useEffect, useRef, useState } from 'react'

const preferenceKey = 'dateandtime.cookie-notice.v1'
const acceptedCookie = 'dateandtime_cookie_notice=accepted_v1'

function isAccepted() {
  try {
    if (localStorage.getItem(preferenceKey) === 'accepted') return true
  } catch { /* Try the cookie when local storage is unavailable. */ }
  try {
    return document.cookie.split(';').some(cookie => cookie.trim() === acceptedCookie)
  } catch { return false }
}

export default function CookieNotice() {
  const [visible, setVisible] = useState(() => !isAccepted())
  const noticeRef = useRef<HTMLElement>(null)

  useEffect(() => {
    if (!visible || !noticeRef.current) return
    const updateSpace = () => document.documentElement.style.setProperty('--cookie-notice-space', `${(noticeRef.current?.offsetHeight ?? 0) + 32}px`)
    updateSpace()
    const observer = new ResizeObserver(updateSpace)
    observer.observe(noticeRef.current)
    return () => {
      observer.disconnect()
      document.documentElement.style.removeProperty('--cookie-notice-space')
    }
  }, [visible])

  const accept = () => {
    try { localStorage.setItem(preferenceKey, 'accepted') } catch { /* The notice can still be dismissed when storage is unavailable. */ }
    try { document.cookie = `${acceptedCookie}; Max-Age=31536000; Path=/; SameSite=Lax${window.location.protocol === 'https:' ? '; Secure' : ''}` } catch { /* Cookie storage may be disabled by the browser. */ }
    setVisible(false)
  }

  if (!visible) return null
  return <aside className="cookie-notice" ref={noticeRef} aria-labelledby="cookie-notice-title">
    <div><h2 id="cookie-notice-title">Cookies на сайте</h2><p>Мы используем cookies и локальное хранилище для работы сайта и сохранения настроек. Нажимая «Принять», вы соглашаетесь с их использованием. <a href="/privacy#cookies" target="_blank" rel="noopener noreferrer">Подробнее в политике конфиденциальности</a>.</p></div>
    <button className="btn btn-dark rounded-pill px-4" type="button" onClick={accept}>Принять</button>
  </aside>
}
