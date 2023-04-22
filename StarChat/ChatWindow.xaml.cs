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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StarChat
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatWindow : Window
    {

        private IntPtr hwnd;
        private AppWindow appWindow;

        private void window_close(AppWindow a, AppWindowClosingEventArgs e)
        {
            LogWriter.LogInfo("用户关闭登陆窗口，程序退出");
            App.application_exit_event();
        }

        private void window_sizechange(object a, WindowSizeChangedEventArgs e)
        {
            LogWriter.LogInfo("用户正在尝试自行调整窗口大小，正在强制调回...");
            var rect = appWindow.Size;
            rect.Width = 1200;
            rect.Height = 700;
            appWindow.Resize(rect);
        }

        public ChatWindow()
        {
            this.InitializeComponent();
            this.Title = "StarChat";
            ExtendsContentIntoTitleBar = true;
            hwnd = WindowNative.GetWindowHandle(this);
            WindowId id = Win32Interop.GetWindowIdFromWindow(hwnd);
            appWindow = AppWindow.GetFromWindowId(id);
            appWindow.Closing += window_close;
            var rect = appWindow.Size;
            rect.Width = 1200;
            rect.Height = 700;
            appWindow.Resize(rect);
            this.SizeChanged += window_sizechange;
            LogWriter.LogInfo("窗口Width与Height设置完成，Resize完毕");
            SetTitleBar(AppTitleBar);
            if (Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("AppsUseLightTheme").ToString() == "1")
            {
                LogWriter.LogInfo("好吧，看来目前系统使用的是浅色模式...切换背景颜色咯！");
                RootGrid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 243, 243, 243));
            }
            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
            AppTitleTextBlock.Text = "StarChat - Connect the world";
#if DEBUG
            AppTitleTextBlock.Text = "StarChat - Connect the world" + "   [Development Build]";
#endif
            IntPtr hImc = Win32Api.ImmGetContext(hwnd);
            Win32Api.ShowReadingWindow(hImc, true);
            RunningDataSave.chatwindow_static = this;
            RunningDataSave.chatwindow_bar_skp = this.bar_skp;

            var sseproto = new ProtobufSSEConnectReq
            {
                uid = RunningDataSave.useruid,
                token = RunningDataSave.token,
            };
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(memoryStream, sseproto);
                StarChatReq.ConnectSSE(Convert.ToBase64String(memoryStream.ToArray()));
            }
            RunningDataSave.chatwindow_nav_static = this.NavigationViewControl;
            var cd = new ContentDialog
            {
                Title = "欢迎来到 StarChat X 兼容性测试 （Close Beta）",
                Content = "此版本只应用于测试软件的兼容性与基础功能，因此存在很多的bug和已知问题，具体可以有如下几点：\n1. 在多个好友中快速切换会造成内存泄漏，导致程序直接吃掉350M以上内存\n2. 连续多次快速点按发送按钮发送消息（刷屏）时会出现内存泄露的问题，会导致程序直接吃掉400M以上内存\n3. 群组和图片/文件/表情发送功能没做好，本次测试不开放\n4. 有些时候消息滚动条并不会在有新消息时自动滚到底部，可通过切换到另一好友的聊天页面再切回来解决\n\n祝体验愉快，本次测试将开放至3月31日，有问题请及时汇报给开发者",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            };
            cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
            cd.ShowAsync();
        }

        private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            LogWriter.LogInfo("NavigationView SelectionChanged，选择的item tag为" + (string)selectedItem.Tag);
            if ((string)selectedItem.Tag == "fri") contentFrame.Navigate(typeof(FriendsPage));
            else if ((string)selectedItem.Tag == "gp") contentFrame.Navigate(typeof(GroupsPage));
            else if ((string)selectedItem.Tag == "afog") contentFrame.Navigate(typeof(AddFriendsOrGroupsPage));
            else if ((string)selectedItem.Tag == "ab") contentFrame.Navigate(typeof(AboutPage));
            else if ((string)selectedItem.Tag == "Settings") contentFrame.Navigate(typeof(SettingsPage));
        }
    }
}
