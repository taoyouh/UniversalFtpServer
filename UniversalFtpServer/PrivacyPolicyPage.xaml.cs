// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable
namespace UniversalFtpServer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PrivacyPolicyPage : Page
    {
        public static int VersionNumber = 1;
        private readonly Settings _settings = new Settings();
        private NavigationArgs? _args;

        public PrivacyPolicyPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is NavigationArgs args)
            {
                _args = args;
            }

            _bottomPanel.Visibility = _args?.NeedAgree == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public class NavigationArgs
        {
            public bool NeedAgree { get; set; }

            public Type? NextPageAfterAgree { get; set; }
        }

        private void AgreeButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.AcceptedPrivacyPolicyVersion = VersionNumber;
            Frame.Navigate(_args?.NextPageAfterAgree, null, new EntranceNavigationTransitionInfo());
            Frame.BackStack.RemoveAt(Frame.BackStack.Count - 1);
        }

        private void DisagreeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        public static void CheckAgreedAndNavigate(Settings settings, Frame frame, Type pageType)
        {
            if (settings.AcceptedPrivacyPolicyVersion != VersionNumber)
            {
                frame.Navigate(typeof(PrivacyPolicyPage), new NavigationArgs()
                {
                    NeedAgree = true,
                    NextPageAfterAgree = pageType,
                });
            }
            else
            {
                frame.Navigate(pageType);
            }
        }
    }
}
