using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Piipan.Shared.Deidentification.Tests
{
    public class LdsDeidentifierTests
    {
        public class RunTests
        {
            public LdsDeidentifier _ldsDeidentifier;

            public RunTests()
            {
                _ldsDeidentifier = new LdsDeidentifier(
                    new NameNormalizer(),
                    new DobNormalizer(),
                    new SsnNormalizer(),
                    new LdsHasher()
                );
            }

            [Theory]
            [InlineData("Foobar", "2000-12-29", "425-46-5417")]
            [InlineData("von Neuman", "2020-01-01", "195-26-4789")]
            public void returnsValidDigest(string lname, string dob, string ssn)
            {
                string result = _ldsDeidentifier.Run(lname, dob, ssn);
                Assert.Matches("^[0-9a-f]{128}$", result);
                Assert.Equal(128, result.Length);
            }

            [Theory]
            [InlineData("Foobar", "1987-11-29", "425-46-541")] // missing serial number
            public void ThrowsExWhenInvalidInput(string lname, string dob, string ssn)
            {
                Assert.Throws<ArgumentException>(() => _ldsDeidentifier.Run(lname, dob, ssn));
            }

            [Fact]
            public void ReturnsExpectedResult()
            {
                string result = _ldsDeidentifier.Run("hopper", "1978-08-14", "425-46-5417");
                string expectedDigest = "e733ee077eb82e13874a270bf170e3b999031c71eb5f0b47fc51c7cc677d0b8dd3b79615d79fa4ba2779c5fb9764b81aaa219dce20edb978a79903b647b5b714";
                Assert.Equal(expectedDigest, result);
            }

            [Fact]
            public void ReturnsExpectedExamples()
            {
                using(var reader = new StreamReader("plaintext-example.csv"))
                {
                    string headerLine = reader.ReadLine();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split(',');
                        List<string> names = new List<string>() { values[0], values[1], values[2] };
                        string result = _ldsDeidentifier.Run(values[0], values[1], values[2]);
                    }
                }

                using (StreamReader pReader = new StreamReader("plaintext-example.csv"))
                using (StreamReader reader = new StreamReader("example.csv")) {

                    string pHeader = pReader.ReadLine();
                    string header = reader.ReadLine();

                    string pLine;
                    while ((pLine = pReader.ReadLine()) != null)
                    {
                        var pValues = pLine.Split(',');
                        string result = _ldsDeidentifier.Run(pValues[0], pValues[1], pValues[2]);
                        string expected = reader.ReadLine().Split(',')[0];
                        Assert.Equal(expected, result);
                    }
                }
            }
        }
    }
}
