using System;
using System.Collections.Generic;
using System.Text;
using Console = Colorful.Console;
using System.Drawing;

namespace StarChat
{
    public static class LogWriter
    {

        public static string now_log_file_name = "log-" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString() + ".txt";

        public static void InitLogWriterStep1()
        {
            System.IO.File.Create(now_log_file_name).Close();
            System.IO.File.WriteAllText(now_log_file_name, "--- LOG BEGIN ---\n");
        }
        public static void InitLogWriterStep2()
        {
            System.IO.File.Delete("latest_log_file.txt");
            System.IO.File.Create("latest_log_file.txt").Close();
            System.IO.File.WriteAllText("latest_log_file.txt", now_log_file_name);
        }
        public static void LogInfo(string content)
        {
            Console.Write("[INFO]",Color.LightBlue);
            Console.Write(" [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] ");
            Console.WriteLine(content);
            System.IO.File.WriteAllText(now_log_file_name,System.IO.File.ReadAllText(now_log_file_name) + "[INFO] [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] " + content + "\n");
        }
        public static void LogWarn(string content)
        {
            Console.Write("[WARN]", Color.LightYellow);
            Console.Write(" [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] ");
            Console.WriteLine(content);
            System.IO.File.WriteAllText(now_log_file_name, System.IO.File.ReadAllText(now_log_file_name) + "[WARN] [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] " + content + "\n");
        }
        public static void LogError(string content)
        {
            Console.Write("[ERROR]", Color.Red);
            Console.Write(" [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] ");
            Console.WriteLine(content);
            System.IO.File.WriteAllText(now_log_file_name, System.IO.File.ReadAllText(now_log_file_name) + "[ERROR] [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] " + content + "\n");
        }
        public static void EndLog()
        {
            System.IO.File.WriteAllText(now_log_file_name, "--- LOG END ---");
        }
    }
}
