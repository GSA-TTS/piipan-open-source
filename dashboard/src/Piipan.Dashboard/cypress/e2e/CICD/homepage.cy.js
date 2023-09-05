let pa11yOptions = {};

describe('dashboard homepage', () => {
  beforeEach(() => {
    // Cypress starts out with a blank slate for each test
    // so we must tell it to visit our website with the `cy.visit()` command.
    // Since we want to visit the same URL at the start of all our tests,
    // we include it in our beforeEach function so that it runs before each test
      cy.visit('https://localhost:5001');
      cy.get('.usa-banner__header', { timeout: 10000 }).should('be.visible');
      pa11yOptions = {
          actions: [
              'wait for element #metrics-dashboard to be added'
          ],
          standard: 'WCAG2AA',
          runners: [
              'htmlcs'
          ],
      };
  })

  it('collapses usa banner on page load', () => {
    // assert
    cy.get('.usa-banner__header')
      .should('not.have.class', 'usa-banner__header--expanded');
  });

  it('opens usa banner on click', () => {
    cy.get('.usa-banner__button').contains('how you know').click();

    cy.get('.usa-banner__header')
      .should('have.class', 'usa-banner__header--expanded');

    cy.get('.usa-banner__content')
      .should('not.have.attr', 'hidden');
  });

  it('re-collapses usa banner on subsequent click', () => {
    cy.get('.usa-banner__button').contains('how you know').click();

    cy.get('.usa-banner__header')
      .should('have.class', 'usa-banner__header--expanded');

    cy.get('.usa-banner__content')
      .should('not.have.attr', 'hidden');

    cy.get('.usa-banner__button').contains("how you know").click();

    cy.get('.usa-banner__header')
      .should('not.have.class', 'usa-banner__header--expanded');

    cy.get('.usa-banner__content')
      .should('have.attr', 'hidden');
  });

  it('collapses user navigation on page load', () => {
    cy.get('.usa-accordion__button[aria-controls="nav-user"]')
      .should('have.attr', 'aria-expanded')
      .and('equal', 'false');
  });

  it('shows sign out button on click of user navigation', () => {
    cy.get('.usa-accordion__button[aria-controls="nav-user"]').click();

    cy.get('.usa-accordion__button[aria-controls="nav-user"]')
      .should('have.attr', 'aria-expanded')
      .and('equal', 'true');

    cy.contains('Sign out').should('be.visible');
  });

    it('passes accessibility check', () => {
        cy.pa11y(pa11yOptions);
    });
});
