using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Zhaobang.FtpServer;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace UniversalFtpServer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string Ipv4Listening = "MainPage_Ipv4Listening";
        const string Ipv6Listening = "MainPage_Ipv6Listening";
        const string Ipv4Error = "MainPage_Ipv4Error";
        const string Ipv6Error = "MainPage_Ipv6Error";
        const string Ipv4Stopped = "MainPage_Ipv4Stopped";
        const string Ipv6Stopped = "MainPage_Ipv6Stopped";
        const string Ok = "Dialog_Ok";
        const string PortIncorrect = "MainPage_PortIncorrect";
        const string PortNumberSetting = "PortNumber";
        const string AllowAnonymousSetting = "AllowAnonymous";
        const string UserNameSetting = "UserName";
        const string PasswordSetting = "Password";
        const string SettingVersionSetting = "SettingVersion";
        const string RootFolderSetting = "RootFolder";

        FtpServer server4;
        Task server4Run;
        FtpServer server6;
        Task server6Run;
        CancellationTokenSource cts;
        string rootPath;
        StorageFolder rootFolder;
        readonly object rootFolderSyncRoot = new object();

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;

            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values[RootFolderSetting] is string token)
            {
                var result = LoadRootFolderAsync(token);
            }
            else
            {
                var folder = ApplicationData.Current.LocalFolder;

                lock (rootFolderSyncRoot)
                {
                    rootFolder = folder;
                    rootPath = folder.Path;
                    rootFolderBlock.Text = rootPath;
                }
            }
            if (!(settings.Values[SettingVersionSetting] is int version && version == 1))
            {
                settings.Values[SettingVersionSetting] = 1;
                settings.Values[PortNumberSetting] = 21;
            }
            if (settings.Values[PortNumberSetting] is int port)
                portBox.Text = port.ToString();
            if (settings.Values[AllowAnonymousSetting] is bool allowAnonymous)
                allowAnonymousBox.IsChecked = allowAnonymous;
            else
                allowAnonymousBox.IsChecked = true;
            if (settings.Values[UserNameSetting] is string userName)
                userNameBox.Text = userName;
            if (settings.Values[PasswordSetting] is string password)
                passwordBox.Text = password;

            var addresses = from host in NetworkInformation.GetHostNames()
                            where host.Type == Windows.Networking.HostNameType.DomainName ||
                                  host.Type == Windows.Networking.HostNameType.Ipv4 ||
                                  host.Type == Windows.Networking.HostNameType.Ipv6
                            select host.DisplayName;
            addressesBlock.Text = string.Join('\n', addresses);
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Suspending -= App_Suspending;
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var settings = ApplicationData.Current.LocalSettings;

            if (int.TryParse(portBox.Text, out int port))
                settings.Values[PortNumberSetting] = port;

            var allowAnonymous = allowAnonymousBox.IsChecked == true;
            string userName = userNameBox.Text;
            string password = passwordBox.Text;

            settings.Values[AllowAnonymousSetting] = allowAnonymous;
            settings.Values[UserNameSetting] = userName;
            settings.Values[PasswordSetting] = password;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Suspending += App_Suspending;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var loader = new ResourceLoader();

            if (!int.TryParse(portBox.Text, out int port))
            {
                NotifyUser(loader.GetString(PortIncorrect));
                return;
            }

            var allowAnonymous = allowAnonymousBox.IsChecked == true;
            string userName = userNameBox.Text;
            string password = passwordBox.Text;

            var settings = ApplicationData.Current.LocalSettings;
            settings.Values[PortNumberSetting] = port;
            settings.Values[AllowAnonymousSetting] = allowAnonymous;
            settings.Values[UserNameSetting] = userName;
            settings.Values[PasswordSetting] = password;

            var x = VisualStateManager.GoToState(this, nameof(runningState), true);

            statusBlock4.Text = string.Format(loader.GetString(Ipv4Listening), port);
            statusBlock6.Text = string.Format(loader.GetString(Ipv6Listening), port);

            cts = new CancellationTokenSource();
            IPEndPoint ep4 = new IPEndPoint(IPAddress.Any, port);
            IPEndPoint ep6 = new IPEndPoint(IPAddress.IPv6Any, port);

            await Task.Run(() =>
            {
                if (allowAnonymous)
                {
                    server4 = new FtpServer(ep4, rootPath);
                    server6 = new FtpServer(ep6, rootPath);
                }
                else
                {
                    server4 = new FtpServer(
                        ep4,
                        new UwpFileProviderFactory(rootPath),
                        new Zhaobang.FtpServer.Connections.LocalDataConnectionFactory(),
                        new Zhaobang.FtpServer.Authenticate.SimpleAuthenticator(userName, password));
                    server6 = new FtpServer(
                        ep6,
                        new UwpFileProviderFactory(rootPath),
                        new Zhaobang.FtpServer.Connections.LocalDataConnectionFactory(),
                        new Zhaobang.FtpServer.Authenticate.SimpleAuthenticator(userName, password));
                }
            });

            var server4Deferral = ExtendedExecutionHelper.GetDeferral();
            server4Run = server4.RunAsync(cts.Token).ContinueWith(async t =>
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (t.IsFaulted)
                        statusBlock4.Text = string.Format(loader.GetString(Ipv4Error), t.Exception);
                    else
                        statusBlock4.Text = loader.GetString(Ipv4Stopped);
                    server4Deferral.Complete();
                }));
            var server6Deferral = ExtendedExecutionHelper.GetDeferral();
            server6Run = server6.RunAsync(cts.Token).ContinueWith(async t =>
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (t.IsFaulted)
                        statusBlock6.Text = string.Format(loader.GetString(Ipv6Error), t.Exception);
                    else
                        statusBlock6.Text = loader.GetString(Ipv6Stopped);
                    server6Deferral.Complete();
                }));
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            var addresses = from host in NetworkInformation.GetHostNames()
                            select host.DisplayName;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                addressesBlock.Text = string.Join('\n', addresses);
            });
        }

        private void NotifyUser(string v)
        {
            var loader = new ResourceLoader();
            ContentDialog dialog = new ContentDialog
            {
                Content = v,
                CloseButtonText = loader.GetString(Ok)
            };
            var result = dialog.ShowAsync();
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            var x = VisualStateManager.GoToState(this, nameof(stoppedState), true);

            cts?.Cancel();
            try { await server4Run; } catch { }
            try { await server6Run; } catch { }
        }

        private async void LaunchFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(rootFolder);
        }

        private void allowAnonymousBox_Checked(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(anonymousState), true);
        }

        private void allowAnonymousBox_Unchecked(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(notAnonymousState), true);
        }

        private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            var folder = await picker.PickSingleFolderAsync();
            if (folder == null)
                return;
            string token = StorageApplicationPermissions.MostRecentlyUsedList.Add(folder);
            lock (rootFolderSyncRoot)
            {
                rootFolder = folder;
                rootPath = folder.Path;
                rootFolderBlock.Text = rootPath;
                ApplicationData.Current.LocalSettings.Values[RootFolderSetting] = token;
            }
        }

        private async Task LoadRootFolderAsync(string token)
        {
            var folder = ApplicationData.Current.LocalFolder;

            try
            {
                folder = await StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(token);
            }
            catch { }

            lock (rootFolderSyncRoot)
            {
                rootFolder = folder;
                rootPath = folder.Path;
                rootFolderBlock.Text = rootPath;
            }
        }
    }
}
