// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System.IO;
using Microsoft.Win32;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.Core;
using Microsoft.Toolkit.Uwp.Notifications;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StarChat
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private IntPtr hwnd;
        private AppWindow appWindow;
        private int register_btn_click_num = 0;
        private bool openchatwindow_close_this_win = false;

        private void window_close(AppWindow a, AppWindowClosingEventArgs e)
        {
            if (!openchatwindow_close_this_win)
            {
                LogWriter.LogInfo("�û��رյ�½���ڣ������˳�");
                App.application_exit_event();
            }
        }

        private void window_sizechange(object a, WindowSizeChangedEventArgs e)
        {
            LogWriter.LogInfo("�û����ڳ������е������ڴ�С������ǿ�Ƶ���...");
            var rect = appWindow.Size;
            rect.Width = 600;
            rect.Height = 350;
            appWindow.Resize(rect);
        }

        public MainWindow()
        {
            this.InitializeComponent();
            RunningDataSave.mainwindow_static = this;
            this.Title = "StarChat";
            ExtendsContentIntoTitleBar = true;
            hwnd = WindowNative.GetWindowHandle(this);
            WindowId id = Win32Interop.GetWindowIdFromWindow(hwnd);
            appWindow = AppWindow.GetFromWindowId(id);
            appWindow.Closing += window_close;
            var rect = appWindow.Size;
            rect.Width = 600;
            rect.Height = 350;
            appWindow.Resize(rect);
            this.SizeChanged += window_sizechange;
            LogWriter.LogInfo("����Width��Height������ɣ�Resize���");
            SetTitleBar(AppTitleBar);
            if(Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("AppsUseLightTheme").ToString() == "1")
            {
                LogWriter.LogInfo("�ðɣ�����Ŀǰϵͳʹ�õ���ǳɫģʽ...�л�������ɫ����");
                RootGrid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255,243,243,243));
                LoginBtn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0,0,103,192));
            }
            AppTitleTextBlock.Text = "StarChat - Connect the world";
#if DEBUG
            AppTitleTextBlock.Text = "StarChat - Connect the world"+ "   [Development Build]";
