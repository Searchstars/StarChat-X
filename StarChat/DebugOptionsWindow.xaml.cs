// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StarChat
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DebugOptionsWindow : Window
    {

        private IntPtr hwnd;
        private AppWindow appWindow;

        public DebugOptionsWindow()
        {
            this.InitializeComponent();
            this.Title = "scdbg";
            //ExtendsContentIntoTitleBar = true;
            hwnd = WindowNative.GetWindowHandle(this);
            WindowId id = Win32Interop.GetWindowIdFromWindow(hwnd);
            appWindow = AppWindow.GetFromWindowId(id);
            var rect = appWindow.Size;
            rect.Width = 600;
            rect.Height = 100;
            appWindow.Resize(rect);
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileUploadProgressBAR fswindow = new FileUploadProgressBAR("Debug");
            fswindow.Activate();
        }
    }
}
