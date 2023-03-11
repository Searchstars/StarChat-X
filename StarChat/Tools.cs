using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using Vanara.Extensions;

namespace StarChat
{
    public static class Tools
    {

        public static class AesEncryption
        {

            public async static Task<string> enc_aes_normal(string s)
            {
                return await EncryptString(s, UTF8Encoding.UTF8.GetBytes("!@!@DAS%G}{A<>?,./;p][=-"), UTF8Encoding.UTF8.GetBytes("a]|=-/.,>>;<\"a@^"));
            }

            public async static Task<string> enc_aes_sse(string s)
            {
                return await EncryptString(s, UTF8Encoding.UTF8.GetBytes("azazsxpl=-[],><<</;\"\"\"*#"), UTF8Encoding.UTF8.GetBytes("[ssf(53)12-=_+%4"));
            }

            public async static Task<string> enc_aes_log(string s)
            {
                return await EncryptString(s, UTF8Encoding.UTF8.GetBytes("alal$%:?>,,,<<<<{sdp_+@!"), UTF8Encoding.UTF8.GetBytes("\\][azpcmd!@#&*$#"));
            }

            public async static Task<string> dec_aes_normal(string s)
            {
                return await DecryptString(s, UTF8Encoding.UTF8.GetBytes("!@!@DAS%G}{A<>?,./;p][=-"), UTF8Encoding.UTF8.GetBytes("a]|=-/.,>>;<\"a@^"));
            }

            public async static Task<string> dec_aes_sse(string s)
            {
                return await DecryptString(s, UTF8Encoding.UTF8.GetBytes("azazsxpl=-[],><<</;\"\"\"*#"), UTF8Encoding.UTF8.GetBytes("[ssf(53)12-=_+%4"));
            }

            public async static Task<string> dec_aes_log(string s)
            {
                return await DecryptString(s, UTF8Encoding.UTF8.GetBytes("alal$%:?>,,,<<<<{sdp_+@!"), UTF8Encoding.UTF8.GetBytes("\\][azpcmd!@#&*$#"));
            }

            public async static Task<string> EncryptString(string plainText, byte[] key, byte[] iv)
            {
                using (Aes aes = Aes.Create())
                {

                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    aes.Key = key;
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                await swEncrypt.WriteAsync(plainText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }

            public async static Task<string> DecryptString(string cipherText, byte[] key, byte[] iv)
            {
                using (Aes aes = Aes.Create())
                {

                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    aes.Key = key;
                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return await srDecrypt.ReadToEndAsync();
                            }
                        }
                    }
                }
            }
        }

        public static string sha256(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = SHA256.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("X2"));
            }

            return builder.ToString();
        }
        public static class HttpContentGet
        {
            /// <summary>
            /// HttpClient实例。
            /// </summary>
            private static HttpClient http_client = new HttpClient();
            /// <summary>
            /// 向指定的URI发送GET请求，并返回字符串。
            /// </summary>
            /// <param name="uri">指定的URI。</param>
            /// <returns>URI返回的字符串。</returns>
            public static async Task<string> get(string uri)
            {
                http_client.Timeout = new TimeSpan(0, 0, 3);
                return await http_client.GetStringAsync(uri);
            }
        }
        public static string UnBase64String(string value)
        {
            if (value == null || value == "")
            {
                return "";
            }
            byte[] bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ToBase64String(string value)
        {
            if (value == null || value == "")
            {
                return "";
            }
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        public static void Delay(int mm)
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            return;
        }
    }
}
