// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;
using Microsoft.Win32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StarChat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatWindowFrame_FriendChat : Page
    {

        private Windows.UI.Text.Core.CoreTextEditContext _editContext;

        public async void InitChatHistory()
        {
            var gethisproto = new ProtobufGetChatHistory
            {
                target = "friend",
                clientuid = RunningDataSave.useruid.ToString(),
                token = RunningDataSave.token,
                targetid = RunningDataSave.chatframe_targetid.ToString()
            };
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(memoryStream, gethisproto);
                var result = await StarChatReq.GetChatHistory(Convert.ToBase64String(memoryStream.ToArray()));

                List<JsonChatHistory> chathis = JsonConvert.DeserializeObject<List<JsonChatHistory>>(result);
                foreach (var item in chathis)
                {
                    if (item.msgtype == "text")
                    {
                        sp_chatcontent.Children.Add(new TextBlock
                        {
                            Text = item.msgcontent,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(30, 20, 0, 0)
                        });
                    }
                    if (item.msgtype == "hyperlink")
                    {
                        sp_chatcontent.Children.Add(new HyperlinkButton
                        {
                            Content = item.msgcontent,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(18, 20, 0, 0),
                            NavigateUri = new Uri(item.msglink)
                        });
                    }
                    if (item.msgtype == "image")
                    {
                        BitmapImage bipm = new BitmapImage();
                        bipm.UriSource = new Uri(item.msglink);
                        sp_chatcontent.Children.Add(new Microsoft.UI.Xaml.Controls.Image
                        {
                            Source = bipm,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(30, 20, 0, 0),
                        });
                    }
                }
            }
            if (Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("AppsUseLightTheme").ToString() == "1")
            {
                LogWriter.LogInfo("好吧，看来目前系统使用的是浅色模式...切换背景颜色咯！");
                SendBtn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 192, 192));
            }
        }

        public ChatWindowFrame_FriendChat()
        {
            this.InitializeComponent();
            InitChatHistory();
        }

        private void Button_Click(object sender, RoutedEventArgs e)//SendFile Onclick
        {
            var cd = new ContentDialog
            {
                Title = "暂未开放",
                Content = "在 \"StarChat X 兼容性测试\" 中，该功能暂时不可用",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            };
            cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
            cd.ShowAsync();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//SendEmoticons Onclick
        {
            var cd = new ContentDialog
            {
                Title = "暂未开放",
                Content = "在 \"StarChat X 兼容性测试\" 中，该功能暂时不可用",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            };
            cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
            cd.ShowAsync();
        }
    }
}
