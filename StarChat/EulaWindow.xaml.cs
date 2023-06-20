using ABI.System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Web.Http;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StarChat
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EulaWindow : Window
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
            rect.Width = 600;
            rect.Height = 900;
            appWindow.Resize(rect);
        }

        private async void set_eula_box()
        {
            EulaBox.Text = await Tools.HttpContentGet.get(StarChatReq.http_or_https + App.chatserverip + "/GetFileShare/geteula");
        }

        private async void set_pcyla_box()
        {
            System.Net.Http.HttpClient http_client_copy = new System.Net.Http.HttpClient();
            http_client_copy.Timeout = new System.TimeSpan(0, 0, 3);
            PcylaBox.Text = await http_client_copy.GetStringAsync(StarChatReq.http_or_https + App.chatserverip + "/GetFileShare/getpcyla");
        }

        public EulaWindow()
        {
            this.InitializeComponent();
            this.Title = "StarChat";
            ExtendsContentIntoTitleBar = true;
            hwnd = WindowNative.GetWindowHandle(this);
            WindowId id = Win32Interop.GetWindowIdFromWindow(hwnd);
            appWindow = AppWindow.GetFromWindowId(id);
            appWindow.Closing += window_close;
            var rect = appWindow.Size;
            rect.Width = 600;
            rect.Height = 900;
            appWindow.Resize(rect);
            this.SizeChanged += window_sizechange;
            LogWriter.LogInfo("����Width��Height������ɣ�Resize���");
            SetTitleBar(AppTitleBar);
            this.AppTitleTextBlock.Text = "StarChat EULA";
            if (Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("AppsUseLightTheme").ToString() == "1")
            {
                LogWriter.LogInfo("�ðɣ�����Ŀǰϵͳʹ�õ���ǳɫģʽ...�л�������ɫ����");
                RootGrid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 243, 243, 243));
                AgreeBtn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 192, 192));
            }
            set_eula_box();
            set_pcyla_box();
            File.WriteAllText("eula_not_agree","true");
            AgreeBtn.IsEnabled = false;
            check_agree_btn_enable();
        }

        private async Task check_agree_btn_enable()
        {
            while (true)
            {
                await Task.Delay(10);
                if (ckbox_eula.IsChecked.Value && ckbox_pcyla.IsChecked.Value)
                {
                    AgreeBtn.IsEnabled = true;
                }
                else
                {
                    AgreeBtn.IsEnabled = false;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var cd = new ContentDialog
            {
                Title = "��...",
                Content = "����д��...Ϊʲô��ֱ������ͬ���أ���doge��",
                CloseButtonText = "�â�",
                DefaultButton = ContentDialogButton.Close
            };
            cd.XamlRoot = this.Content.XamlRoot;
            await cd.ShowAsync();
        }

        private async void AgreeBtn_Click(object sender, RoutedEventArgs e)
        {
            File.Delete("eula_not_agree");
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll",".exe"));
            Environment.Exit(0);
        }
    }
}
