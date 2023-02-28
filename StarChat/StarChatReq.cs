﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Microsoft.UI.Xaml.Controls;

namespace StarChat
{
    public static class StarChatReq
    {
        public static string ClientUserLoginReq(string protob64)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://" + App.chatserverip + "/ClientUserLoginReq");
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
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://" + App.chatserverip + "/ClientUserRegisterReq");
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
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://" + App.chatserverip + "/GetFriendNameFromId");
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
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://" + App.chatserverip + "/GetChatHistory");
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
    }
}
