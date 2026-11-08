import { useId } from 'react'

export default function PrivacyConsent() {
  const id = useId()
  return <div className="privacy-consent">
    <input className="form-check-input" id={id} name="privacyConsent" type="checkbox" required />
    <label htmlFor={id}>Я согласен с <a href="/privacy" target="_blank" rel="noopener noreferrer">политикой конфиденциальности</a> и даю согласие на обработку моих персональных данных при использовании этой формы.</label>
  </div>
}
