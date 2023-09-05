using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Piipan.Shared.Cryptography
{
    public class AzureAesCryptographyClient : ICryptographyClient
    {
        private readonly Aes _aesManaged;
        private const string COLUMN_ENCRYPT_KEY_FUNC_VARIABLE_NAME = "ColumnEncryptionKey";

        public AzureAesCryptographyClient() : this(Environment.GetEnvironmentVariable(COLUMN_ENCRYPT_KEY_FUNC_VARIABLE_NAME))
        {

        }

        public AzureAesCryptographyClient(string base64EncodedEncryptionKey)
        {
            var encryptionKeyBytes = Convert.FromBase64String(base64EncodedEncryptionKey);
            _aesManaged = Create(encryptionKeyBytes);
        }

        public string DecryptFromBase64String(string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;
            var bytesToDecrypt = Convert.FromBase64String(value);

            SHA512 sha = SHA512.Create();
            var iv = sha.ComputeHash(_aesManaged.Key);
            _aesManaged.IV = iv.Take(16).ToArray();

            var decryptor = _aesManaged.CreateDecryptor();
            
            var decryptedResult = decryptor.TransformFinalBlock(bytesToDecrypt, 0, bytesToDecrypt.Length);
            var decryptedString = Encoding.Unicode.GetString(decryptedResult);
            return decryptedString;
        }

        public string EncryptToBase64String(string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            var bytesToEncrypt = Encoding.Unicode.GetBytes(value);

            SHA512 sha = SHA512.Create();
            var iv = sha.ComputeHash(_aesManaged.Key);
            _aesManaged.IV = iv.Take(16).ToArray();

            var encryptor = _aesManaged.CreateEncryptor(_aesManaged.Key, _aesManaged.IV);
            var encryptorResult = encryptor.TransformFinalBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
            var encryptedStringResult = Convert.ToBase64String(encryptorResult);
            return encryptedStringResult;
        }

        private Aes Create(byte[] key)
        {
            var aes = Aes.Create();

            aes.Mode = CipherMode.CBC;
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            return aes;
        }
    }
}
