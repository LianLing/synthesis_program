
using HtsCommon.DBMySql8;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using static HtsCommon.DBMySql8.HtsDB;
using iHtsDB = HtsCommon.iDBMySql8.HtsDB;

namespace synthesis_program.Tools
{
    public static class Hts
    {
        /// <summary>
        /// 当前工作路径
        /// </summary>
        public static string WorkPath { set; get; }
        /// <summary>
        /// 最后的错误信息，当返回失败时，错误信息通过ErrMsg获取
        /// </summary>
        public static string ErrMsg { set; get; }
        public static string ErrCode { set; get; }
        ///// <summary>
        /// 登录用户账号ID
        /// </summary>
        public static string UserID { set; get; }
        /// <summary>
        /// 登录用户名称
        /// </summary>
        public static string UserName { set; get; }
        /// <summary>
        /// 机型代码
        /// </summary>
        public static string TypeCode { set; get; }
        /// <summary>
        /// 机型名称
        /// </summary>
        public static string TypeName { set; get; }

        /// <summary>
        /// 模组代码
        /// </summary>
        public static string ModuleCode { set; get; }
        /// <summary>
        /// 模组名称
        /// </summary>
        public static string ModuleName { set; get; }

        /// <summary>
        /// 当前模组产出品号
        /// </summary>
        public static string ModulePn { set; get; }

        /// <summary>
        /// 工艺代码
        /// </summary>
        public static string StageCode { set; get; }
        /// <summary>
        /// 工艺名称
        /// </summary>
        public static string StageName { set; get; }
        /// <summary>
        /// 制程代码
        /// </summary>
        public static string ProcessCode { set; get; }
        /// <summary>
        /// 制程名称
        /// </summary>
        public static string ProcessName { set; get; }
        /// <summary>
        /// 站点代码
        /// </summary>
        public static string StationCode { set; get; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public static string StationName { set; get; }
        public static string Line { set; get; }
        public static string Team { set; get; }
        public static string LineName { set; get; }

        ///// <summary>
        ///// 流线表中下一个站点的站点代码
        ///// </summary>
        //public static string NextStationCode { set; get; }


        /// <summary>
        /// 版本代码
        /// </summary>
        public static string ProdVerCode { set; get; }
        /// <summary>
        /// 版本名称
        /// </summary>
        public static string ProdVerName { set; get; }
        /// <summary>
        /// 工单
        /// </summary>
        public static string ProdMo { set; get; }
        /// <summary>
        /// 通过个站作业界面选择的工单拼接字符串
        /// </summary>
        public static string ProdMoSelect { set; get; }
        /// <summary>
        /// 当前站点选择的工单
        /// </summary>
        public static string StationMo { set; get; }
        /// <summary>
        /// 当前站点选择订单
        /// </summary>
        public static string StationPo { set; get; }
        /// <summary>
        /// 当前站点选择的品号
        /// </summary>
        public static string StationPn { set; get; }


        /// <summary>
        /// 是否是多版本
        /// </summary>
        public static bool IsMuteVersion { set; get; }
        /// <summary>
        /// 个站工具版本号
        /// </summary>
        public static string ToolVersion { set; get; }
        /// <summary>
        /// HTS版本号
        /// </summary>
        public static string HtsVersion { set; get; }
        public static string HtsDBVersion { set; get; }
        public static string HtsDBCompail { set; get; }
        /// <summary>
        /// 调试Log等级
        /// </summary>
        public static int DbgLevel { set; get; }
        /// <summary>
        /// 登录用户角色ID
        /// </summary>
        //public static int UserRoleId { set; get; }
        public static string UserRoleId { set; get; }
        public static bool IsUploadEnable { set; get; }
        public static bool IsOperater { set; get; }
        /// <summary>
        /// 登录用户角色名称
        /// </summary>
        public static string UserRoleName { set; get; }
        /// <summary>
        /// 个站工具配置文件文件名
        /// </summary>
        //public static string CfgFileSuggest { set; get; }
        public static string CfgFile { set; get; }
        /// <summary>
        /// 个站工具配置文件字段名
        /// </summary>
        public static string CfgSection { set; get; }
        /// <summary>
        /// 通用配置关键字 [CONFIG_模组码_站点码_XXX]
        /// </summary>
        public static string ComCfgSection { set; get; }
        /// <summary>
        /// 个站配置文件文件名
        /// </summary>
        public static string UploadCfgFile { set; get; }
        public static string UploadCfgSection { set; get; }
        //public static string StationCfgFile { set; get; }
        /// <summary>
        /// 个站配置文件字段名
        /// </summary>
        //public static string StationCfgSection { set; get; }
        /// <summary>
        /// 本地打印机设置配置文件
        /// </summary>
        //public static string PrtCfgFile { set; get; }
        public static string LocalCfgFile { set; get; }
        public static string LocalCfgSection { set; get; }
        /// <summary>
        /// 本地打印机设置配置字段名
        /// </summary>
        //public static string PrtCfgSection { set; get; }

        /// <summary>
        /// HTS服务器IP地址
        /// </summary>
        public static string HtsDBIp { set; get; }
        /// <summary>
        /// HTS服务器IP地址
        /// </summary>
        public static string HtsErpIp { set; get; }
        /// <summary>
        /// HTS版本服务器
        /// </summary>
        public static string HtsVerIp { set; get; }

        /// <summary>
        /// 厂区码 ： NJ / VT
        /// </summary>
        public static string Factory { set; get; }

        /// <summary>
        /// HTS服务器是否是调试服务器
        /// </summary>
        public static bool IsDbgHtsDB { set; get; }

        /// <summary>
        /// 是否由FTM测试或adb测试启动
        /// </summary>
        public static bool IsFtmTest { set; get; }

        public static bool IsHtsOfflineMode { set; get; }

        private const int DBGLEVEL = 2;
  
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool Init(string[] args)
        {
            ErrCode = "";
            ErrMsg = "";

            if (args.Length == 0)
            {
                //ErrCode=DefCode.Config; 
                ErrMsg = "请从HTS打开工具";
                return false;
            }

            if (args[0] != "ProcessStartedByHtsFramework" && args[0] != "FTMTEST" && args[0] != "UserID")
            {
                ErrMsg = $"请从HTS打开工具";
                return false;
            }

            return true;
        }


    }
}
