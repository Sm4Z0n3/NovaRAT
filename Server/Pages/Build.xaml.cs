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
            string ipAddressText = IPADDRESS_.Text;
            string portText = PORT_.Text;

            bool isPortValid = IsValidPort(portText);

            if (isPortValid)
            {
                string fileContent = File.ReadAllText("Client\\Program.cs");

                // 使用正则表达式替换文本
                fileContent = fileContent.Replace(@"{CONNECTIPADDRESS}", ipAddressText);
                fileContent = fileContent.Replace(@"{CONNECTPORT}", portText);

                // 将修改后的内容写回文件
                File.WriteAllText("Client\\Program.cs", fileContent);
                BuildAsync();
            }
            else
            {
                MessageBox.Show("端口无效！");
            }
        }

        static async Task BuildAsync()
        {
            string projectDirectory = @"Client"; // 替换为实际项目目录的路径

            // 创建一个进程启动信息
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = psi };
            process.Start();

            process.StandardInput.WriteLine("cd " + projectDirectory);
            process.StandardInput.WriteLine("dotnet build");

            // 异步等待进程完成
            await process.WaitForExitAsync();

            string output = process.StandardOutput.ReadToEnd();

            process.Close();

            Console.WriteLine(output);
        }

        private bool IsValidIpAddress(string ipAddress)
        {
            IPAddress address;
            return IPAddress.TryParse(ipAddress, out address);
        }

        private bool IsValidPort(string port)
        {
            int portNumber;
            if (int.TryParse(port, out portNumber))
            {
                return portNumber >= 1 && portNumber <= 65535;
            }
            return false;
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
