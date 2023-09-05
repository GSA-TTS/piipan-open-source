# Sample tests for daily participant uploads

Only active participants can be included in bulk uploads.  An active participant is an individual who is certified to receive SNAP benefits for the current monthly benefit cycle on the day when the bulk upload file is being generated. 

When filtering participant records for upload, you will need to consider these factors: 

- Has the individual been certified for benefits? 
- Has the household already received benefits for the individual in the current month’s benefit cycle? 
- Will the household receive benefits for the individual in the current month’s benefit cycle? 

## Examples 
The following examples can be used to generate test cases to help you ensure that your state is uploading the correct participant records. 

### Certifying new applications 

#### Certification example 1: 
A household applies for SNAP benefits on October 1.  The household is certified on October 12 to receive SNAP benefits, with the first issuance month of October. 

_Daily bulk upload contents:_
- October 1 - October 11: Members of this household are not included.  
- October 12+: All individuals in the household are included.

#### Certification example 2:
A household applies for SNAP benefits on October 5.  The household is certified on October 12 to receive SNAP benefits, with the first issuance month of November. 

_Daily bulk upload contents:_
- October 5 - October 31: Members of this household are not included. 
- November 1+: All individuals in the household are included. 


### Adding household members 

#### Household addition example 1: 
A single head of household has already been certified for SNAP benefits.  On October 1, the household applies to add another member.  The addition of the new household member is certified on October 3 and included in the benefit issuance for the month of October. 

_Daily bulk upload contents:_
- October 1 - October 2: Only the previously certified individuals are included. 
- October 3+:  The newly certified individual of the household is now included. 

#### Household addition example 2: 
A single head of household has already been certified for SNAP benefits.  On October 1, the head of household applies to add another household member.  The addition of the new household member is certified on October 5 but the first month of benefit issuance will be December. 

_Daily bulk upload contents:_

- October 1 - November 30: Only the previously certified individuals are included 
- December 1+: The newly certified individual of the household is now included


### Removing a household member 

#### Household member removal example: 
A member of a household moves out of the house.  The household submits documentation to remove a household member on October 15.  The household received benefits for this individual for the month of October. 

_Daily bulk upload contents:_
- October 1 - October 31: Each member of this household must be included, including the  individual whose case closed as benefits were received for this individual. 
- November 1+: The individual who moved is no longer included in daily uploads.  The rest of the certified household members are included. 


### Closing cases 

#### Case closure example 1: 
A household closed its case on October 15.  Prior to closing their case, the household received benefits for the month of October. 

_Daily bulk upload contents:_
- October 1 - October 31: Each certified member of this household must be included 
- November 1+: Each member of this household is excluded. 
 
#### Case closure example 2: 
A household closes its case on October 20, with the last issuance taking place in November. 

_Daily bulk upload contents:_
- October 1 - November 30: Each member of this household is included 
- December 1+: Each member of this household is excluded.
