Background  
----------

The National Accuracy Clearinghouse (NAC) is a new congressionally mandated system that will allow states to check if a SNAP applicant or participant is receiving benefits in more than one state at the same time in order to prevent duplicate participation.  

The [2018 Farm Bill](https://www.congress.gov/bill/115th-congress/house-bill/2/text) in Section 4011 tasked USDA Food and Nutrition Services (FNS) with implementing the NAC. In fiscal year 2021, FNS entered into an interagency agreement (IAA) with General Services Administration (GSA) digital design agency 18F to build the NAC system and provide assisted acquisition services for the procurement of a software development vendor to continue building and maintaining the NAC.


2018 Farm Bill Requirements
---------------------------

The 2018 Farm Bill Section 4011 specifies that the NAC must adhere to the following requirements. 

-   The NAC should collect the minimum amount of information necessary to prevent duplicate SNAP participation. 

-   Information collected by the NAC can only be used to prevent duplicate SNAP participation. 

-   Information collected by the NAC can only be retained as long as the information is necessary for prevention of duplicate SNAP participation. 

-   The NAC must adhere to USDA security standards. 

-   The NAC will not store any sensitive PII for the ~40 million people applying for or receiving SNAP benefits.  

-   The initial match and corresponding actions must occur within three years of enacting the 2018 Farm Bill: by December 20, 2021. 

-   USDA's security requirements were enhanced in July 2021 to include direction not to store PII. Given the change, the deadline of December 2021 can no longer be met, which has been communicated to stakeholders including the Hill. The NAC is still being developed as efficiently as possible to minimize schedule delay while meeting the enhanced security requirements. 

-   State agencies must take appropriate action once the match is found.  

-   The NAC must protect the identity and location of vulnerable individuals, including victims of domestic violence. 

-   The NAC must incorporate best practices and lessons learned from the NAC pilot program, which was part of the 2014 Farm Bill.  

[Here is the full text of the 2018 Farm Bill.](https://usdagcc.sharepoint.com/:w:/r/sites/FNS-NationalAccuracyClearinghouse-Acquisition/Shared%20Documents/Acquisition/NAC%20wiki%20structure%20and%20content.docx?d=wf3d2f95a22d1435597e32871d7d43090&csf=1&web=1&e=Kg6SY8) See page 152-153: "SEC. 4011. INTERSTATE DATA MATCHING TO PREVENT MULTIPLE ISSUANCES." The regulations for the NAC are still in development.

Our approach to building the NAC  
----------------------------------

-   Based on our research with state workers, duplicate participation is most often the result of someone moving to a new state and applying for benefits there. This is more often accidental oversight rather than attempted fraud. 

-   We're taking a customer service orientation --- this is meant to help SNAP participants resolve duplicates, not punish them for not updating their state residence information. 

-   We want to minimize the impact of matches on SNAP participants --- minimize delay in benefit determination and burden when individuals relocate. 

-   We also want to minimize the potential burden of the NAC on state workers by not having a large number of false positives they need to resolve.  


How the NAC will work
---------------------

1.  State eligibility systems will share de-identified data on all active SNAP participants to the NAC system daily.

2.  During application, re-certification, and when adding a new household member, state eligibility workers will query the NAC system to find possible matches for a SNAP applicant or participant.

3.  When an exact match is found between states, both states will be notified.

4.  States will work together to resolve matches and reach an eligibility determination as efficiently as possible. 

5.  The NAC will also check for matches on a monthly basis as part of a bulk process to discover matches that may have otherwise been missed.

See NAC process diagrams below.







Planned handling of vulnerable individuals
----------------------------------------------

When a match is found, the relevant state agencies need to know if the matched person is considered a vulnerable individual and, if so, must ensure that the location of the vulnerable individual (for example, a person living in a domestic violence shelter) isn't shared in a way that could put that person at risk.

When states upload participant data to the NAC, they will need to flag vulnerable individuals. When a match is found, the NAC will inform states if the matching participant is a vulnerable individual and must have their location protected. State workers receiving the match notice must take extra precaution not to disclose the participant's location - for example, not sharing with the state where they now reside or any other location information with any member of the household in the matching state.


NAC Pilot (and learnings)
-------------------------

The need for the NAC became apparent after Hurricane Katrina in 2005 when Louisiana residents were relocated to neighboring states and there wasn't a quick, easy way for eligibility workers to check if a SNAP participant was already enrolled in another state. The NAC Pilot was then part of the 2014 Farm Bill.

In 2014, LexisNexis developed software for the NAC Pilot in partnership with five pilot states: Mississippi, Louisiana, Florida, Alabama, and Georgia.

Dual participation has a relatively low occurrence overall, ranging from less than 0.1% of Louisiana's eligible individuals in May 2014 to just below 0.2% of Georgia's population. However, in a program as large as SNAP---with total allotments exceeding $69 billion in FY2014---even small percentages of benefits issued in error translate into a significant expenditure of taxpayer dollars.

The five NAC Pilot states implemented the system in different ways, and have seen different levels of success.

Here are some learnings, as summarized from the NAC Pilot final report [[PDF link](https://risk.lexisnexis.com/-/media/files/government/report/b7de1d11976a4bdd82a039a8f272265busdareportonnac2016117614%20pdf.pdf)]: 

-   States should integrate and automate the NAC system with state eligibility systems as much as possible.

-   States should be consistent in terms of how they define a match.

-   The match algorithm of exact last name, exact SSN, exact DOB will capture most of the high-quality match candidates (est. 85-94%). 

-   Comprehensive training materials for state workers is essential for successful implementation of a new system like the NAC.

-   When states join the NAC, the initial matching process will identify many matches and states should dedicate staff to the effort of addressing an initial "big bang" number of matches.

The NAC pilot solution is incompatible with the current solution being developed due to additional security requirements, so NAC pilot states will eventually need to migrate to the new FNS NAC system.


PARIS report 
-------------

Add section [including more background on its challenges: run infrequently, matches are out of date]

Public Assistance Reporting Information System, or PARIS, is a data matching service run by HHS that many SNAP state agencies use to check if SNAP recipients are receiving duplicate benefits in two or more states. PARIS matches help identify improper payments, not just for SNAP recipients but for those receiving other public benefits as well, including Medicaid. PARIS matching is only conducted quarterly, which contributes to a significant delay in identifying duplicate participation and also contributes to a burden on SNAP recipients if they are required to reimburse overpayments back to the government. The NAC system, by comparison, is a real-time system to ensure matches are accurate and discovered earlier to incur less of a burden on state workers and SNAP recipients.
