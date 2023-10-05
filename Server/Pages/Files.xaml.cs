using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Server.Pages
{
    /// <summary>
    /// Files.xaml 的交互逻辑
    /// </summary>
    public partial class Files
    {
        private string folderPath = @"C:\";
        public Files()
        {
            InitializeComponent();
            LoadContents(folderPath);
            PathBox.Text = folderPath;
        }

        void LoadContents(string folderPath)
        {
            if(folderPath.IndexOf(@"\") != -1)
            {
                folderPath += @"\";
            }
            DirectoryInfo folderInfo = new DirectoryInfo(folderPath);
            if (!folderInfo.Exists)
            {
                MessageBox.Show("文件夹不存在。");
                return;
            }

            // 清空列表
            FilesList.Items.Clear();

            var files = new List<FileSystemInfo>();

            // 添加文件和文件夹到列表
            try
            { 
                files.AddRange(folderInfo.GetDirectories());
                files.AddRange(folderInfo.GetFiles());
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }

            foreach (var item in files)
            {
                FilesList.Items.Add(new
                {
                    Name = item.Name,
                    Size = (item is FileInfo) ? ((FileInfo)item).Length : 0,
                    Type = (item is DirectoryInfo) ? "文件夹" : "文件"
                });
            }
            PathBox.Text = folderPath;
        }

        private void ReturnToParentFolder()
        {
            DirectoryInfo currentFolder = new DirectoryInfo(folderPath);
            if (currentFolder.Parent != null)
            {
                int lastSeparatorIndex = folderPath.LastIndexOf(@"\");
                if (lastSeparatorIndex != -1)
                {
                    string newPath = folderPath.Substring(0, lastSeparatorIndex);
                    folderPath = newPath;
                }
                LoadContents(currentFolder.Parent.FullName);
            }
        }


        private void Home_btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ReturnToParentFolder();
        }

        private void FilesList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (folderPath.IndexOf(@"\") != -1)
            {
                folderPath += @"\";
            }
            var selectedItems = FilesList.SelectedItems;

            if (selectedItems.Count > 0)
            {
                var selectedItem = selectedItems[0];
                string itemName = ((dynamic)selectedItem).Name;
                string itemPath = Path.Combine(folderPath, itemName);
                if (Directory.Exists(itemPath))
                {
                    folderPath = itemPath; // 更新 currentFolderPath
                    LoadContents(folderPath); // 加载新文件夹的内容
                }
            }
        }

        private void PathBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                LoadContents(PathBox.Text);
                folderPath = PathBox.Text;
            }
        }

        private void UploadFile_Click(object sender, RoutedEventArgs e)
        {
            // 在这里编写上传文件的逻辑
            MessageBox.Show("上传文件");
        }

        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {
            // 在这里编写下载文件的逻辑
            MessageBox.Show("下载文件");
        }

    }
}
