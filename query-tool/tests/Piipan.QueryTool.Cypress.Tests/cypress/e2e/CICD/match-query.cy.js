let pa11yOptions = {};

describe('match query CICD tests', () => {
    beforeEach(() => {
        pa11yOptions = {
            actions: [
                'wait for element #match-form-search-btn to be added'
            ],
            standard: 'WCAG2AA',
            runners: [
                'htmlcs'
            ],
        };
        cy.visit('/match');
        cy.injectAxe();
        cy.get('#match-form-search-btn', { timeout: 10000 }).should('be.visible');
    })
    it("shows an empty state on successful submission without match  (no mock)", () => {
        cy.get('#Query_MatchId').type("1234567").blur();

        cy.get('form').submit();

        cy.contains('This Match ID does not exist or you do not have adequate permissions.').should('be.visible');

        cy.pa11y(pa11yOptions);
    });

    it("shows results table on successful submission with a match (no mock)", () => {
        cy.visit('/');
        cy.get('#query-form-search-btn', { timeout: 10000 }).should('be.visible');
        cy.get('#QueryFormData_LastName').type('Farrington');
        cy.get('#QueryFormData_DateOfBirth').type('1931-10-13');
        cy.get('#QueryFormData_SocialSecurityNum').type('425-46-5417');
        cy.get('#QueryFormData_ParticipantId').type('p123');
        cy.get('input[name="QueryFormData.SearchReason"]').first().click({ force: true });

        cy.get('#query-form-search-btn').click();

        cy.get('#query-results-area tbody tr td a').invoke('text').then(matchId => {
            cy.visit('/match');
            cy.get('#match-form-search-btn', { timeout: 10000 }).should('be.visible');
            cy.get('#Query_MatchId').type(matchId).blur();
            cy.get('form').submit();

            cy.contains('Match ID').should('be.visible');
            cy.contains('Matching State').should('be.visible');
            cy.contains('Vulnerable Individual').should('be.visible');
        });
    });
    it('shows required field errors when form is submitted with no data', () => {
        cy.get('form').submit();

        cy.get('#Query_MatchId-message').contains('Match ID is required').should('be.visible');

        // make sure pa11y runs successfully when errors are shown
        pa11yOptions.actions.push('click element #match-form-search-btn');
        cy.pa11y(pa11yOptions);
        cy.checkA11y();
    });

    it("shows number of characters error for match ID", () => {
        cy.get('#Query_MatchId').type("12345").blur();
        cy.get('#Query_MatchId-message').contains('Match ID must be 7 characters').should('be.visible');

        cy.get('form').submit();

        cy.get('.usa-alert').contains('Match ID must be 7 characters').should('be.visible');
        cy.checkA11y();
    });

    it("server errors are shown and accessible", () => {
        cy.intercept('GET', '/api/match/*',
            {
                statusCode: 200,
                body: {
                    "value": null,
                    "errors": [
                        {
                            "Property": "Query_MatchId",
                            "Error": "Some error with @@@"
                        }
                    ],
                    "isUnauthorized": false
                }
            },
        );

        // set to a valid value, otherwise we can't submit the form. The mock will return a server error.
        cy.get('#Query_MatchId').type("1234567").blur();

        cy.get('form').submit();

        cy.get('.usa-alert .usa-alert__text li').should('include.text', 'Some error with Match ID');
        cy.checkA11y();
    });

    it("shows invalid characters error for match ID", () => {
        cy.get('#Query_MatchId').type("m12$345").blur();
        cy.get('#Query_MatchId-message').contains('Match ID contains invalid characters').should('be.visible');

        cy.get('form').submit();

        cy.get('.usa-alert').contains('Match ID contains invalid characters').should('be.visible');
        cy.checkA11y();
    });

    it("shows an empty state on successful submission without match", () => {
        cy.intercept('GET', '/api/match/*',
            {
                statusCode: 200,
                body: {
                    "value": null,
                    "errors": [],
                    "isUnauthorized": true
                }
            },
        );

        cy.get('#Query_MatchId').type("1234567").blur();

        cy.get('form').submit();

        cy.contains('This Match ID does not exist or you do not have adequate permissions.').should('be.visible');
        cy.checkA11y();
    });

    it("shows results table on successful submission with a match", () => {
        cy.intercept('GET', '/api/match/*',
            {
                statusCode: 200,
                body: {
                    "value":
                    {
                        "data":
                        {
                            "matchId": "M123456",
                            "states": ["EA", "EB"],
                            "dispositions": [
                                {
                                    "state": "EA",
                                    "vulnerableIndividual": false
                                },
                                {
                                    "state": "EB",
                                    "vulnerableIndividual": true
                                }
                            ]
                        }
                    }
,
                    "errors": [],
                    "isUnauthorized": false
                }
            },
        );

        cy.get('#Query_MatchId').type("M123456").blur();
        cy.get('form').submit();

        cy.contains('Match ID').should('be.visible');
        cy.contains('Matching State').should('be.visible');
        cy.contains('Vulnerable Individual').should('be.visible');

        cy.checkA11y();
    });
})