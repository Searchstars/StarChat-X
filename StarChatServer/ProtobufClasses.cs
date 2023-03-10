using MongoDB.Bson;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarChatServer
{
    [ProtoContract]
    class ProtobufRegister
    {
        [ProtoMember(1)]
        /// <summary>
        /// 输入框内的用户名
        /// </summary>
        public string username { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// 输入框内的密码
        /// </summary>
        public string password { get; set; }
        [ProtoMember(3)]
        /// <summary>
        /// 注册按钮被点击的次数，防刷号
        /// </summary>
        public int regbutton_click_num { get; set; }
    }
    [ProtoContract]
    class ProtobufLogin
    {
        [ProtoMember(1)]
        /// <summary>
        /// 输入框内的用户名
        /// </summary>
        public string username { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// 输入框内的密码
        /// </summary>
        public string password { get; set; }
    }
    [ProtoContract]
    class ProtobufGetUserAccountInfoRes
    {
        [ProtoMember(1)]
        /// <summary>
        /// 用户聊天名
        /// </summary>
        public string userchatname { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// 用户头像url
        /// </summary>
        public string userimageurl { get; set; }
        [ProtoMember(3)]
        /// <summary>
        /// 用户好友列表bsonvalue string
        /// </summary>
        public string friend_list { get; set; }
        [ProtoMember(4)]
        /// <summary>
        /// 用户群组列表bsonvalue string
        /// </summary>
        public string group_list { get; set; }
        [ProtoMember(5)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
        [ProtoMember(6)]
        /// <summary>
        /// 用户uid
        /// </summary>
        public string uid { get; set; }
    }
    [ProtoContract]
    class ProtobufGetChatHistory
    {
        [ProtoMember(1)]
        /// <summary>
        /// 获取聊天记录的对象类型 group/friend 群组或好友
        /// </summary>
        public string target { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// 对象id（好友uid 群组gid）
        /// </summary>
        public string targetid { get; set; }
        [ProtoMember(3)]
        /// <summary>
        // 查询聊天记录的用户的uid
        /// </summary>
        public string clientuid { get; set; }
        [ProtoMember(4)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
    }
    [ProtoContract]
    class ProtobufGidToGroupName
    {
        [ProtoMember(1)]
        /// <summary>
        /// 目标群组gid
        /// </summary>
        public int targetid { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
    }
    [ProtoContract]
    class ProtobufUidToUserName
    {
        [ProtoMember(1)]
        /// <summary>
        /// 目标用户uid
        /// </summary>
        public int targetid { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
    }
    [ProtoContract]
    class ProtobufSSEConnectReq
    {
        [ProtoMember(1)]
        /// <summary>
        /// 向服务端发送SSE连接请求的客户端的登录uid
        /// </summary>
        public int uid { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
    }
    [ProtoContract]
    class ProtobufSendAddFriendRequestReq
    {
        [ProtoMember(1)]
        /// <summary>
        /// 对方uid
        /// </summary>
        public int targetuid { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// 本人uid
        /// </summary>
        public int uid { get; set; }
        [ProtoMember(3)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
    }
    [ProtoContract]
    class ProtobufSendAllowFriendRequestReq
    {
        [ProtoMember(1)]
        /// <summary>
        /// 对方uid
        /// </summary>
        public int targetuid { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// 本人uid
        /// </summary>
        public int uid { get; set; }
        [ProtoMember(3)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
    }
    [ProtoContract]
    class ProtobufSendRejectFriendRequestReq
    {
        [ProtoMember(1)]
        /// <summary>
        /// 对方uid
        /// </summary>
        public int targetuid { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// 本人uid
        /// </summary>
        public int uid { get; set; }
        [ProtoMember(3)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
    }
    [ProtoContract]
    class ProtobufGetMyRequestsReq
    {
        [ProtoMember(1)]
        /// <summary>
        /// 本人uid
        /// </summary>
        public int uid { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
    }
    [ProtoContract]
    class ProtobufGetFriendsList
    {
        [ProtoMember(1)]
        /// <summary>
        /// 本人uid
        /// </summary>
        public int uid { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// token验证
        /// </summary>
        public string token { get; set; }
    }
    [ProtoContract]
    class ProtobufLogUpload
    {
        /// <summary>
        /// base64编码的日志字符串
        /// </summary>
        public string logstr_b64 { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// 上传时的时间戳
        /// </summary>
        public long upload_timestamp { get; set; }
        /// <summary>
        /// 进程列表
        /// </summary>
        public string processlist { get; set; }
        /// <summary>
        /// 用户电脑基本信息
        /// </summary>
        public class cmp_info
        {
            public string ip_addr { get; set; }
            public string ram { get; set; }
            public string gpu { get; set; }
            public string cpu { get; set; }
            public string deviceid { get; set; }
        }
    }
    [ProtoContract]
    class ProtobufMessageSend
    {
        /// <summary>
        /// base64编码的AES或RSA加密消息
        /// </summary>
        public string msg_b64 { get; set; }
        /// <summary>
        /// 消息类型，分别是text，img，vid，bin
        /// </summary>
        public string msg_type { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string username { get; set; }
    }
}
