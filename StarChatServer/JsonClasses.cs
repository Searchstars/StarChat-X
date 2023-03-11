using System;
using System.Collections.Generic;
using System.Text;

namespace StarChatServer
{
    public class JsonFriendsList
    {
        public string id { get; set; }
        public string chat_history { get; set; }
    }

    public class JsonGroupsList
    {
        public string id { get; set; }
    }

    public class JsonFriendRequestsList
    {
        public string id { get; set; }
    }

    public class JsonChatHistory
    {
        /// <summary>
        /// msg_send_time 一般是个string timestamp，如果是机器人不显示的话，就写dont_view
        /// </summary>
        public string msg_send_time { get; set; }
        /// <summary>
        /// msgtype 表示信息类型，支持的有 text hyperlink（超链接） image video file（展示下载按钮）
        /// </summary>
        public string msgtype { get; set; }
        /// <summary>
        /// msglink 表示信息的链接 如hyperlink（超链接）的指定URL image图片的URL video视频的URL file文件的URL 不需要就dont_need
        /// </summary>
        public string msglink { get; set; }
        /// <summary>
        /// msgcontent 表示信息的内容 通过文本展示 不需要就dont_need
        /// </summary>
        public string msgcontent { get; set; }
    }

    public class JsonObsceneBlockList
    {
        public string[] blocklist { get; set; }
    }

    public class JsonMemesWarningList
    {
        public string[] blocklist { get; set; }
    }

}
