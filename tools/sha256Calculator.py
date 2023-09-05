#!/usr/bin/env python3

# Python code to calculate sha256 sum 

# Usage: sha256Calculator.py <hex string>

# Usage example: 
# $ ./sha256Calculator.py "F7A3B34DCA354262D1BD7B996CC1FFB15CF7C7A3AF2D199C33E807EA63B08797"
  
import codecs
import sys
import hashlib

hex = sys.argv[1]

value=codecs.decode(hex, 'hex')
shaResult=hashlib.sha256(value)

# Print the resultant string
print (shaResult.hexdigest())
