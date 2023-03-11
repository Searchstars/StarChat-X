using Microsoft.UI.Xaml.Controls;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Geolocation;

namespace StarChat
{
    public static class RunningDataSave
    {
        public static string user_ip_addr = "noinit";

        public static bool init_network_error_need_dialog = false;

        public static MainWindow mainwindow_static;

        public static ChatWindow chatwindow_static;

        public static List<JsonFriendsList> friends_list = null;

        public static List<JsonGroupsList> groups_list = null;

        public static string userchatname = "";

        public static int useruid = 0;

        public static string token = null;

        public static string chatframe_type = null;//friend 或 group

        public static int chatframe_targetid = 0;

        public static NavigationView chatwindow_nav_static = null;

        public static StackPanel newreqlist_stackpanel = null;

        public static Pivot addfriorgrouppage_pivot = null;

        public static StackPanel friendchatframe_sp_chatcontent = null;

        public static ScrollViewer scrollviewer_chatcontent = null;
    }
}
