//由人类赛博科技灵魂集合体：ChatGPT和NewBing编写

using System;
using System.Runtime.InteropServices;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class Win32Api
{
    [DllImport("imm32.dll")]
    public static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("imm32.dll")]
    public static extern bool ImmSetCompositionWindow(IntPtr hImc, ref COMPOSITIONFORM lpCompForm);

    [DllImport("imm32.dll")]
    public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hImc);

    [DllImport("CHxReadingStringIME.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool ShowReadingWindow(IntPtr hwnd, bool bShow);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COMPOSITIONFORM
    {
        public uint dwStyle;
        public POINT ptCurrentPos;
        public RECT rcArea;
    }

    public const uint CFS_POINT = 0x0002;

    public static string GetComputerInfo()
    {
        string ramName = "";
        string gpuName = "";
        string cpuName = "";

        // 获取 RAM 名称
        ManagementObjectSearcher ramSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory");
        foreach (ManagementObject queryObj in ramSearcher.Get())
        {
            ramName = queryObj["Name"].ToString();
            break;
        }

        // 获取 GPU 名称
        ManagementObjectSearcher gpuSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
        foreach (ManagementObject queryObj in gpuSearcher.Get())
        {
            gpuName = queryObj["Name"].ToString();
            break;
        }

        // 获取 CPU 名称
        ManagementObjectSearcher cpuSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
        foreach (ManagementObject queryObj in cpuSearcher.Get())
        {
            cpuName = queryObj["Name"].ToString();
            break;
        }
        
        return ramName + "*&^" + gpuName + "*&^" + cpuName;
    }

    public static string GetHardwareId(string className, string propertyName)
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT * FROM {className}");
        foreach (ManagementObject queryObj in searcher.Get())
        {
            return queryObj[propertyName].ToString();
        }
        return "";
    }

    public static string GetDeviceID()
    {
        // 获取硬件设备的 ID 信息
        string baseBoardId = GetHardwareId("Win32_BaseBoard", "SerialNumber");
        string processorId = GetHardwareId("Win32_Processor", "ProcessorId");
        string biosId = GetHardwareId("Win32_BIOS", "SerialNumber");
        string videoControllerId = GetHardwareId("Win32_VideoController", "PNPDeviceID");
        string diskDriveId = GetHardwareId("Win32_DiskDrive", "PNPDeviceID");

        // 拼接字符串
        string combinedString = baseBoardId + processorId + biosId + videoControllerId + diskDriveId;

        // 计算哈希值
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
            string deviceId = BitConverter.ToString(hashBytes).Replace("-", "");
            return deviceId;
        }
    }

    public class ProcessContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // Find and remove the SafeHandle property
            properties = properties.Where(p => p.PropertyName != "SafeHandle" && p.PropertyName != "Handle" && p.PropertyName != "ExitCode" && p.PropertyName != "HasExited" && p.PropertyName != "StartTime" && p.PropertyName != "ExitTime").ToList();

            return properties;
        }
    }
}