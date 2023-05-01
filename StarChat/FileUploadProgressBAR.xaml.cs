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
    public sealed partial class FileUploadProgressBAR : Window
    {

        private IntPtr hwnd;
        private AppWindow appWindow;

        private void window_close(AppWindow a, AppWindowClosingEventArgs e)
        {
            RunningDataSave.upload_window_open = false;
            this.Close();
        }

        public FileUploadProgressBAR(string filename_text)
        {
            this.InitializeComponent();
            this.Title = "StarChat - FileUpload";
            ExtendsContentIntoTitleBar = true;
            hwnd = WindowNative.GetWindowHandle(this);
            WindowId id = Win32Interop.GetWindowIdFromWindow(hwnd);
            appWindow = AppWindow.GetFromWindowId(id);
            appWindow.Closing += window_close;
            var rect = appWindow.Size;
            rect.Width = 600;
            rect.Height = 210;
            appWindow.Resize(rect);
            SetTitleBar(AppTitleBar);

            this.UploadFileName.Text = filename_text;
            RunningDataSave.upload_window_open = true;
            RunningDataSave.FileUploadWindow_FileNameTxb = this.UploadFileName;
            RunningDataSave.FileUploadWindow_UploadSpeedTxb = this.UploadFileSpeed;
            RunningDataSave.FileUploadWindow_UploadPGBR = this.UploadFilePgbr;
        }
    }
}
