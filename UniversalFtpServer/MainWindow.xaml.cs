﻿using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UniversalFtpServer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly Settings _settings = new Settings();

        public MainWindow()
        {
            InitializeComponent();

            AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Icon.ico"));
            Title = _appTitleTextBlock.Text;
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(_appTitleBar);

            PrivacyPolicyPage.CheckAgreedAndNavigate(_settings, _rootFrame, typeof(MainPage));
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            _rootFrame.GoBack();
        }
    }
}
