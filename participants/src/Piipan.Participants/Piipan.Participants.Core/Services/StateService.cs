using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piipan.Participants.Core.Services
{
    public class StateService : IStateService
    {
        private const string KEY = "States";

        public async Task<IEnumerable<string>> GetStates()
        {
            return await Task.FromResult(Environment
                .GetEnvironmentVariable(KEY)
                .Split(','));
        }
    }
}