// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StarChat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddFriendsOrGroupsPage : Page
    {
        public AddFriendsOrGroupsPage()
        {
            this.InitializeComponent();
            RunningDataSave.newreqlist_stackpanel = newfrilist_StackPanel;
        }

        public static int inputuid;

        private void Button_Click(object sender, RoutedEventArgs e)//Search User
        {

            if (SearchUser_TextBox_Uid.Text == "0" || SearchUser_TextBox_Uid.Text == "1")
            {
                var cd = new ContentDialog
                {
                    Title = "无法搜索该UID",
                    Content = "不能添加官方机器人为好友",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                cd.ShowAsync();
            }

            else
            {

                var uidtonameproto = new ProtobufUidToUserName
                {
                    targetid = int.Parse(SearchUser_TextBox_Uid.Text),
                    token = RunningDataSave.token
                };
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(memoryStream, uidtonameproto);
                    if (StarChatReq.GetFriendNameFromId(Convert.ToBase64String(memoryStream.ToArray())).Contains("ERR"))
                    {
                        SearchStatUser.Text = "该用户不存在，请核对uid是否正确";
                        SearchUser_SendReq_Button.IsEnabled= false;
                    }
                    else
                    {
                        SearchStatUser.Text = "用户名：" + StarChatReq.GetFriendNameFromId(Convert.ToBase64String(memoryStream.ToArray()));
                        inputuid = int.Parse(SearchUser_TextBox_Uid.Text);
                        SearchUser_SendReq_Button.IsEnabled = true;
                    }
                }

            }
        }

        private void SearchUser_SendReq_Button_Click(object sender, RoutedEventArgs e)
        {
            var sendaddfriendreqproto = new ProtobufSendAddFriendRequestReq
            {
                targetuid = inputuid,
                token = RunningDataSave.token,
                uid = RunningDataSave.useruid
            };
            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (SearchUser_TextBox_Uid.Text == RunningDataSave.useruid.ToString())
                {
                    var cd = new ContentDialog
                    {
                        Title = "蚌埠住了兄弟们",
                        Content = "真要给自己发好友申请啊，什么寄吧人格分裂症😅",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    };
                    cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                    cd.ShowAsync();
                }
                else
                {
                    ProtoBuf.Serializer.Serialize(memoryStream, sendaddfriendreqproto);
                    var ret = StarChatReq.SendAddFriendRequest(Convert.ToBase64String(memoryStream.ToArray()));
                    LogWriter.LogInfo("发送好友请求ret：" + ret);
                    if (ret.Contains("ok"))
                    {
                        var cd = new ContentDialog
                        {
                            Title = "成功",
                            Content = "好友请求已发送，火速让你朋友同意",
                            CloseButtonText = "OK",
                            DefaultButton = ContentDialogButton.Close
                        };
                        cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                        cd.ShowAsync();
                    }
                }
            }
        }

        private void rootPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LogWriter.LogInfo("添加好友或群组Page Pivot SelectionChanged触发，SelectedIndex为：" + rootPivot.SelectedIndex.ToString());
            if (rootPivot.SelectedIndex == 2)
            {
                var get_reqs_proto = new ProtobufGetMyRequestsReq
                {
                    uid = RunningDataSave.useruid,
                    token = RunningDataSave.token
                };
                using (var memoryStream = new MemoryStream())
                {
                    Serializer.Serialize(memoryStream, get_reqs_proto);
                    var res = StarChatReq.GetMyRequests(Convert.ToBase64String(memoryStream.ToArray()));
                    List<JsonFriendRequestsList> fri_req_list =  Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonFriendRequestsList>>(res);
                    string hexColorCode = "#06ffffff"; // Replace with your HEX color code
                    Windows.UI.Color color = Windows.UI.Color.FromArgb(
                        byte.Parse(hexColorCode.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(hexColorCode.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(hexColorCode.Substring(5, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(hexColorCode.Substring(7, 2), System.Globalization.NumberStyles.HexNumber)
                    );
                    foreach (var i in fri_req_list)
                    {
                        var id_to_name_res = "";
                        var utnproto = new ProtobufUidToUserName
                        {
                            targetid = int.Parse(i.id),
                            token = RunningDataSave.token
                        };
                        using (MemoryStream memoryStream2 = new MemoryStream())
                        {
                            ProtoBuf.Serializer.Serialize(memoryStream2, utnproto);
                            id_to_name_res = StarChatReq.GetFriendNameFromId(Convert.ToBase64String(memoryStream2.ToArray()));
                        }
                        Border bd = new Border
                        {
                            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(10),
                            Child = new Grid
                            {
                                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(color),
                                Height = 75,
                                Width = 7000,
                                Children =
                                    {
                                        new TextBlock
                                        {
                                            Margin = new Microsoft.UI.Xaml.Thickness(25),
                                            Text = "好友请求：",
                                            FontSize = 16
                                        },
                                        new TextBlock
                                        {
                                            Margin = new Microsoft.UI.Xaml.Thickness(102,25,0,0),
                                            Text = "来自 " + id_to_name_res + "（UID：" + i.id + "）",
                                            FontSize = 16
                                        },
                                        new Button
                                        {
                                            Margin = new Microsoft.UI.Xaml.Thickness(860,0,0,0),
                                            Content = "同意",
                                            Tag = "accept_friend_req_uid=" + i.id
                                        },
                                        new Button
                                        {
                                            Margin = new Microsoft.UI.Xaml.Thickness(950,0,0,0),
                                            Content = "拒绝",
                                            Tag = "reject_friend_req_uid=" + i.id
                                        }
                                    }
                            }
                        };
                        RunningDataSave.newreqlist_stackpanel.Children.Add(bd);
                        RunningDataSave.newreqlist_stackpanel.Children.Add(new Image { Height = 20 });
                    }
                }
            }
        }
    }
}
