﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Core.Configuration;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using ProtoBuf;
using SharpCompress.Writers;
using System.Text.Unicode;
using System.Security.Cryptography;
using System.Reflection.Metadata;
using System.Web;

namespace StarChatServer
{
    class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://*:8000/";//监听所有IP 一般不用改
        public static string clientcontent_url = "http://127.0.0.1:8000/";//客户端在获取在线内容（如图片 文件等）的url前缀，一般用于硬写jsonchathistory，也方便迁移
        public static string dburl = "mongodb://127.0.0.1:27017";//本地调试27017
        public static double client_version = 0.3;//客户端最新版本（更新检测）
        public static double client_min_version = 0.3;//客户端最低版本（强制更新检测）
        //public static string dburl = "mongodb://csharpserveradmin:cservpwd@43.152.199.64:27017/?authMechanism=DEFAULT";//数据库url，按照搭建环境进行调整
        public static MongoClient client = new MongoClient(dburl);
        public static IMongoCollection<BsonDocument> dbcollection_test = client.GetDatabase("StarChatServer").GetCollection<BsonDocument>("test");
        public static IMongoCollection<BsonDocument> dbcollection_account = client.GetDatabase("StarChatServer").GetCollection<BsonDocument>("accounts");
        public static IMongoCollection<BsonDocument> dbcollection_groups = client.GetDatabase("StarChatServer").GetCollection<BsonDocument>("groups");
        public static IMongoCollection<BsonDocument> dbcollection_tokens = client.GetDatabase("StarChatServer").GetCollection<BsonDocument>("tokens");
        public static IMongoCollection<BsonDocument> dbcollection_logs = client.GetDatabase("StarChatServer").GetCollection<BsonDocument>("logs");
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static string pageData = "Welcome to StarChat Server";
        public static int now_can_use_uid = 114514;

        public static Dictionary<int,HttpListenerContext> sse_dict = new Dictionary<int,HttpListenerContext>();

        public static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)//随机字符串函数，一般token生成用
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }

        public static byte[] ObjectToBytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // 类型或成员已过时
                formatter.Serialize(ms,obj);
