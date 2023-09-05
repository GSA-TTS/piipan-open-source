using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Piipan.States.Api;
using Piipan.States.Api.Models;

namespace Piipan.Shared.Locations
{
    public class LocationsProvider : ILocationsProvider
    {
        private const string StateInfoCacheName = "StatesInfo";
        private readonly LocationOptions _options;
        private readonly IStatesApi _statesApi;
        private readonly IMemoryCache _memoryCache;
        private StatesInfoResponse states;


        public LocationsProvider(IOptions<LocationOptions> options, IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _statesApi = serviceProvider.GetRequiredService<IStatesApi>();
            _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        }

        public async Task<String[]> GetStates(string location)
        {
            if (location == _options.NationalOfficeValue)
            {
                return new string[] { "*" };
            }
            // TODO: Fetch states from State Func API and cache. Return string[] with the states matching our region
            
            if (string.IsNullOrEmpty(location))
            {
                return Array.Empty<string>();
            }
            if (location.Length == 2)
            {
                //location is a state
                return new string[] { location };
            }
            else
            {
                //location is a region
                var allstates = await GetStatesFromStatesApi();
                
                var statesInLocationList = new List<string>();
                foreach (var state in allstates.Results)
                {
                    if (!String.IsNullOrEmpty(state.Region) && state.Region.Equals(location, StringComparison.OrdinalIgnoreCase)) {
                        statesInLocationList.Add(state.State);
                    }
                }
                return statesInLocationList.ToArray();
            }
        }

        public async Task<StatesInfoResponse> GetStatesFromStatesApi()
        {
            // If there are no states or it's null, let's try to fetch it again.
            if ((states?.Results?.Count() ?? 0) == 0)
            {
                _memoryCache.Remove(StateInfoCacheName);
            }
            
            states = await _memoryCache.GetOrCreateAsync(StateInfoCacheName, async (e) =>
            {
                try
                {
                    return await _statesApi.GetStates();
                }
                catch
                {
                    // If an error occurs while fetching the states just return an empty enumerable
                    return new StatesInfoResponse { Results = Enumerable.Empty<StateInfoDto>() };
                }
            });
            return states;
        }
    }
}
