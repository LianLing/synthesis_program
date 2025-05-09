using System.Windows;
using synthesis_program.Service;
using synthesis_program.Views;
using synthesis_program.Tools;

namespace synthesis_program
{
    public partial class App : Application
    {
        public NavigationService NavigationService { get; private set; }
        private static System.Threading.Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {

            // 1. 创建唯一命名的Mutex（推荐用GUID或应用唯一标识）
            const string mutexName = "synthesis_program";
            bool createdNew;
            _mutex = new System.Threading.Mutex(true, mutexName, out createdNew);

            // 2. 检查是否已存在实例
            if (!createdNew)
            {
                MessageBox.Show("程序已运行，请勿重复启动！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                Current.Shutdown(); // 关闭当前实例
                return;
            }

            base.OnStartup(e);

            if (!Hts.Init(e.Args))
            {
                MessageBox.Show($"{Hts.ErrCode}: {Hts.ErrMsg}", "初始化失败", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown(); // 初始化失败时退出
                return;
            }

            // 先创建主窗口实例
            var mainWindow = new MainWindow();

            // 初始化导航服务
            var navService = new NavigationService();
            navService.Initialize(mainWindow.MainFrame);

            // 将服务存入Application属性
            Properties["NavigationService"] = navService;


        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex(); // 释放Mutex
            base.OnExit(e);
        }
    }
}