describe('homepage', () => {
  beforeAll(async () => {
    await page.goto(process.env.FRONTEND_URL);
  });

  it('should fetch title from backend', async () => {
    await expect(page).toMatch('Rules of Wumbo');
  });
});
