import sys
from tableschema import Table

table = Table(sys.argv[1], schema='import-schema.json')

errors = []
def exc_handler(exc, row_number=None, row_data=None, error_data=None):
    errors.append((exc, row_number, row_data, error_data))

for row in table.iter(exc_handler=exc_handler):
    None

for e in errors:
    print(e)
