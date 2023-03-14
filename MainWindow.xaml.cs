using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;

namespace Chief
{
    /// <summary>
    /// InitializeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public MainWindow()
        {
            InitializeComponent();
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(System.TimeSpan.FromSeconds(1)))
            {
                BeginTime = System.TimeSpan.FromMilliseconds(1000)
            };
            fadeOut.Completed += (sender, e) => { Logo.Visibility = Visibility.Hidden; };
            Logo.BeginAnimation(Image.OpacityProperty, fadeOut);
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, new Duration(System.TimeSpan.FromSeconds(1)))
            {
                BeginTime = System.TimeSpan.FromMilliseconds(2000)
            };
            LoadingInfo.BeginAnimation(Grid.OpacityProperty, fadeIn);
            Task.Run(() =>
            {
                Thread.Sleep(4500);
                Dispatcher.Invoke(() =>
                {
                    Views.IndexWindow indexWindow = new();
                    this.Close();
                    indexWindow.Show();
                });
            });
        }
    }
}