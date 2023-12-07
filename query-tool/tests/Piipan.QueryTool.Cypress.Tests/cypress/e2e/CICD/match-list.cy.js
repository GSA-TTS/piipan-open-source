// Important! User must be set to "Location-National" in mock-user.json for these tests to pass!

let pa11yOptions = {};

function setupTest()
{
    // Because the GET request happens right on page load, we must intercept it before navigating to the page. Thus we cannot use beforeEach
    cy.visit('/list');
    cy.title({ timeout: 10000 }).should('eq', 'Piipan Matches List');
    cy.injectAxe();
}

describe('match query CICD tests', () => {
    it("shows results table when authorized (no mock)", () => {
        cy.contains('.usa-table').should('not.exist');
        cy.contains('You do not have adequate permissions.').should('be.visible');

        cy.pa11y(pa11yOptions);
    });
    it("shows results table when authorized", () => {
        cy.intercept('GET', '/api/match',
            {
                statusCode: 200,
                body: dummySuccessfulListResponse
            },
        );
        setupTest();

        let resultTable = () => cy.get('.usa-table');
        let resultTableHeader = () => resultTable().get('thead');
        let resultTableBody = () => resultTable().get('tbody');

        // Verify column headers
        resultTableHeader().find('th').eq(0).contains('Match ID').should('be.visible');
        resultTableHeader().find('th').eq(1).contains('Matching States').should('be.visible');
        resultTableHeader().find('th').eq(2).contains('Created At').should('be.visible');
        resultTableHeader().find('th').eq(3).contains('Vulnerable Individual').should('be.visible');

        // Verify 3 results in the table
        resultTableBody().find('tr').should('have.length', 3);
        cy.checkA11y();
    });

    it("shows message when unauthorized", () => {
        cy.intercept('GET', '/api/match',
            {
                statusCode: 200,
                body: dummyUnauthorizedListResponse
            },
        );
        setupTest();

        cy.contains('.usa-table').should('not.exist');
        cy.contains('You do not have adequate permissions.').should('be.visible');
        cy.checkA11y();
    });
})

const dummyUnauthorizedListResponse =
{
    "value": null,
    "errors": [],
    "isUnauthorized": true
}

const dummySuccessfulListResponse =
{
    "value": {
        "data": [
            {
                "dispositions": [
                    {
                        "initialActionAt": null,
                        "initialActionTaken": null,
                        "invalidMatch": true,
                        "invalidMatchReason": "System Error",
                        "otherReasoningForInvalidMatch": null,
                        "finalDisposition": null,
                        "finalDispositionDate": null,
                        "vulnerableIndividual": null,
                        "state": "ea"
                    },
                    {
                        "initialActionAt": null,
                        "initialActionTaken": null,
                        "invalidMatch": true,
                        "invalidMatchReason": "Incorrect Client Information",
                        "otherReasoningForInvalidMatch": null,
                        "finalDisposition": null,
                        "finalDispositionDate": null,
                        "vulnerableIndividual": true,
                        "state": "eb"
                    }
                ],
                "initiator": "ea",
                "matchId": "KCN6CPC",
                "createdAt": "2022-11-15T16:32:56.418177Z",
                "participants": [
                    {
                        "caseId": "caseid1",
                        "participantClosingDate": "2021-05-15T00:00:00",
                        "participantId": "participantid1",
                        "recentBenefitIssuanceDates": [
                            {
                                "start": "2021-04-01T00:00:00",
                                "end": "2021-04-15T00:00:00"
                            },
                            {
                                "start": "2021-03-01T00:00:00",
                                "end": "2021-03-30T00:00:00"
                            },
                            {
                                "start": "2021-02-01T00:00:00",
                                "end": "2021-02-28T00:00:00"
                            }
                        ],
                        "state": "eb"
                    },
                    {
                        "caseId": null,
                        "participantClosingDate": null,
                        "participantId": "123",
                        "recentBenefitIssuanceDates": [],
                        "state": "ea"
                    }
                ],
                "states": [
                    "ea",
                    "eb"
                ],
                "status": "closed"
            },
            {
                "dispositions": [
                    {
                        "initialActionAt": null,
                        "initialActionTaken": null,
                        "invalidMatch": true,
                        "invalidMatchReason": "Other",
                        "otherReasoningForInvalidMatch": "123",
                        "finalDisposition": null,
                        "finalDispositionDate": null,
                        "vulnerableIndividual": null,
                        "state": "ea"
                    },
                    {
                        "initialActionAt": null,
                        "initialActionTaken": null,
                        "invalidMatch": true,
                        "invalidMatchReason": "Incorrect Client Information",
                        "otherReasoningForInvalidMatch": null,
                        "finalDisposition": null,
                        "finalDispositionDate": null,
                        "vulnerableIndividual": true,
                        "state": "eb"
                    }
                ],
                "initiator": "ea",
                "matchId": "8K4H87Y",
                "createdAt": "2022-11-15T17:46:12.747747Z",
                "participants": [
                    {
                        "caseId": "caseid1",
                        "participantClosingDate": "2021-05-15T00:00:00",
                        "participantId": "participantid1",
                        "recentBenefitIssuanceDates": [
                            {
                                "start": "2021-04-01T00:00:00",
                                "end": "2021-04-15T00:00:00"
                            },
                            {
                                "start": "2021-03-01T00:00:00",
                                "end": "2021-03-30T00:00:00"
                            },
                            {
                                "start": "2021-02-01T00:00:00",
                                "end": "2021-02-28T00:00:00"
                            }
                        ],
                        "state": "eb"
                    },
                    {
                        "caseId": null,
                        "participantClosingDate": null,
                        "participantId": "123",
                        "recentBenefitIssuanceDates": [],
                        "state": "ea"
                    }
                ],
                "states": [
                    "ea",
                    "eb"
                ],
                "status": "closed"
            },
            {
                "dispositions": [
                    {
                        "initialActionAt": null,
                        "initialActionTaken": "",
                        "invalidMatch": false,
                        "invalidMatchReason": null,
                        "otherReasoningForInvalidMatch": null,
                        "finalDisposition": null,
                        "finalDispositionDate": null,
                        "vulnerableIndividual": true,
                        "state": "ea"
                    },
                    {
                        "initialActionAt": null,
                        "initialActionTaken": null,
                        "invalidMatch": null,
                        "invalidMatchReason": null,
                        "otherReasoningForInvalidMatch": null,
                        "finalDisposition": null,
                        "finalDispositionDate": null,
                        "vulnerableIndividual": true,
                        "state": "eb"
                    }
                ],
                "initiator": "ea",
                "matchId": "RPVQ7CG",
                "createdAt": "2022-11-16T22:08:42.992052Z",
                "participants": [
                    {
                        "caseId": "caseid1",
                        "participantClosingDate": "2021-05-15T00:00:00",
                        "participantId": "participantid1",
                        "recentBenefitIssuanceDates": [
                            {
                                "start": "2021-04-01T00:00:00",
                                "end": "2021-04-15T00:00:00"
                            },
                            {
                                "start": "2021-03-01T00:00:00",
                                "end": "2021-03-30T00:00:00"
                            },
                            {
                                "start": "2021-02-01T00:00:00",
                                "end": "2021-02-28T00:00:00"
                            }
                        ],
                        "state": "eb"
                    },
                    {
                        "caseId": null,
                        "participantClosingDate": null,
                        "participantId": "p123",
                        "recentBenefitIssuanceDates": [],
                        "state": "ea"
                    }
                ],
                "states": [
                    "ea",
                    "eb"
                ],
                "status": "open"
            }
        ]
    },
    "errors": [],
    "isUnauthorized": false
}
