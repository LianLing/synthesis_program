using System.Collections.Generic;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using synthesis_program.Views;

namespace synthesis_program.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Frame MainFrame => this.mainFrame;
        private List<Window> _childWindows = new List<Window>();
        public MainWindow()
        {
            InitializeComponent();

        }

        // 保存记录菜单点击事件
        private void SaveRecord_Click(object sender, RoutedEventArgs e)
        {
            //var page = new synthesis_program.Views.SaveRecordPage();

            //OpenPage(page);
            WindowState = WindowState.Maximized;          // 最大化窗口
            mainFrame.Navigate(new SaveRecordPage());
        }

        // 生成序列号菜单点击事件
        private void GenerateSerialNumber_Click(object sender, RoutedEventArgs e)
        {
            //var page = new synthesis_program.Views.CreateSequencePage();

            //OpenPage(page);
            WindowState = WindowState.Maximized;          // 最大化窗口
            mainFrame.Navigate(new CreateSequencePage());
        }

        protected override void OnClosed(EventArgs e)
        {
            // 关闭所有子窗口
            foreach (var window in _childWindows.ToArray())
            {
                window.Close();
            }

            // 确保应用退出
            Application.Current.Shutdown();

            base.OnClosed(e);
        }

        private void OpenPage(Page page)
        {
            var hostWindow = new Window
            {
                Title = "标签管理",
                WindowState = WindowState.Maximized,          // 关键设置1：最大化窗口
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = page,
                SizeToContent = SizeToContent.Manual,        // 关键设置3：禁用自动尺寸
            };
            hostWindow.Closed += (s, e) => _childWindows.Remove(hostWindow);
            _childWindows.Add(hostWindow);

            // 适配多显示器配置
            hostWindow.Width = SystemParameters.VirtualScreenWidth;
            hostWindow.Height = SystemParameters.VirtualScreenHeight;

            hostWindow.ShowDialog();
        }

        private void EditSequenceRule_Click(object sender, RoutedEventArgs e)
        {

        }

        private void QueryRecords_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;          // 最大化窗口
            mainFrame.Navigate(new ProductPassRate());
        }

        private void ProductRecords_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;          // 最大化窗口
            mainFrame.Navigate(new ProductRecordPage());
        }
    }
}