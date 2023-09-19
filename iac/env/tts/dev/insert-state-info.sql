/***************** READ ME FISRT ********************

THIS FILE MUST BE MODIFIED ON THE ENVIRONMENT FOLDER
/piipan/iac/env/ PLEASE CHECK THE DEVELOPMENT EXAMPLE
ON /piipan/iac/env/tts/dev/insert-state-info.sql

DURING THE IAC PROCESS THIS FILE IS OVERWRITE WITH THE
COPY ON THE ENVIRONMENT FOLDER


***
For dummy emails use the following TLDs (https://datatracker.ietf.org/doc/html/rfc2606#section-2):
            .test
            .example
            .invalid
            .localhost

For dummy phone numbers use 555-0100 through 555-0199 which are reserved for fictional use. (https://en.wikipedia.org/wiki/555_(telephone_number)
*/


BEGIN;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '15' as id, 'Iowa' as state, 'IA' as state_abbreviation, 'IA-test@agency.example' as email, '1235550101' as phone, 'MWRO' AS region, 'IA-test-cc@agency.example' as email_cc) as temp
WHERE NOT EXISTS
(select id from state_info where id = '15') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '18' as id, 'Louisiana' as state, 'LA' as state_abbreviation, 'LA-test@agency.example' as email, '1235550102' as phone, 'SWRO' AS region, 'LA-test-cc@agency.example' as email_cc) as temp
WHERE NOT EXISTS
(select id from state_info where id = '18') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '21' as id, 'Massachusetts' as state, 'MA' as state_abbreviation, 'MA-test@agency.example' as email, '1235550103' as phone, 'NERO' AS region, 'MA-test-cc@agency.example' as email_cc) as temp
WHERE NOT EXISTS
(select id from state_info where id = '21') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '26' as id, 'Montana' as state, 'MT' as state_abbreviation, 'MT-test@agency.example' as email, '1235550104' as phone, 'MPRO' AS region, 'MT-test-cc@agency.example' as email_cc) as temp
WHERE NOT EXISTS
(select id from state_info where id = '26') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '101' as id, 'Echo Alpha' as state, 'EA' as state_abbreviation, 'IN34nX+2kMS9wDQ1POgDQN6nxS4FGDdcNL9bmkCpB9caMzkelRzhcNfATGGcmu9z' as email, 'mcOFeXkDkjeluyLmZ0KmKkM6Q3wW2cdaZ7XMgbBFTRE=' as phone, 'EA-MPRO' AS region, '+8jPdZIC4EVxV9WvYv21WcOlmOTSmE42vKT/0PY/5O8=' as email_cc) as temp
WHERE NOT EXISTS
(select id from state_info where id = '101') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '102' as id, 'Echo Bravo' as state, 'EB' as state_abbreviation, 'EB-test@agency.example' as email, '1235550106' as phone, 'EB-MPRO' AS region, 'EB-test-cc@agency.example' as email_cc) as temp
WHERE NOT EXISTS
(select id from state_info where id = '102') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '103' as id, 'Echo Charlie' as state, 'EC' as state_abbreviation, 'EC-test@agency.example' as email, '1235550107' as phone, 'EC-MPRO' AS region, 'EC-test-cc@agency.example' as email_cc) as temp
WHERE NOT EXISTS
(select id from state_info where id = '103') limit 1;

COMMIT;
