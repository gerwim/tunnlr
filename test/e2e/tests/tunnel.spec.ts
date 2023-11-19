import { test, expect } from '@playwright/test';

test('test', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('complementary').getByRole('link', { name: 'Tunnels' }).click();
  await page.getByRole('button', { name: 'Create tunnel' }).click();
  await page.getByPlaceholder('https://127.0.0.1:').click();
  await page.getByPlaceholder('https://127.0.0.1:').fill('https://httpbin.tunnlr.dev/status/200');
  await page.getByPlaceholder('https://127.0.0.1:').press('Tab');
  await page.getByRole('button', { name: 'Add' }).click();
  await page.getByRole('button', { name: 'Start' }).click();
  await expect(page.getByRole('button', { name: 'Stop' })).toHaveCount(1);
  await page.getByRole('button', { name: 'Stop' }).first().click();
  await page.getByRole('cell', { name: 'Start delete' }).getByLabel('delete').click();
  await page.getByRole('button', { name: 'Delete', exact: true }).click();
});