using Xunit;

namespace Piipan.Shared.Cryptography.Tests
{
    public class AzureAESCryptographyClientTests
    {

        [Fact]
        public void Test_EncryptDecryptIntegrationTest()
        {
            //Arrange
            string stringToEncrypt = "This string should get encrypted and then decrypted";
            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";

            var cryptographyClient = new AzureAesCryptographyClient(key);

            //Act
            string encryptedValue = cryptographyClient.EncryptToBase64String(stringToEncrypt);

            //Use a separate instace of AzureAesCryptographyClient to ensure we're not reusing key/iv from the encrypt call. It should be initialized fresh.
            var decryptClient = new AzureAesCryptographyClient(base64EncodedKey);

            string decryptedValue = decryptClient.DecryptFromBase64String(encryptedValue);

            //Assert
            Assert.Equal(stringToEncrypt, decryptedValue);
        }
    }
}
