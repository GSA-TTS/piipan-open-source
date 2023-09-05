const {parse} = require('csv-parse/sync');

const fs = require('fs')
const [,,inputFilename,outputFilename] = process.argv
const file = fs.readFileSync(inputFilename)
const parsed = parse(file, {
  columns: true,
  skip_empty_lines: true
})
const chosenRecord = parsed[Math.floor(Math.random() * parsed.length)]
const rowLength = parsed.length;
chosenRecord["Total_Row_Count"]=rowLength;
fs.writeFileSync(outputFilename, JSON.stringify([chosenRecord]))
