let pa11yOptions = {};

describe('participant query CICD tests for Eligibility Worker', () => {
    beforeEach(() => {
        pa11yOptions = {
            actions: [
                'wait for element #query-form-search-btn to be added'
            ],
            standard: 'WCAG2AA',
            runners: [
                'htmlcs'
            ],
        };
        cy.visit('/');
        cy.injectAxe();
        cy.get('#query-form-search-btn', { timeout: 10000 }).should('be.visible');
    })

    it('shows required field errors when form is submitted with no data', () => {
        cy.get('form').submit();

        cy.get('#QueryFormData_LastName-message').contains('Last Name is required').should('be.visible');
        cy.get('#QueryFormData_DateOfBirth-message').contains('Date of Birth is required').should('be.visible');
        cy.get('#QueryFormData_SocialSecurityNum-message').contains('Social Security Number is required').should('be.visible');
        cy.get('#QueryFormData_ParticipantId-message').contains('Participant ID is required').should('be.visible');
        cy.get('#QueryFormData_SearchReason-message').contains('Search Reason is required').should('be.visible');

        // make sure pa11y runs successfully when errors are shown
        pa11yOptions.actions.push('navigate to https://localhost:5001/#query-form-search-btn');
        pa11yOptions.actions.push('click element #query-form-search-btn');
        cy.pa11y(pa11yOptions);

        cy.checkA11y();
    });

    it("shows formatting error for incorrect SSN", () => {
        cy.get('#QueryFormData_SocialSecurityNum').type("12345").blur();
        cy.get('#QueryFormData_SocialSecurityNum-message').contains('Social Security Number must have the form ###-##-####').should('be.visible');

        cy.get('form').submit();

        cy.get('.usa-alert').contains('Social Security Number must have the form ###-##-####').should('be.visible');
        cy.checkA11y();
    });

    it("shows proper error for too old dates of birth", () => {
        cy.get('#QueryFormData_DateOfBirth').type("1899-12-31").blur();
        cy.get('#QueryFormData_DateOfBirth-message').contains('Date of Birth must be between 01-01-1900 and today\'s date').should('be.visible');
        cy.get('form').submit();

        cy.get('.usa-alert').contains('Date of Birth must be between 01-01-1900 and today\'s date').should('be.visible');
        cy.checkA11y();
    });

    it("shows proper error for non-ascii characters in last name", () => {
        cy.get('#QueryFormData_LastName').type("garcía").blur();
        cy.get('#QueryFormData_LastName-message').contains('Change í in garcía').should('be.visible');
        cy.get('form').submit();

        cy.get('.usa-alert').contains('Change í in garcía').should('be.visible');
        cy.checkA11y();
    });

    it('can select the radio buttons', () => {
        cy.get('input[value="application"]').check({ force: true });
        cy.checkA11y();
    });

    it('can arrow to the other radio buttons', () => {
        cy.get('[id=QueryFormData_CaseId]')
            .click()
            .realPress("Tab")
            .realPress("ArrowDown");

        cy.get('input[value="new_household_member"]').should('be.checked');
        cy.checkA11y();
    });

    it('can press space to select radio buttons', () => {
        cy.get('[id=QueryFormData_CaseId]')
            .click()
            .realPress("Tab")
            .realPress("Space");

        cy.get('input[value="application"]').should('be.checked');
        cy.checkA11y();
    });

    it("shows an empty state on successful submission without match", () => {
        cy.intercept('POST', '/api/duplicateparticipantsearch', // that have a URL that matches '/users/*'
            {
                statusCode: 200,
                body: { "value": { "results": [{ "index": 0, "matches": [], "errors": [] }], "errors": [], "isUnauthorized": false } }
            },
        );

        cy.get('#QueryFormData_LastName').type("schmo");
        cy.get('#QueryFormData_DateOfBirth').type("1997-01-01");
        cy.get('#QueryFormData_SocialSecurityNum').type("550-01-6981");
        cy.get('#QueryFormData_ParticipantId').type('p123');
        cy.get('input[name="QueryFormData.SearchReason"]').first().click({ force: true });

        cy.get('form').submit();

        cy.contains('This participant does not have a matching record in any other states.').should('be.visible');
        cy.checkA11y();
    });

    it("shows results table on successful submission with a match", () => {
        cy.intercept('POST', '/api/duplicateparticipantsearch', // that have a URL that matches '/users/*'
            {
                statusCode: 200,
                body: { "value": { "results": [{ "index": 0, "matches": [{ "matchId": "YD97W48", "ldsHash": null, "state": "eb", "caseId": "caseid1", "participantId": "participantid1", "participantClosingDate": "2021-05-15T00:00:00", "recentBenefitIssuanceDates": [{ "start": "2021-04-01T00:00:00", "end": "2021-04-15T00:00:00" }, { "start": "2021-03-01T00:00:00", "end": "2021-03-30T00:00:00" }, { "start": "2021-02-01T00:00:00", "end": "2021-02-28T00:00:00" }], "vulnerableIndividual": true, "matchUrl": "https://cc-fd-querytool-cjc.azurefd.net/match/YD97W48", "matchCreation": "Already Existing Match" }] }], "errors": [] }, "errors": [], "isUnauthorized": false }
            },
        );

        cy.get('#QueryFormData_LastName').type('Farrington');
        cy.get('#QueryFormData_DateOfBirth').type('1931-10-13');
        cy.get('#QueryFormData_SocialSecurityNum').type('425-46-5417');
        cy.get('#QueryFormData_ParticipantId').type('p123');
        cy.get('input[name="QueryFormData.SearchReason"]').first().click({ force: true });
        cy.get('#query-form-search-btn').click();

        cy.contains('Match ID').should('be.visible');
        cy.contains('Matching State').should('be.visible');
        cy.checkA11y();
    });
    it("shows an empty state on successful submission without match (no mock)", () => {
        cy.get('#QueryFormData_LastName').type("schmo");
        cy.get('#QueryFormData_DateOfBirth').type("1997-01-01");
        cy.get('#QueryFormData_SocialSecurityNum').type("550-01-6981");
        cy.get('#QueryFormData_ParticipantId').type('p123');
        cy.get('input[name="QueryFormData.SearchReason"]').first().click({ force: true });

        cy.get('form').submit();

        cy.contains('This participant does not have a matching record in any other states.').should('be.visible');

        cy.pa11y(pa11yOptions);
    });

    it("shows results table on successful submission with a match (no mock)", () => {
        cy.get('#QueryFormData_LastName').type('Farrington');
        cy.get('#QueryFormData_DateOfBirth').type('1931-10-13');
        cy.get('#QueryFormData_SocialSecurityNum').type('425-46-5417');
        cy.get('#QueryFormData_ParticipantId').type('p123');
        cy.get('input[name="QueryFormData.SearchReason"]').first().click({ force: true });
        cy.get('#query-form-search-btn').click();

        cy.contains('Match ID').should('be.visible');
        cy.contains('Matching State').should('be.visible');
    });

})

describe('participant query CICD tests for Program Oversight', () => {
    beforeEach(() => {
        pa11yOptions = {
            actions: [
                'wait for element #query-form-search-btn to be added'
            ],
            standard: 'WCAG2AA',
            runners: [
                'htmlcs'
            ],
        };
        cy.visit('https://localhost:5002');
        cy.injectAxe();
        cy.get('#query-form-search-btn', { timeout: 10000 }).should('be.visible');
    })

    it('shows required field errors when form is submitted with no data', () => {
        cy.get('.usa-alert.usa-alert--slim.usa-alert--info ').contains('You do not have adequate permissions. For additional information, please contact the help desk.').should('be.visible');
        cy.get('#snap-participants-query-form-wrapper.disabled-area[inert]').should('be.visible');

        cy.checkA11y();
    });
})
