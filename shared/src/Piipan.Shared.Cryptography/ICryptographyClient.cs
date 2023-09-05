namespace Piipan.Shared.Cryptography
{
    public interface ICryptographyClient
    {
        string EncryptToBase64String(string value);
        string DecryptFromBase64String(string value);
    }
}
