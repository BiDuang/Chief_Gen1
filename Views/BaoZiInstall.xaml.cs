using Chief.Core;
using ControlzEx.Theming;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using LibGit2Sharp;
using Path = System.IO.Path;

namespace Chief.Views
{
    /// <summary>
    /// BaoZiInstall.xaml 的交互逻辑
    /// </summary>
    public partial class BaoZiInstall : Page
    {
        public BaoZiInstall()
        {
            ThemeManager.Current.ChangeTheme(this, Core.SystemInfo.IsLightTheme() ? "Light.Blue" : "Dark.Blue");
            InitializeComponent();
        }

        private bool InstallBaoZi()
        {
            string selectedPath = string.Empty;
            try
            {
                Dispatcher.Invoke(() =>
                {
                    InstallSettings.Visibility = Visibility.Hidden;
                    InstallingPanel.Visibility = Visibility.Visible;
                    ProgressRing.IsActive = false;
                    TitleText.Content = "好的，请您指定包子的安装目录...";
                    var pathBrowserDialog = new FolderBrowserDialog()
                    {
                        ShowNewFolderButton = true,
                        AutoUpgradeEnabled = true
                    };



                    if (pathBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        selectedPath = Path.Combine(pathBrowserDialog.SelectedPath, "BaoZi");
                    }
                    else
                    {
                        ReturnIndex();
                        throw new Exception("用户取消了安装");
                    }
                    ProgressRing.IsActive = true;
                    TitleText.Content = "正在获取包子...";
                    NeedWait.Visibility = Visibility.Visible;
                });
            }
            catch
            {
                return false;
            }


            string baoziPath;
            try
            {
                baoziPath = Repository.Clone("https://git.cinogama.net/cinogamaproject/woolangpackages/baozi.git", selectedPath);
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show("尝试获取 包子 时出错\n" + e.Message, "Chief");
                });
                return false;
            }

            try
            {
                Dispatcher.Invoke(() =>
                {
                    TitleText.Content = "正在升级子模块...";
                });
                using var repo = new Repository(baoziPath);
                foreach (var submodule in repo.Submodules)
                {
                    var options = new SubmoduleUpdateOptions()
                    {
                        Init = true
                    };
                    repo.Submodules.Update(submodule.Name, options);
                }
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show("尝试更新子模块时出错\n" + e.Message, "Chief");
                });
                return false;
            }

            Dispatcher.Invoke(() =>
            {
                NeedWait.Visibility = Visibility.Hidden;
                TitleText.Content = "正在完成安装...";
            });

            const string variable = "PATH";
            var value = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);
            value += ";" + selectedPath;
            Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.User);

            return true;

        }

        private bool CheckDepend()
        {
            Dispatcher.Invoke(() =>
            {
                TitleText.Content = "正在进行安装前环境检查，请稍候...";
                InstallSettings.Visibility = Visibility.Hidden;
                InstallingPanel.Visibility = Visibility.Visible;
            });

            var msbuildPath = string.Empty;

            if (Environment.GetEnvironmentVariable("MSBUILD", EnvironmentVariableTarget.User) != null)
            {
                msbuildPath = Environment.GetEnvironmentVariable("MSBUILD", EnvironmentVariableTarget.User);
            }
            else if (Environment.GetEnvironmentVariable("MSBUILD", EnvironmentVariableTarget.Machine) != null)
            {
                msbuildPath = Environment.GetEnvironmentVariable("MSBUILD", EnvironmentVariableTarget.Machine);
            }
            else
            {
                string vswherePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Microsoft Visual Studio", "Installer", "vswhere.exe");
                if (!File.Exists(vswherePath))
                {
                    var result = System.Windows.MessageBox.Show("未在您的计算机上检测到 Visual Studio Installer\n点击\"确认\"前往下载页面", "Chief",
                        MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        Process.Start(new ProcessStartInfo("https://visualstudio.microsoft.com")
                        {
                            UseShellExecute = true
                        });
                    }
                    return false;
                }
                var cmd = new ProcessStartInfo(vswherePath)
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                var process = new Process()
                {
                    StartInfo = cmd
                };
                try
                {
                    process.Start();
                }
                catch
                {
                    System.Windows.MessageBox.Show("尝试获取 Visual Studio 安装信息时出错", "Chief");
                    ReturnIndex();
                    return false;
                }
                var outPut = process.StandardOutput.ReadToEnd();
                var installationPath = outPut.Split(new[] { "installationPath" }, StringSplitOptions.None)[1]
                    .Split(new[] { "installationVersion" }, StringSplitOptions.None)[0].Split(new[] { ": " }, StringSplitOptions.None)[1].Trim();

                msbuildPath = Path.Combine(installationPath, "Msbuild", "Current", "Bin");
            }

            if (File.Exists(Path.Combine(msbuildPath, "MSBuild.exe")))
            {
                const string variable = "MSBUILD";
                Environment.SetEnvironmentVariable(variable, msbuildPath, EnvironmentVariableTarget.User);
                return true;
            };
            System.Windows.MessageBox.Show("未能找到 MSBuild.exe", "Chief");
            return false;
        }

        private void ReturnIndex()
        {
            var animation = new AnimationControl();
            animation.FadeSwitch(this, new MainView());
        }

        private async void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            Task<bool> checkDependTask = new Task<bool>(CheckDepend);
            checkDependTask.Start();
            if (!await checkDependTask)
            {
                ReturnIndex();
                return;
            }
            ProgressRing.IsActive = true;
            Task<bool> installTask = new Task<bool>(InstallBaoZi);
            installTask.Start();
            if (await installTask)
            {
                var animation = new AnimationControl();
                animation.FadeSwitch(this, new InstallBaoZiSuccess());
                return;
            }
            ReturnIndex();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ReturnIndex();
        }
    }
}
