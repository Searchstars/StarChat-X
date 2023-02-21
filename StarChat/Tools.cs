using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace StarChat
{
    public static class Tools
    {
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
