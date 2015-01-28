using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

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
        public void SetRsaKeys(string D, string E, string N)
        {
            RSAParameters param = new RSAParameters
            {
                D = Convert.FromBase64String(D),
                Exponent = Convert.FromBase64String(E),
                Modulus = Convert.FromBase64String(N)
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
                    CryptoStream encStream = new CryptoStream(memoryOut, Des.CreateEncryptor(), CryptoStreamMode.Write))
                {

                    byte[] tempData = new byte[2000];
                    int read = 0;
                    do
                    {
                        read = memoryIn.Read(tempData, 0, tempData.Length);
                        encStream.Write(tempData, 0, read);
                    } while (read != 0);

                    return Convert.ToBase64String(memoryOut.GetBuffer());
                }
            }
        }

        //Получить зашифрованный синхронный ключ
        //После обновления синхронный ключ обновляется
        //Тут ошибка! Предусмотреть получение публичного ключа получателя для RSA
        public string GetEncryptedSymmKey()
        {
            using (RSA Rsa = new RSACryptoServiceProvider())
            {
                Rsa.ImportParameters(RsaKeys);

//                byte[] symmKey = Rsa.EncryptValue(Des.Key);
                byte[] symmKey = (Rsa as RSACryptoServiceProvider).Encrypt(Des.Key, false);
//                byte[] symmIV = Rsa.EncryptValue(Des.IV);
                byte[] symmIV = (Rsa as RSACryptoServiceProvider).Encrypt(Des.IV, false);

                string encryptedKeys = Convert.ToBase64String(symmKey) + Convert.ToBase64String(symmIV);
                Des = new DESCryptoServiceProvider();

                return encryptedKeys;
            }
        }
    }
}