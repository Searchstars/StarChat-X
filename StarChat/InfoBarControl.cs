using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarChat
{
    public static class InfoBarControl
    {
        public static InfoBar infobar(StackPanel skp, bool IsOpen, String Title, String Msg)
        {
            var new_ifb = new InfoBar()
            {
                IsOpen = IsOpen,
                Severity = InfoBarSeverity.Informational,
                Title = Title,
                Message = Msg
            };
            skp.Children.Add(new_ifb);
            return new_ifb;
        }

        public static InfoBar sucbar(StackPanel skp, bool IsOpen, String Title, String Msg)
        {
            var new_ifb = new InfoBar()
            {
                IsOpen = IsOpen,
                Severity = InfoBarSeverity.Success,
                Title = Title,
                Message = Msg
            };
            skp.Children.Add(new_ifb);
            return new_ifb;
        }

        public static InfoBar warnbar(StackPanel skp, bool IsOpen, String Title, String Msg)
        {
            var new_ifb = new InfoBar()
            {
                IsOpen = IsOpen,
                Severity = InfoBarSeverity.Warning,
                Title = Title,
                Message = Msg
            };
            skp.Children.Add(new_ifb);
            return new_ifb;
        }

        public static InfoBar errbar(StackPanel skp, bool IsOpen, String Title, String Msg)
        {
            var new_ifb = new InfoBar()
            {
                IsOpen = IsOpen,
                Severity = InfoBarSeverity.Error,
                Title = Title,
                Message = Msg
            };
            skp.Children.Add(new_ifb);
            return new_ifb;
        }
    }
}
