using Chief.Core;
using Chief.Models;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Chief.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Page
    {
        public MainView()
        {
            ThemeManager.Current.ChangeTheme(this, Core.SystemInfo.isLightTheme() ? "Light.Blue" : "Dark.Blue");
            InitializeComponent();
            getReleaseList();
        }

        private async Task getReleaseList()
        {
            List<Models.Core.CommitInfo> revCommits = await Requests.GetReleaseInfo();
            List<View.TinyCommitInfo> tinyCommits = revCommits.Select(revCommit => new View.TinyCommitInfo() { Version = revCommit.Version }).ToList();

            RevList.ItemsSource = tinyCommits;
            RevList.Columns.First().Width = 190;
            RevList.Columns.First().Header = "已发行版本";

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
            var window = Window.GetWindow(this);
            var contentControl = window.FindName("ContentControl") as ContentControl;
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (sender, args) =>
            {
                contentControl.Content = new Frame()
                {
                    Content = new ReleaseView()
                };
                contentControl.BeginAnimation(OpacityProperty, fadeIn);
            };
            contentControl.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
