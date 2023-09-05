using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Xunit;

namespace Piipan.Shared.Authorization.Tests
{
    public class AuthorizationPolicyBuilderTests
    {
        [Fact]
        public void Build_NullOptions()
        {
            // Arrange
            // noop

            // Act
            var policy = AuthorizationPolicyBuilder.Build(null);

            // Assert
            Assert.Equal(2, policy.Requirements.Count);
            AssertHasDenyAnonymousAuthorizationRequirement(policy);
            Assert.Single(policy.Requirements, req =>
            {
                return !((req as AssertionRequirement) is null);
            });
        }

        [Fact]
        public void Build_EmptyRequirements()
        {
            // Arrange
            var options = new AuthorizationPolicyOptions
            {
                RequiredClaims = new List<RequiredClaim> { }
            };

            // Act
            var policy = AuthorizationPolicyBuilder.Build(options);

            // Assert
            Assert.Equal(1, policy.Requirements.Count);
            AssertHasDenyAnonymousAuthorizationRequirement(policy);
        }

        [Fact]
        public void Build_ClaimsRequirements_SingleValue()
        {
            // Arrange
            var allowedValues = new List<string> { "val_1" };
            var options = new AuthorizationPolicyOptions
            {
                RequiredClaims = new List<RequiredClaim>
                {
                    new RequiredClaim
                    {
                        Type = "type_1",
                        Values = allowedValues
                    }
                }
            };

            // Act
            var policy = AuthorizationPolicyBuilder.Build(options);

            // Assert
            Assert.Equal(2, policy.Requirements.Count);
            AssertHasDenyAnonymousAuthorizationRequirement(policy);
            AssertHasClaimRequirement(policy, "type_1", allowedValues);
        }

        [Fact]
        public void Build_ClaimsRequirements_MultiValue()
        {
            // Arrange
            var allowedValues = new List<string> { "val_1", "val_2", "val_3" };
            var options = new AuthorizationPolicyOptions
            {
                RequiredClaims = new List<RequiredClaim>
                {
                    new RequiredClaim
                    {
                        Type = "type_1",
                        Values = allowedValues
                    }
                }
            };

            // Act
            var policy = AuthorizationPolicyBuilder.Build(options);

            // Assert
            Assert.Equal(2, policy.Requirements.Count);
            AssertHasDenyAnonymousAuthorizationRequirement(policy);
            AssertHasClaimRequirement(policy, "type_1", allowedValues);
        }

        [Fact]
        public async void Build_ClaimsRequirements_AnyValue_Allow()
        {
            // Arrange
            var allowedValues = new List<string> { "*" };
            var options = new AuthorizationPolicyOptions
            {
                RequiredClaims = new List<RequiredClaim>
                {
                    new RequiredClaim
                    {
                        Type = "type_1",
                        Values = allowedValues
                    }
                }
            };

            // Act 
            var policy = AuthorizationPolicyBuilder.Build(options);

            // Assert
            Assert.Equal(2, policy.Requirements.Count);
            AssertHasDenyAnonymousAuthorizationRequirement(policy);
            Assert.Single(policy.Requirements, req =>
            {
                return !((req as AssertionRequirement) is null);
            });

            // we should be able to use any value for the type_1 claim
            var requirement = policy.Requirements.Single(req => !((req as AssertionRequirement) is null)) as AssertionRequirement;
            var user = UserWithClaim("type_1", "asiuhdfiasuhdfsd");
            var authorizationContext = new AuthorizationHandlerContext(policy.Requirements, user, null);
            Assert.True(await requirement.Handler.Invoke(authorizationContext));
        }

        [Fact]
        public async void Build_ClaimsRequirements_AnyValue_Reject()
        {
            // Arrange
            var allowedValues = new List<string> { "*" };
            var options = new AuthorizationPolicyOptions
            {
                RequiredClaims = new List<RequiredClaim>
                {
                    new RequiredClaim
                    {
                        Type = "type_1",
                        Values = allowedValues
                    }
                }
            };

            // Act 
            var policy = AuthorizationPolicyBuilder.Build(options);

            // Assert
            Assert.Equal(2, policy.Requirements.Count);
            AssertHasDenyAnonymousAuthorizationRequirement(policy);
            Assert.Single(policy.Requirements, req =>
            {
                return !((req as AssertionRequirement) is null);
            });

            // we should be rejected for not having a type_1 claim
            var requirement = policy.Requirements.Single(req => !((req as AssertionRequirement) is null)) as AssertionRequirement;
            var user = UserWithClaim("type_2", "isdhfiuhsdfiuhs");
            var authorizationContext = new AuthorizationHandlerContext(policy.Requirements, user, null);
            Assert.False(await requirement.Handler.Invoke(authorizationContext));
        }