#endif

            try
            {
                // Requires Microsoft.Toolkit.Uwp.Notifications NuGet package version 7.0 or greater
                new ToastContentBuilder()
                    .AddText("StarChat")
                    .AddText("��ӭ����StarChat��\n�汾�ţ�" + App.appver + "\nRelease Type��" + App.appreleasetype + "\nServer Address��" + App.chatserverip + "\nClient IP��" + RunningDataSave.user_ip_addr + "\n��������������ڲ⣬�������������ַ�벻Ҫй¶��")
                    .Show(); // Not seeing the Show() method? Make sure you have version 7.0, and if you're using .NET 6 (or later), then your TFM must be net6.0-windows10.0.17763.0 or greater
            }
            catch(Exception ex)
            {
                new ToastContentBuilder()
                    .AddText("StarChat Error Report")
                    .AddText("���ֱ������ͼ�ϱ��������ߣ�\n" + ex.ToString())
                    .Show();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)//Login Click
        {
            if(usernameinput.Text == "" || pwdinput.Password == "")
            {
                LogWriter.LogInfo("�û�û���û����������룬����");
                var cd = new ContentDialog
                {
                    Title = "��¼ʧ��",
                    Content = "���ƺ���û�������û���������...���Ƕ�û�䣿",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = this.Content.XamlRoot;
                var result = await cd.ShowAsync();
            }
            else
            {
                var loginproto = new ProtobufLogin
                {
                    username = usernameinput.Text,
                    password = Tools.sha256(pwdinput.Password)
                };
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(memoryStream, loginproto);
                    LogWriter.LogInfo("ClientUserLoginReq �� Protobuf���л��ɹ������ݣ�" + Convert.ToBase64String(memoryStream.ToArray()));
                    LogWriter.LogInfo("���Խ����ݷ��͵�������...");
                    var result = StarChatReq.ClientUserLoginReq(Convert.ToBase64String(memoryStream.ToArray()));
                    LogWriter.LogInfo("��¼�����" + result);
                    if (result.Contains("E-R-R-O-R-M-S-G="))
                    {
                        var cd = new ContentDialog
                        {
                            Title = "��¼ʧ��",
                            Content = "��������ʧ�ܻ�������ѹرգ�������Ϣ��\n" + result.Split("E-R-R-O-R-M-S-G=")[1],
                            CloseButtonText = "OK",
                            DefaultButton = ContentDialogButton.Close
                        };
                        cd.XamlRoot = this.Content.XamlRoot;
                        await cd.ShowAsync();
                    }
                    else if (result.Contains("NO-OK-RETURN-MSG="))
                    {
                        var cd = new ContentDialog
                        {
                            Title = "��¼ʧ��",
                            Content = "" + result.Split("NO-OK-RETURN-MSG=")[1],
                            CloseButtonText = "OK",
                            DefaultButton = ContentDialogButton.Close
                        };
                        cd.XamlRoot = this.Content.XamlRoot;
                        await cd.ShowAsync();
                    }
                    else
                    {
                        LogWriter.LogInfo("test�û������Ⱥ��[0]: " + RunningDataSave.groups_list[0].id);
                        LogWriter.LogInfo("test�û��ĺ���[1]��" + RunningDataSave.friends_list[1].chat_history);
                        LogWriter.LogInfo("test�û������������ƣ�" + RunningDataSave.userchatname);
                        LogWriter.LogInfo("test����˸���ͻ��˵�token��" + RunningDataSave.token);
                        ChatWindow chatwindow = new ChatWindow();
                        chatwindow.Activate();
                        openchatwindow_close_this_win = true;
                        this.Close();
                    }
                }
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)//Register Click
        {
            register_btn_click_num++;
            if (usernameinput.Text == "" || pwdinput.Password  == "")
            {
                LogWriter.LogInfo("�û�û���û����������룬����");
                var cd = new ContentDialog
                {
                    Title = "ע��ʧ��",
                    Content = "���ƺ���û�������û���������...���Ƕ�û�䣿",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = this.Content.XamlRoot;
                var result = await cd.ShowAsync();
            }
            else
            {
                var regproto = new ProtobufRegister
                {
                    username = usernameinput.Text,
                    password = Tools.sha256(pwdinput.Password),
                    regbutton_click_num= register_btn_click_num
                };
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(memoryStream, regproto);
                    LogWriter.LogInfo("ClientUserRegisterReq �� Protobuf���л��ɹ������ݣ�" + Convert.ToBase64String(memoryStream.ToArray()));
                    LogWriter.LogInfo("���Խ����ݷ��͵�������...");
                    var result = StarChatReq.ClientUserRegisterReq(Convert.ToBase64String(memoryStream.ToArray()));
                    LogWriter.LogInfo("ע������" + result);
                    if (result.Contains("E-R-R-O-R-M-S-G="))
                    {
                        var cd = new ContentDialog
                        {
                            Title = "ע��ʧ��",
                            Content = "��������ʧ�ܻ�������ѹرգ�������Ϣ��\n" + result.Split("E-R-R-O-R-M-S-G=")[1],
                            CloseButtonText = "OK",
                            DefaultButton = ContentDialogButton.Close
                        };
                        cd.XamlRoot = this.Content.XamlRoot;
                        await cd.ShowAsync();
                    }
                    else if (result.Contains("NO-OK-RETURN-MSG="))
                    {
                        var cd = new ContentDialog
                        {
                            Title = "ע��ʧ��",
                            Content = "" + result.Split("NO-OK-RETURN-MSG=")[1],
                            CloseButtonText = "OK",
                            DefaultButton = ContentDialogButton.Close
                        };
                        cd.XamlRoot = this.Content.XamlRoot;
                        await cd.ShowAsync();
                    }
                }
            }
        }
    }
}
