import { test, expect } from '@playwright/test'

test('cookie choice persists and privacy is available without accepting', async ({ page }) => {
  await page.goto('/request')
  const notice = page.getByRole('complementary', { name: 'Cookies на сайте' })
  await expect(notice).toBeVisible()
  const policyTab = page.waitForEvent('popup')
  await notice.getByRole('link').click()
  const policy = await policyTab
  await expect(policy.getByRole('heading', { level: 1, name: 'Политика конфиденциальности' })).toBeVisible()
  await policy.close()
  await notice.getByRole('button', { name: 'Принять', exact: true }).click()
  await expect(notice).toBeHidden()
  await page.reload()
  await expect(notice).toBeHidden()
})

test('request cannot be submitted without explicit consent', async ({ page }) => {
  let submissions = 0
  page.on('request', request => {
    if (request.method() === 'POST' && new URL(request.url()).pathname === '/api/applications') submissions++
  })
  await page.goto('/request')
  await page.getByRole('button', { name: 'Принять', exact: true }).click()
  await page.locator('#clientName').fill('Тест согласия')
  await page.locator('#phone').fill('+79991234567')
  await page.locator('#email').fill('privacy@example.test')
  await page.locator('#eventDate').fill(new Date(Date.now() + 86400000 * 30).toISOString().slice(0, 10))
  await page.locator('#guestCount').fill('10')
  const consent = page.getByRole('checkbox')
  await expect(consent).not.toBeChecked()
  await page.getByRole('button', { name: 'Отправить заявку', exact: true }).click()
  await expect(consent).toBeFocused()
  expect(submissions).toBe(0)
  await expect(page.getByRole('heading', { name: 'Спасибо! Начало положено.' })).toHaveCount(0)
  await consent.check()
  const policyTab = page.waitForEvent('popup')
  await page.locator('form').getByRole('link', { name: 'политикой конфиденциальности' }).click()
  const policy = await policyTab
  await expect(policy.getByRole('heading', { level: 1 })).toHaveText('Политика конфиденциальности')
  await policy.close()
  await expect(page.locator('#clientName')).toHaveValue('Тест согласия')
  await expect(consent).toBeChecked()
})

test('login, registration and admin login require fresh consent', async ({ page }) => {
  await page.goto('/account')
  await page.getByRole('button', { name: 'Принять', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Вход', exact: true })).toBeVisible()
  const consent = page.getByRole('checkbox')
  await expect(consent).toHaveAttribute('required', '')
  await consent.check()
  await page.getByRole('button', { name: 'Создать аккаунт' }).click()
  await expect(consent).not.toBeChecked()
  await expect(consent).toHaveAttribute('required', '')
  await page.goto('/admin')
  await expect(page.getByRole('heading', { name: 'Вход администратора' })).toBeVisible()
  await expect(page.getByRole('checkbox')).not.toBeChecked()
  await expect(page.getByRole('checkbox')).toHaveAttribute('required', '')
})
