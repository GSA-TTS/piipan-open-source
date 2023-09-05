using System;

namespace Piipan.Participants.Api.Models
{
    public record ParticipantUploadErrorDetails(string State, DateTime StartTime, DateTime EndTime, Exception Exception, string FileName);
}
