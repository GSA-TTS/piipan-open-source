BEGIN;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '15' as id, 'Iowa' as state, 'IA' as state_abbreviation, '+i+CUgMFxO+JgaZpSk3/pbTC3Jv99a+XlZdBMInWtU1mbnvat2tFEaEPXcQCjVNQ' as email, 'mcOFeXkDkjeluyLmZ0KmKkM6Q3wW2cdaZ7XMgbBFTRE=' as phone, 'EC-MPRO' AS region, 'NLy5kNYq5RbFNY0S2eS2Zft7RS1dIFQaXNaGbzkY17M=' as email_cc) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '15') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '18' as id, 'Louisiana' as state, 'LA' as state_abbreviation, 'h/JFlPotYMa3QS/Nuk0f3+0Qw8ZlYe+/RY+3ts+0rjJ88Irf3Ob/rGUx4unN+kNM' as email, 'mcOFeXkDkjeluyLmZ0KmKkM6Q3wW2cdaZ7XMgbBFTRE=' as phone, 'EC-MPRO' AS region, 'umPiE3ltUwVV92R0Gd0J3REtK0fVHABRY5kM8gNNmXE=' as email_cc) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '18') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '21' as id, 'Massachusetts' as state, 'MA' as state_abbreviation, 'VIdzDjq9P7+cVgzDjebThMzTyt3KCnZBu5Sn28YA3s1Kt3+xBWhTrcTBikBUcXCt' as email, 'mcOFeXkDkjeluyLmZ0KmKkM6Q3wW2cdaZ7XMgbBFTRE=' as phone, 'EC-MPRO' AS region, 'hXlaJ5kavuAS/mfdY3Pb8LmuL+qLwD7NJiKz20NUdoQ=' as email_cc) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '21') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '26' as id, 'Montana' as state, 'MT' as state_abbreviation, 'N9vk5XiGhHKP1gknT0ZoZ43QJNeYnF5osdu+iNInbubj4LrD+2LZSUYTZoDerxag' as email, 'mcOFeXkDkjeluyLmZ0KmKkM6Q3wW2cdaZ7XMgbBFTRE=' as phone, 'EC-MPRO' AS region, '8RCfb5uDkt7WSquX4lMUY4iQAJAMMupl5uJEvW4JARE=' as email_cc) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '26') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '101' as id, 'Echo Alpha' as state, 'EA' as state_abbreviation, 'IN34nX+2kMS9wDQ1POgDQN6nxS4FGDdcNL9bmkCpB9caMzkelRzhcNfATGGcmu9z' as email, 'mcOFeXkDkjeluyLmZ0KmKkM6Q3wW2cdaZ7XMgbBFTRE=' as phone, 'EA-MPRO' AS region, '+8jPdZIC4EVxV9WvYv21WcOlmOTSmE42vKT/0PY/5O8=' as email_cc) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '101') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region, email_cc)
select * from (select '102' as id, 'Echo Bravo' as state, 'EB' as state_abbreviation, 'eq3FzX+HKTvwYDpgjITO6nBF+jDsC8Ujxm9g+6+UOZ5x8MtH4YPg2S1wAZ59c0ia' as email, 'mcOFeXkDkjeluyLmZ0KmKkM6Q3wW2cdaZ7XMgbBFTRE=' as phone, 'EB-MPRO' AS region, 'BKVL7Ec0fswGaiqBE5pfROIadlNoBnulM+0ccgQJ8Mc=' as email_cc) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '102') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region,email_cc)
select * from (select '103' as id, 'Echo Charlie' as state, 'EC' as state_abbreviation, 'NbQdvFRNCLNnTh6lUKziAesBxtaKGcXxqQkdU48b0+M4GvhyPKUpttwGAjOit9Yo' as email, 'mcOFeXkDkjeluyLmZ0KmKkM6Q3wW2cdaZ7XMgbBFTRE=' as phone, 'EC-MPRO' AS region, 'gzfnJBremmsSKzD7ohyEsnRPuD9OJmRloqrz9JNairo=' as email_cc) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '103') limit 1;

COMMIT;
