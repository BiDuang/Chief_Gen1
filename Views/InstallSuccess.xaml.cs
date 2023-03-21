using Chief.Core;
using ControlzEx.Theming;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Chief.Views
{
    /// <summary>
    /// InstallSuccess.xaml 的交互逻辑
    /// </summary>
    public partial class InstallSuccess : Page
    {
        public InstallSuccess()
        {
            ThemeManager.Current.ChangeTheme(this, Core.SystemInfo.IsLightTheme() ? "Light.Blue" : "Dark.Blue");
            InitializeComponent();
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            var animation = new AnimationControl();
            animation.FadeSwitch(this, new MainView());
        }

        private void InstallBaoZi_Click(object sender, RoutedEventArgs e)
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
    }
}