        [Theory]
        [InlineData("val_123", true)]
        [InlineData("val_ab_234", true)]
        [InlineData("val_", false)]
        [InlineData("val-123", false)]
        [InlineData("VAL_123", false)]
        public async void Build_ClaimsRequirements_PartialWildcardValue(string testValue, bool expectAuthorized)
        {
            // Arrange
            var allowedValues = new List<string> { "val_*" };
            var options = new AuthorizationPolicyOptions
            {
                RequiredClaims = new List<RequiredClaim>
                {
                    new RequiredClaim
                    {
                        Type = "type_1",
                        Values = allowedValues
                    }
                }
            };

            // Act
            var policy = AuthorizationPolicyBuilder.Build(options);

            // Assert
            Assert.Equal(2, policy.Requirements.Count);
            AssertHasDenyAnonymousAuthorizationRequirement(policy);
            Assert.Single(policy.Requirements, req =>
            {
                return !((req as AssertionRequirement) is null);
            });

            // we should be able to use any value for the type_1 claim
            var requirement = policy.Requirements.Single(req => !((req as AssertionRequirement) is null)) as AssertionRequirement;
            var user = UserWithClaim("type_1", testValue);
            var authorizationContext = new AuthorizationHandlerContext(policy.Requirements, user, null);
            Assert.Equal(expectAuthorized, await requirement.Handler.Invoke(authorizationContext));
        }

        [Theory]
        [InlineData("val_123", true)]
        [InlineData("val_ab_234", true)]
        [InlineData("val_", false)]
        [InlineData("val-123", false)]
        [InlineData("VAL_123", false)]
        [InlineData("VAL-123", true)]
        public async void Build_ClaimsRequirements_PartialWildcardValue_Multiple_Values(string testValue, bool expectAuthorized)
        {
            // Arrange
            var allowedValues = new List<string> { "val_*", "VAL-*" };
            var options = new AuthorizationPolicyOptions
            {
                RequiredClaims = new List<RequiredClaim>
                {
                    new RequiredClaim
                    {
                        Type = "type_1",
                        Values = allowedValues
                    }
                }
            };

            // Act
            var policy = AuthorizationPolicyBuilder.Build(options);

            // Assert
            Assert.Equal(2, policy.Requirements.Count);
            AssertHasDenyAnonymousAuthorizationRequirement(policy);
            Assert.Single(policy.Requirements, req =>
            {
                return !((req as AssertionRequirement) is null);
            });

            // we should be able to use any value for the type_1 claim
            var requirement = policy.Requirements.Single(req => !((req as AssertionRequirement) is null)) as AssertionRequirement;
            var user = UserWithClaim("type_1", testValue);
            var authorizationContext = new AuthorizationHandlerContext(policy.Requirements, user, null);
            Assert.Equal(expectAuthorized, await requirement.Handler.Invoke(authorizationContext));
        }

        [Theory]
        [InlineData("val_123", true)]
        [InlineData("val_ab_234", true)]
        [InlineData("val_", false)]
        [InlineData("val-123", false)]
        [InlineData("VAL_123", false)]
        [InlineData("testvalue", true)]
        [InlineData("testvalues", false)]
        public async void Build_ClaimsRequirements_PartialWildcardValue_Multiple_Values_Including_SpecificValue(string testValue, bool expectAuthorized)
        {
            // Arrange
            var allowedValues = new List<string> { "val_*", "testvalue" };
            var options = new AuthorizationPolicyOptions
            {
                RequiredClaims = new List<RequiredClaim>
                {
                    new RequiredClaim
                    {
                        Type = "type_1",
                        Values = allowedValues
                    }
                }
            };

            // Act
            var policy = AuthorizationPolicyBuilder.Build(options);

            // Assert
            Assert.Equal(2, policy.Requirements.Count);
            AssertHasDenyAnonymousAuthorizationRequirement(policy);
            Assert.Single(policy.Requirements, req =>
            {
                return !((req as AssertionRequirement) is null);
            });

            // we should be able to use any value for the type_1 claim
            var requirement = policy.Requirements.Single(req => !((req as AssertionRequirement) is null)) as AssertionRequirement;
            var user = UserWithClaim("type_1", testValue);
            var authorizationContext = new AuthorizationHandlerContext(policy.Requirements, user, null);
            Assert.Equal(expectAuthorized, await requirement.Handler.Invoke(authorizationContext));
        }

        private void AssertHasDenyAnonymousAuthorizationRequirement(AuthorizationPolicy policy)
        {
            Assert.Single(policy.Requirements, req =>
            {
                return !((req as DenyAnonymousAuthorizationRequirement) is null);
            });
        }

        private void AssertHasClaimRequirement(AuthorizationPolicy policy, string type, IEnumerable<string> values)
        {
            Assert.Single(policy.Requirements, req =>
            {
                var claimsReq = req as ClaimsAuthorizationRequirement;
                if (claimsReq is null)
                {
                    return false;
                }

                if (claimsReq.ClaimType != type)
                {
                    return false;
                }

                if (claimsReq.AllowedValues.Except(values).Any())
                {
                    return false;
                }

                if (values.Except(claimsReq.AllowedValues).Any())
                {
                    return false;
                }

                return true;
            });
        }

        private ClaimsPrincipal UserWithClaim(string type, string value)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(type, value)
                    }
                )
            );
        }
    }
}