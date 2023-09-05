# Bulk import format for participant records

CSV files are used as a data exchange format for the bulk import of program participant records. A description of the required CSV fields can be found in [import-schema.json](csv/import-schema.json), using the [Table Schema specification](https://specs.frictionlessdata.io/table-schema).

UTF-8 encoding is required for all fields.

## Participant records to exclude

Exclude records from the CSV files that you upload to Piipan for individuals who:

- Are missing any of these required data:
  - Last name
  - Social Security number
  - Date of birth
- Do not meet the definition of `active participants`

### Definition of active participants
> An active participant is an individual who is certified to receive SNAP benefits for the current monthly benefit cycle on the day when the bulk upload file is being generated.

Note that the following groups of individuals are not considered active participants and as a result should be **excluded** from daily uploads:
- Applicants who have not yet been certified for benefits.
- Participants who are no longer receiving benefits.  These records should only be included through the last day of the participant's final benefit month and excluded from subsequent uploads.

The [example test scenarios](./example-test-scenarios.md) file contains several examples of correct behavior after relevant case actions that you can use to develop tests for the contents of your daily uploads.

## Cleaning participant records

The `lds_hash` csv column represents the hashed value of the various identity data necessary for matching participants. Refer to [our Privacy-Preserving Record Linkage approach](../../docs/pprl.md) for how to validate, normalize, concatenate, and hash this data. [Csv-level validation](#validating-files) will only validate that the column data is a hexadecimal digest as specified in the PPRL documententation.

## Validating files

There are Table Schema libraries available for [multiple programming languages](https://github.com/orgs/frictionlessdata/repositories?language=&q=tableschema&sort=&type=). The following example will use macOS, homebrew, Python, and [tableschema-py](https://github.com/frictionlessdata/tableschema-py):

```
brew install python
pip3 install tableschema
cd piipan/etl/docs/csv
python3 validate.py example.csv
```
Any errors present in the supplied CSV file will be printed to `stdout`.

## Notes
- Microsoft Excel is notorious for mangling CSV files. By default, it will reformat date fields, remove leading zeros in integer-like strings, etc. Round-tripping CSV files using Excel is only possible through careful import and export:
  1. Open a new workbook
  1. Use File → Import, not File → Open 
  1. When specifying column format, highlight all the columns in the preview and select Text
  1. Use File → Save As and specify the `CSV UTF-8 (Comma delimited) (.csv)` file format
- Microsoft Excel uses [`CR/LF` as a line ending (not just `LF`)](https://en.wikipedia.org/wiki/Newline#Representation), when exporting sheets as CSV. `CR/LF` is also specified in [RFC 4180](https://tools.ietf.org/html/rfc4180). This can confuse Unix-oriented text processing tools.
- Microsoft Excel will put a UTF-8 [Byte Order Mark](https://en.wikipedia.org/wiki/Byte_order_mark) (BOM) at the very start of the CSV file. UTF-8 has no byte ordering, but Excel uses this to automatically detect the encoding used in a CSV file. BOMs will frequently confuse Unix-oriented text processing tools.  
