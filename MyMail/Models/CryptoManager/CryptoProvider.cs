using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using MyMail.Models.Entities;
using NHibernate.Classic;

namespace MyMail.Models.CryptoManager
{
    public class CryptoProvider : ICryptoProvider
    {
        public DES Des { get; private set; }

        public RSAParameters RsaKeys { get; private set; }

        public CryptoProvider()
        {
            Des = new DESCryptoServiceProvider();
        }

        //Новый ключ для RSA
        public RSAParameters NewRsaKeys()
        {
            RSA rsa = new RSACryptoServiceProvider();
            RsaKeys = rsa.ExportParameters(true);

            return RsaKeys;
        }

        //Задать ключ RSA
        //!!!!!!
        public void SetRsaKeys(string D, string E, string N, string DP, string DQ, string InverseQ, string P, string Q)
        {
            RSAParameters param = new RSAParameters
            {
                D = Convert.FromBase64String(D),
                Exponent = Convert.FromBase64String(E),
                Modulus = Convert.FromBase64String(N),
                DP = Convert.FromBase64String(DP),
                DQ = Convert.FromBase64String(DQ),
                InverseQ = Convert.FromBase64String(InverseQ),
                P = Convert.FromBase64String(P),
                Q = Convert.FromBase64String(Q)
            };

            RsaKeys = param;
        }

        //Возвращаем зашифрованные данные в виде строки base64 
        public string EncrytpData(byte[] data)
        {
            using(MemoryStream memoryIn = new MemoryStream(data))
            using (MemoryStream memoryOut = new MemoryStream())
            {

                using (
                    CryptoStream encStream = new CryptoStream(memoryOut, Des.CreateEncryptor(Des.Key, Des.IV), CryptoStreamMode.Write))
                {

                    byte[] tempData = new byte[2000];
                    int read = 0;
                    do
                    {
                        read = memoryIn.Read(tempData, 0, tempData.Length);
                        encStream.Write(tempData, 0, read);
                        encStream.Flush();
                    } while (read != 0);
//                    encStream.FlushFinalBlock();
                    encStream.Close();
                }

                byte[] test = memoryOut.GetBuffer();
                return Convert.ToBase64String(memoryOut.ToArray());
            }
        }

        //Получить зашифрованный синхронный ключ
        //После обновления синхронный ключ обновляется
        //Тут ошибка! Предусмотреть получение публичного ключа получателя для RSA
        public string GetEncryptedSymmKey(AsymmKey reciever_key)
        {
            using (RSA Rsa = new RSACryptoServiceProvider())
            {
                RSAParameters rec_key = new RSAParameters
                {
                    D = Convert.FromBase64String(reciever_key.D),
                    DP = Convert.FromBase64String(reciever_key.DP),
                    DQ = Convert.FromBase64String(reciever_key.DQ),
                    Exponent = Convert.FromBase64String(reciever_key.E),
                    InverseQ = Convert.FromBase64String(reciever_key.InverseQ),
                    Modulus = Convert.FromBase64String(reciever_key.N),
                    P = Convert.FromBase64String(reciever_key.P),
                    Q = Convert.FromBase64String(reciever_key.Q)
                };

                Rsa.ImportParameters(rec_key);

                byte[] symmKey = (Rsa as RSACryptoServiceProvider).Encrypt(Des.Key, false);
                byte[] symmIV = (Rsa as RSACryptoServiceProvider).Encrypt(Des.IV, false);

                string encryptedKeys = Convert.ToBase64String(symmKey) + Convert.ToBase64String(symmIV);
                Des = new DESCryptoServiceProvider();

                return encryptedKeys;
            }
        }

        //!!!!
        public string DecryptData(byte[] data, byte[] symm_key, byte[] iv)
        {
            using (RSA Rsa = new RSACryptoServiceProvider())
            {
                Rsa.ImportParameters(RsaKeys);

                byte[] symmKey = (Rsa as RSACryptoServiceProvider).Decrypt(symm_key, false);
                byte[] symmIv = (Rsa as RSACryptoServiceProvider).Decrypt(iv, false);

                using (MemoryStream memoryIn = new MemoryStream(data))
                using (MemoryStream memoryOut = new MemoryStream())
                {
//                    Des.Key = symmKey;
//                    Des.IV = symmIv;

                    using (CryptoStream cryptoStream = 
                        new CryptoStream(memoryIn, Des.CreateDecryptor(symmKey, symmIv), CryptoStreamMode.Read))
                    {
                        byte[] temp = new byte[2000];
                        int read = 0;

                        do
                        {
                            read = cryptoStream.Read(temp, 0, temp.Length);
                            memoryOut.Write(temp, 0, read);
                        } while (read != 0);

                    }

                    return Encoding.ASCII.GetString(memoryOut.ToArray());//с этим могут быть проблемы
                }
            }
        }
    }
}