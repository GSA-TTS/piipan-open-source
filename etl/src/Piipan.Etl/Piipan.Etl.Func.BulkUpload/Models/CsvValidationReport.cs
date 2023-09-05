using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piipan.Etl.Func.BulkUpload.Models
{
    /// <summary>
    /// Represents a validation report for CSV file
    /// </summary>
    public class CsvValidationReport
    {
        public CsvValidationReport(string fileName, string eTag, bool isValid)
        {
            FileName = fileName;
            ETag = eTag;
            IsValid = isValid;
        }

        public string FileName { get; set; }
        public string ETag { get; set; }
        public bool IsValid { get; set; }
        public string ErrorInfo { get; set; }
        [JsonProperty("row_index_with_errors")]
        public Dictionary<int, List<CsvError>> Errors { get; set; } = new Dictionary<int, List<CsvError>>();
    }

    /// <summary>
    /// Represents a validation error message and property name for an individual row from a CSV file
    /// </summary>
    public class CsvError
    {
        [JsonProperty("property_name")]
        public string PropertyName { get; set; }
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }
    }
}
