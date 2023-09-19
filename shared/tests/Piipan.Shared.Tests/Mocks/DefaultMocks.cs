using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Metrics.Api;
using Piipan.Shared.API.Constants;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using Piipan.Shared.Locations;
using Piipan.Shared.Roles;
using Piipan.Shared.Web;
using Piipan.States.Api;
using Piipan.States.Api.Models;

namespace Piipan.Shared.Tests.Mocks
{
    public static class DefaultMocks
    {
        public const string Role_Worker = "Worker";
        public const string Role_Oversight = "Oversight";
        public const string ValidMatchIDFromEA = "MVALIDA";
        public const string ValidMatchIDFromEB = "MVALIDB";

        static StateInfoDto EA() =>
            new StateInfoDto
            {
                Email = "ea-test@agency.example",
                Phone = "123-123-1234",
                State = "Echo Alpha",
                StateAbbreviation = "EA"
            };
        static StatesInfoResponse DefaultStatesResponse() =>
            new StatesInfoResponse
            {
                Results = new List<StateInfoDto>
                        {
                            EA()
                        }
            };

        public static Mock<IRolesProvider> RoleProviderMock()
        {
            Mock<IRolesProvider> roleProviderMock = new();
            roleProviderMock.Setup(c => c.GetRoles()).Returns(new RoleOptions
            {
                { RoleConstants.ViewMatchArea, new string[] { Role_Worker, Role_Oversight } },
                { RoleConstants.EditMatchArea, new string[] { Role_Worker } },
            });
            roleProviderMock.Setup(c => c.TryGetRoles(It.IsAny<string>()))
                .Returns((string t) => roleProviderMock.Object.GetRoles().FirstOrDefault(n => n.Key == t).Value);

            return roleProviderMock;
        }

        public static IConfiguration ConfigurationMock(Dictionary<string, string> inMemorySettings = null)
        {
            inMemorySettings ??= new Dictionary<string, string> {
                {"HelpDeskEmail", "test@agency.example"},
            };

            return new ConfigurationBuilder()
                    .AddInMemoryCollection(inMemorySettings)
                    .Build();
        }

        public static Mock<IStatesApi> StatesApiMock(Action setupGetStates = null)
        {
            Mock<IStatesApi> statesApiMock = new();
            if (setupGetStates == null)
            {
                statesApiMock.Setup(c => c.GetStates())
                    .ReturnsAsync(
                        new StatesInfoResponse
                        {
                            Results = new System.Collections.Generic.List<StateInfoDto>
                            {
                                new StateInfoDto
                                {
                                    Email = "ea-test@agency.example",
                                    Phone = "123-123-1234",
                                    State = "Echo Alpha",
                                    StateAbbreviation = "EA"
                                }
                            }
                        }
                    );
            }
            else
            {
                setupGetStates();
            }
            return statesApiMock;
        }

        public static Mock<ILogger<T>> ILoggerMock<T>()
        {
            return new Mock<ILogger<T>>();
        }

        public static Mock<ILdsDeidentifier> ILdsDeidentifierMock()
        {
            return new Mock<ILdsDeidentifier>();
        }

