using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace StarChatServer
{
    public static class AesEncryption
    {

        public static string enc_aes_normal(string s)
        {
            return EncryptString(s, UTF8Encoding.UTF8.GetBytes("!@!@DAS%G}{A<>?,./;p][=-"), UTF8Encoding.UTF8.GetBytes("a]|=-/.,>>;<\"a@^"));
        }

        public static string enc_aes_sse(string s)
        {
            return EncryptString(s, UTF8Encoding.UTF8.GetBytes("azazsxpl=-[],><<</;\"\"\"*#"), UTF8Encoding.UTF8.GetBytes("[ssf(53)12-=_+%4"));
        }

        public static string enc_aes_log(string s)
        {
            return EncryptString(s, UTF8Encoding.UTF8.GetBytes("alal$%:?>,,,<<<<{sdp_+@!"), UTF8Encoding.UTF8.GetBytes("\\][azpcmd!@#&*$#"));
        }

        public static string dec_aes_normal(string s)
        {
            return DecryptString(s, UTF8Encoding.UTF8.GetBytes("!@!@DAS%G}{A<>?,./;p][=-"), UTF8Encoding.UTF8.GetBytes("a]|=-/.,>>;<\"a@^"));
        }

        public static string dec_aes_sse(string s)
        {
            return DecryptString(s, UTF8Encoding.UTF8.GetBytes("azazsxpl=-[],><<</;\"\"\"*#"), UTF8Encoding.UTF8.GetBytes("[ssf(53)12-=_+%4"));
        }

        public static string dec_aes_log(string s)
        {
            return DecryptString(s, UTF8Encoding.UTF8.GetBytes("alal$%:?>,,,<<<<{sdp_+@!"), UTF8Encoding.UTF8.GetBytes("\\][azpcmd!@#&*$#"));
        }

        public static string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public static string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
