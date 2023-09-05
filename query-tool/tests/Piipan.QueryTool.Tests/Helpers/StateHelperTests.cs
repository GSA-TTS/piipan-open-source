using Piipan.QueryTool.Client.Helpers;
using Xunit;

namespace Piipan.QueryTool.Tests.Helpers
{
    public class StateHelperTests
    {
        [Theory]
        [InlineData("al", "Alabama")]
        [InlineData("ak", "Alaska")]
        [InlineData("az", "Arizona")]
        [InlineData("ar", "Arkansas")]
        [InlineData("ca", "California")]
        [InlineData("cz", "Canal Zone")]
        [InlineData("co", "Colorado")]
        [InlineData("ct", "Connecticut")]
        [InlineData("de", "Delaware")]
        [InlineData("dc", "District of Columbia")]
        [InlineData("fl", "Florida")]
        [InlineData("ga", "Georgia")]
        [InlineData("gu", "Guam")]
        [InlineData("hi", "Hawaii")]
        [InlineData("id", "Idaho")]
        [InlineData("il", "Illinois")]
        [InlineData("in", "Indiana")]
        [InlineData("ia", "Iowa")]
        [InlineData("ks", "Kansas")]
        [InlineData("ky", "Kentucky")]
        [InlineData("la", "Louisiana")]
        [InlineData("me", "Maine")]
        [InlineData("md", "Maryland")]
        [InlineData("ma", "Massachusetts")]
        [InlineData("mi", "Michigan")]
        [InlineData("mn", "Minnesota")]
        [InlineData("ms", "Mississippi")]
        [InlineData("mo", "Missouri")]
        [InlineData("mt", "Montana")]
        [InlineData("ne", "Nebraska")]
        [InlineData("nv", "Nevada")]
        [InlineData("nh", "New Hampshire")]
        [InlineData("nj", "New Jersey")]
        [InlineData("nm", "New Mexico")]
        [InlineData("ny", "New York")]
        [InlineData("nc", "North Carolina")]
        [InlineData("nd", "North Dakota")]
        [InlineData("oh", "Ohio")]
        [InlineData("ok", "Oklahoma")]
        [InlineData("or", "Oregon")]
        [InlineData("pa", "Pennsylvania")]
        [InlineData("pr", "Puerto Rico")]
        [InlineData("ri", "Rhode Island")]
        [InlineData("sc", "South Carolina")]
        [InlineData("sd", "South Dakota")]
        [InlineData("tn", "Tennessee")]
        [InlineData("tx", "Texas")]
        [InlineData("ut", "Utah")]
        [InlineData("vt", "Vermont")]
        [InlineData("vi", "Virgin Islands")]
        [InlineData("va", "Virginia")]
        [InlineData("wa", "Washington")]
        [InlineData("wv", "West Virginia")]
        [InlineData("wi", "Wisconsin")]
        [InlineData("wy", "Wyoming")]
        [InlineData("ea", "Echo Alpha")]
        [InlineData("eb", "Echo Bravo")]
        [InlineData("ec", "Echo Charlie")]
        public void State_Abbreviation_Returns_Correct_State(string abbreviation, string expectedState)
        {
            // Act
            var state = StateHelper.GetStateName(abbreviation);

            // Assert
            Assert.Equal(expectedState, state);
        }

        [Theory]
        [InlineData("AL", "Alabama")]
        [InlineData("AK", "Alaska")]
        [InlineData("AZ", "Arizona")]
        [InlineData("AR", "Arkansas")]
        [InlineData("CA", "California")]
        [InlineData("CZ", "Canal Zone")]
        [InlineData("CO", "Colorado")]
        [InlineData("CT", "Connecticut")]
        [InlineData("DE", "Delaware")]
        [InlineData("DC", "District of Columbia")]
        [InlineData("FL", "Florida")]
        [InlineData("GA", "Georgia")]
        [InlineData("GU", "Guam")]
        [InlineData("HI", "Hawaii")]
        [InlineData("ID", "Idaho")]
        [InlineData("IL", "Illinois")]
        [InlineData("IN", "Indiana")]
        [InlineData("IA", "Iowa")]
        [InlineData("KS", "Kansas")]
        [InlineData("KY", "Kentucky")]
        [InlineData("LA", "Louisiana")]
        [InlineData("ME", "Maine")]
        [InlineData("MD", "Maryland")]
        [InlineData("MA", "Massachusetts")]
        [InlineData("MI", "Michigan")]
        [InlineData("MN", "Minnesota")]
        [InlineData("MS", "Mississippi")]
        [InlineData("MO", "Missouri")]
        [InlineData("MT", "Montana")]
        [InlineData("NE", "Nebraska")]
        [InlineData("NV", "Nevada")]
        [InlineData("NH", "New Hampshire")]
        [InlineData("NJ", "New Jersey")]
        [InlineData("NM", "New Mexico")]
        [InlineData("NY", "New York")]
        [InlineData("NC", "North Carolina")]
        [InlineData("ND", "North Dakota")]
        [InlineData("OH", "Ohio")]
        [InlineData("OK", "Oklahoma")]
        [InlineData("OR", "Oregon")]
        [InlineData("PA", "Pennsylvania")]
        [InlineData("PR", "Puerto Rico")]
        [InlineData("RI", "Rhode Island")]
        [InlineData("SC", "South Carolina")]
        [InlineData("SD", "South Dakota")]
        [InlineData("TN", "Tennessee")]
        [InlineData("TX", "Texas")]
        [InlineData("UT", "Utah")]
        [InlineData("VT", "Vermont")]
        [InlineData("VI", "Virgin Islands")]
        [InlineData("VA", "Virginia")]
        [InlineData("WA", "Washington")]
        [InlineData("WV", "West Virginia")]
        [InlineData("WI", "Wisconsin")]
        [InlineData("WY", "Wyoming")]
        [InlineData("EA", "Echo Alpha")]
        [InlineData("EB", "Echo Bravo")]
        [InlineData("EC", "Echo Charlie")]
        public void State_Abbreviation_Returns_Correct_State_When_UpperCase(string abbreviation, string expectedState)
        {
            // Act
            var state = StateHelper.GetStateName(abbreviation);

            // Assert
            Assert.Equal(expectedState, state);
        }


        [Fact]
        public void State_Abbreviation_Returns_Empty_String_When_Invalid()
        {
            // Act
            var state = StateHelper.GetStateName("ed");

            // Assert
            Assert.Equal("", state);
        }
    }
}
