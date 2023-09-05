User research approach
----------------------

18F engages in user research as part of a human-centered design (HCD) approach. Broadly, HCD means choosing what and how to build a product based on constant real-world research about the people who will use the product, how they work, and whether the product meets their needs.

In the case of the NAC, our primary focus is on state agency users and program staff at FNS. Our research ensures that we understand not only the needs of those interacting with the system, but also the statutory requirements, regulations, and business needs for the NAC.

Taking this approach helps reduce risk by ensuring that we build the right thing, and that it is used in the way we expect. Regularly talking to users through a variety of research methods helps us:

-   Test assumptions and course-correct early

-   Keep from building features that sound interesting but don't meet real needs

-   Prioritize bugs or features based on how they impact users and business needs

-   Set the product up for long-term success by anticipating future needs

-   Unite the development team and stakeholders by providing consistent feedback about the value delivered

### Recruiting research participants

-   FNS + 18F assembled a NAC Steering Committee made up of representatives from each of the 8 USDA FNS regional offices as well as different parts of the SNAP national office who helped with initial outreach to states gauging interest in participating in discovery research and/or as first round state partners.

-   We now have direct lines to the 10 states in our Group 1 (see [State implementation](https://docs.google.com/document/d/1avFBWc2h3_ZRagMyePvZXmKczUayt9ym6yKuaSd4b8Y/edit#heading=h.oqf0e8k48cds)) and primarily recruit from these states.

-   We recruit for specific research activities via email to the points of contact in our Group 1 states, specifying the type of user we are looking to talk to and offering a few times.

-   We also send ad-hoc questions via email to the Group 1 states and states that participated in the original discovery research.

### Glossary

-   **Duplicate participation:** When a SNAP participant is receiving benefits in more than one State at the same time.

    - Use instead of: "dual participation" or "dual enrollment."

-   **SNAP participant:** A person receiving SNAP benefits.

    -   Use instead of: SNAP enrollee

    -   AKA: States often refer to them as "clients" or "customers"

-   **SNAP household:** All the people receiving benefits from the same SNAP case

-   **NAC query:** A request sent to the NAC for an individual to see if there is a match

-   **NAC match:** A response from the NAC indicating that the individual is already receiving benefits in one or more states 

-   **Initiating state:** With a real-time NAC query, the state conducting the query

-   **Matching state(s):** With a real-time NAC query, the state(s) in which the individual is found to already be receiving benefits

-   **Eligibility worker:** State worker processing applications, recertifications, household member additions, who would conduct a NAC query

    -   AKA: case worker, income maintenance worker, SNAP worker, etc.

-   **Inquiry worker:** State worker receiving and responding to inquiries about NAC matches from other states

    -   AKA: data match specialist, inquiry services worker, integrated claims unit worker, etc.

-   **Case ID:** Unique identifier for a SNAP case, which may have multiple household members

    -   AKA: Case number, Eligibility Unit number, Assistance Unit (AU) number, etc.

-   **Participant ID:** Unique identifier for an individual, often used across multiple benefits programs 

    -   AKA: Client ID, Unique Person Identifier (UPI), Member ID, State ID, etc.

### Resources

-   The [18F UX Guide](https://ux-guide.18f.gov/) describes the overarching approach and principles we have used.

-   The [18F Methods](https://methods.18f.gov/) provides an overview of the majority of the user research and design methods we use.

NAC user roles, pain points, and needs
--------------------------------------

### Primary user roles

-   **State eligibility workers** (AKA income maintenance workers, case workers, etc)

    -   Role: Work with SNAP participants to determine eligibility and issue benefits.

    -   Anticipated interaction with the NAC:

        -   Required to run NAC queries on SNAP applications, recertifications, and household member additions

        -   If the NAC indicates that there is a match, the worker will take appropriate actions to communicate with the participant and the other state(s) to resolve the potential duplicate participation 

        -   Will record match resolution information in the NAC website

-   **State inquiry workers** (AKA inquiry services worker, data match specialist, etc)

    -   Role: Scope of role varies between states, but generally responsible for following up matches returned by the PARIS report and responding to inquiries from other states regarding potential duplicate participation. 

    -   Anticipated interaction with the NAC:

        -   Will receive inquiries regarding NAC matches and will be required to take appropriate actions to communicate with the participant and the other state(s) to resolve the potential duplicate participation

        -   Will receive the monthly bulk match report and be required to take action to communicate with the participant and/or the other state(s) to resolve the existing duplicate participation

        -   Will record match resolution information in the NAC website

-   **State and Federal Quality Control (QC) workers**

    -   Role: Assess adherence to SNAP policy

    -   Anticipated interaction with the NAC: Will review a sample of cases to assess whether state workers have been meeting the requirements set forth by the NAC rule.

-   **FNS program administration team members**

    -   Role: Perform program administration functions 

    -   Anticipated interaction with the NAC: Will oversee and continue to develop the NAC system, review metrics, and report findings.

-   (Indirect / Passive) **SNAP applicants and participants** 

    -   Role: Members of the public applying for or receiving SNAP benefits

    -   Anticipated interaction with the NAC: Will have their deidentified data provided to the NAC and may receive notices of NAC matches from states upon which they are asked to act.

### Pain points

-   **State eligibility and inquiry workers**

    -   Existing processes for detecting duplicate participation are not satisfactory

        -   State workers don't always trust applicants' answer to the question: "Have you received benefits in another state?"

        -   State workers look for "clues" that an applicant may be coming from another state (e.g., out of state drivers license)

        -   The PARIS match report is run quarterly, so the information is often out of date and/or duplicate participation is not discovered in a timely manner

    -   Working with other states to resolve potential duplicate participation is unreliable

        -   Response time from other states is often long and always unpredictable

        -   The process of corresponding with other states varies greatly from state to state, with regard to method, turnaround time, etc. 

        -   It can be difficult to identify and get through to the right point of contact 

        -   Correspondence involves lots of back and forth to get all the information needed

        -   There's a lot of manual entry and copy/paste needed to communicate with other states

        -   Some states require the participant to contact them directly to initiate closure of their case; this can delay the process of getting them approved in the new state.

-   **QC workers**

    -   Documentation of existing data matches is inconsistent and often difficult to understand state to state (e.g., screenshots that don't provide all the information needed)

-   **FNS program administration team members**

    -   It is difficult to collect metrics on duplicate participation

    -   States are sending sensitive PII via email to resolve potential duplicate participation

-   **SNAP applicants and participants**  
*Note: We have not conducted primary research with SNAP applicants and participants. Below are the pain points surfaced in conversations with state workers.*

    -   When duplicate participation is not identified in a timely manner, participants can be confronted with the unexpected need to repay incorrectly issued benefits 

    -   Different states have different processes for applying for and managing SNAP benefits, which can make it difficult for SNAP participants to understand what is required of them. 

Overview of user research activities & findings to date
-------------------------------------------------------

-   **Discovery research with 14 states** (December 2020)

    -   Format: 1 hour [contextual inquiries](https://methods.18f.gov/discover/contextual-inquiry/) / [interviews](https://methods.18f.gov/discover/stakeholder-and-user-interviews/) with state workers

    -   Questions:

        -   What is the workflow that an eligibility worker goes through to process a SNAP application?

        -   How are existing required data matches conducted?

        -   How is duplicate enrollment discovered and handled currently?

    -   High-level learnings:

        -   Although workflows are similar across states, there's not a consistent way that states connect to existing required data match interfaces.

        -   All states currently rely on workers to screen via application and interview questions about prior receipt of out-of-state benefits.

        -   PARIS matches, which are generally processed by a specialized team, are considered unreliable and labor-intensive.

        -   The most commonly cited pain point was the often cumbersome and time-consuming process of communicating between states.

        -   States are enthusiastic about the NAC, but they primarily see the value if it's verified upon receipt, which would allow them to resolve matches using only the information provided by the NAC without verifying the details with the other state. As a federal system of records used for data matching, the NAC is subject to the provisions of the Privacy Act, which provides specific protections to individuals whose information has been used in a match.  Protections include the need for states to verify the information and notify the matched individual and provide them an opportunity to contest before any adverse action can be taken on the case as a result of the match. In addition, the NAC is not considered a primary data source because it aggregates data from other sources, specifically the states that provide information to populate it. Therefore, information received from the NAC cannot be considered verified upon receipt.

-   **Technical inquiry with 14 states** (January 2021)

    -   Format: Questions sent via email to technical points of contact

    -   Questions: 

        -   From a technical perspective, would you be able to provide records of all current active enrollees via a web service API the NAC would host?

        -   From a technical perspective, would your system be able to support real-time match queries (via API call)?

        -   What is the transport mechanism and frequency with which your system currently connects to existing data verification systems: (e.g., "real-time web service API call initiated by worker" / "SFTP monthly batch file" / other mechanism and frequency)?

    -   High-level learnings:

        -   Most states are able to provide records via API

        -   Most states are able to support real-time match queries via API

        -   Most prevalent transport mechanism varied by system

            -   eDRS: Real-time API call

            -   NDNH: Monthly batch process

            -   SSA PVS and DMF: Daily or monthly batch process

            -   SAVE: Separate system login

-   **Group 1 state partner feasibility research with 10 states** (March 2021)

    -   Format: Structured interviews and discussion

    -   Questions: 

        -   Is the state a good fit to be an early adopter of the NAC? 

            -   Are they able to commit to the timeline?

            -   Are they able to allocate sufficient support/resources? 

            -   Are they able to make required technical updates?

            -   Are they able to make necessary updates to operations? 

    -   High-level learnings:

        -   Most states met the majority of our criteria​; all had pros/cons

        -   No single regional cluster was the strongest​

        -   All states showed ability to participate and were invited to join one of three groups --- 1A, 1B, and 1C --- with the expectation that 1A states would be the first to implement the NAC and would receive the most support from the NAC project team

        -   States selected for Group 1A had strong confidence in meeting October deadline​, extra leadership buy-in and/or political pressure, NAC pilot interest or previous involvement​, and/or a strong tech team with agile development practices

-   **Deep dives with the 4 Group 1A states** (April 2021)

    -   Format: 1.5 hour interviews

    -   Questions: 

        -   Do we understand the current state for handling duplicate participation correctly?

        -   Which state agency workers will be NAC users?

        -   What are your questions and feedback on initial NAC workflows?

    -   High-level learnings:

        -   There are two primary front-line user groups for the NAC among state agency workers with whom features and functionality should be tested: eligibility workers and inquiry workers.

        -   There is an opportunity to reduce burden on the participant by creating an efficient and reliable interstate communication process.

        -   We should assume that workers may be dealing with multiple out-of-state inquiries about NAC matches simultaneously and will use both phone and email. There is an opportunity to establish common procedures and expectations for correspondence regarding NAC matches that may streamline the process.

        -   NAC matches for multiple household members and/or heads of household are likely to be more complicated. 

        -   States use different terminology to describe participant status; we need to define for states what "active participants"means so they know which participants to include in each daily upload.

-   **Quality Control (QC) discovery research** (August 2021)

    -   Format: 1 hour interview with members of the FNS QC team

    -   Questions: 

        -   What are the responsibilities of FNS Quality Control (QC)?

        -   How does QC perform oversight? 

        -   How does QC propose to monitor the NAC?

        -   What information would QC need, when would they need it, and where does most of their work take place today?

    -   High-level learnings:

        -   QC is responsible for assessing adherence to SNAP policy, but is not responsible for monitoring systems or correcting mistakes.

        -   The bulk of QC work is done at the state level, where workers sample cases on a monthly basis and review case documentation. 

        -   QC will establish requirements in response to the final rule, but we anticipate that:

            -   QC will look for evidence that NAC matches were run and what the results were in the case records.

            -   Making that evidence easy to gather for eligibility workers and uniform across states will help QC.

-   **Match resolution research with 7 states** (September 2021)

    -   Format: 2 hour participatory workshops with 6-15 state participants

    -   Questions: 

        -   What might it look like for states to resolve NAC matches? 

        -   Can states work efficiently without exchanging PII? 

        -   How much does this change the assumed and/or existing process?

        -   What pain points do state workers anticipate?

        -   What ideas do state workers have about how to make this easy?

    -   High-level learnings:

        -   State workers are used to using PII in correspondence with other states by email and phone; they will likely continue to do so

        -   State workers want the other state (or the NAC itself) to provide as much information about the case as possible in order to help them understand the situation and find a resolution

        -   Resolving a match for a minor may be meaningfully different than for an adult since it's generally about custody, not just residency. We will want to dig into this further.

        -   In an ideal world, the NAC would significantly simplify and streamline collaboration and communication between states

        -   NAC needs to facilitate multiple workers working on a case

        -   Email is generally the preferred method of communication today because it creates a paper trail and is easier to document for QC

        -   State workers suggested setting up dedicated teams to address concerns about increased workload, response time, and potential delays for clients

-   **Ad hoc research with states** (multiple rounds)

    -   Format: Emails as needed to state contacts

    -   Questions (and corresponding learnings) include: 

        -   Does your state have unique identifiers for participants and cases? How are they generated? Are they considered PII? Are they shared across programs? 

            -   States generally have randomly generated unique participant and case IDs, which are shared across programs

        -   What information (like PII, location, etc) is included on notices sent to participants as a result of a data match?

            -   States generally include name, address, and a unique case or participant identifier

        -   How does your State process differ for running federally required data matches at application versus at recertification?

            -   States reported no difference in process

        -   What programming languages are your teams using?

            -   Majority of states in Group 1 use Java; others use .NET or COBOL
