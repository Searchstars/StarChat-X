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
using Windows.UI.ViewManagement;
using System.Windows.Input;

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

        public ChatWindowFrame_FriendChat()
        {
            this.InitializeComponent();
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
                LogWriter.LogInfo("GetChatHistory 的 Protobuf序列化成功，内容：" + Convert.ToBase64String(memoryStream.ToArray()));
                LogWriter.LogInfo("尝试将内容发送到服务器...");
                var result = StarChatReq.GetChatHistory(Convert.ToBase64String(memoryStream.ToArray()));
                LogWriter.LogInfo("注册结果：" + result);
            }
        }
    }
}
