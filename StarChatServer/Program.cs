using System;
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

namespace StarChatServer
{
    class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://*:8000/";//监听所有IP 一般不用改
        public static string dburl = "mongodb://localhost:27017/";//数据库url，按照搭建环境进行调整
        public static MongoClient client = new MongoClient(dburl);
        public static IMongoCollection<BsonDocument> dbcollection_test = client.GetDatabase("StarChatServer").GetCollection<BsonDocument>("test");
        public static IMongoCollection<BsonDocument> dbcollection_account = client.GetDatabase("StarChatServer").GetCollection<BsonDocument>("accounts");
        public static IMongoCollection<BsonDocument> dbcollection_tokens = client.GetDatabase("StarChatServer").GetCollection<BsonDocument>("tokens");
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
                    new BsonDocument{ { "id", "1" }},
                }
            },
            { "Friends", new BsonArray
                {
                    new BsonDocument{ { "id", "0" }, { "chat_history", "[{\"msg_send_time\": \"dont_view\",\"msgtype\":\"text\",\"msglink\":\"dont_need\",\"msgcontent\":\"shhhh...看起来，这里有一个新来的？\n别再等啦！查看官方wiki并加入官方群组，开始你的StarChat之旅！\nWelcome to StarChat !\"}]" } },
                    new BsonDocument{ { "id", "1" }, { "chat_history", "[{\"msg_send_time\": \"dont_view\",\"msgtype\":\"text\",\"msglink\":\"dont_need\",\"msgcontent\":\"哈喽哇？不知道怎么使用？给我发个/help吧！当然，你也可以查看：\"},{\"msg_send_time\": \"dont_view\",\"msgtype\":\"hyperlink\",\"msglink\":\"https://chat.stargazing.studio/documents\",\"msgcontent\":\"官方文档\"}]" } }
                }
            },
            { "FriendRequests" , new BsonArray
            {

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
                else if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/ClientUserLoginReq"))
                {
                    LoginPostReqAsync(req,resp);
                }
                else if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/ClientUserRegisterReq"))
                {
                    RegisterPostReqAsync(req,resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetFriendNameFromId")
                {
                    GetFriendNameFromIdAsync(req,resp);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetChatHistory")
                {
                    GetChatHistoryAsync(req,resp);
                }
                else if ((req.Headers["Accept"] == "text/event-stream") && (req.Url.AbsolutePath == "/ListenMsg"))
                {
                    SSE_ListenMsgReqAsync(ctx);
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/AddFriendReq")
                {
                    AddFriendReqAsync(req, resp);
                }
                else
                {
                    // Write the response info
                    string disableSubmit = !runServer ? "disabled" : "";
                    byte[] data = Encoding.UTF8.GetBytes("404 Not Found<br/><image style=\"color: black;width: 600;height: 2; background-color: black\"/><br/>Simple C# HTTP Server For StarChat 20230211<br/>" + req.UserAgent + "<br/>" + req.RawUrl);
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;

                    resp.ContentLength64 = data.LongLength;
                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
            }
        }

        private static async Task<byte[]> ReadRequestData(HttpListenerRequest req)
        {
            using (var body = req.InputStream)
            using (var reader = new StreamReader(body, req.ContentEncoding))
            {
                return Convert.FromBase64String(await reader.ReadToEndAsync());
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
            var dataBytes = Encoding.UTF8.GetBytes(content);
            resp.ContentType = "text/html";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = dataBytes.LongLength;
            resp.OutputStream.Write(dataBytes, 0, dataBytes.Length);
            resp.Close();
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
                    var message = $"data: {"newaddfriendreq>" + request.targetuid.ToString() + ">" + ""}\n\n";
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

                if (request.target != "friend")
                {
                    throw new Exception("Unsupported target type");
                }

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

        private static async Task<string> ReadRequestDataStr(HttpListenerRequest req)
        {
            using (var body = req.InputStream)
            using (var reader = new StreamReader(body, req.ContentEncoding))
            {
                return await reader.ReadToEndAsync();
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
                var data_poststr = await reader.ReadToEndAsync();
                var requestData = Convert.FromBase64String(data_poststr);
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

                await Task.Delay(3000); // Server tick 0.3s
            }
        }

        public static void Main(string[] args)
        {

            if (!File.Exists("firstrun"))
            {
                File.WriteAllText("currentuid.txt", "0");
                int now_can_use_uid = int.Parse(File.ReadAllText("currentuid.txt"));

                string[] chatnames = { "🤖 探索 StarChat", "🤖 StarChat 账号管理器" };

                List<BsonDocument> docs = new List<BsonDocument>();
                foreach (string chatname in chatnames)
                {
                    var doc = new BsonDocument{
                        { "RegTime",new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()},
                        { "username","StarChatRobotOfficial_"},
                        { "password","cantlogin938293892398298392838929382893289398293829skdskdksjdjkskdjskjdksjdkj"},
                        { "uid",now_can_use_uid},
                        { "chatname",chatname },
                        { "isFinishFirstLoginSettings", "no"},
                        { "ban","no" },
                        { "banreason","该账号未被封禁，若您看到了这段信息，请联系开发者" },
                        { "JoinedGroups",new  BsonArray
                        {

                        }
                        },
                        { "Friends",new  BsonArray
                            {

                            }
                        },
                    };
                    docs.Add(doc);
                    now_can_use_uid++;
                }

                File.WriteAllText("currentuid.txt", now_can_use_uid.ToString());
                dbcollection_account.InsertMany(docs);
                File.Create("firstrun").Close();
            }


            now_can_use_uid = int.Parse(File.ReadAllText("currentuid.txt"));

            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Server Start!");
            Console.WriteLine("StarChatServer is supported by ChatGPT, a language model developed by OpenAI.");
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