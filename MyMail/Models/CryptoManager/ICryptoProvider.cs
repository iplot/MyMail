using System.Security.Cryptography;

namespace MyMail.Models.CryptoManager
{
    public interface ICryptoProvider
    {
        DES Des { get; }
        RSAParameters RsaKeys { get; }
        string EncrytpData(byte[] data);
        string GetEncryptedSymmKey();
        RSAParameters NewRsaKeys();
        void SetRsaKeys(string D, string E, string N);
    }
}