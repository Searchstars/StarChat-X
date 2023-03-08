﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Microsoft.UI.Xaml.Controls;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static System.Resources.ResXFileRef;

namespace StarChat
{
    public static class StarChatReq
    {

        public static string http_or_https = "http://";

        public static int sse_recv_count = 0;

        public static int lacheck_sse_recv_count = 0;

        public static string ClientUserLoginReq(string protob64)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(http_or_https + App.chatserverip + "/ClientUserLoginReq");
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(protob64);
                httpWebRequest.ContentType = "application/text";
                httpWebRequest.ContentLength = bs.Length;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 20000;
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                if (responseContent.Contains("success>^<"))
                {
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(responseContent.Split("success>^<")[1])))
                    {
                        var a = ProtoBuf.Serializer.Deserialize<ProtobufGetUserAccountInfoRes>(ms);
                        RunningDataSave.friends_list = JsonConvert.DeserializeObject<List<JsonFriendsList>>(a.friend_list);
                        RunningDataSave.groups_list = JsonConvert.DeserializeObject<List<JsonGroupsList>>(a.group_list);
                        RunningDataSave.userchatname = a.userchatname;
                        RunningDataSave.token = a.token;
                        RunningDataSave.useruid = int.Parse(a.uid);
                    }
                    return "OK";
                }
                else
                {
                    return "NO-OK-RETURN-MSG=" + responseContent;
                }
            }
            catch (Exception e)
            {
                return "E-R-R-O-R-M-S-G=" + e.ToString();
            }
        }
        public static string ClientUserRegisterReq(string protob64)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(http_or_https + App.chatserverip + "/ClientUserRegisterReq");
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(protob64);
                httpWebRequest.ContentType = "application/text";
                httpWebRequest.ContentLength = bs.Length;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 20000;
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                if (responseContent.Contains("success"))
                {
                    return "OK";
                }
                else
                {
                    return "NO-OK-RETURN-MSG=" + responseContent;
                }
            }
            catch (Exception e)
            {
                return "E-R-R-O-R-M-S-G=" + e.ToString();
            }
        }
        public static string GetFriendNameFromId(string protob64)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(http_or_https + App.chatserverip + "/GetFriendNameFromId");
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(protob64);
                httpWebRequest.ContentType = "application/text";
                httpWebRequest.ContentLength = bs.Length;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 20000;
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                if (responseContent.Contains("success>^<"))
                {
                    return responseContent.Split("success>^<")[1];
                }
                else
                {
                    return "ERR";
                }
            }
            catch (Exception e)
            {
                var cd = new ContentDialog
                {
                    Title = "StarChat程序错误",
                    Content = e,
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                return "ERR";
            }
        }

        public static string GetChatHistory(string protob64)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(http_or_https + App.chatserverip + "/GetChatHistory");
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(protob64);
                httpWebRequest.ContentType = "application/text";
                httpWebRequest.ContentLength = bs.Length;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 20000;
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                if (responseContent.Contains("success>^<"))
                {
                    return responseContent.Split("success>^<")[1];
                }
                else
                {
                    var cd = new ContentDialog
                    {
                        Title = "Error",
                        Content = "您的账号数据有问题，请联系开发者重置",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    };
                    cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                    return "ERR";
                }
            }
            catch (Exception e)
            {
                var cd = new ContentDialog
                {
                    Title = "StarChat程序错误",
                    Content = e,
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                return "ERR";
            }
        }

        public static string SendAddFriendRequest(string protob64)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(http_or_https + App.chatserverip + "/AddFriendReq");
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(protob64);
                httpWebRequest.ContentType = "application/text";
                httpWebRequest.ContentLength = bs.Length;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 20000;
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                if (responseContent.Contains("success>^<"))
                {
                    LogWriter.LogInfo("AddFriendReq_RespContent：" + responseContent);
                    return responseContent.Split("success>^<")[1];
                }
                else
                {
                    var cd = new ContentDialog
                    {
                        Title = "Error",
                        Content = "您的账号数据有问题，请联系开发者重置",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    };
                    cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                    return "ERR: " + responseContent;
                }
            }
            catch (Exception e)
            {
                var cd = new ContentDialog
                {
                    Title = "StarChat程序错误",
                    Content = e,
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                return "ERR";
            }
        }

        public static string SendAllowFriendRequest(string protob64)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(http_or_https + App.chatserverip + "/AllowFriendReq");
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(protob64);
                httpWebRequest.ContentType = "application/text";
                httpWebRequest.ContentLength = bs.Length;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 20000;
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                if (responseContent.Contains("success>^<"))
                {
                    return responseContent.Split("success>^<")[1];
                }
                else
                {
                    var cd = new ContentDialog
                    {
                        Title = "Error",
                        Content = "您的账号数据有问题，请联系开发者重置",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    };
                    cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                    return "ERR: " + responseContent;
                }
            }
            catch (Exception e)
            {
                var cd = new ContentDialog
                {
                    Title = "StarChat程序错误",
                    Content = e,
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                return "ERR";
            }
        }

        public static string GetMyRequests(string protob64)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(http_or_https + App.chatserverip + "/GetMyRequestsReq");
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(protob64);
                httpWebRequest.ContentType = "application/text";
                httpWebRequest.ContentLength = bs.Length;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 20000;
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                if (responseContent.Contains("success>^<"))
                {
                    return responseContent.Split("success>^<")[1];
                }
                else
                {
                    var cd = new ContentDialog
                    {
                        Title = "Error",
                        Content = "您的账号数据有问题，请联系开发者重置",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    };
                    cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                    return "ERR: " + responseContent;
                }
            }
            catch (Exception e)
            {
                var cd = new ContentDialog
                {
                    Title = "StarChat程序错误",
                    Content = e,
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                return "ERR";
            }
        }

        public static string SendRejectFriendRequest(string protob64)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(http_or_https + App.chatserverip + "/RejectFriendReq");
                //字符串转换为字节码
                byte[] bs = Encoding.UTF8.GetBytes(protob64);
                httpWebRequest.ContentType = "application/text";
                httpWebRequest.ContentLength = bs.Length;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 20000;
                httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                httpWebRequest.Abort();
                if (responseContent.Contains("success>^<"))
                {
                    return responseContent.Split("success>^<")[1];
                }
                else
                {
                    var cd = new ContentDialog
                    {
                        Title = "Error",
                        Content = "您的账号数据有问题，请联系开发者重置",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    };
                    cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                    return "ERR: " + responseContent;
                }
            }
            catch (Exception e)
            {
                var cd = new ContentDialog
                {
                    Title = "StarChat程序错误",
                    Content = e,
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };
                cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                return "ERR";
            }
        }

        public static void ConnectSSE(string protob64)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(http_or_https + App.chatserverip + "/ListenMsg");
            //字符串转换为字节码
            byte[] bs = Encoding.UTF8.GetBytes(protob64);
            httpWebRequest.ContentType = "application/text";
            httpWebRequest.ContentLength = bs.Length;
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 20000;
            httpWebRequest.Accept = "text/event-stream";
            httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
            SSEMsgRecv(streamReader);
            CheckSSEConnectionStat();
        }

        static async Task CheckSSEConnectionStat()
        {
            while (true)
            {
                await Task.Delay(8000);
                if(lacheck_sse_recv_count == sse_recv_count)
                {
                    LogWriter.LogWarn("SSE超时，给予用户警告弹窗");
                    var cd = new ContentDialog
                    {
                        Title = "网络连接出现问题",
                        Content = "你可能注意到群聊或好友的消息突然停下来了，这是客户端过久没收到来自服务器的消息导致的。若此弹窗在短期内反复出现，可能是你的网络已断开，若此弹窗会不定时的间隔超过10秒出现或只是偶尔出现几次，则可能是你的网络连接速率过于缓慢",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    };
                    cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                    await cd.ShowAsync();
                }
                else
                {
                    lacheck_sse_recv_count = sse_recv_count;
                }
            }
        }

        static async Task SSEMsgRecv(StreamReader streamReader)
        {
            while (true)
            {
                var line = await streamReader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) // 一个消息结束
                {
                    //Console.WriteLine(); // 消息结束后输出空行，便于调试
                }
                else if (line.StartsWith("data: "))
                {
                    sse_recv_count++;
                    var data = line.Substring("data: ".Length);
                    if (data.Contains("checklive"))
                    {
                        Console.WriteLine(data);
                    }
                    else
                    {
                        LogWriter.LogInfo(data);

                    }
                    if (data.Length <= 0)
                    {
                        LogWriter.LogWarn("SSE状态异常，给予用户警告弹窗");
                        var cd = new ContentDialog
                        {
                            Title = "网络连接中断",
                            Content = "服务器返回了空消息，这大概率是你的网络异常，或是StarChat的服务器被关闭，StarChat将退出",
                            CloseButtonText = "OK",
                            DefaultButton = ContentDialogButton.Close
                        };
                        cd.XamlRoot = RunningDataSave.chatwindow_static.Content.XamlRoot;
                        await cd.ShowAsync();
                        Environment.Exit(0);
                    }
                    if (data.Split(">")[0] == "newaddfriendreq")
                    {
                        string hexColorCode = "#06ffffff"; // Replace with your HEX color code
                        Windows.UI.Color color = Windows.UI.Color.FromArgb(
                            byte.Parse(hexColorCode.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(hexColorCode.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(hexColorCode.Substring(5, 2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(hexColorCode.Substring(7, 2), System.Globalization.NumberStyles.HexNumber)
                        );
                        if (RunningDataSave.chatwindow_nav_static.SelectedItem == RunningDataSave.chatwindow_nav_static.MenuItems[2])
                        {
                            var id_to_name_res = "";
                            var utnproto = new ProtobufUidToUserName
                            {
                                targetid = int.Parse(data.Split(">")[1]),
                                token = RunningDataSave.token
                            };
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                ProtoBuf.Serializer.Serialize(memoryStream, utnproto);
                                id_to_name_res = GetFriendNameFromId(Convert.ToBase64String(memoryStream.ToArray()));
                            }
                            Border bd = new Border {
                                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(10),
                                Child = new Grid
                                {
                                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(color),
                                    Height = 75,
                                    Width = 7000,
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Margin = new Microsoft.UI.Xaml.Thickness(25),
                                            Text = "好友请求：",
                                            FontSize = 16
                                        },
                                        new TextBlock
                                        {
                                            Margin = new Microsoft.UI.Xaml.Thickness(102,25,0,0),
                                            Text = "来自 " + id_to_name_res + "（UID：" + data.Split(">")[1] + "）",
                                            FontSize = 16
                                        },
                                        new Button
                                        {
                                            Margin = new Microsoft.UI.Xaml.Thickness(860,0,0,0),
                                            Content = "同意",
                                            Tag = "accept_friend_req_uid=" + data.Split(">")[1]
                                        },
                                        new Button
                                        {
                                            Margin = new Microsoft.UI.Xaml.Thickness(950,0,0,0),
                                            Content = "拒绝",
                                            Tag = "reject_friend_req_uid=" + data.Split(">")[1]
                                        }
                                    }
                                }
                            };
                            RunningDataSave.newreqlist_stackpanel.Children.Add(bd);
                            RunningDataSave.newreqlist_stackpanel.Children.Add(new Image {Height=20});
                        }
                    }
                }
            }
        }
    }
}
