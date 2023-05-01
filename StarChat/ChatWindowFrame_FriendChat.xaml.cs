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
using Windows.Foundation;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

        public static int meme_content_send_count = 0;

        public static JsonMemesWarningList memes_warn_list = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonMemesWarningList>(File.ReadAllText("memes_warning_list.json"));

        public static JsonObsceneBlockList obscene_warn_list = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonObsceneBlockList>(File.ReadAllText("obscene_text_block_list.json"));

        public static bool all_chathistory_place_ok = false;

        public static double lat_scrollviewer_height = 0;

        public static int nowtargetid = RunningDataSave.chatframe_targetid;

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
                all_chathistory_place_ok = true;
            }
            if (Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("AppsUseLightTheme").ToString() == "1")
            {
                LogWriter.LogInfo("好吧，看来目前系统使用的是浅色模式...切换背景颜色咯！");
                SendBtn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 192, 192));
            }
        }

        public async Task scroll_to_under()
        {
            while (true)
            {
                await Task.Delay(200);
                if (lat_scrollviewer_height != scrollviewer_chatcontent.ExtentHeight && scrollviewer_chatcontent.ExtentHeight > 520)
                {
                    Console.WriteLine("scr_output: " + lat_scrollviewer_height + " != " + scrollviewer_chatcontent.ExtentHeight);
                    while (lat_scrollviewer_height != scrollviewer_chatcontent.ExtentHeight)
                    {
                        scrollviewer_chatcontent.ChangeView(null, scrollviewer_chatcontent.ExtentHeight, null, false);
                        lat_scrollviewer_height = scrollviewer_chatcontent.ExtentHeight;
                    }
                    Console.WriteLine("finish");
                }
                if(nowtargetid != RunningDataSave.chatframe_targetid)
                {
                    Console.WriteLine("scr_break_reason: " + nowtargetid + "!=" + RunningDataSave.chatframe_targetid);
                    break;
                }
            }
        }

        public ChatWindowFrame_FriendChat()
        {
            RunningDataSave.scrollviewer_chatcontent = this.scrollviewer_chatcontent;
            this.InitializeComponent();
            RunningDataSave.friendchatframe_sp_chatcontent = this.sp_chatcontent;
            lat_scrollviewer_height = scrollviewer_chatcontent.ExtentHeight;
            nowtargetid = RunningDataSave.chatframe_targetid;
            InitChatHistory();
            scroll_to_under();
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

        private async void SendBtn_Click(object sender, RoutedEventArgs e)//SendMsg Onclick
        {
            if(ChatSendContentBox.Text == "")
            {
                var cd = new ContentDialog
                {
                    Title = "呃...",
                    Content = "你是否知道，发送空消息是没有意义的？",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                cd.ShowAsync();
            }
            else if(meme_content_send_count >= 3)
            {
                var cd = new ContentDialog
                {
                    Title = "梗含量警告",
                    Content = "你可能是个很喜欢玩梗的人，以至于到处发送此类内容，但请注意，适度玩梗有助于活跃气氛，但一直玩梗很可能会让他人感到厌烦，甚至让不懂这些梗的人对你产生误解，请三思而后行",
                    PrimaryButtonText = "仍然发送",
                    CloseButtonText = "还是算了吧",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                var res_select = await cd.ShowAsync();
                LogWriter.LogInfo("meme warn 用户选择：" + res_select);
                if(res_select == ContentDialogResult.Primary)
                {
                    MsgSender.SendTextToFriend(ChatSendContentBox.Text, RunningDataSave.chatframe_targetid);
                }
            }
            else
            {
                foreach(var i in memes_warn_list.blocklist)
                {
                    if (ChatSendContentBox.Text.Contains(i))
                    {
                        meme_content_send_count ++;
                        break;
                    }
                }
                foreach (var k in obscene_warn_list.blocklist)
                {
                    if (ChatSendContentBox.Text.Contains(k))
                    {
                        var cd = new ContentDialog
                        {
                            Title = "内容警告",
                            Content = "看起来你正在尝试发送一些含有暴力/歧视/色情/政治内容的消息，请慎重决定是否发送，这可能会导致你的账号被封禁",
                            PrimaryButtonText = "仍然发送",
                            CloseButtonText = "还是算了吧",
                            DefaultButton = ContentDialogButton.Close
                        };
                        cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                        var res_select = await cd.ShowAsync();
                        LogWriter.LogInfo("obscene warn 用户选择：" + res_select);
                        if(res_select == ContentDialogResult.Primary)
                        {
                            MsgSender.SendTextToFriend(ChatSendContentBox.Text, RunningDataSave.chatframe_targetid);
                        }
                        break;
                    }
                }
                MsgSender.SendTextToFriend(ChatSendContentBox.Text,RunningDataSave.chatframe_targetid);
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)//图片发送
        {
            /*
            var filePicker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, WindowNative.GetWindowHandle(RunningDataSave.chatwindow_static));
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".gif");
            filePicker.FileTypeFilter.Add(".svg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".webp");
            var file = await filePicker.PickSingleFileAsync();
            LogWriter.LogInfo("用户正在尝试发送图片，选取的图片路径：" + file.Path);
            */
        }

        private async void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)//视频发送
        {
            /*
            var filePicker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, WindowNative.GetWindowHandle(RunningDataSave.chatwindow_static));
            filePicker.FileTypeFilter.Add(".mp4");
            filePicker.FileTypeFilter.Add(".webm");
            filePicker.FileTypeFilter.Add(".mov");
            filePicker.FileTypeFilter.Add(".wmv");
            filePicker.FileTypeFilter.Add(".flv");
            filePicker.FileTypeFilter.Add(".avi");
            var file = await filePicker.PickSingleFileAsync();
            LogWriter.LogInfo("用户正在尝试发送视频，选取的视频路径：" + file.Path);
            */
        }

        private async void MenuFlyoutItem_Click_2(object sender, RoutedEventArgs e)//文件发送
        {
            var filePicker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, WindowNative.GetWindowHandle(RunningDataSave.chatwindow_static));
            filePicker.FileTypeFilter.Add("*");
            var file = await filePicker.PickSingleFileAsync();
            LogWriter.LogInfo("用户正在尝试发送文件，选取的文件路径：" + file.Path);
        }
    }
}
