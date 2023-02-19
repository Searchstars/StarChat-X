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
            LogWriter.LogInfo("�û��رյ�½���ڣ������˳�");
            App.application_exit_event();
        }

        private void window_sizechange(object a, WindowSizeChangedEventArgs e)
        {
            LogWriter.LogInfo("�û����ڳ������е������ڴ�С������ǿ�Ƶ���...");
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
            LogWriter.LogInfo("����Width��Height������ɣ�Resize���");
            SetTitleBar(AppTitleBar);
            if (Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("AppsUseLightTheme").ToString() == "1")
            {
                LogWriter.LogInfo("�ðɣ�����Ŀǰϵͳʹ�õ���ǳɫģʽ...�л�������ɫ����");
                RootGrid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 243, 243, 243));
            }
            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
            AppTitleTextBlock.Text = "StarChat - Connect the world";
#if DEBUG
            AppTitleTextBlock.Text = "StarChat - Connect the world" + "   [Development Build]";
#endif
            IntPtr hImc = Win32Api.ImmGetContext(hwnd);
            Win32Api.ShowReadingWindow(hImc,true);
            RunningDataSave.chatwindow_static = this;
        }

        private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            LogWriter.LogInfo("NavigationView SelectionChanged��ѡ���item tagΪ" + (string)selectedItem.Tag);
            if ((string)selectedItem.Tag == "fri") contentFrame.Navigate(typeof(FriendsPage));
            else if ((string)selectedItem.Tag == "gp") contentFrame.Navigate(typeof(GroupsPage));
            else if ((string)selectedItem.Tag == "ab") contentFrame.Navigate(typeof(AboutPage));
            else if ((string)selectedItem.Tag == "Settings") contentFrame.Navigate(typeof(SettingsPage));
        }
    }
}
