using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ControlzEx.Theming;
using Downloader;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;

namespace Chief.Views
{
    /// <summary>
    /// ReleaseView.xaml 的交互逻辑
    /// </summary>
    public partial class ReleaseView : Page
    {
        public bool isWoolangInstalled = false;

        public ReleaseView()
        {
            isWoolangInstalled = Core.SystemInfo.IsWoolangInstalled();
            ThemeManager.Current.ChangeTheme(this, Core.SystemInfo.isLightTheme() ? "Light.Blue" : "Dark.Blue");
            InitializeComponent();
            InitialSettings();

        }

        private void InitialSettings()
        {
            WelcomeMessage.Content = isWoolangInstalled
                ? "您希望如何更新 Woolang 编译器？"
                : "您希望如何安装 Woolang 编译器？";
            UpdateButton.IsEnabled = isWoolangInstalled;
            if (!isWoolangInstalled)
            {
                UpdateButtonGrid.ToolTip = "您尚未安装 Woolang 编译器、未正确设置环境变量\n或更改了编译器名称，因此选项不可用。";
            }
        }


        public void OnDownloadCompleted(object? sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            Dispatcher.Invoke(() => { Title.Content = "正在安装 Woolang 编译器更新，请稍候..."; });
        }

        public async Task DownloadLatestBuild(string workingDir)
        {
            if (workingDir == string.Empty || !Directory.Exists(workingDir))
            {
                MessageBox.Show("指定的安装路径错误", "Chief");
                return;
            }

            if (File.Exists(workingDir + "woodriver.exe"))
            {
                File.Delete(workingDir + "woodriver.exe");
            }

            if (File.Exists(workingDir + "libwoo.dll"))
            {
                File.Delete(workingDir + "libwoo.dll");
            }

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer 7gz8QHnPyB_9HWtyjdfP");
            var response = await client.GetAsync(new Uri("https://git.cinogama.net/api/v4/projects/68/jobs"));
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                return;
            }

            var resp = await response.Content.ReadAsStringAsync();
            var data = JArray.Parse(resp);
            var jobs =
                from item in data
                select (JObject)item;
            string latestBuildId = "";
            foreach (var item in jobs)
            {
                if ((string)item["name"] == "build_release_win32")
                {
                    latestBuildId = (string)item["id"];
                }
            }

            string latestBuildUrl = $"https://git.cinogama.net/cinogamaproject/woolang/-/jobs/{latestBuildId}/artifacts/download";
            DirectoryInfo path = new DirectoryInfo(workingDir);
            var downloadOpt = new DownloadConfiguration()
            {
                ChunkCount = 8,
                ParallelDownload = true
            };

            var downloader = new DownloadService(downloadOpt);

            downloader.DownloadFileCompleted += OnDownloadCompleted;
            Stream destinationStream = await downloader.DownloadFileTaskAsync(latestBuildUrl);
            await using var s = new ZipInputStream(destinationStream);
            while (s.GetNextEntry() is { } theEntry)
            {
                string fileName = System.IO.Path.GetFileName(theEntry.Name);


                if (fileName != string.Empty)
                {
                    using (FileStream streamWriter = File.Create(workingDir + fileName))
                    {
                        byte[] datBytes = new byte[2048];
                        while (true)
                        {
                            var size = s.Read(datBytes, 0, datBytes.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(datBytes, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            var window = Window.GetWindow(this);
            var contentControl = window.FindName("ContentControl") as ContentControl;
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (sender, args) =>
            {
                contentControl.Content = new Frame()
                {
                    Content = new InstallSuccess()
                };
                contentControl.BeginAnimation(OpacityProperty, fadeIn);
            };
            contentControl.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void ReturnIndex_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            var contentControl = window.FindName("ContentControl") as ContentControl;
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (sender, args) =>
            {
                contentControl.Content = new Frame()
                {
                    Content = new MainView()
                };
                contentControl.BeginAnimation(OpacityProperty, fadeIn);
            };
            contentControl.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void UpdateButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string woolangDir = Core.SystemInfo.GetWoolangDir();
                DownloadLatestBuild(woolangDir).Start();
                InstallSettings.Visibility = Visibility.Hidden;
                InstallingPanel.Visibility = Visibility.Visible;
            }
            catch
            {
                MessageBox.Show("尝试更新 Woolang 编译器时出错。\n如果此问题持续出现，请联系技术人员。", "Chief Error");
            }
        }

        private void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
