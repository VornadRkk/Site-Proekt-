import { test, expect } from '@playwright/test'

test('account appears once on the local test site and original navigation remains', async ({ page }) => {
  await page.goto('/')
  const header = page.getByRole('banner')
  await expect(header.getByRole('link', { name: 'Личный кабинет', exact: true })).toHaveCount(1)
  await expect(header.getByRole('link', { name: 'Контакты', exact: true })).toBeVisible()
})

test('public HTTP site hides the account and redirects its unavailable page', async ({ page, baseURL }) => {
  if (!baseURL) throw new Error('Set the test baseURL')
  const origin = new URL(baseURL)
  await page.route('http://public-site.test/**', async route => {
    const url = new URL(route.request().url())
    const response = await route.fetch({ url: new URL(url.pathname + url.search, origin).href })
    await route.fulfill({ response })
  })
  await page.goto('http://public-site.test/account')
  await expect(page).toHaveURL('http://public-site.test/')
  await expect(page.getByRole('banner').getByRole('link', { name: 'Личный кабинет', exact: true })).toHaveCount(0)
  await expect(page.getByText('Вход станет доступен после подключения HTTPS.')).toHaveCount(0)
  await expect(page.getByRole('banner').getByRole('link', { name: 'Контакты', exact: true })).toBeVisible()
})