#pragma warning restore SYSLIB0011 // 类型或成员已过时
                return ms.GetBuffer();
            }
        }

        public static void AddNewGroup(IMongoCollection<BsonDocument> dbcollection_groups, string groupname, int group_own_uid)
        {
            int gid = int.Parse(File.ReadAllText("currentgid.txt"));
            var doc = new[]
            {
                new BsonDocument{
                    { "AddTime", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() },
                    { "GroupName", groupname },
                    { "GroupOwnUid", group_own_uid },
                    { "gid", gid},
                    { "members_uid", new BsonArray
                        {
                            0,
                            1
                        } 
                    },
                    { "chathistory_clips" , new BsonArray 
                        { 
                            new BsonDocument{{ "timestamp", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }, {"clip_id",0 } },
                            new BsonDocument{{ "timestamp", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + 2 }, {"clip_id",1 } }
                        } 
                    },
                    { "chathistorys", new BsonArray
                        {
                            new BsonDocument{ { "clip_id",0 }, { "chathistory_json", "[{\"msg_send_time\": \"dont_view\",\"msgtype\":\"placeholder\",\"msglink\":\"dont_need\",\"msgcontent\":\"\"}]" } }
                        } 
                    }
                }
            };
            dbcollection_groups.InsertMany(doc);
            gid++;
            File.WriteAllText("currentgid.txt",gid.ToString());
        }

        public static void AddNewUser(IMongoCollection<BsonDocument> dbcollection_account, string username, string password, int uid)
        {
            var doc = new[]
            {
                new BsonDocument{
                    { "RegTime", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() },
                    { "username", username },
                    { "password", password },
                    { "uid", uid },
                    { "chatname", "用户_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString() },
                    { "isFinishFirstLoginSettings", "no" },
                    { "ban", "no" },
                    { "banreason", "该账号未被封禁，若您看到了这段信息，请联系开发者" },
                    { "JoinedGroups", new BsonArray
                        {
                            new BsonDocument{ { "id", 0 }},
                        }
                    },
                    { "Friends", new BsonArray
                        {
                            new BsonDocument{ { "id", 0 }, { "chat_history", "[{\"msg_send_time\": \"dont_view\",\"msgtype\":\"text\",\"msglink\":\"dont_need\",\"msgcontent\":\"shhhh...看起来，这里有一个新来的？\n别再等啦！查看官方wiki并加入官方群组，开始你的StarChat之旅！\nWelcome to StarChat !\"}]" } },
                            new BsonDocument{ { "id", 1 }, { "chat_history", "[{\"msg_send_time\": \"dont_view\",\"msgtype\":\"text\",\"msglink\":\"dont_need\",\"msgcontent\":\"哈喽哇？不知道怎么使用？给我发个/help吧！当然，你也可以查看：\"},{\"msg_send_time\": \"dont_view\",\"msgtype\":\"hyperlink\",\"msglink\":\"https://chat.stargazing.studio/documents\",\"msgcontent\":\"官方文档\"}]" } }
                        }
                    },
                    { "FriendRequests" , new BsonArray
                        {

                        }
                    },
                    {"BindServices", new BsonArray
                        {
                            new BsonDocument{ { "name", "StarNetwork" }, {"stat","wait_to_bind" }, {"service_data_json","{}" } }
                        } 
                    }
                }
            };
            dbcollection_account.InsertMany(doc);
        }


        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    byte[] data = Encoding.UTF8.GetBytes("我草，什么Simple Http Server攻击");
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }

                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/"))
                {
                    // Write the response info
                    string disableSubmit = !runServer ? "disabled" : "";
                    byte[] data = Encoding.UTF8.GetBytes(pageData);
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
                else if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath.Contains("/GetFileShare")))
                {
                    FileShareSerivce(req, resp);
                }
                else if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/ClientUserLoginReq"))
                {
                    LoginPostReqAsync(req, resp);
                }
                else if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/ClientUserRegisterReq"))
                {
                    RegisterPostReqAsync(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetFriendNameFromId")
                {
                    GetFriendNameFromIdAsync(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetGroupNameFromId")
                {
                    GetGroupNameFromIdAsync(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetChatHistory")
                {
                    GetChatHistoryAsync(req, resp);
                }
                else if ((req.Headers["Accept"] == "text/event-stream") && (req.Url.AbsolutePath == "/ListenMsg"))
                {
                    SSE_ListenMsgReqAsync(ctx);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/AddFriendReq")
                {
                    AddFriendReqAsync(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetMyRequestsReq")
                {
                    GetMyRequestsReq(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/AllowFriendRequest")
                {
                    AllowFriendRequest(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/RejectFriendRequest")
                {
                    RejectFriendRequest(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetFriendsList")
                {
                    GetFriendsList(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetGroupsList")
                {
                    GetGroupsList(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/SendMessageReq")
                {
                    SendMsg(req, resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/clientlog_starchat_winui3_desktop")
                {
                    RecvLog(req, resp);
                }
                else
                {
                    // Write the response info
                    string disableSubmit = !runServer ? "disabled" : "";
                    byte[] data = Encoding.UTF8.GetBytes("Congratulations on finding the address of the StarChat server, but please do not visit a non-existent page :(<br/>Why not use DnSpy to decompile StarChat first? :)<br/>Fun fact: StarChat was actually an open-source software, but its code was so bad :( you should understand.<br/><br/><image style=\"color: black;width: 600;height: 2; background-color: black\"/><br/>Simple C# HTTP Server For StarChat 20230310<br/>" + req.UserAgent + "<br/>" + req.RawUrl);
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;

                    resp.ContentLength64 = data.LongLength;
                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
            }
        }

        static async Task FileShareSerivce(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var get_filename = HttpUtility.UrlDecode(req.Url.AbsolutePath.Split('/')[2],Encoding.UTF8);
            Console.WriteLine("FileShareService: TryGet (URL Decode) =" + get_filename);
            try
            {
                if (get_filename == "geteula")
                {
                    var dataBytes = Encoding.UTF8.GetBytes(File.ReadAllText("./EULA_RESP.txt"));
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = dataBytes.LongLength;
                    resp.OutputStream.Write(dataBytes, 0, dataBytes.Length);
                    resp.Close();
                }
                else if (get_filename == "getpcyla")
                {
                    var dataBytes = Encoding.UTF8.GetBytes(File.ReadAllText("./PCYLA_RESP.txt"));
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = dataBytes.LongLength;
                    resp.OutputStream.Write(dataBytes, 0, dataBytes.Length);
                    resp.Close();
                }
                else if(get_filename == "getver")
                {
                    var dataBytes = Encoding.UTF8.GetBytes(client_version.ToString());
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = dataBytes.LongLength;
                    resp.OutputStream.Write(dataBytes, 0, dataBytes.Length);
                    resp.Close();
                }
                else if(get_filename == "getminver")
                {
                    var dataBytes = Encoding.UTF8.GetBytes(client_min_version.ToString());
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = dataBytes.LongLength;
                    resp.OutputStream.Write(dataBytes, 0, dataBytes.Length);
                    resp.Close();
                }
                else
                {
                    var dataBytes = File.ReadAllBytes("./GetFileShare/" + get_filename);
                    resp.ContentType = "application/octet-stream";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = dataBytes.LongLength;
                    resp.OutputStream.Write(dataBytes, 0, dataBytes.Length);
                    resp.Close();
                }
            }
            catch
            {
                SetResponseContent(resp, "别扫了，再扫解析gov.cn");
            }
        }

        static async Task RecvLog(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var requestData = await ReadRequestData(req);
            var request = Serializer.Deserialize<ProtobufLogUpload>(new MemoryStream(requestData));

            var doc = new[]
            {
                new BsonDocument{
                    { "logstr_b64", request.logstr_b64},
                    { "username", request.username },
                    { "uid", request.uid },
                    { "upload_timestamp", request.upload_timestamp },
                    { "processlist", request.processlist },
                    { "ComputerInfo",new BsonDocument
                        {
                            { "ip_addr", request.ComputerInfo.ip_addr},
                            { "ram", request.ComputerInfo.ram},
                            { "gpu", request.ComputerInfo.gpu},
                            { "cpu", request.ComputerInfo.cpu},
                            { "deviceid", request.ComputerInfo.deviceid},
                        }
                    }
                }
            };
            dbcollection_logs.InsertMany(doc);
            SetResponseContent(resp,"success>^<ok");
        }

        static async Task SendMsg(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var requestData = await ReadRequestData(req);
            var request = Serializer.Deserialize<ProtobufMessageSend>(new MemoryStream(requestData));

            var tokenExists = await CheckTokenExists(request.token);

            Console.WriteLine("SendMsg");

            if (!tokenExists)
            {
                SetResponseContent(resp, "你说得对，但是token error");
                return;
            }
            if(request.target_type == "group")
            {
                var filter = Builders<BsonDocument>.Filter.Eq("gid", request.targetid);//Object reference not set to an instance of an object.报错原因：target错写成client
                var result = await dbcollection_groups.Find(filter).FirstOrDefaultAsync();
                if (result != null)
                {}
                else
                {
                    SetResponseContent(resp, "Server Error");
                    return;
                }
                string json_chat_history_string = "";
                int last_clip_id = 0;
                foreach (var clip_ in result["chathistorys"].AsBsonArray)
                {
                    json_chat_history_string = (string)clip_["chathistory_json"];//操你妈的BsonArray拿不到Length只能开For拿末尾json了
                    last_clip_id = (int)clip_["clip_id"];
                }
                var json_chat_history = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonChatHistory>>(json_chat_history_string);
                var ser_json = new JsonChatHistory{};
                if (request.msg_type == "text")
                {
                    ser_json = new JsonChatHistory
                    {
                        msg_send_time = "dont_view",
                        msgtype = "text",
                        msglink = "dont_need",
                        msgcontent = request.userchatname + ": " + request.msg_b64
                    };
                }
                else if (request.msg_type == "bin" || request.msg_type == "vid" || request.msg_type == "img")
                {
                    if (!Directory.Exists("GetFileShare"))
                    {
                        Directory.CreateDirectory("GetFileShare");
                    }

                    var filesavename = "GetFileShare/" + GetRandomString(40, true, true, true, false, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ") + "__}&origname&{__" + request.msg_b64.Split(new string[] { ">biname^split<" }, StringSplitOptions.None)[0];
                    Console.WriteLine("bin-checked");
                    //将base64字符串转换为字节数组
                    byte[] bytes = Convert.FromBase64String(request.msg_b64.Split(new string[] { ">biname^split<" }, StringSplitOptions.None)[1]);
                    //.WriteLine("readed getb64=" + request.msg_b64.Split(new string[] { ">biname^split<" }, StringSplitOptions.None)[1]);
                    Console.WriteLine("./" + filesavename);
                    //将字节数组写入文件
                    File.WriteAllBytes("./" + filesavename, bytes);
                    Console.WriteLine("writed");
                    string pre_msgtype = "hyperlink";
                    if (request.msg_type == "vid")
                    {
                        pre_msgtype = "video";
                    }
                    else if (request.msg_type == "img")
                    {
                        pre_msgtype = "image";
                    }
                    ser_json = new JsonChatHistory
                    {
                        msg_send_time = "dont_view",
                        msgtype = pre_msgtype,
                        msglink = clientcontent_url + filesavename,
                        msgcontent = "来自 " + request.userchatname + " 的文件: " + request.msg_b64.Split(new string[] { ">biname^split<" }, StringSplitOptions.None)[0]//Split字符串方法by newbing
                    };
                }
                json_chat_history.Add(ser_json);
                var update = Builders<BsonDocument>.Update.Set("chathistorys.$[elem].chathistory_json", json_chat_history.ToJson());
                var arrayFilters = new List<ArrayFilterDefinition>
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem.clip_id", last_clip_id))
                };
                dbcollection_groups.UpdateOne(filter, update, new UpdateOptions { ArrayFilters = arrayFilters });
                foreach (int memberuid in result["members_uid"].AsBsonArray)
                {
                    if (sse_dict.ContainsKey(memberuid))
                    {
                        var writer = new StreamWriter(sse_dict[memberuid].Response.OutputStream);
                        var message = $"data: {"newgrpmsg>" + request.targetid.ToString() + ">" + (Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(ser_json.ToJson())))}\n\n";
                        await writer.WriteAsync(message);
                        await writer.FlushAsync();
                    }//挨个发SSE
                }
                SetResponseContent(resp, "success>^<ok");
            }
            else if(request.target_type == "friend")//下面屎山别动 随时可能崩
            {

                List<JsonChatHistory> chathis_list = new List<JsonChatHistory>();

                var filter_对方 = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("uid", request.targetid),
                    Builders<BsonDocument>.Filter.ElemMatch("Friends", Builders<BsonDocument>.Filter.Eq("id",request.selfuid)));
                var filter_自己 = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("uid", request.selfuid),
                    Builders<BsonDocument>.Filter.ElemMatch("Friends", Builders<BsonDocument>.Filter.Eq("id", request.targetid)));
                var update = Builders<BsonDocument>.Update.Set("Friends.$.chat_history", "{\"msg\": \"abca\"}");//这里初始化是因为不在这里初始化下面就用不了了
                var projection = Builders<BsonDocument>.Projection.Include("Friends.$");
                var result = dbcollection_account.Find(filter_自己).Project(projection).FirstOrDefault();
                if (result != null)
                {
                    var chatHistory = result["Friends"][0]["chat_history"].AsString;
                    chathis_list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonChatHistory>>(chatHistory);
                }
                else
                {
                    SetResponseContent(resp,"Server Error");
                }
                if (request.msg_type == "text")
                {
                    chathis_list.Add(new JsonChatHistory
                    {
                        msg_send_time = "dont_view",
                        msgtype = "text",
                        msglink = "dont_need",
                        msgcontent = request.userchatname + ": " + request.msg_b64
                    });
                    update = Builders<BsonDocument>.Update.Set("Friends.$.chat_history", chathis_list.ToJson());
                    dbcollection_account.UpdateOne(filter_对方, update);
                    dbcollection_account.UpdateOne(filter_自己, update);
                    if (sse_dict.ContainsKey(request.targetid))
                    {
                        var writer = new StreamWriter(sse_dict[request.targetid].Response.OutputStream);
                        var message = $"data: {"newfrimsg>" + request.selfuid.ToString() + ">" + (Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(new JsonChatHistory
                        {
                            msg_send_time = "dont_view",
                            msgtype = "text",
                            msglink = "dont_need",
                            msgcontent = request.userchatname + ": " + request.msg_b64
                        }.ToJson())))}\n\n";
                        await writer.WriteAsync(message);
                        await writer.FlushAsync();
                    }

                    if (sse_dict.ContainsKey(request.selfuid))
                    {
                        var writer = new StreamWriter(sse_dict[request.selfuid].Response.OutputStream);
                        var message = $"data: {"newfrimsg>" + request.targetid.ToString() + ">" + (Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(new JsonChatHistory
                        {
                            msg_send_time = "dont_view",
                            msgtype = "text",
                            msglink = "dont_need",
                            msgcontent = request.userchatname + ": " + request.msg_b64
                        }.ToJson())))}\n\n";
                        await writer.WriteAsync(message);
                        await writer.FlushAsync();
                    }
                }
                else if (request.msg_type == "bin" || request.msg_type == "vid" || request.msg_type == "img")
                {
                    if (!Directory.Exists("GetFileShare"))
                    {
                        Directory.CreateDirectory("GetFileShare");
                    }

                    var filesavename = "GetFileShare/" + GetRandomString(40, true, true, true, false, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ") + "__}&origname&{__" + request.msg_b64.Split(new string[] { ">biname^split<" }, StringSplitOptions.None)[0];
                    Console.WriteLine("bin-checked");
                    //将base64字符串转换为字节数组
                    byte[] bytes = Convert.FromBase64String(request.msg_b64.Split(new string[] { ">biname^split<" }, StringSplitOptions.None)[1]);
                    //.WriteLine("readed getb64=" + request.msg_b64.Split(new string[] { ">biname^split<" }, StringSplitOptions.None)[1]);
                    Console.WriteLine("./" + filesavename);
                    //将字节数组写入文件
                    File.WriteAllBytes("./" + filesavename, bytes);
                    Console.WriteLine("writed");
                    string pre_msgtype = "hyperlink";
                    if(request.msg_type == "vid")
                    {
                        pre_msgtype = "video";
                    }
                    else if (request.msg_type == "img")
                    {
                        pre_msgtype = "image";
                    }
                    JsonChatHistory new_jchis = new JsonChatHistory{
                        msg_send_time = "dont_view",
                        msgtype = pre_msgtype,
                        msglink = clientcontent_url + filesavename,
                        msgcontent = "来自 " + request.userchatname + " 的文件: " + request.msg_b64.Split(new string[] { ">biname^split<" }, StringSplitOptions.None)[0]//Split字符串方法by newbing
                    };
                    chathis_list.Add(new_jchis);
                    update = Builders<BsonDocument>.Update.Set("Friends.$.chat_history", chathis_list.ToJson());
                    dbcollection_account.UpdateOne(filter_对方, update);
                    dbcollection_account.UpdateOne(filter_自己, update);
                    Console.WriteLine("updated");
                    if (sse_dict.ContainsKey(request.targetid))
                    {
                        var writer = new StreamWriter(sse_dict[request.targetid].Response.OutputStream);
                        var message = $"data: {"newfrimsg>" + request.selfuid.ToString() + ">" + (Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(new_jchis.ToJson())))}\n\n";
                        await writer.WriteAsync(message);
                        await writer.FlushAsync();
                    }

                    if (sse_dict.ContainsKey(request.selfuid))
                    {
                        var writer = new StreamWriter(sse_dict[request.selfuid].Response.OutputStream);
                        var message = $"data: {"newfrimsg>" + request.targetid.ToString() + ">" + (Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(new_jchis.ToJson())))}\n\n";
                        await writer.WriteAsync(message);
                        await writer.FlushAsync();
                    }
                    Console.WriteLine("sended");
                }

                SetResponseContent(resp,"success>^<ok");
            }
        }

        static async Task GetFriendsList(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var requestData = await ReadRequestData(req);
            var request = Serializer.Deserialize<ProtobufGetFriendsList>(new MemoryStream(requestData));

            var tokenExists = await CheckTokenExists(request.token);

            if (!tokenExists)
            {
                SetResponseContent(resp, "你说得对，但是token error");
                return;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("uid", request.uid);
            var result = await dbcollection_account.Find(filter).FirstOrDefaultAsync();

            SetResponseContent(resp, "success>^<" + result["Friends"].ToJson());
        }

        static async Task GetGroupsList(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var requestData = await ReadRequestData(req);
            var request = Serializer.Deserialize<ProtobufGetGroupsList>(new MemoryStream(requestData));

            var tokenExists = await CheckTokenExists(request.token);

            if (!tokenExists)
            {
                SetResponseContent(resp, "你说得对，但是token error");
                return;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("uid", request.uid);
            var result = await dbcollection_account.Find(filter).FirstOrDefaultAsync();

            SetResponseContent(resp, "success>^<" + result["JoinedGroups"].ToJson());
        }

        static async Task AllowFriendRequest(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var requestData = await ReadRequestData(req);
            var request = Serializer.Deserialize<ProtobufSendAllowFriendRequestReq>(new MemoryStream(requestData));

            var tokenExists = await CheckTokenExists(request.token);

            if (!tokenExists)
            {
                SetResponseContent(resp, "success>^<" + "你说得对，但是token error");
                return;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("uid", request.targetuid);
            var update = Builders<BsonDocument>.Update.AddToSet("Friends", new BsonDocument { { "id", request.uid }, { "chat_history", "[{\"msg_send_time\": \"dont_view\",\"msgtype\":\"placeholder\",\"msglink\":\"dont_need\",\"msgcontent\":\"\"}]" } });
            await dbcollection_account.UpdateOneAsync(filter, update);

            filter = Builders<BsonDocument>.Filter.Eq("uid", request.uid);
            update = Builders<BsonDocument>.Update.AddToSet("Friends", new BsonDocument { { "id", request.targetuid }, { "chat_history", "[{\"msg_send_time\": \"dont_view\",\"msgtype\":\"placeholder\",\"msglink\":\"dont_need\",\"msgcontent\":\"\"}]" } });
            await dbcollection_account.UpdateOneAsync(filter, update);

            var filter_r = Builders<BsonDocument>.Filter.Eq("uid", request.uid);
            var update_r = Builders<BsonDocument>.Update.Pull("FriendRequests", new BsonDocument ("id",request.targetuid));
            await dbcollection_account.UpdateOneAsync(filter_r, update_r);

            SetResponseContent(resp,"success>^<ok");
        }

        static async Task RejectFriendRequest(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var requestData = await ReadRequestData(req);
            var request = Serializer.Deserialize<ProtobufSendRejectFriendRequestReq>(new MemoryStream(requestData));

            var tokenExists = await CheckTokenExists(request.token);

            if (!tokenExists)
            {
                SetResponseContent(resp, "你说得对，但是token error");
                return;
            }

            var filter_r = Builders<BsonDocument>.Filter.Eq("uid", request.uid);
            var update_r = Builders<BsonDocument>.Update.Pull("FriendRequests", new BsonDocument("id", request.targetuid));
            await dbcollection_account.UpdateOneAsync(filter_r, update_r);

            SetResponseContent(resp, "success>^<ok");
        }

        private static async Task<byte[]> ReadRequestData(HttpListenerRequest req)
        {
            using (var body = req.InputStream)
            using (var reader = new StreamReader(body, req.ContentEncoding))
            {
                return Convert.FromBase64String(AesEncryption.dec_aes_normal(await reader.ReadToEndAsync()));
            }
        }

        private static async Task<bool> CheckTokenExists(string token)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("tk", token);
            var projection = Builders<BsonDocument>.Projection.Include("tk");
            var result = await dbcollection_tokens.Find(filter).Project(projection).FirstOrDefaultAsync();
            return result != null;
        }

        private static void SetResponseContent(HttpListenerResponse resp, string content)
        {
            content = AesEncryption.enc_aes_normal(content);
            var dataBytes = Encoding.UTF8.GetBytes(content);
            resp.ContentType = "text/html";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = dataBytes.LongLength;
            resp.OutputStream.Write(dataBytes, 0, dataBytes.Length);
            resp.Close();
        }

        static async Task GetMyRequestsReq(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var requestData = await ReadRequestData(req);
            var request = Serializer.Deserialize<ProtobufGetMyRequestsReq>(new MemoryStream(requestData));

            var tokenExists = await CheckTokenExists(request.token);

            if (!tokenExists)
            {
                SetResponseContent(resp,"你说得对，但是token error");
                return;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("uid", request.uid);
            var result = await dbcollection_account.Find(filter).FirstOrDefaultAsync();
            SetResponseContent(resp, "success>^<" + result["FriendRequests"].ToJson());
        }

        static async Task AddFriendReqAsync(HttpListenerRequest req, HttpListenerResponse resp)
        {
            try
            {
                var requestData = await ReadRequestData(req);
                var request = Serializer.Deserialize<ProtobufSendAddFriendRequestReq>(new MemoryStream(requestData));

                var tokenExists = await CheckTokenExists(request.token);
                if (!tokenExists)
                {
                    throw new Exception("Token not found");
                }

                if (sse_dict.ContainsKey(request.targetuid))
                {
                    var writer = new StreamWriter(sse_dict[request.targetuid].Response.OutputStream);
                    var message = $"data: {"newaddfriendreq>" + request.uid.ToString() + ">" + "nomsg"}\n\n";
                    await writer.WriteAsync(message);
                    await writer.FlushAsync();
                }

                var filter = Builders<BsonDocument>.Filter.Eq("uid", request.targetuid);
                var update = Builders<BsonDocument>.Update.AddToSet("FriendRequests", new BsonDocument("id", request.uid));
                await dbcollection_account.UpdateOneAsync(filter, update);

                SetResponseContent(resp, "success>^<ok");
            }
            catch (Exception ex)
            {
                SetResponseContent(resp, "error>^<" + ex.Message);
            }
        }

        static async Task GetChatHistoryAsync(HttpListenerRequest req, HttpListenerResponse resp)
        {
            try
            {
                var requestData = await ReadRequestData(req);
                var request = Serializer.Deserialize<ProtobufGetChatHistory>(new MemoryStream(requestData));

                Console.WriteLine($"GetChatHistory  TargetId：{request.targetid} Token: {request.token} Target: {request.target}");

                var tokenExists = await CheckTokenExists(request.token);
                if (!tokenExists)
                {
                    throw new Exception("Token not found");
                }

                if (request.target == "group")
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("gid", int.Parse(request.targetid));//Object reference not set to an instance of an object.报错原因：target错写成client
                    var result = await dbcollection_groups.Find(filter).FirstOrDefaultAsync();

                    foreach (var clip_ in result["chathistorys"].AsBsonArray)
                    {
                        Console.WriteLine("遍历中 clip_id=" + clip_["clip_id"]);
                        if (clip_["clip_id"] == request.clip_id)
                        {
                            SetResponseContent(resp, $"success>^<{clip_["chathistory_json"]}");
                        }
                    }
                }
                else if (request.target == "friend")
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("uid", int.Parse(request.clientuid));
                    var result = await dbcollection_account.Find(filter).FirstOrDefaultAsync();

                    foreach (var friend in result["Friends"].AsBsonArray)
                    {
                        if (friend["id"].ToString() == request.targetid)
                        {
                            var chatname = result?["chatname"]?.AsString ?? "";
                            SetResponseContent(resp, $"success>^<{friend["chat_history"]}");
                            return;
                        }
                    }

                    throw new Exception("Friend not found");
                }
                else
                {
                    throw new Exception("Unsupported target type");
                }
            }
            catch (Exception ex)
            {
                SetResponseContent(resp, "error>^<" + ex.Message);
            }
        }


        static async Task GetFriendNameFromIdAsync(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var data_poststr = await ReadRequestDataStr(req);
            var data = Convert.FromBase64String(data_poststr);

            var request = Serializer.Deserialize<ProtobufUidToUserName>(new MemoryStream(data));
            Console.WriteLine($"GetFriendNameFromId  TargetId：{request.targetid} Token: {request.token}");

            var tokenExists = await CheckTokenExists(request.token);
            if (!tokenExists)
            {
                SetResponseContent(resp, "eheheh_token_err");
                return;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("uid", request.targetid);
            var projection = Builders<BsonDocument>.Projection.Include("chatname");
            var result = await dbcollection_account.Find(filter).Project(projection).FirstOrDefaultAsync();

            var chatname = result?["chatname"]?.AsString ?? "";
            if (chatname == null || chatname == "")
            {
                SetResponseContent(resp, "eheheh_user_not_found");
                return;
            }

            SetResponseContent(resp, $"success>^<{chatname}");
        }

        static async Task GetGroupNameFromIdAsync(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var data_poststr = await ReadRequestDataStr(req);
            var data = Convert.FromBase64String(data_poststr);

            var request = Serializer.Deserialize<ProtobufGidToGroupName>(new MemoryStream(data));
            Console.WriteLine($"GetGroupNameFromId  TargetId：{request.targetid} Token: {request.token}");

            var tokenExists = await CheckTokenExists(request.token);
            if (!tokenExists)
            {
                SetResponseContent(resp, "eheheh_token_err");
                return;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("gid", request.targetid);
            var projection = Builders<BsonDocument>.Projection.Include("GroupName");
            var result = await dbcollection_groups.Find(filter).Project(projection).FirstOrDefaultAsync();

            var GroupName = result?["GroupName"]?.AsString ?? "";
            if (GroupName == null || GroupName == "")
            {
                SetResponseContent(resp, "eheheh_user_not_found");
                return;
            }

            SetResponseContent(resp, $"success>^<{GroupName}");
        }

        private static async Task<string> ReadRequestDataStr(HttpListenerRequest req)
        {
            using (var body = req.InputStream)
            using (var reader = new StreamReader(body, req.ContentEncoding))
            {
                return AesEncryption.dec_aes_normal(await reader.ReadToEndAsync());
            }
        }

        static async Task RegisterPostReqAsync(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var requestData = await ReadRequestData(req);
            var request = Serializer.Deserialize<ProtobufRegister>(new MemoryStream(requestData));

            Console.WriteLine($"ClientUserRegisterReq  Username: {request.username} Password: {request.password} RegisterBtnClickNum: {request.regbutton_click_num}");

            if (request.regbutton_click_num > 2)
            {
                SetResponseContent(resp, "注册次数过多，请勿刷号");
                return;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("username", request.username);
            var result = await dbcollection_account.Find(filter).FirstOrDefaultAsync();
            if (result != null)
            {
                SetResponseContent(resp, "该用户已存在");
                return;
            }

            AddNewUser(dbcollection_account, request.username, request.password, now_can_use_uid);
            now_can_use_uid++;
            File.WriteAllText("currentuid.txt", now_can_use_uid.ToString());

            SetResponseContent(resp, "success");
        }

        static async Task LoginPostReqAsync(HttpListenerRequest req, HttpListenerResponse resp)
        {
            using (var body = req.InputStream)
            using (var reader = new StreamReader(body, req.ContentEncoding))
            {
                var requestData = await ReadRequestData(req);
                var request = Serializer.Deserialize<ProtobufLogin>(new MemoryStream(requestData));
                Console.WriteLine($"ClientUserLoginReq  Username: {request.username} Password: {request.password}");

                var filter = Builders<BsonDocument>.Filter.Eq("username", request.username);
                var result = await dbcollection_account.Find(filter).FirstOrDefaultAsync();

                if (result == null || result["password"] != request.password)
                {
                    SetResponseContent(resp, "用户名或密码错误");
                    return;
                }

                if (result["ban"] == "forever")
                {
                    SetResponseContent(resp, $"您的账号已被永久封禁，原因：{result["banreason"]}");
                    return;
                }

                if (result["ban"] != "forever" && result["ban"] != "no")
                {
                    SetResponseContent(resp, $"您的账号已被封禁到{result["ban"]}，原因：{result["banreason"]}");
                    return;
                }

                if (result["ban"] == "no")
                {
                    var acinfoproto = new ProtobufGetUserAccountInfoRes
                    {
                        userchatname = result?["chatname"]?.ToString() ?? "",
                        userimageurl = "该功能暂不开放",
                        friend_list = result?["Friends"].ToJson() ?? "",
                        group_list = result?["JoinedGroups"].ToJson() ?? "",
                        token = GetRandomString(30, true, true, true, false, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"),
                        uid = result?["uid"].ToString() ?? ""
                    };

                    await dbcollection_tokens.InsertOneAsync(new BsonDocument("tk", acinfoproto.token));

                    using (var memoryStream = new MemoryStream())
                    {
                        Serializer.Serialize(memoryStream, acinfoproto);
                        SetResponseContent(resp, $"success>^<{Convert.ToBase64String(memoryStream.ToArray())}");
                        return;
                    }
                }
            }
        }

        static async Task SSE_ListenMsgReqAsync(HttpListenerContext context)
        {
            var requestData = await ReadRequestData(context.Request);
            var request = Serializer.Deserialize<ProtobufSSEConnectReq>(new MemoryStream(requestData));

            var tokenExists = await CheckTokenExists(request.token);
            if (!tokenExists)
            {
                SetResponseContent(context.Response, "eheheh_token_err");
                return;
            }

            Console.WriteLine("SSE Connected UID: " + request.uid);

            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            var writer = new StreamWriter(context.Response.OutputStream);

            var message = $"data: {"sse_status>connected"}\n\n";
            await writer.WriteAsync(message);
            await writer.FlushAsync();

            sse_dict.Add(request.uid, context);
        }

        static async Task WhileCheckDictContext()
        {
            Console.WriteLine("SSE Dict Check Start!");
            while (true)
            {
                var keysToRemove = new List<int>();

                foreach (int key in sse_dict.Keys)
                {
                    try
                    {
                        var writer = new StreamWriter(sse_dict[key].Response.OutputStream);

                        var message = $"data: {"sse_event>checklive>" + DateTime.Now}\n\n";
                        await writer.WriteAsync(message);
                        await writer.FlushAsync();
                    }
                    catch
                    {
                        Console.WriteLine("SSE Disconnected UID: " + key);
                        keysToRemove.Add(key);
                    }
                }

                // Remove disconnected SSE clients from the dictionary
                foreach (var key in keysToRemove)
                {
                    sse_dict.Remove(key);
                }

                await Task.Delay(1000);

                /*
                 * 根据用户流量进行修改
                 * 高峰4tick (250)
                 * 普通运营2tick (500)
                 * 测试版1tick (1000)
                 * 服务寄0.5tick (2000)
                 */
            }
        }

        public static void Main(string[] args)
        {

            if (!File.Exists("firstrun"))//第一次运行即初始化数据库
            {
                File.WriteAllText("currentuid.txt", "0");
                File.WriteAllText("currentgid.txt", "0");
                int now_can_use_uid = int.Parse(File.ReadAllText("currentuid.txt"));
                int now_can_use_gid = int.Parse(File.ReadAllText("currentgid.txt"));

                string[] chatnames = { "🤖 探索 StarChat", "🤖 StarChat 账号管理器" };

                List<BsonDocument> docs = new List<BsonDocument>();
                foreach (string chatname in chatnames)
                {
                    var doc = new BsonDocument{
                        { "username","StarChatRobotOfficial_"},
                        { "password","cantlogin938293892398298392838929382893289398293829skdskdksjdjkskdjskjdksjdkj"},
                        { "uid",now_can_use_uid},
                        { "chatname",chatname },
                    };
                    docs.Add(doc);
                    now_can_use_uid++;
                }

                File.WriteAllText("currentuid.txt", now_can_use_uid.ToString());
                dbcollection_account.InsertMany(docs);

                AddNewGroup(dbcollection_groups, "StarChatOfficial",0);

                File.Create("firstrun").Close();
            }//创建两个默认bot与1个Official Group


            now_can_use_uid = int.Parse(File.ReadAllText("currentuid.txt"));

            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Server Start!");
            Console.WriteLine("StarChatServer is a private program. DONT LEAK IT !!!");
            Console.WriteLine("Listening for connections on {0}", url);

            Task.Run(WhileCheckDictContext);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}