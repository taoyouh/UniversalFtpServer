using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
        FtpServer server4;
        Task server4Run;
        FtpServer server6;
        Task server6Run;
        CancellationTokenSource cts;
        string rootPath;
        StorageFolder rootFolder;

        public MainPage()
        {
            this.InitializeComponent();
            rootFolder = ApplicationData.Current.LocalFolder;
            rootPath = rootFolder.Path;
            allowAnonymousBox.IsChecked = true;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(portBox.Text, out int port))
            {
                NotifyUser("PortIncorrect");
                return;
            }

            var allowAnonymous = allowAnonymousBox.IsChecked == true;
            string userName = string.Empty, password = string.Empty;
            if (!allowAnonymous)
            {
                userName = userNameBox.Text;
                password = passwordBox.Text;
            }

            var x = VisualStateManager.GoToState(this, nameof(runningState), true);

            statusBlock4.Text = $"Server at IPv4 is listening at port {port}";
            statusBlock6.Text = $"Server at IPv6 is listening at port {port}";

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
                        new Zhaobang.FtpServer.File.SimpleFileProviderFactory(rootPath),
                        new Zhaobang.FtpServer.Connections.LocalDataConnectionFactory(),
                        new Zhaobang.FtpServer.Authenticate.SimpleAuthenticator(userName, password));
                    server6 = new FtpServer(
                        ep6,
                        new Zhaobang.FtpServer.File.SimpleFileProviderFactory(rootPath),
                        new Zhaobang.FtpServer.Connections.LocalDataConnectionFactory(),
                        new Zhaobang.FtpServer.Authenticate.SimpleAuthenticator(userName, password));
                }
            });

            server4Run = server4.RunAsync(cts.Token).ContinueWith(async t =>
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (t.IsFaulted)
                        statusBlock4.Text = $"Server at IPv4 error: \n{t.Exception}";
                    else
                        statusBlock4.Text = "Server at IPv4 has stopped.";

                }));
            server6Run = server6.RunAsync(cts.Token).ContinueWith(async t =>
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (t.IsFaulted)
                        statusBlock6.Text = $"Server at IPv6 error: \n{t.Exception}";
                    else
                        statusBlock6.Text = "Server at IPv6 has stopped.";
                }));
        }

        private void NotifyUser(string v)
        {
            ContentDialog dialog = new ContentDialog
            {
                Content = v,
                CloseButtonText = "Ok"
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
    }
}
