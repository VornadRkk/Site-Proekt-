import { test, expect } from '@playwright/test'
test('registration, application, account and admin denial',async({page})=>{
 const email=`e2e-${Date.now()}@example.test`
 await page.goto('/account');await page.getByRole('button',{name:'Создать аккаунт'}).click()
 await page.getByLabel('Имя',{exact:true}).fill('Тестовый клиент');await page.getByLabel('Email',{exact:true}).fill(email);await page.getByLabel('Пароль',{exact:true}).fill('E2e-password-long-123')
 await page.getByRole('button',{name:'Зарегистрироваться',exact:true}).click();await expect(page.getByRole('heading',{name:'Мои заявки'})).toBeVisible()
 await page.getByRole('main').getByRole('link',{name:'Оставить заявку',exact:true}).click()
 await page.locator('#clientName').fill('Тестовый клиент');await page.locator('#phone').fill('+79991234567');await page.locator('#email').fill(email)
 await page.locator('#eventDate').fill(new Date(Date.now()+86400000*30).toISOString().slice(0,10));await page.locator('#guestCount').fill('10');await page.getByRole('button',{name:'Отправить заявку',exact:true}).click()
 await expect(page.getByRole('heading',{name:'Спасибо! Начало положено.'})).toBeVisible()
 await page.goto('/account');await expect(page.getByText(/— Новая/)).toBeVisible()
 await page.goto('/admin');await expect(page).toHaveURL(/account/);await expect(page.getByRole('heading',{name:'Мои заявки'})).toBeVisible()
})
