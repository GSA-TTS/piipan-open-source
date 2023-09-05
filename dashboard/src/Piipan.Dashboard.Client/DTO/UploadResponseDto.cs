using Piipan.Metrics.Api;

namespace Piipan.Dashboard.Client.DTO
{
    /// <summary>
    /// This response object is returned after calling the GetUploads endpoint in UploadsController.
    /// It contains all of the necessary information for the Dashboard app, including each upload info, the Total numer uploaded,
    /// and the PageParams, which are used when changing pages.
    /// </summary>
    public class UploadResponseDto
    {
        /// <summary>
        /// The list of participant uploads that were made.
        /// </summary>
        public List<ParticipantUpload> ParticipantUploadResults { get; set; } = new List<ParticipantUpload>();

        /// <summary>
        /// The query string parameters that get added onto each page navigation link. These are dependent on the filters on the request.
        /// </summary>
        public string? PageParams { get; set; }

        /// <summary>
        /// The total number of uploads found that match the request. This number should be equal to or greater than the number of results in the
        /// results object. It reflects all uploads across multiple pages.
        /// </summary>
        public long Total { get; set; }

    }
}
