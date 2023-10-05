using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server.Pages
{
    /// <summary>
    /// Build.xaml 的交互逻辑
    /// </summary>
    public partial class Build
    {
        public Build()
        {
            InitializeComponent();
        }

        private void Build__Click(object sender, RoutedEventArgs e)
        {
        }

    }
    public static class ProcessExtensions
    {
        public static Task<bool> WaitForExitAsync(this Process process, int timeoutMilliseconds = -1)
        {
            var tcs = new TaskCompletionSource<bool>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(true);

            if (timeoutMilliseconds >= 0)
            {
                var timeoutTimer = new System.Timers.Timer(timeoutMilliseconds);
                timeoutTimer.Elapsed += (sender, args) =>
                {
                    timeoutTimer.Stop();
                    tcs.TrySetResult(false);
                };
                timeoutTimer.Start();
            }

            return tcs.Task;
        }
    }
}
