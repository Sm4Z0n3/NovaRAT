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
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;
using Microsoft.SqlServer.Server;

namespace Server.Page
{
    /// <summary>
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home
    {
        public TcpListener listener;
        public TcpClient client;

        public Home()
        {
            InitializeComponent();
            Loaded += (sender, e) => {
                var fadeInAnimation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5) };
                BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
            };

            StartServerAsync(); // 启动服务器异步方法
        }

        private async Task StartServerAsync()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 12345); // 12345 是服务器端口号
                listener.Start();
                Console.WriteLine("服务器已启动，正在监听端口 12345...");
                while (true)
                {
                    client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("客户端已连接...");

                    await HandleClientAsync(client);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            
            IPEndPoint remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            string ipAddress = remoteEndPoint.Address.ToString();
            int port = remoteEndPoint.Port;
            string newItem = $"{ipAddress} | {port}";
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    if (!Devices.Items.Contains(newItem))
                    {
                        Devices.Items.Add(newItem);
                    }
                });

                byte[] buffer = new byte[1024];
                int bytesRead;
                NetworkStream stream = client.GetStream();
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(">>> " + dataReceived);

                    await Dispatcher.InvokeAsync(() =>
                    {
                        Log_.Text += dataReceived + "\n";
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误: " + ex.Message);
            }
            finally
            {
                client.Close();

                await Dispatcher.InvokeAsync(() =>
                {
                    Devices.Items.Remove(newItem);
                });
            }
        }


        private async void CommandTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string mode = "cmd";
                    if(MsgMode.SelectedIndex == 0)
                        mode = "command";
                    if (MsgMode.SelectedIndex == 1)
                        mode = "string";
                    NetworkStream stream = client.GetStream();
                    byte[] responseBuffer = Encoding.UTF8.GetBytes(mode+"|"+CommandTextBox.Text);
                    await stream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
                    Log_.Text += ">>>" + CommandTextBox.Text + "\n";
                    CommandTextBox.Text = "";
                }
            }
            catch (Exception ex)
            {
                Log_.Text += ex.Message.ToString() + "\n";
            }
        }

    }
}
