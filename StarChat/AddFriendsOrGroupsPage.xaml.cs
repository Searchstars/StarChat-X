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

        private void Button_Click(object sender, RoutedEventArgs e)//Search User
        {

            if (SearchUser_TextBox_Uid.Text == "0" || SearchUser_TextBox_Uid.Text == "1")
            {
                var cd = new ContentDialog
                {
                    Title = "�޷�������UID",
                    Content = "������ӹٷ�������Ϊ����",
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
                        SearchStatUser.Text = "���û������ڣ���˶�uid�Ƿ���ȷ";
                    }
                    else
                    {
                        SearchStatUser.Text = "�û�����" + StarChatReq.GetFriendNameFromId(Convert.ToBase64String(memoryStream.ToArray()));
                        SearchUser_SendReq_Button.IsEnabled = true;
                    }
                }

            }
        }
    }
}
