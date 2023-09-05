#!/usr/bin/env python3

# Python code convert hex string
# to the Base64 string 
  
# Usage: hexToBase64Converter.py <hex string>

# Usage example: 
# $ ./hexToBase64Converter.py "F7A3B34DCA354262D1BD7B996CC1FFB15CF7C7A3AF2D199C33E807EA63B08797"

import codecs
import sys

hex = sys.argv[1]
b64 = codecs.encode(codecs.decode(hex, 'hex'), 'base64').decode()

# Print the resultant string
print (str(b64))
