import { expect, test } from '@playwright/test'

test('клиент регистрируется, отправляет заявку и видит только свой кабинет', async ({ page }) => {
  const email = `client-${Date.now()}@example.test`

  await page.goto('/login')
  await page.getByRole('button', { name: 'Создать аккаунт' }).click()
  await page.getByLabel('Имя *').fill('Тестовый клиент')
  await page.getByLabel('Email *').fill(email)
  await page.getByLabel('Пароль *').fill('StrongTestPassword123!')
  await page.getByRole('button', { name: 'Зарегистрироваться' }).click()
  await expect(page).toHaveURL(/\/account$/)
  await expect(page.getByRole('heading', { name: /Здравствуйте, Тестовый клиент/ })).toBeVisible()

  await page.getByRole('link', { name: 'Оставить заявку' }).click()
  await expect(page.getByLabel('Email *')).toHaveValue(email)
  await page.getByLabel('Телефон *').fill('+7 999 111-22-33')
  await page.getByLabel('Предполагаемая дата *').fill('2099-12-31')
  await page.getByLabel('Что для вас важно?').fill('Фестивальная программа для городской площадки')
  await page.getByRole('button', { name: 'Отправить заявку' }).click()
  await expect(page.getByRole('heading', { name: 'Спасибо! Начало положено.' })).toBeVisible()

  await page.goto('/account')
  await expect(page.getByText('Фестивальная программа для городской площадки')).toBeVisible()

  const session = await page.evaluate(() => JSON.parse(sessionStorage.getItem('dateandtime.client.session') ?? '{}')) as { accessToken: string }
  const adminResponse = await page.request.get('/api/applications', { headers: { Authorization: `Bearer ${session.accessToken}` } })
  expect(adminResponse.status()).toBe(403)

  await page.goto('/admin')
  await expect(page).toHaveURL(/\/account$/)
})
