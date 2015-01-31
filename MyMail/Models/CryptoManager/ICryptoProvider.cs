using System.Security.Cryptography;
using MyMail.Models.Entities;

namespace MyMail.Models.CryptoManager
{
    public interface ICryptoProvider
    {
        DES Des { get; }
        RSAParameters RsaKeys { get; }
        string EncrytpData(byte[] data);
        string GetEncryptedSymmKey(AsymmKey reciever_key);
        RSAParameters NewRsaKeys();
        void SetRsaKeys(string D, string E, string N, string DP, string DQ, string InverseQ, string P, string Q);
        string DecryptData(byte[] data, byte[] symm_key, byte[] iv);
    }
}