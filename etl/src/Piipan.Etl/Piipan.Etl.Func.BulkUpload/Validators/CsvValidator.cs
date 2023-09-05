using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using Piipan.Etl.Func.BulkUpload.Models;

namespace Piipan.Etl.Func.BulkUpload.Validators
{
    /// <summary>
    /// Validates a record from a CSV file formatted in accordance with
    /// <c>/etl/docs/csv/import-schema.json</c>.
    /// </summary>
    public class CsvValidator : ICsvValidator
    {
        private const string FILE_VALIDATION_FAILURE_MSG = "Upload File Validation Failure";
        private readonly IValidator<ParticipantCsv> _validator;

        public CsvValidator(IValidator<ParticipantCsv> validator)
        {
            _validator = validator;
        }
        /// <summary>
        /// Validates CSV file and creates validation report.
        /// Protects to pass into validation report Personally Identifiable Information
        /// </summary>
        public CsvValidationReport ValidateCsvDoc(Stream input, string fileName, string eTag)
        {
            var report = new CsvValidationReport(fileName, eTag, true);
            var rowIndex = 1;

            try
            {
                var reader = new StreamReader(input);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim,
                    ExceptionMessagesContainRawData = false
                };

                var csv = new CsvReader(reader, config);

                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    rowIndex++;
                    var participant = csv.GetRecord<ParticipantCsv>();
                    var validationResult = _validator.Validate(participant);

                    if (validationResult.IsValid)
                        continue;

                    AddValidationErrorsToReport(report, rowIndex, validationResult);

                    if (report.Errors.Count >= 1000)
                    {
                        report.ErrorInfo = "The maximum number of errors has been reached. Please retry your upload after addressing these errors.";
                        break;
                    }
                }

                if (report.Errors.Any())
                    report.IsValid = false;

                //allow to read file again
                reader.BaseStream.Position = 0;
            }
            catch (CsvHelper.MissingFieldException e)
            {
                report.IsValid = false;

                string errorMessage; 
                using (var reader = new StringReader(e.Message))
                {
                    errorMessage = reader.ReadLine();  //only retrieve first line of error so as to avoid incorporating PII
                    errorMessage = errorMessage.Replace(" You can ignore missing fields by setting MissingFieldFound to null.", "");
                }

                var errorItem = new CsvError()
                {
                    ErrorMessage = errorMessage,
                    PropertyName = FILE_VALIDATION_FAILURE_MSG
                };

                report.Errors.Add(rowIndex, new List<CsvError>() { errorItem });
            }
            catch (Exception)
            {
                report.IsValid = false;

                var errorItem = new CsvError()
                {
                    //Intentionally not adding the Exception message to the report to avoid posting potential PII.
                    ErrorMessage = "Failed to parse & validate uploaded csv file",
                    PropertyName = FILE_VALIDATION_FAILURE_MSG
                };

                report.Errors.Add(rowIndex, new List<CsvError>() { errorItem });
            }

            return report;
        }

        private static void AddValidationErrorsToReport(CsvValidationReport report, int rowIndex, FluentValidation.Results.ValidationResult validationResult)
        {
            foreach (var error in validationResult.Errors)
            {
                var errorItem = new CsvError()
                {
                    ErrorMessage = error.ErrorMessage,
                    PropertyName = error.PropertyName
                };

                //prevent put PII info into error messages
                if (error.ErrorMessage.Contains(error.AttemptedValue.ToString()))
                {
                    errorItem.ErrorMessage = error.ErrorMessage.Replace(error.AttemptedValue.ToString(),
                        "REDACTED", System.StringComparison.InvariantCultureIgnoreCase);
                }

                if (report.Errors.ContainsKey(rowIndex))
                    report.Errors[rowIndex].Add(errorItem);

                else
                {
                    if (report.Errors.Count >= 1000)
                        break;

                    report.Errors.Add(rowIndex, new List<CsvError>() { errorItem });
                }
            }
        }
    }

    public interface ICsvValidator
    {
        CsvValidationReport ValidateCsvDoc(Stream input, string fileName, string eTag);
    }
}