        public static Mock<IMatchSearchApi> IMatchApiMock()
        {
            Mock<IMatchSearchApi> matchApiMock = new();
            matchApiMock
                .Setup(m => m.FindAllMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ReturnsAsync(new OrchMatchResponse
                {
                    Data = new OrchMatchResponseData
                    {
                        Results = new List<OrchMatchResult>
                        {
                            new OrchMatchResult
                            {
                                Index = 0,
                                Matches = new List<ParticipantMatch>
                                {
                                    new ParticipantMatch
                                    {
                                        LdsHash = "foobar",
                                        State = "ea",
                                        CaseId = "caseId",
                                        ParticipantId = "pId",
                                        ParticipantClosingDate = new DateTime(2021, 05, 31),
                                        RecentBenefitIssuanceDates = new List<DateRange>
                                        {
                                            new DateRange(new DateTime(2021, 4, 1),new DateTime(2021, 5, 1)),
                                            new DateRange(new DateTime(2021, 6, 1),new DateTime(2021, 7, 1)),
                                            new DateRange(new DateTime(2021, 02, 28),new DateTime(2021, 3, 15))
                                        },
                                        VulnerableIndividual = false
                                    }
                                }
                            }
                        },
                        Errors = new List<OrchMatchError>()
                    }
                });
            return matchApiMock;
        }
        public static Mock<IMatchResolutionApi> IMatchResolutionApiMock()
        {
            Mock<IMatchResolutionApi> matchResolutionApiMock = new();
            var apiReturnValueForEA = new MatchResApiResponse
            {
                Data = new MatchDetailsDto
                {
                    States = new string[] { "ea", "eb" },
                    Initiator = "ea",
                    MatchId = ValidMatchIDFromEA
                }
            };
            var apiReturnValueForEB = new MatchResApiResponse
            {
                Data = new MatchDetailsDto
                {
                    States = new string[] { "ea", "eb" },
                    Initiator = "eb",
                    MatchId = ValidMatchIDFromEB
                }
            };

            matchResolutionApiMock
                .Setup(n => n.GetMatch(ValidMatchIDFromEA, "EA"))
                .ReturnsAsync(apiReturnValueForEA);
            matchResolutionApiMock
                .Setup(n => n.GetMatch(It.IsNotIn(ValidMatchIDFromEA), "EA"))
                .ReturnsAsync((MatchResApiResponse)null);
            matchResolutionApiMock
                .Setup(n => n.GetMatch(ValidMatchIDFromEB, "EB"))
                .ReturnsAsync(apiReturnValueForEB);
            matchResolutionApiMock
                .Setup(n => n.GetMatch(It.IsNotIn(ValidMatchIDFromEB), "EB"))
                .ReturnsAsync((MatchResApiResponse)null);
            matchResolutionApiMock
                .Setup(n => n.GetMatches())
                .ReturnsAsync(
                    new MatchResListApiResponse
                    {
                        Data = new List<MatchDetailsDto>
                        {
                            apiReturnValueForEA.Data, apiReturnValueForEB.Data
                        }
                    });
            return matchResolutionApiMock;
        }

        public static Mock<IParticipantUploadReaderApi> IParticipantUploadReaderApiMock()
        {
            Mock<IParticipantUploadReaderApi> participantUploadReaderApiMock = new();
            var getUploadsResponse = new GetParticipantUploadsResponse
            {
                Data = new List<ParticipantUpload>()
                {
                    new ParticipantUpload
                    {
                        State = "EA",
                        UploadedAt = DateTime.Now,
                        Status = "COMPLETE",
                        UploadIdentifier = "Upload1",
                        ParticipantsUploaded = 10
                    }
                },
                Meta = new()
                {
                    PerPage = 53,
                    Total = 1
                }
            };
            var getUploadStatisticsResponse = new ParticipantUploadStatistics
            {
                TotalComplete = 2,
                TotalFailure = 1
            };

            participantUploadReaderApiMock
                .Setup(n => n.GetUploads(It.IsAny<ParticipantUploadRequestFilter>()))
                .ReturnsAsync(getUploadsResponse);
            participantUploadReaderApiMock
                .Setup(n => n.GetUploadStatistics(It.IsAny<ParticipantUploadStatisticsRequest>()))
                .ReturnsAsync(getUploadStatisticsResponse);

            return participantUploadReaderApiMock;
        }

        public static Mock<IClaimsProvider> IClaimsProviderMock()
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(m => m.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns("noreply@tts.test");
            claimsProviderMock
                .Setup(m => m.GetLocation(It.IsAny<ClaimsPrincipal>()))
                .Returns("EA");
            claimsProviderMock
                .Setup(m => m.GetRole(It.IsAny<ClaimsPrincipal>()))
                .Returns("Worker");
            return claimsProviderMock;
        }

        public static Mock<IWebAppDataServiceProvider> IWebAppDataServiceProviderMock()
        {
            Mock<IWebAppDataServiceProvider> dataServiceProvider = new Mock<IWebAppDataServiceProvider>();

            dataServiceProvider.Setup(c => c.Email).Returns("noreply@tts.test");
            dataServiceProvider.Setup(c => c.Location).Returns("EA");
            dataServiceProvider.Setup(c => c.States).Returns(new string[] { "EA" });
            dataServiceProvider.Setup(c => c.AppRolesByArea).Returns(
                new RoleOptions
                {
                    { RoleConstants.ViewMatchArea, new string[] { Role_Worker, Role_Oversight } },
                    { RoleConstants.EditMatchArea, new string[] { Role_Worker } },
                }
            );
            dataServiceProvider.Setup(c => c.HelpDeskEmail).Returns("help@agency.example");
            dataServiceProvider.Setup(c => c.Role).Returns(Role_Worker);
            dataServiceProvider.Setup(c => c.BaseUrl).Returns("https://webapp.agency.example");
            dataServiceProvider.Setup(c => c.StateInfo).Returns(DefaultStatesResponse());
            dataServiceProvider.Setup(c => c.LoggedInUsersState).Returns(EA());
            dataServiceProvider.Setup(c => c.IsNationalOffice).Returns(() => dataServiceProvider.Object.States?.Contains("*") ?? false);
            return dataServiceProvider;
        }

        public static IMemoryCache MemoryCacheMock(MemoryCacheOptions memoryCacheOptions = null)
        {
            memoryCacheOptions ??= new MemoryCacheOptions();
            return new MemoryCache(memoryCacheOptions);
        }

        public static Mock<AzureServiceTokenProvider> MockAzureServiceTokenProvider(string token = "token")
        {
            var mockProvider = new Mock<AzureServiceTokenProvider>(() =>
                new AzureServiceTokenProvider(null, "https://tts.test")
            );

            mockProvider
                .Setup(m => m.GetAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            return mockProvider;
        }

        public static Mock<HttpContext> HttpContextMock(Mock<HttpRequest> request = null)
        {
            request ??= new Mock<HttpRequest>();
            var defaultHttpContext = new DefaultHttpContext();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));

            request
                .Setup(m => m.Headers)
                .Returns(new HeaderDictionary());

            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);
            context.Setup(m => m.Response).Returns(defaultHttpContext.Response);

