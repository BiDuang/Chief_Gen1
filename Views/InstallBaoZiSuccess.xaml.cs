using Chief.Core;
using ControlzEx.Theming;
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
    /// InstallSuccess.xaml 的交互逻辑
    /// </summary>
    public partial class InstallBaoZiSuccess : Page
    {
        public InstallBaoZiSuccess()
        {
            ThemeManager.Current.ChangeTheme(this, Core.SystemInfo.IsLightTheme() ? "Light.Blue" : "Dark.Blue");
            InitializeComponent();
        }

        private void ReturnIndex_Click(object sender, RoutedEventArgs e)
        {
            var animation = new AnimationControl();
            animation.FadeSwitch(this, new MainView());
        }

        private void UpgradeWoolang_Click(object sender, RoutedEventArgs e)
        {
            var animation = new AnimationControl();
            animation.FadeSwitch(this, new ReleaseView());
        }
    }
}
