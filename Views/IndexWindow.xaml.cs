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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Chief.Core;
using Chief.Models;
using ControlzEx.Theming;
using MahApps.Metro.Controls;

namespace Chief.Views
{
    /// <summary>
    /// IndexWindow.xaml 的交互逻辑
    /// </summary>
    public partial class IndexWindow : MetroWindow
    {

        Views.MainView mainView = new();
        public IndexWindow()
        {
            ThemeManager.Current.ChangeTheme(this, Core.SystemInfo.IsLightTheme() ? "Light.Blue" : "Dark.Blue");
            InitializeComponent();
            ContentControl.Content = new Frame() { Content = mainView };
        }

    }
}