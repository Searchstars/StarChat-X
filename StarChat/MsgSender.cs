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
            if(size > threshold)
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
                ProtoBuf.Serializer.Serialize(memoryStream, sendproto);
                FileUploadProgressBAR fswindow = new FileUploadProgressBAR("正在上传：" + new FileInfo(path).Name);
                fswindow.Activate();
                await StarChatReq.SendMessageReq(Convert.ToBase64String(memoryStream.ToArray()),true,RunningDataSave.FileUploadWindow_UploadPGBR,RunningDataSave.FileUploadWindow_FileNameTxb,RunningDataSave.FileUploadWindow_UploadSpeedTxb);
            }
        }
    }
}
