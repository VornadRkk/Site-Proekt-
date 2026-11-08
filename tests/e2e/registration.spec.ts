import { test, expect } from '@playwright/test'

test('registration validates password and consent, rejects duplicate email and permits later login', async ({ page }) => {
  const email = `registration-${Date.now()}@example.test`
  const password = 'Registration-long-password-123'
  let registrationRequests = 0
  page.on('request', request => {
    if (request.method() === 'POST' && new URL(request.url()).pathname === '/api/auth/register') registrationRequests++
  })
  const submitAndWait = async (endpoint: string, button: string) => {
    const [response] = await Promise.all([
      page.waitForResponse(response => new URL(response.url()).pathname === endpoint && response.request().method() === 'POST'),
      page.getByRole('button', { name: button, exact: true }).click(),
    ])
    return response.status()
  }
  await page.goto('/account')
  await page.getByRole('button', { name: 'Принять', exact: true }).click()
  await page.getByRole('button', { name: 'Создать аккаунт' }).click()
  await page.getByLabel('Имя', { exact: true }).fill('Проверка регистрации')
  await page.getByLabel('Email', { exact: true }).fill(email)
  const passwordField = page.getByLabel('Пароль', { exact: true })
  const consent = page.getByRole('checkbox')
  await passwordField.fill('short')
  await consent.check()
  await page.getByRole('button', { name: 'Зарегистрироваться', exact: true }).click()
  await expect(passwordField).toBeFocused()
  expect(registrationRequests).toBe(0)
  await passwordField.fill(password)
  await consent.uncheck()
  await page.getByRole('button', { name: 'Зарегистрироваться', exact: true }).click()
  await expect(consent).toBeFocused()
  expect(registrationRequests).toBe(0)
  await consent.check()
  expect(await submitAndWait('/api/auth/register', 'Зарегистрироваться')).toBe(200)
  await expect(page.getByRole('heading', { name: 'Мои заявки' })).toBeVisible()
  await page.getByRole('button', { name: 'Выйти', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Регистрация', exact: true })).toBeVisible()
  await expect(consent).not.toBeChecked()
  await page.getByLabel('Email', { exact: true }).fill(email.toUpperCase())
  await passwordField.fill(password)
  await consent.check()
  expect(await submitAndWait('/api/auth/register', 'Зарегистрироваться')).toBe(409)
  await expect(page.getByRole('alert')).toBeVisible()
  await page.getByRole('button', { name: 'Уже есть аккаунт', exact: true }).click()
  await passwordField.fill('Incorrect-long-password-123')
  await consent.check()
  expect(await submitAndWait('/api/auth/login', 'Войти')).toBe(401)
  await expect(page.getByRole('alert')).toBeVisible()
  await passwordField.fill(password)
  expect(await submitAndWait('/api/auth/login', 'Войти')).toBe(200)
  await expect(page.getByRole('heading', { name: 'Мои заявки' })).toBeVisible()
  await page.reload()
  await expect(page.getByRole('heading', { name: 'Мои заявки' })).toBeVisible()
})
