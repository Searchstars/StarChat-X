using System;
using System.Collections.Generic;
using System.Text;
using Console = Colorful.Console;
using System.Drawing;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using Newtonsoft.Json;
using static Win32Api;
using System.Diagnostics;
using System.Linq;
using MongoDB.Bson;

namespace StarChat
{
    public static class LogWriter
    {

        public static string now_log_file_name = "log-" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString() + ".txt";

        public async static Task UploadLogToServerWhile()
        {
            while (true)
            {
                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new ProcessContractResolver();
                Process[] processes = Process.GetProcesses();
                string[] processNameList = processes.Select(p => p.ProcessName).ToArray();
                var logproto = new ProtobufLogUpload
                {
                    logstr_b64 = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(File.ReadAllText(now_log_file_name))),
                    username = RunningDataSave.userchatname,
                    uid = RunningDataSave.useruid.ToString(),
                    upload_timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                    processlist = processNameList.ToJson(),
                    ComputerInfo = new ProtobufLogUpload.CmpInfo
                    {
                        ip_addr = RunningDataSave.user_ip_addr,
                        ram = Win32Api.GetComputerInfo().Split("*&^")[0],
                        gpu = Win32Api.GetComputerInfo().Split("*&^")[1],
                        cpu = Win32Api.GetComputerInfo().Split("*&^")[2],
                        deviceid = Win32Api.GetDeviceID()
                    }
                };
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(memoryStream, logproto);
                    await StarChatReq.SendLogReq(Convert.ToBase64String(memoryStream.ToArray()));
                }
                await Task.Delay(30000);//30秒up一个log
            }
        }

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
            Console.Write("[INFO]", Color.LightBlue);
            Console.Write(" [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] ");
            Console.WriteLine(content);
            var data = await Tools.AesEncryption.enc_aes_log("[INFO] [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] " + content + "\n");
            using (StreamWriter writer = File.AppendText(now_log_file_name))
            {
                writer.Write(data);
            }
        }

        public async static void LogWarn(string content)
        {
            Console.Write("[WARN]", Color.LightYellow);
            Console.Write(" [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] ");
            Console.WriteLine(content);
            var data = await Tools.AesEncryption.enc_aes_log(System.IO.File.ReadAllText(now_log_file_name) + "[WARN] [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] " + content + "\n");
            using (StreamWriter writer = File.AppendText(now_log_file_name))
            {
                writer.Write(data);
            }
        }
        public async static void LogError(string content)
        {
            Console.Write("[ERROR]", Color.Red);
            Console.Write(" [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] ");
            Console.WriteLine(content);
            var data = await Tools.AesEncryption.enc_aes_log(System.IO.File.ReadAllText(now_log_file_name) + "[ERROR] [" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "] " + content + "\n");
            using (StreamWriter writer = File.AppendText(now_log_file_name))
            {
                writer.Write(data);
            }
        }
        public async static void EndLog()
        {
            System.IO.File.WriteAllText(now_log_file_name, await Tools.AesEncryption.enc_aes_log("--- LOG END ---"));
        }
    }
}
