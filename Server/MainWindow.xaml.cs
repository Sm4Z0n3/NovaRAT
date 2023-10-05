using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Server
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try{this.Main_.MouseLeftButtonDown += (o, e) => { DragMove(); };}catch { }
            Loaded += (serder, e) => {
                var fadeInAnimation = new DoubleAnimation{From = 0,To = 1,Duration = TimeSpan.FromSeconds(0.5)};
                BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        private void Home_btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Uri pageUri = new Uri($"Pages/Home.xaml", UriKind.Relative);
            MainFrame.Navigate(pageUri);
        }

        private void Files_btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Uri pageUri = new Uri($"Pages/Files.xaml", UriKind.Relative);
            //MainFrame.Navigate(pageUri);
            MessageBox.Show("有点难写，下次一定.");
        }

        private void Build_btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Uri pageUri = new Uri($"Pages/Build.xaml", UriKind.Relative);
            MainFrame.Navigate(pageUri);
        }
    }


}
