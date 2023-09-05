using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Enums;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Cryptography;

namespace Piipan.Participants.Core.Services
{
    public class ParticipantService : IParticipantApi
    {
        private readonly IParticipantDao _participantDao;
        private readonly IStateService _stateService;
        private readonly ILogger<ParticipantService> _logger;
        private readonly ICryptographyClient _cryptographyClient;
        private readonly IParticipantUploadApi _uploadService;

        public ParticipantService(
            IParticipantDao participantDao,
            IParticipantUploadApi uploadService,
            IStateService stateService,
            ILogger<ParticipantService> logger,
            ICryptographyClient cryptographyClient
            )
        {
            _participantDao = participantDao;
            _uploadService = uploadService;
            _stateService = stateService;
            _logger = logger;
            _cryptographyClient = cryptographyClient;
        }

        public async Task<IEnumerable<IParticipant>> GetParticipants(string state, string ldsHash)
        {
            try
            {
                var upload = await _uploadService.GetLatestUpload(state);
                var participants = await _participantDao.GetParticipants(state, ldsHash, upload.Id);

                // Set the participant State before returning
                return participants.Select(p => new ParticipantDto(p)
                {
                    State = state,
                    ParticipantId = _cryptographyClient.DecryptFromBase64String(p.ParticipantId),
                    CaseId = _cryptographyClient.DecryptFromBase64String(p.CaseId),
                    VulnerableIndividual = p.VulnerableIndividual
                });
            }
            catch (InvalidOperationException)
            {
                return Enumerable.Empty<IParticipant>();
            }
        }

        public async Task AddParticipants(IEnumerable<IParticipant> participants, IUpload upload, string state, Func<Exception, string> redacteErrorMessageCallback)
        {
            var uploadIdentifier = upload.UploadIdentifier;

            try
            {
                using (TransactionScope scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    TimeSpan.FromSeconds(600),
                    TransactionScopeAsyncFlowOption.Enabled))
                {
                    var participantDbos = participants.Select(p => new ParticipantDbo(p)
                    {
                        UploadId = upload.Id,
                        LdsHash = _cryptographyClient.EncryptToBase64String(p.LdsHash),
                        CaseId = _cryptographyClient.EncryptToBase64String(p.CaseId),
                        ParticipantId = _cryptographyClient.EncryptToBase64String(p.ParticipantId)
                    });

                    var count = await _participantDao.AddParticipants(participantDbos);
                    long participantsUploaded = Convert.ToInt64(count);

                    upload.ParticipantsUploaded = participantsUploaded;
                    upload.Status = UploadStatuses.COMPLETE.ToString();

                    await _uploadService.UpdateUpload(upload, state);

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                var redactedErrorMessage = redacteErrorMessageCallback?.Invoke(ex);
                await _uploadService.UpdateUpload(upload, state, redactedErrorMessage);
            }
        }

        public async Task<IEnumerable<string>> GetStates()
        {
            return await _stateService.GetStates();
        }
        public async Task DeleteOldParticpants(string state = null)
        {

            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                TimeSpan.FromSeconds(600),
                TransactionScopeAsyncFlowOption.Enabled))
            {
                var upload = await _uploadService.GetLatestUpload(state);
                await _participantDao.DeleteOldParticipantsExcept(state, upload.Id);
                scope.Complete();
            }
        }
    }
}
