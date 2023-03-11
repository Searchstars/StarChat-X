using System;
using System.Collections.Generic;
using System.Text;
using Console = Colorful.Console;
using System.Drawing;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;

namespace StarChat
{
    public static class LogWriter
    {

        public static string now_log_file_name = "log-" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString() + ".txt";

        public async static void InitLogWriterStep1()
        {
            System.IO.File.Create(now_log_file_name).Close();
            System.IO.File.WriteAllText(now_log_file_name, await Tools.AesEncryption.enc_aes_log("--- LOG BEGIN ---\n"));
        }
        public static void InitLogWriterStep2()
        {
            System.IO.File.Delete("latest_log_file.txt");
            System.IO.File.Create("latest_log_file.txt").Close();
            System.IO.File.WriteAllText("latest_log_file.txt", now_log_file_name);
        }
        public async static void LogInfo(string content)
        {
            Console.Write("[INFO]",Color.LightBlue);
            Console.Write(" [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] ");
            Console.WriteLine(content);
            var data = await Tools.AesEncryption.enc_aes_log(System.IO.File.ReadAllText(now_log_file_name) + "[INFO] [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] " + content + "\n");
            using (StreamWriter writer = new StreamWriter(now_log_file_name))
            {
                // Write data to file in chunks
                for (int i = 0; i < data.Length; i += 20)
                {
                    int length = Math.Min(20, data.Length - i);
                    writer.Write(data.Substring(i, length));
                }
            }
        }
        public async static void LogWarn(string content)
        {
            Console.Write("[WARN]", Color.LightYellow);
            Console.Write(" [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] ");
            Console.WriteLine(content);
            var data = await Tools.AesEncryption.enc_aes_log(System.IO.File.ReadAllText(now_log_file_name) + "[WARN] [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] " + content + "\n");
            using (StreamWriter writer = new StreamWriter(now_log_file_name))
            {
                // Write data to file in chunks
                for (int i = 0; i < data.Length; i += 20)
                {
                    int length = Math.Min(20, data.Length - i);
                    writer.Write(data.Substring(i, length));
                }
            }
        }
        public async static void LogError(string content)
        {
            Console.Write("[ERROR]", Color.Red);
            Console.Write(" [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] ");
            Console.WriteLine(content);
            var data = await Tools.AesEncryption.enc_aes_log(System.IO.File.ReadAllText(now_log_file_name) + "[ERROR] [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] " + content + "\n");
            using (StreamWriter writer = new StreamWriter(now_log_file_name))
            {
                // Write data to file in chunks
                for (int i = 0; i < data.Length; i += 20)
                {
                    int length = Math.Min(20, data.Length - i);
                    writer.Write(data.Substring(i, length));
                }
            }
        }
        public async static void EndLog()
        {
            System.IO.File.WriteAllText(now_log_file_name, await Tools.AesEncryption.enc_aes_log("--- LOG END ---"));
        }
    }
}
