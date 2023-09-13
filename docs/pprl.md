# A Privacy-Preserving Record Linkage (PPRL) approach

Piipan incorporates a secure hash encoding technique to de-identify Personally Identifiable Information (PII) using a centralized [Privacy-Preserving Record Linkage (PPRL)](https://link.springer.com/referenceworkentry/10.1007%2F978-3-319-63962-8_17-1) model.

A detailed technical description is below; a [high-level treatment](./pprl-plain.md) is also available.

## Overview

Our PPRL technique in brief:
- Tenants compute a secure hash for de-identified data using PII as input
- The secure hash is submitted in the tenants's daily bulk upload instead of the source PII
- Piipan searches for exact matches across the secure hashes of the participating tenants
- When tenant eligibility systems query the duplicate participation API during (re)certification, the secure hash for the de-identified data is provided as input, instead of their PII

Only one secure hash is currently defined for piipan and is derived from *last name*, *Date of Birth (DoB)*, and *Social Security Number (SSN)* and the application of a [cryptographic hash function](https://en.wikipedia.org/wiki/SHA-2). Additional secure hashes may be defined in the future to support additional matching algorithms beyond an exact match against last name, DoB, and SSN.

Finally, in order for exact matching against de-identified PII to be effective, tenants must consistently validate and normalize the PII values, concatenate them in a specified manner, and then apply the hash function. These steps are described in detail below.

## 1. Validation and Normalization

### Last name

The Last name must be normalized and validated in accordance with these **ordered** rules. These transformations assume ASCII-encoded input.

1. Convert to lower case
1. Replace hyphens with a space
1. Replace multiple spaces with one space
1. Trim any spaces at the start and end of the last name
1. Remove suffixes (only removes a single suffix at the end of the name):
    1. Roman numerals i-ix
    1. Variations of junior: junior, jr, jr., jnr
    1. Variations of senior: senior, sr, sr., snr
1. Remove any character that is not an ASCII space (`0x20`) or in the range `[a-z]` (`0x61`-`0x70`)
1. Validate that the resulting value is at least one ASCII character in length

If your source data set includes non-ASCII characters using the ISO-8859-1 (Latin-1) or Unicode encoding formats, [an ASCII normalization process to remove diacritics and derive base characters](https://ahinea.com/en/tech/accented-translate.html) must be applied before these rules.

Reference: [Social Security Program Operations Manual System](https://secure.ssa.gov/poms.nsf/lnx/0110205125)

Examples of *correct* output:

| Input       | Correct         |
|-------------|-----------------|
| Hopper      | `hopper`        |
| von Neuman  | `von neumann`   |
| O'Sullivan  | `osullivan`     |
| Jones-Drew  | `jones drew`    |
| Nguyễn      | `nguyen`        |

Examples of *incorrect* output:

| Input                         | Incorrect                       | Issue                                 |
|-------------------------------|---------------------------------|---------------------------------------|
| García                        | `garcía`                        | includes non-ASCII character          |
| Jones III                     | `jones iii`                     | includes suffix                       |
| Thatcher                      | `Thatcher`                      | not lower-cased                       |
| Barrable-Tishauer             | `barrable-tishauer`             | hyphen not replaced with space        |
| Heathcote-Drummond-Willoughby | `heathcote drummond-willoughby` | only first hyphen replaced with space |
| O'Grady                       | `o'grady`                       | apostrophe not removed                |


### Date of Birth (DoB)

The Date of Birth must be formatted in accordance with [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601#Dates). The ISO 8601 format uses a 4-digit year, a zero-padded month, and a zero-padded day. The 3 values are separated by a hyphen: `YYYY-MM-DD`.

Before normalizing, dates must be validated against the Gregorian calendar and be within the past 130 years.

Examples of *correct* output:

| Input              | Correct         |
|--------------------|-----------------|
| August 14, 1978    | `1978-08-14`    |
| February 29, 2004  | `2004-02-29`    |
| December 3, 1999   | `1999-12-03`    |

Examples of *incorrect* output:

| Input              | Incorrect         | Issue                                                                  |
|--------------------|-------------------|------------------------------------------------------------------------|
| August 14, 1998    | `98-08-14`        | year is not fully specified                                            |
| May 15, 2002       | `5/15/2002`       | wrong value order, wrong separator character, value is not zero-padded |
| November 2, 2000   | `2000-11-2`       | day is not zero-padded                                                 |
| February 29, 2001  | `2001-02-29`      | date does not exist on Gregorian calendar, 2001 is not a leap year     |

### Social Security Number (SSN)

The nine-digit Social Security Number is to be formatted in 3 parts: the 3-digit Area Number, the 2-digit Group Number, and the 4-digit Serial Number. The 3 parts must be separated by a hyphen: `AAA-GG-SSSS`.

Before normalizing, SSNs must be validated against the following Social Security Administration (SSA) rules:
- Area numbers `000`, `666`, and `900-999` [are invalid](https://www.ssa.gov/employer/randomization.html)
- Group number `00` [is invalid](https://www.ssa.gov/employer/randomizationfaqs.html)
- Serial number `0000` [is invalid](https://www.ssa.gov/employer/randomizationfaqs.html)

Reference: [Social Security Number Randomization](https://www.ssa.gov/employer/randomization.html)

Examples of *correct* output:

| Input     | Correct         |
|-----------|-----------------|
| 078051121 | `078-05-1121`   |
| 219099998 | `219-09-9998`   |
| 987654219 | `987-65-4219`   |

Examples of *incorrect* output:

| Input     | Incorrect        | Issue                                |
|-----------|------------------|--------------------------------------|
| 000345678 | `000345678`      | invalid area number, missing hyphens |
| 066481234 | `0664-81-234 `   | hyphen misplaced                     |
| 567890000 | `567-89-0000`    | invalid serial number                |

## 2. Concatenation

The normalized values of `Last name`, `DoB`, and `SSN` must be concatenated, using a comma as a field separator, before applying the hashing function in the next step.

For example:
- `hopper,1978-08-14,078-05-1121`
- `von neumann,2004-02-29,987-65-4219`

## 3. Hash generation

1. Take the validated, normalized, and concatenated values and apply the hash algorithm, resulting in a 64-byte byte array
1. Convert the byte array to a 128-character lower-cased hexadecimal digest to get a value suitable for the bulk upload and duplicate participation APIs


Executing all examples using an input of:
```
hopper,1978-08-14,078-05-1121
``` 
results in a hash digest of:
```
04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef
```

