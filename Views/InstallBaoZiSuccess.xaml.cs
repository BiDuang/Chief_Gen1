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
            var window = Window.GetWindow(this);
            var contentControl = window!.FindName("ContentControl") as ContentControl;
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (_, _) =>
            {
                contentControl!.Content = new Frame()
                {
                    Content = new MainView()
                };
                contentControl.BeginAnimation(OpacityProperty, fadeIn);
            };
            contentControl!.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void UpgradeWoolang_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            var contentControl = window!.FindName("ContentControl") as ContentControl;
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (_, _) =>
            {
                contentControl!.Content = new Frame()
                {
                    Content = new MainView()
                };
                contentControl.BeginAnimation(OpacityProperty, fadeIn);
            };
            contentControl!.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
