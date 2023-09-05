# Status
The piipan is currently in pre-release status. Much of the core functionality is demonstrably working. The ATO process is underway but not complete. Four tenants have begun work on integration.

Piipan has a working continuous integration pipeline in TTS-managed environments that includes automated unit and accessibility tests.  Work is underway to create a similar CI pipeline in FNS-managed environments.

# Product Roadmap
The roadmap represents our latest thinking about:
- The order in which the piipan's features will be developed
- Which features are needed prior to initial use by tenant agencies
- Problems, opportunities, and refinements that are being deferred as enhancements after the piipan's initial rollout

Plans and features outlined in this roadmap are subject to change.

# Launch strategy and sequence 
Prior to being used to prevent duplicate participation, there will be two piipan deployments that will be used to reduce launch risks.  These will be followed by a minimum viable product (MVP) deployment.  The MVP is the first release that will be used in production by tenants to take action on potential cases of duplicate participation. 

The three deployments leading to production piipan usage are: 
- ATO (First production deployment)
- Pre-MVP Test Launch 
- MVP 

The goals and features of each phase are described in more detail below. 

## ATO
**Goal: Deliver working, secure software using FNS toolchains to the production environment that does not centrally collect PII.**

_Target date: January 2022_

**How this will be used in production**
- Tenant agencies will be able to verify systems access and credentials to integrate with the production piipan.  Tenants will only use the production piipan with test data that verifies successful systems access. 
- Tenant agency users who will need access to the piipan website will submit access requests, as needed, and verify their ability to log in to the site  
- FNS users who will have access to administrative features will have their accounts set up and verify the ability to reach the metrics dashboard 

**Risks mitigated**
- Discover and address any deficiencies in the piipan’s implementation of required security controls 
- Ensure there are no unknown challenges to delivering software to the FNS environment 
- Discover and address systems access needs with ample time to resolve access limitations 

**External dependencies** 
- Tenant agencies will need to take these actions: 
  - Test their production API keys 
  - Identify which users will need access to the piipan website 
  - Submit access requests for those users 
  - Verify that users have the access that has been granted 

**Acceptance criteria**
- ATO has been achieved 
- The production environment has been deployed through a FNS-hosted CI/CD toolchain 
- Uploads and query-initiated matches work for curated test data in production, using updated deidentification designs
- Each tenant agency in Group 1A has verified their ability to interact with the production API 
- Primary users at the tenant agencies and FNS have been identified, access has been granted, and each user has verified their ability to access the piipan website

A detailed list of the issues involved in achieving this goal can be found in the [1st release: Initial Production Deployment milestone](https://github.com/18F/piipan/milestone/21) 

## Pre-MVP Test Launch
**Goal: FNS can confirm that uploads and matching will work by allowing tenants to send production data (without triggering match actions).  FNS begins monitoring key performance indicators.  FNS can test usability of the process for match determinations to be made.**

_Target date: TBD_

**How this will be used in production:**
- Tenant agencies will fully integrate the following activities into their benefits processing systems: 
  - Daily uploads of deidentified records of all active participants 
  - Automated piipan queries to accompany each case action (applications, recertifications, additions of a household member).   
    - All piipan searches will respond with “no match found” 
    - Tenants may disregard search responses at this phase.  Tenants do not need to have user interface updates to their benefits system in place to show the results of piipan searches yet. 
- FNS will use the metrics website and external tools to monitor system health and performance 

In this stage, tenants will not be provided with any information on matches discovered or be asked to take action to evaluate the validity of any matches.

In addition to the production environment usage, the test environment will be used for tenant agency users to perform usability tests for match resolution and disposition tracking workflows.

**Risks mitigated**
- Identify any problems with system performance under full volumes of traffic 
- Prove that tenant API integrations are working.  Surface integration challenges and unforeseen API needs with some time to adapt to any newly discovered scope 
- Learn about usability challenges by providing a prototype that can be used to test system interactions with users

**External dependencies**
- Verification of system performance depends on tenants completing API-based integrations 

**Acceptance criteria**
- The piipan supports the volume of bulk uploads and queries tenants will perform
  - Tenants can test their normalization and validation
  - Tenants can monitor the progress of their uploads
- At least 2 tenants are uploading data to the piipan daily and sending queries to the piipan with each relevant case action
- FNS can monitor and confirm key performance indicators
  - Tenants are uploading data each day
  - Tenants are performing as many searches as they should, given the volume of applications, recertifications, and additions of household members 
  - Some queries result in matches
  - Uptime and typical latency for APIs and websites is acceptable
- Tenants have the ability to report determinations on matches, resolving them when complete
  - Tenants can look up the record for a match that was previously found
  - Tenants can report matches as invalid
  - Each tenant in a 2-tenant match can report the determination
  - Matches are closed when both tenants take action, or when either tenant reports the match as invalid

A full list of issues flagged for the Pre-MVP test launch is available in the [2nd release: Pre-MVP test launch milestone](https://github.com/18F/piipan/milestone/23) 


## MVP Launch: Piipan in use
**Goal: The piipan is fully operational.  Tenants are able to use the piipan to detect potential duplicate participation and take appropriate actions.  The piipan supports all requirements of the (not-yet-published) rule governing the piipan.**

_Target date: TBD_

**How this will be used in production**
- Tenants will continue performing daily uploads of deidentified participant records and searching the piipan for every required case action 
- The piipan will provide match results to tenants 
- Tenants will take action on every match per regulatory guidance from FNS 
- FNS will use the metrics website and external tools to monitor system health, performance, and tenant compliance with regulations 

**Risks mitigated**
- Validate successful operation of a minimal feature set before adding enhancements and improvements that might be based on untested assumptions 

**External dependencies**
- The final piipan rule needs to be published 
- Tenant agencies will update their benefits systems to show the results of piipan searches 

**Acceptance criteria**
- At least 3 tenants are using the piipan for every required case action
- The piipan sends email notifications to each tenant involved in a match when a match is found
- FNS can use dispositions and invalid matches reported by tenants to measure accuracy
- The piipan implements all protections for vulnerable individuals required by the piipan rule
- Tenants are provided with onboarding materials and training to make the launch as smooth as possible 
- When tenants are looking at a match, they are provided with the contact information for their counterparts at the other tenant

A full list of issues flagged for the MVP launch is available in the [3rd release: MVP milestone](https://github.com/18F/piipan/milestone/18) 
