using HtsCommon.DBMySql8;
using MiscApi;
using synthesis_program.Service;
using synthesis_program.Tools;
using synthesis_program.Views;
using System;
using System.IO;
using System.Windows;

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
            string[] EnvArgs = e.Args;
            try
            {
                string myDebugCfg = @"d:\MyDebug.ini";
                if (File.Exists(myDebugCfg) && EnvArgs.Length >= 2)
                {
                    if (EnvArgs[0] == "UserID" && EnvArgs[1] == "UserPwd")
                    {
                        EnvArgs[0] = MiscApi.Misc.GetProfile(myDebugCfg, "CONFIG", "UserID", "");
                        EnvArgs[1] = MiscApi.Misc.GetProfile(myDebugCfg, "CONFIG", "UserPwd", "");
                    }
                }
                if (!HtsDB.Init(EnvArgs))
                {
                    MessageBox.Show(HtsDB.LstMsg.sMsg, "HTS初始化失败");
                    System.Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                System.Environment.Exit(0);
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