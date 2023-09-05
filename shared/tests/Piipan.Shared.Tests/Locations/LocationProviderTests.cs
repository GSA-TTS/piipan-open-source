using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Piipan.States.Api;
using Piipan.States.Api.Models;
using Xunit;

namespace Piipan.Shared.Locations.Tests
{
    public class LocationProviderTests
    {
        LocationOptions locationOptions = new LocationOptions
        {
            NationalOfficeValue = "National"
        };

        public IServiceProvider serviceProviderMock(string location = "EA", string role = "Worker", string[] states = null)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            var statesApiMock = new Mock<IStatesApi>();

            // declare it as object so MemoryCache setup works.
            StatesInfoResponse defaultStateInfoResponse = new StatesInfoResponse
            {
                Results = new System.Collections.Generic.List<StateInfoDto>
                {
                    new StateInfoDto
                    {
                        Email = "ea-test@usda.example",
                        Phone = "123-123-1234",
                        State = "Echo Alpha",
                        StateAbbreviation = "EA"

                    },
                    new StateInfoDto
                    {
                        Email = "ea-test@usda.example",
                        Phone = "123-123-1234",
                        State = "Massachusetts",
                        StateAbbreviation = "MA",
                        Region = "NERO"
                    }
                }
            };

            var statesSetup = statesApiMock.Setup(c => c.GetStates());
            
                statesSetup.ReturnsAsync(defaultStateInfoResponse);
            
            serviceProviderMock.Setup(c => c.GetService(typeof(IStatesApi))).Returns(statesApiMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IMemoryCache))).Returns(new MemoryCache(new MemoryCacheOptions()));


            //IConfiguration configuration = new ConfigurationBuilder()
            //    .AddInMemoryCollection(inMemorySettings)
            //    .Build();


            return serviceProviderMock.Object;
        }

        /// <summary>
        /// Get the
        /// </summary>
        [Fact]
        public async void GetNationalStates()
        {
            // Arrange
            var options = Options.Create(locationOptions);
            var mockServiceProvider = serviceProviderMock();
            var locationProvider = new LocationsProvider(options, mockServiceProvider);

            // Act
            var states = await locationProvider.GetStates("National");

            // Assert
            Assert.Equal(new string[] { "*" }, states);
        }
            
        // TODO: Add regions back in after State Func API is complete
        //[Fact]
        //public void GetMidwestStates()
        //{
        //    // Arrange
        //    var options = Options.Create(locationOptions);
        //    var locationProvider = new LocationsProvider(options);

        //    // Act
        //    var states = locationProvider.GetStates("Midwest");

        //    // Assert
        //    Assert.Equal(new string[] { "WI", "IA", "MN" }, states);
        //}

        [Fact]
        public async void GetSingleState()
        {
            // Arrange
            var options = Options.Create(locationOptions);
            var mockServiceProvider = serviceProviderMock();
            var locationProvider = new LocationsProvider(options, mockServiceProvider);

            // Act
            var states = await locationProvider.GetStates("EA");

            // Assert
            Assert.Equal(new string[] { "EA" }, states);
        }

        [Fact]
        public async void GetRegionStates()
        {
            // Arrange
            var options = Options.Create(locationOptions);
            var mockServiceProvider = serviceProviderMock();
            var locationProvider = new LocationsProvider(options, mockServiceProvider);

            // Act
            var states = await locationProvider.GetStates("NERO");

            // Assert
            Assert.Equal(new string[] { "Massachusetts" }, states);
        }

        [Fact]
        public async void GetNullState()
        {
            // Arrange
            var options = Options.Create(locationOptions);
            var mockServiceProvider = serviceProviderMock();
            var locationProvider = new LocationsProvider(options, mockServiceProvider);

            // Act
            var states = await locationProvider.GetStates(null);

            // Assert
            Assert.Empty(states);
        }
    }
}