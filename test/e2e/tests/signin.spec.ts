import { test, expect } from '@playwright/test';

test('test', async ({ page }) => {
  await page.goto('/');
  await page.getByText('?').click();
  await page.getByText('Login').click();
  await page.getByLabel('Email address').fill(process.env.AUTH_USERNAME);
  await page.getByRole('button', { name: 'Continue', exact: true }).click();
  await page.getByLabel('Password').click();
  await page.getByLabel('Password').fill(process.env.AUTH_PASSWORD);
  await page.getByRole('button', { name: 'Continue', exact: true }).click();
  await page.getByText('Continue without passkeys', { exact: true }).click();
  await page.getByText('t', { exact: true }).click();
  await page.getByText('Logout').click();
});