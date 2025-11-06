using MiscApi;
using synthesis_program.Interface;
using synthesis_program.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Path = System.IO.Path;

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
            //尝试控制菜单是否可用
            ControlMenuItem();
        }

        private void ControlMenuItem()
        {
            string folderPath = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                // 2. 获取所有txt文件
                string txtFiles = Directory.GetFiles(folderPath, "ControlMenuItemConfig.txt").First();

                // 检查是否找到文件
                if (txtFiles.Length == 0)
                {
                    Console.WriteLine("文件夹中未找到txt文件");
                    return;
                }

                // 3. 创建列表存储结果
                List<string> fileContents = new List<string>();

                // 4. 遍历并读取每个文件
                
                try
                {
                    // 使用StreamReader自动处理编码和资源释放
                    using (StreamReader reader = new StreamReader(txtFiles, Encoding.UTF8))
                    {
                        string content = reader.ReadToEnd();
                        fileContents.Add($"文件: {Path.GetFileName(txtFiles)}\n内容:\n{content}\n");
                        switch (content)
                        {
                            case "ServiceTable":
                                ServiceTable.IsEnabled = true;
                                break;
                            case "WholeThrouthRate":
                                WholeThrouthRate.IsEnabled = true;
                                break;
                            case "RecordProductInfo":
                                RecordProductInfo.IsEnabled = true;
                                break;
                            case "DirectRate":
                                DirectRate.IsEnabled = true;
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    fileContents.Add($"读取失败 {Path.GetFileName(txtFiles)}: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"未知错误: {ex.Message}");
            }
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

        private void NSSMConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mainFrame_Navigated()
        {

        }

        private void DirectConfig_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;          // 最大化窗口
            mainFrame.Navigate(new DirectRatePage());
        }

        private void mainFrame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            // 切换当前语言（中文/越南语）
            string currentLang = Misc.GetLanguage();
            string newLang = currentLang == "vi" ? "cn" : "vi";

            // 设置新语言
            Misc.SetLanguage(newLang);

            // 更新语言菜单显示文本
            Language.Header = newLang == "vi" ? "中文" : "Tiếng Việt";

            // 刷新界面语言
            RefreshAllTexts();
        }

        // 刷新所有文本
        private void RefreshAllTexts()
        {
            // 刷新菜单栏文本
            LabelManage.Header = Misc.t("标签管理");
            ServiceTable.Header = Misc.t("标签维护");
            // 其他菜单项类似处理...
            TableReports.Header = Misc.t("报表");
            WholeThrouthRate.Header = Misc.t("直通率");
            RecordProductInfo.Header = Misc.t("生产编号记录");
            ConfigMenu.Header = Misc.t("配置/维护");
            NSSMConfig.Header = Misc.t("NSSM维护项目");
            DirectRate.Header = Misc.t("直通率站点维护");
            Setting.Header = Misc.t("设置");

            // 刷新当前页面
            if (mainFrame.Content is Page currentPage)
            {
                // 通知当前页面刷新文本
                (currentPage as ILanguageRefreshable)?.RefreshTexts();
            }
        }
    }
}