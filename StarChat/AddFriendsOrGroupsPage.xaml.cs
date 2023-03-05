// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
    }
}
