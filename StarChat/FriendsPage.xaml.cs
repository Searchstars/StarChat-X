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
using System.Windows.Forms;
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
    public sealed partial class FriendsPage : Page
    {

        public static bool Navigated_ChatFrame = false;

        public async void init_friend_page()
        {
            var getfrilist = new ProtobufGetFriendsList
            {
                uid = RunningDataSave.useruid,
                token = RunningDataSave.token
            };
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(memoryStream, getfrilist);
                var result = await StarChatReq.GetFriendsListReq(Convert.ToBase64String(memoryStream.ToArray()));
                RunningDataSave.friends_list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonFriendsList>>(result);
            }
            foreach (var item in RunningDataSave.friends_list)
            {
                var uidtonameproto = new ProtobufUidToUserName
                {
                    targetid = int.Parse(item.id),
                    token = RunningDataSave.token
                };
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(memoryStream, uidtonameproto);
                    TextBlock txb = new TextBlock()
                    {
                        Text = await StarChatReq.GetFriendNameFromId(Convert.ToBase64String(memoryStream.ToArray())),
                        Margin = new Thickness(0, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    StackPanel sp = new StackPanel()
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = item.id
                    };
                    sp.Children.Add(txb);
                    FriendsListView.Items.Add(sp);
                }
            }
            ChatFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
            ChatFrame.Navigate(typeof(ChatWindowFrame_NoSelectFriend));
        }

        public FriendsPage()
        {
            this.InitializeComponent();
            init_friend_page();
        }

        private void FriendsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FriendsListView.SelectedItem != null)
            {
                StackPanel selecteditem_sp =  (StackPanel)FriendsListView.SelectedItem;
                RunningDataSave.chatframe_type = "friend";
                RunningDataSave.chatframe_targetid = int.Parse(selecteditem_sp.Tag.ToString());
                if (!Navigated_ChatFrame)
                {
                    ChatFrame.Navigate(typeof(ChatWindowFrame_FriendChat));
                    Navigated_ChatFrame = true;
                }
            }
        }
    }
}
