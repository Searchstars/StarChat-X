// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StarChat
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {

        public static double appver = 3.01;
        public static string appreleasetype = "alpha";
        public static string chatserverip = "127.0.0.1:8000";//本地调试 127.0.0.1:8000

        public async static void getip()
        {
            try
            {
                RunningDataSave.user_ip_addr = await Tools.HttpContentGet.get("https://zh-hans.ipshu.com/myip_info");
                RunningDataSave.user_ip_addr = RunningDataSave.user_ip_addr.Split("<a href=\"/ipv4/")[1].Split("\"")[0];
                LogWriter.LogInfo("成功获取用户IP地址：" + RunningDataSave.user_ip_addr);
            }
            catch {
                LogWriter.LogError("Network Error");
                Tools.Delay(1000);
                var cd = new ContentDialog
                {
                    Title = "网络错误",
                    Content = "在初始化应用程序时出现网络错误，请检查你的网络连接后再试",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.mainwindow_static.Content.XamlRoot;
                await cd.ShowAsync();
                RunningDataSave.user_ip_addr = "network_error";
#if DEBUG
                LogWriter.LogInfo("Debug免死");
#else
                System.Environment.Exit(0);
#endif
            }
        }

        public static void toasterror(object sender,Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
        {
            args.Handled = true;
            LogWriter.LogError(args.Exception.ToString());
            try
            {
                new ToastContentBuilder()
                        .AddText("StarChat Error Report")
                        .AddText("出现报错，请截图完整信息上报给开发者，若程序无法正常关闭，请使用任务管理器结束进程：\n" + args.Exception.ToString())
                        .Show();
            }
            catch {
                new ToastContentBuilder()
                        .AddText("StarChat Error Report")
                        .AddText("出现致命报错，程序将退出！请截图完整信息上报给开发者，若程序无法正常关闭，请使用任务管理器结束进程！错误信息：\n" + args.Exception.ToString())
                        .Show();
            }
        }

        public static void toasterror_appdomain(object sender, System.UnhandledExceptionEventArgs args)
        {
            LogWriter.LogError(args.ExceptionObject.ToString());
            try
            {
                new ToastContentBuilder()
                        .AddText("StarChat Error Report")
                        .AddText("出现报错，请截图完整信息上报给开发者，若程序无法正常关闭，请使用任务管理器结束进程：\n" + args.ExceptionObject.ToString())
                        .Show();
            }
            catch
            {
                new ToastContentBuilder()
                        .AddText("StarChat Error Report")
                        .AddText("出现致命报错，程序将退出！请截图完整信息上报给开发者，若程序无法正常关闭，请使用任务管理器结束进程！错误信息：\n" + args.ExceptionObject.ToString())
                        .Show();
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.UnhandledException += toasterror;
            AppDomain.CurrentDomain.UnhandledException += toasterror_appdomain;
            try
            {
                LogWriter.InitLogWriterStep1();
                try
                {
                    if (System.IO.File.ReadAllText(System.IO.File.ReadAllText("latest_log_file.txt")).Contains("LOG END") != true)
                    {
                        LogWriter.LogWarn("请注意：上次StarChat似乎未正常关闭！若是有什么导致你必须强制关闭StarChat的问题，请务必汇报哦~");
                    }
                }
                catch (FileNotFoundException e)
                {
                    LogWriter.LogInfo("看起来这是你第一次启动StarChat，可以先看看wiki哦~");
                }
                catch (Exception e)
                {
                    LogWriter.LogError("在判断上一次的log是否正常结尾时出现报错，报错信息：" + e);
                }
                LogWriter.InitLogWriterStep2();
                LogWriter.LogInfo("StarChat开始运行");

                getip();

                this.InitializeComponent();
            }
            catch(Exception e)
            {
                new ToastContentBuilder()
                    .AddText("StarChat Error Report")
                    .AddText("出现报错，请截图上报给开发者：\n" + e.ToString())
                    .Show();
            }
        }

        public static void application_exit_event()
        {
            LogWriter.LogInfo("StarChat正在退出");
            LogWriter.EndLog();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            LogWriter.LogInfo("OnLaunched触发");
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
