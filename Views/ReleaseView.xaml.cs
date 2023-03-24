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
using System.Windows.Media.Animation;
using ControlzEx.Theming;
using Downloader;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using Chief.Core;
using Button = System.Windows.Controls.Button;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Chief.Views
{
    /// <summary>
    /// ReleaseView.xaml 的交互逻辑
    /// </summary>
    public partial class ReleaseView : Page
    {
        public bool IsWoolangInstalled;

        public ReleaseView()
        {
            Task task = new Task(() => { IsWoolangInstalled = Core.SystemInfo.IsWoolangInstalled(); });
            task.Start();
            ThemeManager.Current.ChangeTheme(this, Core.SystemInfo.IsLightTheme() ? "Light.Blue" : "Dark.Blue");
            task.Wait();
            InitializeComponent();
            InitialSettings();
        }

        private void InitialSettings()
        {
            WelcomeMessage.Content = IsWoolangInstalled
                ? "您希望如何更新 Woolang 编译器？"
                : "您希望如何安装 Woolang 编译器？";
            UpdateButton.IsEnabled = IsWoolangInstalled;
            if (!IsWoolangInstalled)
            {
                UpdateButtonGrid.ToolTip = "您尚未安装 Woolang 编译器、未正确设置环境变量\n或更改了编译器名称，因此选项不可用。";
            }
        }


        public void OnDownloadCompleted(object? sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            Dispatcher.Invoke(() => { TitleText.Content = "正在安装 Woolang 编译器更新，请稍候..."; });
        }

        public async Task DownloadLatestBuild(string workingDir, bool isPathSetting = false)
        {
            if (workingDir == string.Empty || !Directory.Exists(workingDir))
            {
                System.Windows.MessageBox.Show("指定的安装路径错误", "Chief");
                return;
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
                throw new Exception("Failed to reach the update server!");
            }

            var resp = await response.Content.ReadAsStringAsync();
            var data = JArray.Parse(resp);
            var jobs =
                from item in data
                select (JObject)item;
            string latestBuildId = "";
            foreach (var item in jobs)
            {
                if (item["name"].ToString().Equals("build_release_win32"))
                {
                    latestBuildId = (string)item["id"];
                    break;
                }
            }

            if (File.Exists(workingDir + "woodriver.exe"))
            {
                File.Delete(workingDir + "woodriver.exe");
            }

            if (File.Exists(workingDir + "libwoo.dll"))
            {
                File.Delete(workingDir + "libwoo.dll");
            }

            string latestBuildUrl = $"https://git.cinogama.net/cinogamaproject/woolang/-/jobs/{latestBuildId}/artifacts/download";

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
                string fileName = Path.GetFileName(theEntry.Name);


                if (fileName != string.Empty)
                {
                    await using FileStream streamWriter = File.Create(workingDir + fileName);
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

            if (isPathSetting)
            {
                var variable = "PATH";
                var value = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);
                value += ";" + workingDir[..^2];
                Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.User);
            }

            var animation = new AnimationControl();
            animation.FadeSwitch(this, new InstallSuccess());
        }

        private void ReturnIndex_Click(object sender, RoutedEventArgs e)
        {
            var animation = new AnimationControl();
            animation.FadeSwitch(this, new MainView());
        }

        private void UpdateButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string woolangDir = Core.SystemInfo.GetWoolangDir();
                DownloadLatestBuild(woolangDir);
                InstallSettings.Visibility = Visibility.Hidden;
                InstallingPanel.Visibility = Visibility.Visible;
            }
            catch
            {
                System.Windows.MessageBox.Show("尝试更新 Woolang 编译器时出错。\n如果此问题持续出现，请联系技术人员。", "Chief Error");
            }
        }

        private void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            var senderName = (sender as Button)?.Name;

            var dialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                AutoUpgradeEnabled = true
            };
            string selectedPath;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                selectedPath = dialog.SelectedPath + "\\";
            }
            else
            {
                return;
            }
            try
            {
                DownloadLatestBuild(selectedPath, senderName == "InstallButton");
                InstallSettings.Visibility = Visibility.Hidden;
                InstallingPanel.Visibility = Visibility.Visible;
            }
            catch
            {
                System.Windows.MessageBox.Show("尝试更新 Woolang 编译器时出错。\n如果此问题持续出现，请联系技术人员。", "Chief Error");
            }
        }

        private void UpdateButton_OnMouseEnter(object sender, MouseEventArgs e)
        {
            WelcomeMessage.FontSize = 16;
            WelcomeMessage.Content = "将会更新 " + Core.SystemInfo.GetWoolangDir() + " 中的 Woolang 编译器。";
        }


        private void UpdateButton_OnMouseLeave(object sender, MouseEventArgs e)
        {
            WelcomeMessage.FontSize = 24;
            WelcomeMessage.Content = IsWoolangInstalled
                ? "您希望如何更新 Woolang 编译器？"
                : "您希望如何安装 Woolang 编译器？";
        }
    }
}
