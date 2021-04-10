/// <reference types="Cypress" />

describe('homepage', () => {
  it('should fetch title from backend', () => {
    cy.visit(process.env.FRONTEND_URL || 'http://localhost:3000')
    cy.fixture('homepage').then(homepage => {
      cy.get('h1')
        .should('have.text', homepage.welcomeText)
    })
  });
});