            return context;
        }

        public static Mock<HttpContextAccessor> HttpContextAccessorMock(HttpContext context = null)
        {
            context ??= HttpContextMock().Object;
            Mock<HttpContextAccessor> httpContextAccessorMock = new Mock<HttpContextAccessor>();
            httpContextAccessorMock.Setup(n => n.HttpContext).Returns(context);

            return httpContextAccessorMock;
        }

        public static Mock<ILocationsProvider> ILocationsProviderMock()
        {
            var locationProviderMock = new Mock<ILocationsProvider>();
            locationProviderMock.Setup(c => c.GetStates("EA")).ReturnsAsync(new string[] { "EA" });
            locationProviderMock.Setup(c => c.GetStates("National")).ReturnsAsync(new string[] { "*" });
            locationProviderMock.Setup(c => c.GetStatesFromStatesApi()).ReturnsAsync(
                new StatesInfoResponse
                {
                    Results = new List<StateInfoDto>
                    {
                        new StateInfoDto
                        {
                            Email = "ea@email.example",
                            State = "Echo Alpha",
                            StateAbbreviation = "EA"
                        },
                        new StateInfoDto
                        {
                            Email = "eb@email.example",
                            State = "Echo Bravo",
                            StateAbbreviation = "EB"
                        }
                    }
                });
            return locationProviderMock;
        }

        public static void HttpContextMock(ControllerBase controller, Mock<HttpRequest> request = null)
        {
            request ??= new Mock<HttpRequest>();
            var defaultHttpContext = new DefaultHttpContext();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));

            request
                .Setup(m => m.Headers)
                .Returns(new HeaderDictionary());

            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);
            context.Setup(m => m.Response).Returns(defaultHttpContext.Response);
            controller.ControllerContext.HttpContext = context.Object;

        }
    }
}
