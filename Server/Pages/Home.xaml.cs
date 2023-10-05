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
            Loaded += (serder, e) => {
                var fadeInAnimation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.5) };
                BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
            };
        }
        static void StartServer()
        {
            // 创建一个TCP监听套接字
            TcpListener listener = new TcpListener(IPAddress.Any, 12345); // 12345 是服务器端口号
            listener.Start();
            Console.WriteLine("服务器已启动，正在监听端口 12345...");

            try
            {
                while (true)
                {
                    // 等待客户端连接
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("客户端已连接...");

                    // 处理客户端连接的线程
                    System.Threading.Thread clientThread = new System.Threading.Thread(() => HandleClient(client));
                    clientThread.Start();
                }
            }
            finally
            {
                listener.Stop();
            }
        }
        static void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("接收到客户端消息: " + dataReceived);

                    // 响应客户端消息（这里简单地将消息返回给客户端）
                    byte[] responseBuffer = Encoding.ASCII.GetBytes("服务器已接收到消息: " + dataReceived);
                    stream.Write(responseBuffer, 0, responseBuffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误: " + ex.Message);
            }
            finally
            {
                client.Close();
                Console.WriteLine("客户端连接已关闭.");
            }
        }
    }
}
