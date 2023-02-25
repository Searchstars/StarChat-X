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
                    System.IO.Stream body = req.InputStream;
                    System.Text.Encoding encoding = req.ContentEncoding;
                    System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                    string data_poststr = reader.ReadToEnd();
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data_poststr)))
                    {
                        var a = ProtoBuf.Serializer.Deserialize<ProtobufLogin>(ms);
                        Console.WriteLine("ClientUserLoginReq  Username：" + a.username + " Password: " + a.password);
                        FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                        //约束条件
                        FilterDefinition<BsonDocument> filter = builderFilter.Eq("username", a.username);
                        //获取数据
                        var result = dbcollection_account.Find<BsonDocument>(filter).ToList();
                        try
                        {
                            if (result[0]["password"] == a.password)
                            {
                                if (result[0]["ban"] == "no")
                                {
                                    var acinfoproto = new ProtobufGetUserAccountInfoRes
                                    {
                                        userchatname = result[0]["chatname"].ToString(),
                                        userimageurl = "该功能暂不开放",
                                        friend_list = result[0]["Friends"].ToJson(),
                                        group_list = result[0]["JoinedGroups"].ToJson(),
                                        token = GetRandomString(30, true, true, true, false, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"),
                                        uid = result[0]["uid"].ToString()
                                    };
                                    var doc = new[]
                                    {
                                        new BsonDocument { {"tk",acinfoproto.token} }
                                    };

                                    await dbcollection_tokens.InsertManyAsync(doc);

                                    using (MemoryStream memoryStream = new MemoryStream())
                                    {
                                        ProtoBuf.Serializer.Serialize(memoryStream, acinfoproto);
                                        Console.WriteLine("ProtobufGetUserAccountInfoRes" + Convert.ToBase64String(memoryStream.ToArray()));
                                        byte[] data = Encoding.UTF8.GetBytes("success>^<" + Convert.ToBase64String(memoryStream.ToArray()));
                                        resp.ContentType = "text/html";
                                        resp.ContentEncoding = Encoding.UTF8;
                                        resp.ContentLength64 = data.LongLength;

                                        // Write out to the response stream (asynchronously), then close it
                                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                                        resp.Close();
                                    }
                                }
                                else if (result[0]["ban"] == "forever")//永久ban
                                {
                                    byte[] data = Encoding.UTF8.GetBytes("您的账号已被永久封禁，原因：" + result[0]["banreason"]);
                                    resp.ContentType = "text/html";
                                    resp.ContentEncoding = Encoding.UTF8;
                                    resp.ContentLength64 = data.LongLength;

                                    // Write out to the response stream (asynchronously), then close it
                                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                                    resp.Close();
                                }
                                else
                                {
                                    byte[] data = Encoding.UTF8.GetBytes("您的账号已被封禁到" + result[0]["ban"] + " (timestamp) ，原因：" + result[0]["banreason"]);
                                    resp.ContentType = "text/html";
                                    resp.ContentEncoding = Encoding.UTF8;
                                    resp.ContentLength64 = data.LongLength;

                                    // Write out to the response stream (asynchronously), then close it
                                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                                    resp.Close();
                                }
                            }
                            else
                            {
                                byte[] data = Encoding.UTF8.GetBytes("用户名或密码错误");
                                resp.ContentType = "text/html";
                                resp.ContentEncoding = Encoding.UTF8;
                                resp.ContentLength64 = data.LongLength;

                                // Write out to the response stream (asynchronously), then close it
                                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                                resp.Close();
                            }
                        }
                        catch (Exception e)
                        {
                            byte[] data = Encoding.UTF8.GetBytes("用户名或密码错误");
                            resp.ContentType = "text/html";
                            resp.ContentEncoding = Encoding.UTF8;
                            resp.ContentLength64 = data.LongLength;

                            // Write out to the response stream (asynchronously), then close it
                            await resp.OutputStream.WriteAsync(data, 0, data.Length);
                            resp.Close();
                        }
                    }
                }
                else if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/ClientUserRegisterReq"))
                {
                    System.IO.Stream body = req.InputStream;
                    System.Text.Encoding encoding = req.ContentEncoding;
                    System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                    string data_poststr = reader.ReadToEnd();
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data_poststr)))
                    {
                        var a = ProtoBuf.Serializer.Deserialize<ProtobufRegister>(ms);
                        Console.WriteLine("ClientUserRegisterReq  Username：" + a.username + " Password: " + a.password + " RegisterBtnClickNum: " + a.regbutton_click_num);
                        if (a.regbutton_click_num > 2)
                        {
                            byte[] data = Encoding.UTF8.GetBytes("注册次数过多，请勿刷号");
                            resp.ContentType = "text/html";
                            resp.ContentEncoding = Encoding.UTF8;
                            resp.ContentLength64 = data.LongLength;

                            // Write out to the response stream (asynchronously), then close it
                            await resp.OutputStream.WriteAsync(data, 0, data.Length);
                            resp.Close();
                        }
                        else
                        {
                            try
                            {
                                FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                                FilterDefinition<BsonDocument> filter = builderFilter.Eq("username", a.username);
                                var result = dbcollection_account.Find<BsonDocument>(filter).ToList()[0]["password"];
                                byte[] data = Encoding.UTF8.GetBytes("该用户已存在");
                                resp.ContentType = "text/html";
                                resp.ContentEncoding = Encoding.UTF8;
                                resp.ContentLength64 = data.LongLength;

                                // Write out to the response stream (asynchronously), then close it
                                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                                resp.Close();
                            }
                            catch
                            {
                                AddNewUser(dbcollection_account, a.username,a.password, now_can_use_uid);
                                now_can_use_uid++;
                                File.WriteAllText("currentuid.txt", now_can_use_uid.ToString());
                                byte[] data = Encoding.UTF8.GetBytes("success");
                                resp.ContentType = "text/html";
                                resp.ContentEncoding = Encoding.UTF8;
                                resp.ContentLength64 = data.LongLength;

                                // Write out to the response stream (asynchronously), then close it
                                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                                resp.Close();
                            }
                        }
                    }
                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetFriendNameFromId")
                {
                    using (var body = req.InputStream)
                    using (var reader = new StreamReader(body, req.ContentEncoding))
                    {
                        var data_poststr = reader.ReadToEnd();
                        var data = Convert.FromBase64String(data_poststr);

                        try
                        {
                            var a = Serializer.Deserialize<ProtobufUidToUserName>(new MemoryStream(data));
                            Console.WriteLine($"GetFriendNameFromId  TargetId：{a.targetid} Token: {a.token}");

                            var filter = Builders<BsonDocument>.Filter.Eq("uid", a.targetid);
                            var projection = Builders<BsonDocument>.Projection.Include("chatname");
                            var result = await dbcollection_account.Find(filter).Project(projection).FirstOrDefaultAsync();

                            var Tryfilter = Builders<BsonDocument>.Filter.Eq("tk", a.token);
                            var Tryprojection = Builders<BsonDocument>.Projection.Include("tk");
                            var Tryresult = await dbcollection_tokens.Find(Tryfilter).Project(Tryprojection).FirstOrDefaultAsync();

                            if (Tryresult == null)
                            {
                                throw new Exception("Token not found");
                            }

                            var chatname = result?["chatname"]?.AsString ?? "";
                            var dataBytes = Encoding.UTF8.GetBytes($"success>^<{chatname}");
                            resp.ContentType = "text/html";
                            resp.ContentEncoding = Encoding.UTF8;
                            resp.ContentLength64 = dataBytes.LongLength;

                            await resp.OutputStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                        }
                        catch (Exception e)
                        {
                            var dataBytes = Encoding.UTF8.GetBytes(e.ToString());
                            resp.ContentType = "text/html";
                            resp.ContentEncoding = Encoding.UTF8;
                            resp.ContentLength64 = dataBytes.LongLength;

                            await resp.OutputStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                        }
                        finally
                        {
                            resp.Close();
                        }
                    }

                }
                else if (req.HttpMethod == HttpMethod.Post.Method && req.Url.AbsolutePath == "/GetChatHistory")
                {
                    using (var body = req.InputStream)
                    using (var reader = new StreamReader(body, req.ContentEncoding))
                    {
                        var data_poststr = reader.ReadToEnd();
                        var data = Convert.FromBase64String(data_poststr);

                        try
                        {
                            var a = Serializer.Deserialize<ProtobufGetChatHistory>(new MemoryStream(data));
                            Console.WriteLine($"GetChatHistory  TargetId：{a.targetid} Token: {a.token} Target: {a.target}");

                            var Tryfilter = Builders<BsonDocument>.Filter.Eq("tk", a.token);
                            var Tryprojection = Builders<BsonDocument>.Projection.Include("tk");
                            var Tryresult = await dbcollection_tokens.Find(Tryfilter).Project(Tryprojection).FirstOrDefaultAsync();

                            if (Tryresult == null)
                            {
                                throw new Exception("Token not found");
                            }

                            if(a.target == "friend")
                            {
                                var filter = Builders<BsonDocument>.Filter.Eq("uid", int.Parse(a.clientuid));
                                var result = await dbcollection_account.Find(filter).FirstOrDefaultAsync();
                                foreach(var item in result["Friends"].AsBsonArray)
                                {
                                    if (item["id"].ToString() == a.targetid)
                                    {
                                        var chatname = result?["chatname"]?.AsString ?? "";
                                        var dataBytes = Encoding.UTF8.GetBytes($"success>^<{item["chat_history"]}");
                                        resp.ContentType = "text/html";
                                        resp.ContentEncoding = Encoding.UTF8;
                                        resp.ContentLength64 = dataBytes.LongLength;

                                        await resp.OutputStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            var dataBytes = Encoding.UTF8.GetBytes(e.ToString());
                            resp.ContentType = "text/html";
                            resp.ContentEncoding = Encoding.UTF8;
                            resp.ContentLength64 = dataBytes.LongLength;

                            await resp.OutputStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                        }
                        finally
                        {
                            resp.Close();
                        }
                    }

                }
                else if ((req.Headers["Accept"] == "text/event-stream") && (req.Url.AbsolutePath == "/ListenMsg"))
                {
                    resp.ContentType = "text/event-stream";

                    for (int i = 0; true; i++)
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes("主播666");
                        resp.OutputStream.Write(buffer, 0, buffer.Length);
                        resp.OutputStream.Flush();
                        await Task.Delay(1000); // 每秒钟发送一次消息
                    }
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

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}