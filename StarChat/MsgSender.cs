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
                await StarChatReq.SendMessageReq(Convert.ToBase64String(memoryStream.ToArray()));
            }
        }
    }
}
