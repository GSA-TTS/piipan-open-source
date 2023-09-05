using Azure.Core;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using System;
using System.Text;

namespace Piipan.Shared.Cryptography
{
    public class AzureRsaCryptographyClient : ICryptographyClient
    {
        private readonly CryptographyClient _internalCryptographyClient;
        private EncryptionAlgorithm ENCRYPTION_ALGORITHM = EncryptionAlgorithm.RsaOaep;

        public AzureRsaCryptographyClient(string vaultUrl, TokenCredential credential, string keyName)
        {
            var client = new KeyClient(vaultUri: new Uri(vaultUrl), credential: credential);
            KeyVaultKey key = client.GetKey(keyName);

            var cryptoClient = client.GetCryptographyClient(key.Name, key.Properties.Version);
            _internalCryptographyClient = cryptoClient;
        }
        
        public AzureRsaCryptographyClient(CryptographyClient cryptographyClient)
        {
            _internalCryptographyClient = cryptographyClient;
        }

        public string DecryptFromBase64String(string value)
        {
            var bytesToDecrypt = Convert.FromBase64String(value);
            var decryptedResult = _internalCryptographyClient.Decrypt(ENCRYPTION_ALGORITHM, bytesToDecrypt);
            var decryptedString = Encoding.UTF8.GetString(decryptedResult.Plaintext);
            return decryptedString;
        }

        public string EncryptToBase64String(string value)
        {
            byte[] bytesForValue = Encoding.UTF8.GetBytes(value);
            EncryptResult encryptResult = _internalCryptographyClient.Encrypt(ENCRYPTION_ALGORITHM, bytesForValue);
            var encryptedStringResult = Convert.ToBase64String(encryptResult.Ciphertext);
            return encryptedStringResult;
        }
    }
}
