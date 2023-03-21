using Chief.Core;
using Chief.Models;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Chief.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Page
    {
        public MainView()
        {
            ThemeManager.Current.ChangeTheme(this, Core.SystemInfo.IsLightTheme() ? "Light.Blue" : "Dark.Blue");
            InitializeComponent();
            AnimationSwitch.IsOn = ChiefConfigs.AnimationEnable;
            GetReleaseList();
        }

        private async Task GetReleaseList()
        {
            List<Models.Core.CommitInfo> revCommits = new();
            try
            {
                revCommits = await Requests.GetReleaseInfo();
            }
            catch
            {
                RevLoading.IsActive = false;
                RevList.Visibility = Visibility.Hidden;
                VersionListAlert.Visibility = Visibility.Visible;
                return;
            }

            List<Models.Core.TinyCommitInfo> tinyCommits = revCommits.Select(revCommit => new Models.Core.TinyCommitInfo() { Version = revCommit.Version }).ToList();

            RevLoading.IsActive = false;
            RevList.ItemsSource = tinyCommits;
            RevList.Columns.Clear();
            RevList.Columns.Add(new DataGridTextColumn()
            {
                Header = "Woolang 发行版本列表",
                Binding = new System.Windows.Data.Binding("Version")
            });


            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(System.TimeSpan.FromSeconds(1)));
            fadeOut.Completed += (sender, e) => { RevLoading.IsActive = false; };
            RevLoading.BeginAnimation(ProgressRing.OpacityProperty, fadeOut);
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var flipview = ((FlipView)sender);
            switch (flipview.SelectedIndex)
            {
                case 0:
                    flipview.BannerText = "及时查看最新的 Woolang 构建信息！";
                    break;
                case 1:
                    flipview.BannerText = "嗝";
                    break;
            }
        }

        private void RevChannel_Click(object sender, RoutedEventArgs e)
        {
            var animation = new AnimationControl();
            animation.FadeSwitch(this, new ReleaseView());
        }

        private void BaoZi_Click(object sender, RoutedEventArgs e)
        {
            var animation = new AnimationControl();
            animation.FadeSwitch(this, new BaoZiInstall());
        }

        private void ReadTheDocs_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://git.cinogama.net/cinogamaproject/woolang/-/wikis/home")
            {
                UseShellExecute = true
            });
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyOut.IsOpen = !SettingsFlyOut.IsOpen;
        }

        private void AnimationSettings_OnToggled(object sender, RoutedEventArgs e)
        {
            ChiefConfigs.AnimationEnable = AnimationSwitch.IsOn;
            ChiefConfigs.SaveConfig();
        }
    }
}
