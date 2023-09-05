using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Moq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Piipan.Shared.Cryptography.Tests
{
    public class AzureRsaCryptographyClientTests
    {

        [Fact]
        public void Test_Encrypt()
        {
            //Arrange
            string stringToEncrypt = "This string should get encrypted and then decrypted";
            byte[] plaintext = Encoding.UTF8.GetBytes(stringToEncrypt);

            byte[] encryptedCipher = Encoding.UTF32.GetBytes("Mock encrypted value");

            EncryptResult encryptResult = CryptographyModelFactory.EncryptResult(ciphertext: encryptedCipher);
            
            Mock<CryptographyClient> cryptographyClientMock = new Mock<CryptographyClient>();
            cryptographyClientMock.Setup(x => x.Encrypt(EncryptionAlgorithm.RsaOaep, plaintext, default)).Returns(encryptResult);

            AzureRsaCryptographyClient cryptographyClient = new AzureRsaCryptographyClient(cryptographyClientMock.Object);

            //Act
            string encryptedValue = cryptographyClient.EncryptToBase64String(stringToEncrypt);

            //Assert
            var expectedValue = Convert.ToBase64String(encryptedCipher);
            Assert.Equal(expectedValue, encryptedValue);
        }

        [Fact]
        public void Test_Decrypt()
        {
            //Arrange
            string stringToDecrypt = "This string should get encrypted and then decrypted";
            byte[] decryptedBytes = Encoding.UTF8.GetBytes(stringToDecrypt);

            DecryptResult decryptResult = CryptographyModelFactory.DecryptResult(plaintext: decryptedBytes);
            byte[] bytesToDecrypt = Convert.FromBase64String(stringToDecrypt);

            Mock<CryptographyClient> cryptographyClientMock = new Mock<CryptographyClient>();
            cryptographyClientMock.Setup(x => x.Decrypt(EncryptionAlgorithm.RsaOaep, bytesToDecrypt, default)).Returns(decryptResult);

            AzureRsaCryptographyClient cryptographyClient = new AzureRsaCryptographyClient(cryptographyClientMock.Object);

            //Act
            string decryptedValue = cryptographyClient.DecryptFromBase64String(stringToDecrypt);
            
            //Assert
            Assert.Equal(stringToDecrypt, decryptedValue);
        }

        [Fact]
        public void Test_EncryptDecryptIntegrationTest()
        {
            //Arrange

            var random = new SecureRandom();
            var keyGenerationParameters = new KeyGenerationParameters(random, 2048);
            RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
            generator.Init(keyGenerationParameters);

            var keyPair = generator.GenerateKeyPair();

            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);
            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);
            JsonWebKey webKey = new JsonWebKey(rsa, true);
            var decryptCryptographyClient = new CryptographyClient(webKey);

            string stringToEncrypt = "This string should get encrypted and then decrypted";

            AzureRsaCryptographyClient cryptographyClient = new AzureRsaCryptographyClient(decryptCryptographyClient);

            //Act
            string encryptedValue = cryptographyClient.EncryptToBase64String(stringToEncrypt);
            string decryptedValue = cryptographyClient.DecryptFromBase64String(encryptedValue);

            //Assert
            Assert.Equal(stringToEncrypt, decryptedValue);
        }
    }
}
