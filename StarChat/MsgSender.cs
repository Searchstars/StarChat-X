using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StarChat
{
    public static class MsgSender
    {
        public async static void SendTextToFriend(string txt, int target_id)
        {
            var sendproto = new ProtobufMessageSend
            {
                msg_b64 = txt,
                msg_type = "text",
                userchatname = RunningDataSave.userchatname,
                target_type = "friend",
                targetid = target_id,
                selfuid = RunningDataSave.useruid,
                token = RunningDataSave.token
            };
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(memoryStream, sendproto);
                await StarChatReq.SendMessageReq(Convert.ToBase64String(memoryStream.ToArray()), false, null,null,null);
            }
        }

        public async static void SendFileToFriend(string path, int target_id, bool is_image, bool is_video)
        {
            if (!RunningDataSave.upload_window_open)
            {
                var msg_type_tmp = "";
                if (is_image)
                {
                    msg_type_tmp = "img";
                }
                else if (is_video)
                {
                    msg_type_tmp = "vid";
                }
                else
                {
                    msg_type_tmp = "bin";
                }
                long size = new FileInfo(path).Length;
                // 定义一个阈值（以字节为单位）
                long threshold = 50 * 1024 * 1024; // 50MB
                if (size > threshold)
                {
                    InfoBarControl.errbar(RunningDataSave.chatwindow_bar_skp, true, "文件发送失败", "不能发送大于50MB的文件哦...这个要求很宽松了吧？");
                    return;
                }
                byte[] bytes = File.ReadAllBytes(path);
                string file64 = Convert.ToBase64String(bytes);
                var sendproto = new ProtobufMessageSend
                {
                    msg_b64 = new FileInfo(path).Name + ">biname^split<" + file64,
                    msg_type = msg_type_tmp,
                    userchatname = RunningDataSave.userchatname,
                    target_type = "friend",
                    targetid = target_id,
                    selfuid = RunningDataSave.useruid,
                    token = RunningDataSave.token
                };
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    RunningDataSave.UIdispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    ProtoBuf.Serializer.Serialize(memoryStream, sendproto);
                    FileUploadProgressBAR fswindow = new FileUploadProgressBAR("正在上传：" + new FileInfo(path).Name);
                    fswindow.Activate();
                    await StarChatReq.SendMessageReq(Convert.ToBase64String(memoryStream.ToArray()), true, RunningDataSave.FileUploadWindow_UploadPGBR, RunningDataSave.FileUploadWindow_FileNameTxb, RunningDataSave.FileUploadWindow_UploadSpeedTxb);
                    InfoBarControl.sucbar(RunningDataSave.chatwindow_bar_skp, true, "成功", "文件上传完毕：" + new FileInfo(path).Name);
                }
            }
            else
            {
                var cd = new ContentDialog
                {
                    Title = "重复打开窗口",
                    Content = "目前已经有一个上传窗口被打开，StarChat目前只支持同时上传单个文件、打开单个上传窗口，若文件上传已完成，请关闭已打开的上传窗口，再上传新文件，否则请耐心等待",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                await cd.ShowAsync();
            }
        }
    }
}
