using HsLibs.Utils.File2Srv;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiscApi
{

    ///
    /// 常用方法集
    /// Luke Liu
    /// 2022/9/4
    /// 

    /// <summary>
    /// 通用Misc 类
    /// </summary>
    public static class Misc
    {
        //public const string THIRDLOG = @"c:\hts\ThirdLog";

        //public static string _logSrvRoot; // 改由HtsDB获取， public const string LOGSRV_ROOT = @"\\10.10.1.144\HtsLogs\StnLogs"; // Log的根目录
        //public const string LOGSRV_LOCAL = @"D:\HtsLocal\StnLogs";

        //public static string _logDirOnSrvBaseRoot; // 这是相对于 root的 路径
        //public const string LOGSRV_FORMAL = "";
        //public const string LOGSRV_SIMULATE = "simulate";


        public static string ErrMsg;

        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        //private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);


        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(int Description, int ReservedValue);

        public static bool IsConnectInternet()
        {
            int Description = 0;
            return InternetGetConnectedState(Description, 0);
        }

        /// <summary>
        /// 用于检查IP地址或域名是否可以使用TCP/IP协议访问(使用Ping命令),true表示Ping成功,false表示Ping失败 
        /// </summary>
        /// <param name="strIpOrDName">输入参数,表示IP地址或域名</param>
        /// <returns></returns>
        public static bool PingIpOrDomainName(string strIpOrDName)
        {
            try
            {
                Ping objPingSender = new Ping();
                PingOptions objPinOptions = new PingOptions();
                objPinOptions.DontFragment = true;
                string data = "";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int intTimeout = 120;
                PingReply objPinReply = objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);
                string strInfo = objPinReply.Status.ToString();
                if (strInfo == "Success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        ///// <summary>
        ///// 写配置文件
        ///// </summary>
        ///// <param name="file"></param>
        ///// <param name="section"></param>
        ///// <param name="key"></param>
        ///// <param name="val"></param>
        //public static void WriteProfile(string file, string section, string key, string val)
        //{
        //    WritePrivateProfileString(section, key, val, file);
        //}

        ///// <summary>
        ///// 读配置文件
        ///// </summary>
        ///// <param name="file"></param>
        ///// <param name="section"></param>
        ///// <param name="key"></param>
        ///// <param name="defaultVal"></param>
        ///// <returns></returns>
        //public static string GetProfile(string file, string section, string key, string defaultVal)
        //{
        //    string cfgFile = file;

        //    Byte[] buffer = new Byte[256];//512 65535
        //    //int bufLen = GetPrivateProfileString(section, key, Default, Buffer, Buffer.GetUpperBound(0), INI_Path);
        //    int bufLen = GetPrivateProfileString(section, key, defaultVal, buffer, 256, cfgFile);
        //    //设定 Unicode,UTF8 的编码方式，否则无法支持中文 , 还是有bug
        //    // 1.删除文件   2. 自动创建文件后 或 修改后

        //    //只能支持ASCII(default) 
        //    //string s = Encoding.GetEncoding(Encoding.ASCII.CodePage).GetString(buffer, 0, bufLen);
        //    string s = Encoding.GetEncoding(Encoding.UTF8.CodePage).GetString(buffer, 0, bufLen);
        //    // s = s.Substring(0, bufLen);
        //    //s.Replace('\0', ' ');
        //    return s.Trim();
        //}

        /// <summary>
        /// 写配置文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static bool WriteProfile(string file, string section, string key, string val)
        {
            try
            {
                if (!File.Exists(file))
                {
                    File.WriteAllText(file, "");
                }
            }
            catch
            {
                return false;
            }
            string sect = "[" + section + "]";
            string contentNew = "";
            bool isFoundSect = false;
            bool isOver = false;
            try
            {
                string[] rows = File.ReadAllLines(file);

                foreach (string row in rows)
                {
                    if (!isOver)
                    {
                        string line = row.Trim();

                        if (!isFoundSect)
                        {
                            if (line.StartsWith(sect))
                                isFoundSect = true;
                        }
                        else
                        {
                            if (line.StartsWith("["))
                            {
                                // 发现 section 项结束了，还没有发现Key
                                // 追加一个新的行
                                contentNew += key + "=" + val + "\r\n";
                                isOver = true;
                            }
                            else
                            {
                                int pos = row.IndexOf("=");
                                if (pos > 0)
                                {
                                    if (key == row.Substring(0, pos).Trim())
                                    {
                                        contentNew += row.Substring(0, pos + 1) + val + "\r\n";
                                        isOver = true;
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    contentNew += row + "\r\n";
                }
            }
            catch
            {
                return false;
            }

            if (!isFoundSect)
            {
                contentNew += sect + "\r\n";
            }
            if (!isOver)
            {
                contentNew += key + "=" + val + "\r\n";
            }
            try
            {
                File.WriteAllText(file, contentNew);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 读配置文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static string GetProfile(string file, string section, string key, string defaultVal)
        {
            string ret = defaultVal;
            try
            {
                if (!string.IsNullOrEmpty(file))
                {
                    if (File.Exists(file))
                    {
                        string sect = "[" + section + "]";

                        string[] rows = File.ReadAllLines(file);
                        bool foundSect = false;
                        foreach (string row in rows)
                        {
                            string line = row.Trim();

                            if (!foundSect)
                            {
                                if (line.StartsWith(sect))
                                    foundSect = true;
                            }
                            else
                            {
                                if (line.StartsWith("["))
                                    break;
                                int pos = line.IndexOf("=");
                                if (pos > 0)
                                {
                                    if (key == line.Substring(0, pos).Trim())
                                    {
                                        //int posE = line.IndexOf(" //", pos + 1);
                                        //if (posE < 0)
                                        //{
                                        //    posE = line.Length;
                                        //}
                                        //ret = line.Substring(pos + 1, posE).Trim();
                                        ret = line.Substring(pos + 1).Trim();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return ret;
        }

        /// <summary>
        /// 写配置文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void WriteProfileAnsi(string file, string section, string key, string val)
        {
            WritePrivateProfileString(section, key, val, file);
        }

        /// <summary>
        /// 读配置文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static string GetProfileAnsi(string cfgFile, string section, string key, string defaultVal)
        {
            StringBuilder s = new StringBuilder(1024);
            GetPrivateProfileString(section, key, defaultVal, s, 1024, cfgFile);
            return s.ToString().Trim();
        }



        /// <summary>
        /// 从一个输入的字符串中查找是否有关键字符串key, 如果有 截取关键字后面的字符串到 换行符或结尾,并移除首尾的空格
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="key">关键字</param>
        /// <returns>返回解析的字符串， null 表示没有找到关键字</returns>
        /// 
        public static string GetLineKeyInfo(string input, string key)
        {
            int posS, posE;
            string s = key;
            posS = input.IndexOf(s);
            if (posS != -1)
            {
                posS += s.Length;
                posE = input.IndexOf("\n", posS);
                if (posE == -1)
                    posE = input.Length;

                return input.Substring(posS, posE - posS).Trim();
            }
            return null;
        }

        private static string _language = "";
        static string[] _languageCn = null;
        static string[] _languageCur = null;
        public static void SetLanguage(string language)
        {
            _language = language.ToLower();

            _languageCn = new string[0];
            _languageCur = new string[0];

            //if (_language == "cn")
            if (_language != "vi")
            {
                _language = "cn";
                return;
            }

            string dir = System.Windows.Forms.Application.StartupPath;
            string cn = dir + "\\" + "language.cn.txt";
            string cur = dir + "\\" + $"language.{_language}.txt";

            if (File.Exists(cn) && File.Exists(cur))
            {
                string[] languageCn = File.ReadAllLines(cn);
                string[] languageCur = File.ReadAllLines(cur);
                if (languageCn.Length != languageCur.Length)
                {
                    MessageBox.Show("Inconsistent language configuration files.", "Language setting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    _languageCn = languageCn;
                    _languageCur = languageCur;
                }
            }
        }
        public static string GetLanguage()
        {
            return _language;
        }


        public static string t(string sInput, params string[] values)
        {
            string s = sInput;

            if (_languageCn.Length > 0)
            {
                for (int line = 0; line < _languageCn.Length; line++)
                {
                    if (_languageCn[line] == s)
                    {
                        // 取得对应行的 外文内容
                        s = _languageCur[line];
                        break;
                    }
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                s = s.Replace($"{{{i}}}", values[i]);
            }
            // 如果未找到对应的翻译文档，原样返回
            return s;
        }

        public static string T(string sInput, params string[] values)
        {
            string s = sInput;

            if (_languageCn.Length > 0)
            {
                for (int line = 0; line < _languageCn.Length; line++)
                {
                    if (_languageCn[line] == s)
                    {
                        // 取得对应行的 外文内容
                        s = _languageCur[line] + " " + sInput; ;
                        break;
                    }
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                s = s.Replace($"{{{i}}}", values[i]);
            }
            // 如果未找到对应的翻译文档，原样返回
            return s;
        }
        /// <summary>
        /// log路径
        /// </summary>
        public static string _logFile = "";
        /// <summary>
        /// 当前站点代码
        /// </summary>
        public static string _curStation = "";
        /// <summary>
        /// 当前ip地址
        /// </summary>
        public static string _hostIp = "";




        #region 窗口操作
        /// <summary>
        /// 窗口位置结构
        /// </summary>
        public struct RECT
        {
            /// <summary>
            /// 最左坐标
            /// </summary>
            public int left;                             //最左坐标
            /// <summary>
            /// 最上坐标
            /// </summary>
            public int top;                             //最上坐标
            /// <summary>
            /// 最右坐标
            /// </summary>
            public int right;                           //最右坐标
            /// <summary>
            /// 最下坐标
            /// </summary>
            public int bottom;                        //最下坐标
        }
        /// <summary>
        /// 取得当前窗口位置坐标
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpRect"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        /// <summary>
        /// 发送鼠标单击事件
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SendMouseEvent(int x, int y)
        {
            SendMouseEvent(IntPtr.Zero, x, y);
        }
        /// <summary>
        /// 向指定窗口发送鼠标事件
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SendMouseEvent(IntPtr hWnd, int x, int y)
        {
            if (hWnd != IntPtr.Zero)
            {
                RECT rect = new RECT();
                GetWindowRect(hWnd, ref rect);
                x += rect.left;
                y += rect.top;
            }

            Point curPos;
            curPos = Control.MousePosition;

            WindowsApi.SetCursorPos(x, y);
            //Thread.Sleep(2000); // for debug
            Thread.Sleep(20);

            //移动鼠标 

            WindowsApi.mouse_event(WindowsApi.MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            //     SendMessage(hWnd, WM_LBUTTONDBLCLK, (IntPtr)MK_LBUTTON, (IntPtr)(y * 65536 + x));
            Thread.Sleep(20);
            WindowsApi.mouse_event(WindowsApi.MOUSEEVENTF_LEFTUP, x, y, 0, 0);

            WindowsApi.SetCursorPos(curPos.X, curPos.Y);
        }

        //#endregion
        //#region 文件操作
        ///// <summary>
        /////  拷贝文件夹
        ///// </summary>
        ///// <param name="srcdir"></param>
        ///// <param name="dstdir"></param>
        ///// <param name="overwrite"></param>
        ///// <returns></returns>
        public static bool CopyDirectory(string srcdir, string dstdir, bool overwrite)
        {
            if (string.IsNullOrEmpty(srcdir) || string.IsNullOrEmpty(dstdir))
                return false;

            try
            {
                if (!Directory.Exists(dstdir))
                    Directory.CreateDirectory(dstdir);

                foreach (var s in Directory.GetFiles(srcdir))
                {
                    string f = Path.Combine(dstdir, Path.GetFileName(s));
                    File.Copy(s, f, overwrite);
                }

                foreach (var s in Directory.GetDirectories(srcdir))
                {
                    string f = Path.Combine(dstdir, Path.GetFileName(s));
                    CopyDirectory(s, f, overwrite);

                }
            }
            catch
            {
                throw;
                //return false;
            }
            return true;
        }

        /// <summary>
        /// 拷贝目录下所有文件及子目录
        /// </summary>
        /// <param name="srcdir"></param>
        /// <param name="dstdir"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static string CopyDir(string srcdir, string dstdir, bool overwrite)
        {
            if (string.IsNullOrEmpty(srcdir) || string.IsNullOrEmpty(dstdir))
                return "The directory cannot be empty. Misc.CopyDir(...)";

            try
            {
                if (!Directory.Exists(dstdir))
                    Directory.CreateDirectory(dstdir);

                foreach (var s in Directory.GetFiles(srcdir))
                {
                    string f = Path.Combine(dstdir, Path.GetFileName(s));
                    File.Copy(s, f, overwrite);
                }

                foreach (var s in Directory.GetDirectories(srcdir))
                {
                    string f = Path.Combine(dstdir, Path.GetFileName(s));
                    string sRet = CopyDir(s, f, overwrite);
                    if (sRet != "")
                        return sRet;

                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        /// <summary>
        /// 获取指定目录中的匹配目录
        /// </summary>
        /// <param name="dir">要搜索的目录</param>
        /// <param name="regexPattern">目录名模式（正则）。null表示忽略模式匹配，返回所有目录</param>
        /// <param name="recurse">是否搜索子目录</param>
        /// <param name="throwEx">是否抛异常</param>
        /// <returns></returns>
        public static string[] GetDirectories(string dir, string regexPattern = null, bool recurse = false, bool throwEx = false)
        {
            List<string> lst = new List<string>();

            try
            {
                foreach (string item in Directory.GetDirectories(dir))
                {
                    try
                    {
                        if (regexPattern == null || Regex.IsMatch(Path.GetFileName(item), regexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                        { lst.Add(item); }

                        //递归
                        if (recurse) { lst.AddRange(GetDirectories(item, regexPattern, true)); }
                    }
                    catch { if (throwEx) { throw; } }
                }
            }
            catch { if (throwEx) { throw; } }

            return lst.ToArray();
        }

        ///// <summary>
        ///// 解压文件
        ///// </summary>
        ///// <param name="filePathNames"></param>
        ///// <returns></returns>
        //public static bool UnCompressFile(string filePathNames)
        //{
        //    try
        //    {
        //        Process process = new Process();
        //        process.StartInfo.FileName = "7z.exe";
        //        process.StartInfo.Arguments = " x -y " + filePathNames;
        //        //隐藏DOS窗口
        //        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        //        process.Start();
        //        process.WaitForExit();
        //        process.Close();
        //    }
        //    //catch (ArgumentException ex)
        //    catch
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// 压缩文件
        ///// </summary>
        ///// <param name="filePathNames"></param>
        ///// <param name="zipFilePathName"></param>
        ///// <returns></returns>
        public static bool CompressFile(string filePathNames, string zipFilePathName)
        {
            //Misc.WriteLogLine($"CompressFile()...,{filePathNames},{zipFilePathName})...start.");

            try
            {
                Process process = new Process();
                string dir7z = @"C:\Program Files (x86)\7-Zip\7z.exe";

                if (!File.Exists(dir7z))
                {
                    dir7z = @"C:\Program Files\7-Zip\7z.exe";
                    if (!File.Exists(dir7z))
                    {
                        dir7z = "7z.exe";
                    }
                }
                //process.StartInfo.FileName = @"C:\Program Files (x86)\7-Zip\7z.exe";
                process.StartInfo.FileName = dir7z;
                process.StartInfo.Arguments = " a -t7z " + zipFilePathName + " " + filePathNames + "";
                //隐藏DOS窗口
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                //Misc.WriteLogLine($"CompressFile()...,Arguments = {process.StartInfo.Arguments}");

                process.Start();

                process.WaitForExit();
                process.Close();

            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message;
                //Misc.WriteLogLine($"CompressFile()...Fail. ex.Message= {ex.Message}");
                return false;
            }
            finally
            {
                //Misc.WriteLogLine($"CompressFile()...try -- Finally");

            }
            //Misc.WriteLogLine($"CompressFile({filePathNames},{zipFilePathName})...end.");

            return true;
        }

        ///// <summary>
        ///// 压缩多个文件
        ///// </summary>
        ///// <param name="filePathNames"></param>
        ///// <param name="zipFilePathName"></param>
        ///// <returns></returns>
        public static bool CompressFiles(string[] filePathNames, string zipFilePathName)
        {
            ErrMsg = "";
            try
            {
                foreach (string file in filePathNames)
                {
                    //Misc.WriteLogLine($"CompressFiles({file})...start.");

                    if (!CompressFile(file, zipFilePathName))
                    {
                        //Misc.WriteLogLine($"CompressFiles({file})...Fail.");

                        return false;
                    }
                    //Misc.WriteLogLine($"CompressFiles({file})...end.");

                }

            }
            catch (Exception e)
            {
                ErrMsg = e.Message;
                //Misc.WriteLogLine($"CompressFiles()...Fail，{e.Message}");
                return false;
            }

            return true;
        }



        ///// <summary>
        ///// 发送log到服务器
        ///// </summary>
        ///// <param name="sn">SN</param>
        ///// <param name="isPassed">测试结果，True:成功，False:失败</param>
        ///// <param name="logdir">需要上传的Log在本地的位置，可以是目录名或文件名,如果是目录名则先压缩再上传</param>
        ///// <param name="rootDirToSave"></param>    @"\\10.10.1.27\Test_log" + "\\HtsTestLogs\\" +  A001 + "\\" + A001M1;
        ///// <param name="psn">附加序列号，不使用传空</param>
        ///// <param name="stationCode">站点编码</param>
        ///// <param name="subInfo">附加标识, AP LOG 传"ap",CLIO LOG传"clio",Bist烧录log传"dlbist"，spt烧录：dlspt 等...</param>
        ///// <param name="logLifeTimes">Log保存期限，保存的月份数。</param>
        ///// <returns>true:成功，false:失败</returns>
        public static bool SendLogToServer(string sn, bool isPassed, string logdir, string rootDirToSave, string psn, string stationCode, string subInfo, int logLifeTimes = 36)
        {
            try
            {
                string zipFileName;

                if (Directory.Exists(logdir))
                {
                    // logdir 是目录名，则先压缩再上传
                    string logDir = logdir;

                    // 确定目录下所有文件名或子目录名
                    string[] logs = Misc.GetFiles(logDir);

                    //最终上传的压缩文件的文件名，以sn为开头
                    string strLogFileName7Z = sn;

                    //用于临时存放压缩文件的本地临时目录,采用当前目录下的 子目录 .\log
                    string strTempDir = System.Windows.Forms.Application.StartupPath + "\\log";
                    //如果该子目录不存在则创建它
                    if (!System.IO.Directory.Exists(strTempDir))
                    {
                        System.IO.Directory.CreateDirectory(strTempDir);
                    }
                    //
                    string zipFile = strTempDir + "\\" + strLogFileName7Z;
                    zipFileName = zipFile + ".7z";


                    // 删除上一次的log 压缩包
                    if (File.Exists(zipFileName))
                    {
                        File.Delete(zipFileName);
                    }


                    if (!Misc.CompressFiles(logs, zipFile))
                    {
                        return false;
                    }
                }
                else
                {
                    // 传进来的是文件名，直接上传，
                    zipFileName = logdir;
                }

                // log文件已经压缩好，准备上传服务器

                // LS10MB124210001
                DateTime dt = DateTime.Now;

                // 取得按HTS规则log在服务器上的目录

                // 创建log文件目录和文件名称，尽量采用压缩的方式保存，文件名中包含log生成的日期以及保存期限
                //  \\10.10.1.27\Test_log\HTS\LS10\LS1000\LS10MB1\LS10MB12421\LS10MB1242100\LS10MB124210001
                //  LS10MB124210001_DlBist__020514T202305_L36_pass.7z
                // LS09MB125190001_Dload_spt__220520T160526(36)_pass.7z
                // \\10.10.1.27\Test_log\HTS\A005\A005M1\A005MB0\A005MB02519\A005MB0251900\A005MB02519000001
                // 
                // rootDirToSave 到达机型， 比如：  \\10.10.1.27\Test_log\HTS\A005\A005M0
                string dir = rootDirToSave + "\\sn";

                dir += "\\" + sn.Substring(0, 7); // 机种、板别版本
                dir += "\\" + sn.Substring(0, 11); // 日期
                dir += "\\" + sn.Substring(0, 13); // 流水前2位
                dir += "\\" + sn; // sn 

                // 新服务器 接口会自动创建子目录
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string logSave = dir + "\\";
                logSave += sn;

                // psn 辅助psn信息 加在文件名中便于检索
                logSave += $"_{psn}";

                // 站点代码， 不同站点产生的log,用站点代码区别
                logSave += $"_{stationCode}";
                // 附件信息 subInfo， 同一个站点 有可能有不同类型的log产生，特别是 多功能在同一个站位测试时，比如 LS9模块 在同一个站点 就有bist log ，SPT log等
                // 附件信息如果为空时则只保留一个 _ 符号

                logSave += $"_{subInfo}";

                // 日期时间

                logSave += "__Y"; // __Y表示日期
                logSave += dt.ToString("yy"); // 两位 年份代码
                logSave += dt.ToString("MM");
                logSave += dt.ToString("dd");
                logSave += "T"; // 时间分隔符
                logSave += dt.ToString("HH");
                logSave += dt.ToString("mm");
                logSave += dt.ToString("ss");

                // 保存期限（月）
                logSave += "L"; // 保存期限分隔符
                logSave += $"{logLifeTimes.ToString()}";

                // 成功、失败
                logSave += (isPassed ? "_pass" : "_ng");

                // 扩展名 同于原文件
                string ext = System.IO.Path.GetExtension(zipFileName);
                logSave += ext;


                File.Copy(zipFileName, logSave, true);
                if (!File.Exists(logSave))
                {
                    File.Copy(zipFileName, logSave, true);
                    if (!File.Exists(logSave))
                    {
                        return false;
                    }
                }

                File.Delete(zipFileName);

            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message;
                //Misc.WriteLogLine($"SendLogToServer()...crash, ex={ex.Message}");
                return false;
            }
            return true;
        }

        #endregion

        /// <summary>
        /// 获取指定目录中的匹配文件
        /// </summary>
        /// <param name="dir">要搜索的目录</param>
        /// <param name="regexPattern">文件名模式（正则）。null表示忽略模式匹配，返回所有文件</param>
        /// <param name="recurse">是否搜索子目录</param>
        /// <param name="throwEx">是否抛异常</param>
        /// <returns></returns>
        public static string[] GetFiles(string dir, string regexPattern = null, bool recurse = false, bool throwEx = false)
        {
            List<string> lst = new List<string>();

            try
            {
                foreach (string item in Directory.GetFileSystemEntries(dir))
                {
                    try
                    {
                        bool isFile = (File.GetAttributes(item) & FileAttributes.Directory) != FileAttributes.Directory;

                        if (isFile && (regexPattern == null || Regex.IsMatch(Path.GetFileName(item), regexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline)))
                        { lst.Add(item); }

                        //递归
                        if (recurse && !isFile) { lst.AddRange(GetFiles(item, regexPattern, true)); }
                    }
                    catch { if (throwEx) { throw; } }
                }
            }
            catch { if (throwEx) { throw; } }

            return lst.ToArray();
        }

        /// <summary>
        /// 删除指定目录下所有子目录和文件，保留当前目录名
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static bool DeleteAllInDir(string dir)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(dir);
                foreach (string d in dirs)
                {
                    Directory.Delete(d, true);
                }
                string[] strFiles = Misc.GetFiles(dir);
                foreach (string strFile in strFiles)
                {
                    //  逐个删除找到的文件
                    File.Delete(strFile);
                }

            }
            catch
            {
                return false;
            }
            return true;
        }

        #region LOG


        public static bool _isTstLogDebug = false;

        /// <summary>
        /// 初始化LOG服务器
        /// </summary>
        /// <param name="isDebugHtsDB"></param>
        /// <returns></returns>

        public static string InitTstLogServer(bool isDebugHtsDB)
        {
            // log保存已经全部改成相对路径，无需再获取 服务器的ip
            _isTstLogDebug = isDebugHtsDB;
            HsFile2Srv hsFile2Srv = new HsFile2Srv("LOG", null);
            return hsFile2Srv.GetSrvPosition();
        }

        // 上传服务器的接口进行过调整，新的接口，全部采用相对路径
        public static bool isFileExistsOnServer(string file)
        {
            HsFile2Srv hsFile2Srv = new HsFile2Srv("LOG", null);
            return hsFile2Srv.Exist(file);
        }
        /// <summary>
        /// 拷贝指定文件到服务器
        /// </summary>
        /// <param name="sourceFileName">本地含绝对路径的源文件</param>
        /// <param name="descFileName">云端含相对路径的目的文件</param>
        /// <returns>返回空表示成功，否则返回错误信息</returns>
        public static string CopyTstLogToServer(string sourceFileName, string descFileName)
        {
            if (sourceFileName == "" || descFileName == "")
            {
                return T("测试LOG原文件或目标文件不能为空");
            }

            try
            {
                // 云端是相对路径，log存放在有权限管控的服务器上， 采用HTS提供的接口进行拷贝 , 
                string errMsg;
                HsFile2Srv hsFile2Srv = new HsFile2Srv("LOG", null);
                if (1 != hsFile2Srv.Copy(sourceFileName, descFileName, out errMsg, true))
                {
                    return T("Log上传失败") + "," + errMsg;
                }
            }
            catch (Exception ex)
            {
                return T("Log上传异常") + ",CopyTstLogToServer()...crash," + ex.Message;
            }
            finally
            {
            }
            return "";
        }


        /// <summary>
        /// 按照log保存规则，返回 log文件在服务器上的相对 路径和文件名
        /// </summary>
        /// <param name="sn">SN</param>
        /// <param name="isPassed">测试结果，True:成功，False:失败</param>
        /// <param name="psn">附加序列号，不使用传空</param>
        /// <param name="stationCode">站点编码</param>
        /// <param name="subInfo">附加标识, AP LOG 传"ap",CLIO LOG传"clio",Bist烧录log传"dlbist"，spt烧录：dlspt 等...</param>
        /// <param name="logLifeTimes">Log保存期限，保存的月份数。</param>
        /// <param name="extName">文件扩展名，空表示无扩展名</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>成功：返回路径名和文件名，失败：返回空字符串</returns>
        public static string GenLogBackupPathNameOnServer(string sn, bool isPassed, string psn, string stationCode, string subInfo, int logLifeTimes, string extName, out string errMsg)
        {
            if (string.IsNullOrEmpty(sn))
            {
                errMsg = "GenLogPathOnServer()... Sn is Null or Empty";
                return "";
            }
            errMsg = "";
            try
            {
                // LS10MB124210001
                DateTime dt = DateTime.Now;

                // 取得按HTS规则log在服务器上的目录

                // 创建log文件目录和文件名称，尽量采用压缩的方式保存，文件名中包含log生成的日期以及保存期限
                //  \\10.10.1.27\Test_log\HTS\LS10\LS1000\LS10MB1\LS10MB12421\LS10MB1242100\LS10MB124210001
                //  LS10MB124210001_DlBist__020514T202305_L36_pass.7z
                // LS09MB125190001_Dload_spt__220520T160526(36)_pass.7z
                // \\10.10.1.27\Test_log\HTS\A005\A005M1\A005MB0\A005MB02519\A005MB0251900\A005MB02519000001
                // \\10.10.1.144\HtsLogs\StnLogs\A0\A005\sn\A005MB0\A005MB02519\A005MB0251900\A005MB02519000001
                // rootDirToSave 到达机种， 比如：  \\10.10.1.27\Test_log\HTS\A005\A005M0

                // 2023/6/11 log保存的位置不再按机型分类，完全按 SN的 编码规则存放，便于以后查找
                // 根路径： \\10.10.1.27\Test_log\HTS\StnLogs
                //       或  \\\\10.10.1.144\HtsLogs\StnLogs

                string dir = "";  // 相对于 \\10.10.1.144\HtsLogs\StnLogs 的相对路径
                if (_isTstLogDebug)
                    dir = "Debug\\";
                else
                    dir = "";

                // 一级子目录固定为： \TstLog
                dir += "TstLog";
                // 二级子目录 按SN生成，当SN为 OfflineTest时表示离线测试，统一放在 .\OfflineTest下
                if (sn != "OfflineTest")
                {
                    if (sn.Length == 15)
                    {
                        dir += "\\" + sn.Substring(0, 2); // 机种前两位2
                        dir += "\\" + sn.Substring(0, 4); // 机种4
                        dir += "\\" + sn.Substring(0, 7); // 机种4、板别版本3
                        dir += "\\" + sn.Substring(0, 11); //机种4、版别版本3、日期4
                        dir += "\\" + sn.Substring(0, 13); //机种4、版别版本3、日期4、流水前2位
                        dir += "\\" + sn; // sn 
                    }
                    else if (sn.Length == 13)
                    {
                        // 兼容以后可能使用的 13位 SN码 AAAABVYMDDSSS

                        dir += "\\" + sn.Substring(0, 2); // 机种前两位2
                        dir += "\\" + sn.Substring(0, 4); // 机种4
                        dir += "\\" + sn.Substring(0, 6); // 机种4、板别版本2
                        dir += "\\" + sn.Substring(0, 9); // 机种4、版别版本2、日期3
                        dir += "\\" + sn.Substring(0, 11); //机种4、版别版本2、日期3、流水前2位
                        dir += "\\" + sn; // sn 
                    }
                    else
                    {
                        errMsg = $"SN={sn}" + T("格式不认识");
                        return "";
                    }
                }
                else
                {
                    // OfflineTest 的数据主要用于调试，将文件放在同一个目录下有利于直观地浏览，但要注意需要及时删除过多的冗余数据，避免文件数量过度增长
                    dir += "\\OfflineTest";
                }

                string logSave = dir + "\\";
                logSave += sn;

                // psn 辅助psn信息 加在文件名中便于检索
                logSave += $"_{psn}";

                // 站点代码， 不同站点产生的log,用站点代码区别
                logSave += $"_{stationCode}";

                // 附加信息 subInfo， 同一个站点 有可能有不同类型的log产生，特别是 多功能在同一个站位测试时，比如 LS9模块 在同一个站点 就有bist log ，SPT log等
                // 附加信息如果为空时则只保留一个 _ 符号
                logSave += $"_{subInfo}";

                // 日期时间

                logSave += "__Y"; // __Y表示日期
                logSave += dt.ToString("yyyyMMdd"); //  4位 年份代码，新的格式年份采用4位格式,请注意老数据中有2位年份的log
                logSave += "T"; // 时间分隔符
                logSave += dt.ToString("HHmmss"); // 时分秒

                // 保存期限（月）
                logSave += "L"; // 保存期限分隔符,月份数
                logSave += $"{logLifeTimes.ToString()}";

                // 成功、失败
                logSave += (isPassed ? "_pass" : "_ng");

                if (extName != "")
                    logSave += extName;

                return logSave;
            }
            catch (Exception ex)
            {
                errMsg = $"GenLogPathOnServer()...crash, ex={ex.Message}";
            }
            return "";
        }

        /// <summary>
        /// 对需要备份的文件，安照最后修改日期，产生新的文件名
        /// </summary>
        /// <param name="apProject"></param>
        /// <returns></returns>
        public static string GenDatBackupPathNameOnServer(string sn, string stationCode, string subInfo, string apProject, int logLifeTimes)
        {
            DateTime dt = new FileInfo(apProject).LastWriteTime;

            // 当前工程文件在LOG服务器上保存的文件名
            // 保存期限（月）
            string apPrjName = $"{stationCode}_{subInfo}_" +
                $"{dt.ToString("YyyyyMMddTHHmmss")}L{logLifeTimes.ToString()}__{Path.GetFileName(apProject)}";

            // 服务器的根路径固定为：LOGSRV_ROOT=\\10.10.1.144\HtsLogs\StnLogs
            // 服务器的根路径固定为：LOGSRV_LOCAL=@"D:\HtsLocal\StnLogs"
            // _logDirOnSrvBaseRoot 是根路径的相对路径:为 .\或者 .\simulate
            // tstDataDir 为根路径的相对路径
            //string tstDataDir = _logDirOnSrvBaseRoot + $"\\TstData";

            string tstDataDir;
            if (_isTstLogDebug)
                tstDataDir = "debug\\TstData";
            else
                tstDataDir = $"TstData";

            if (sn == "OfflineTest")
            {
                tstDataDir += "\\OfflineTest";
            }
            else
            {
                tstDataDir += $"\\{stationCode.Substring(0, 2)}\\{stationCode.Substring(0, 4)}";
            }

            string datSave = tstDataDir + "\\";
            datSave += apPrjName;

            return datSave;
        }

        /*
//public static string _dbgLog2Srv;
public const string DBGLOG = @"c:\hts\logs";
//public const string DBGLOG2SRV = @"\\10.10.1.27\Test_log\HTS\StnDbgLog";
public const string DBGLOG2SRV = @"StnDbgLog";
 
public static string _dbgLogLocalDir = "";
public static string _dbgLogRemoteDir = "";
public static string _dbgLogFileName = "";
//public static void InitDbgLogFile(string station)
//{
//    if (File.Exists(@"c:\HtsLocalDebug.ini"))
//    {
//        InitDbgLogFile(station, DBGLOG, "");
//        MessageBox.Show(this,$"发现文件c:\\HtsLocalDebug.ini， 请注意当前是本地调试，调试Log不会上传服务器", "调试提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
//    }
//    else
//    {
//        InitDbgLogFile(station, DBGLOG, DBGLOG2SRV);
//    }
//}
 
public static string InitDbgLogFile(string station, string remoteDir = "")
{
    if (remoteDir == "")
        remoteDir = DBGLOG2SRV;
    string logName = InitDbgLogFile(station, DBGLOG, remoteDir);
    return remoteDir + "\\" + logName;
}
 
 
//static EventWaitHandle _dbgLogWaitHandle;// = new EventWaitHandle(true, EventResetMode.AutoReset, "SHARED_BY_ALL_PROCESSES");
static Mutex _dbgLogMutex;// = new Mutex();
//static int _dbgLogCnt = 0;
 
/// <summary>
/// 初始化log文件
/// </summary>
/// <param name="station"></param>
private static string InitDbgLogFile(string station, string localDir, string remoteDir)
{
    string toolName = System.AppDomain.CurrentDomain.FriendlyName;
 
    string guid = System.Guid.NewGuid().ToString();
    //_dbgLogWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "DbgLog" + guid);
 
    //_dbgLogMutex = new Mutex(); 
    _dbgLogMutex = new Mutex(false, "Global\\MyGlobalMutex"); // 创建一个全局 Mutex
 
    string err = "";
 
    _dbgLogLocalDir = localDir;
    //_dbgLogRemoteDir = remoteDir;
    //按机型分开保存debug log，分开保存的
    //优先是 不用担心同一个目录下的文件太多，不用经常清理log文件
    //缺点是 无法在一个目录下，通过时间排序能看出最近有哪些工具在使用，不过这个功能可以通过后续的系统查询的完善而改善
    if (remoteDir == "" || remoteDir == localDir)
        _dbgLogRemoteDir = "";
    else
        _dbgLogRemoteDir = remoteDir + "\\" + station.Substring(0, 4);
 
    _dbgLogFileName = $"dbglog_{station}_{toolName}_{GetHostName()}_{GetLocalIp()}.txt";
 
 
    //LogWriteLock.EnterWriteLock();
    //_dbgLogWaitHandle.WaitOne();
    //_dbgLogMutex.WaitOne();
    if (!_dbgLogMutex.WaitOne(30000, false))
    {
        TraceDbgLog(DateTime.Now, $"InitDbgLogFile()...,  Fail,30 time out");
    }
    else
    {
        try
        {
            if (_dbgLogLocalDir != "")
            {
                if (!Directory.Exists(_dbgLogLocalDir))
                    Directory.CreateDirectory(_dbgLogLocalDir);
            }
 
            // 调试Log位置改由服务器决策，不再限定.27
            //if (_dbgLogRemoteDir != "")
            if (_dbgLogRemoteDir.StartsWith("\\"))
            {
                if (!Directory.Exists(_dbgLogRemoteDir))
                    Directory.CreateDirectory(_dbgLogRemoteDir);
            }
        }
        catch (Exception ex)
        {
            err = "Crash:" + ex.Message;
        }
        finally
        {
            //LogWriteLock.ExitWriteLock();
            //_dbgLogWaitHandle.Set();
        }
 
        if (err != "")
            TraceDbgLog(DateTime.Now, $"InitDbgLogFile()..., Fail," + err);
        _dbgLogMutex.ReleaseMutex();
    }
 
    return _dbgLogFileName;
}
*/
        //static int _gdbg = 0;

        //public static void WriteLog(string msg)
        //{
        //    _dbgLogMutex.WaitOne();

        //    try
        //    {
        //        string logFile = _dbgLogLocalDir + "\\" + _dbgLogFileName;

        //        // 使用StreamWriter以追加模式打开日志文件
        //        using (StreamWriter writer = new StreamWriter(logFile, true, System.Text.Encoding.UTF8))
        //        {
        //            // 将消息写入日志文件
        //            string s = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + msg;

        //            writer.WriteLine(s);
        //            writer.Write(s);
        //        }
        //        //Console.WriteLine("日志已追加到文件中。");
        //    }
        //    catch (Exception ex)
        //    {
        //        //Console.WriteLine($"写入日志时出现错误：{ex.Message}");
        //    }
        //    finally
        //    {
        //    }

        //    _dbgLogMutex.ReleaseMutex();

        //}

        //public static void WriteLog(string msg)
        //{
        //    if (_dbgLogFileName == "" || _dbgLogLocalDir == "")
        //        return;
        //    DateTime dt = DateTime.Now;
        //    string s = dt.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + msg;

        //    string err = "";
        //    //LogWriteLock.EnterWriteLock();
        //    //_dbgLogWaitHandle.WaitOne();
        //    //_dbgLogMutex.WaitOne();
        //    if (!_dbgLogMutex.WaitOne(30000, false))
        //    {
        //        err = "30s超时";
        //    }
        //    else
        //    {
        //        _dbgLogCnt++;

        //        FileStream fs = null;
        //        StreamWriter sw = null;
        //        try
        //        {
        //            string logFile = _dbgLogLocalDir + "\\" + _dbgLogFileName;
        //            if (_gdbg != 0)
        //                Thread.Sleep(10000);
        //            if (File.Exists(logFile))
        //            {
        //                //FileStream fs = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        //                fs = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.Read);
        //                sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
        //                //sr.WriteLine(s);
        //                sw.Write(s);
        //                sw.Flush();
        //                //sw.Close();
        //                //sw.Dispose();
        //                //fs.Close();
        //            }
        //            else
        //            {
        //                //FileStream fs = new FileStream(logFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        //                fs = new FileStream(logFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        //                sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
        //                //sr.WriteLine(s);
        //                sw.Write(s);
        //                sw.Flush();
        //                //sw.Close();
        //                //sw.Dispose();
        //                //fs.Close();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            err = ex.Message;
        //        }
        //        finally
        //        {
        //            if (sw != null)
        //            {
        //                sw.Close();
        //                sw.Dispose();
        //            }
        //            if (fs != null)
        //            {
        //                fs.Close();
        //            }

        //            _dbgLogCnt--;
        //            _dbgLogMutex.ReleaseMutex();
        //        }
        //    }

        //    if (err != "")
        //    {
        //        //MessageBox.Show(this,"Write Log Error:" + err, "WriteLog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        //        // 下面这个log 是为了 追踪是否还存在 log文件写失败的异常，这个log 单独拷贝在 根目录下

        //        string f = $"c:\\hts\\logs\\__tmp.txt";
        //        if (File.Exists(f))
        //            File.Delete(f);
        //        File.WriteAllText(f, $"WriteLog()...\r\n{s}\r\n_dbgLogCnt={_dbgLogCnt}\r\n" + err);
        //        string fRemoteDir = "StnDbgLog\\" + _dbgLogFileName + "_" + dt.ToString("yyyyMMddTHHmmss.fff") + ".txt";
        //        string sErrMsg;
        //        HsFile2Srv hsFile2Srv = new HsFile2Srv("LOG", null);
        //        int cnt = hsFile2Srv.Copy(f, fRemoteDir, out sErrMsg, true);
        //        File.Delete(f);
        //    }
        //}

        /*
        /// <summary>
        /// 写字符串到文件
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="strings"></param>
        public static void WriteSringToFile(string Path, string strings)
        {
            StreamWriter sw = File.AppendText(Path);
            sw.Write(strings);
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }
*/

        /*
        public const long DBGLOG_MAX_SIZE = 10 * 1024 * 1024;
        //读写锁，当资源处于写入模式时，其他线程写入需要等待本次写入结束之后才能继续写入
        //static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
 
        /// <summary>
        /// 写一行调试日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLogLine(string msg)
        {
            string err = "";
            DateTime dt = DateTime.Now;
            //    //LogWriteLock.EnterWriteLock();
            //    //_dbgLogWaitHandle.WaitOne();
            //    //_dbgLogMutex.WaitOne();
            if (!_dbgLogMutex.WaitOne(30000, false))
            {
                TraceDbgLog(dt, $"WriteLogLine()...{msg},  Fail,30 time out");
            }
            else
            {
                dt = DateTime.Now;
                try
                {
                    string logFile = _dbgLogLocalDir + "\\" + _dbgLogFileName;
                    long len = 0;
                    // 使用StreamWriter以追加模式打开日志文件
                    using (StreamWriter writer = new StreamWriter(logFile, true, System.Text.Encoding.UTF8))
                    {
                        // 将消息写入日志文件
                        //writer.WriteLineAsync(dt.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + msg);
                        writer.WriteLine(dt.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + msg);
                        len = new System.IO.FileInfo(logFile).Length;
                        if (len >= DBGLOG_MAX_SIZE)
                        {
                            writer.WriteLine("AutoBackupDbgLog()");
                        }
                    }
                    if (len >= DBGLOG_MAX_SIZE)
                    {
                        // 自动备份调试日志
                        err = CopyDbgLogToServerProc(true);
                    }
                    ////long len = new System.IO.FileInfo(logFile).Length;
                    //if (new System.IO.FileInfo(logFile).Length >= DBGLOG_MAX_SIZE)
                    //{
                    //    using (StreamWriter writer = new StreamWriter(logFile, true, System.Text.Encoding.UTF8))
                    //    {
                    //        // 将消息写入日志文件
                    //        dt = DateTime.Now;
                    //        msg = dt.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + "AutoBackupDbgLog()";
                    //        writer.WriteLine(msg);
                    //    }
 
                    //    // 自动备份调试日志
                    //    err = CopyDbgLogToServerProc(true);
                    //}
                }
                catch (Exception ex)
                {
                    err = "Crash " + ex.Message;
                }
 
                if (err != "")
                    TraceDbgLog(dt, $"WriteLogLine()...{msg} ,Fail," + err);
 
                _dbgLogMutex.ReleaseMutex();
            }
        }
 
        static void TraceDbgLog(DateTime dt, string s)
        {
            try
            {
                // 下面这个log 是为了追踪是否还存在写调试日志失败的异常，这个log 单独拷贝在 根目录下
                string strName = _dbgLogFileName + "_" + dt.ToString("yyyyMMddTHHmmss.fff") + ".txt";
 
                string f = $"c:\\hts\\logs\\__tmp_" + strName;
                if (File.Exists(f))
                    File.Delete(f);
                File.WriteAllText(f, s);
                string fRemoteDir = "StnDbgLog\\" + strName;
                string sErrMsg;
                HsFile2Srv hsFile2Srv = new HsFile2Srv("LOG", null);
                int cnt = hsFile2Srv.Copy(f, fRemoteDir, out sErrMsg, true);
                File.Delete(f);
            }
            catch { }
        }
        //public static void BackupToolLogToServer()
        //{
        //    CopyDbgLogToServer();
        //}
 
        /// <summary>
        ///  备份一次log 到服务器,用于远程调试
        ///  如果服务器上这个log目录不存在 则 不备份
        /// 一般在开关工具时各备份一次
        ///  在点击 SOP按钮时强制备份一次
        ///  在其它出错的场景强制备份一次
        /// 2024/6/3 从当日开始编译的Log，仅在本地保存一份， 文件名 sourceFileName
        /// 当本地文件的大小达到上限时，自动上传到服务器，服务器上的 log 文件名 依次增加后缀，比如
        /// dbglog_A006S32_PackCsn.exe_hl0407_10.10.3.39.txt_1.txt； dbglog_A006S32_PackCsn.exe_hl0407_10.10.3.39.txt_2.txt
        /// 如果本地的Log文件未达上限，则 文件名不加后缀，比如 dbglog_A006S32_PackCsn.exe_hl0407_10.10.3.39.txt
        /// </summary>
        public static void CopyDbgLogToServer()
        {
            //if (_dbgLogLocalDir == "" || _dbgLogRemoteDir == "" || _dbgLogFileName == "")
            //    return;
 
            //_dbgLogMutex.WaitOne();
 
            string err = "";
            if (!_dbgLogMutex.WaitOne(30000, false))
            {
                TraceDbgLog(DateTime.Now, $"CopyDbgLogToServer()... Fail,30 time out");
            }
            else
            {
                err = CopyDbgLogToServerProc(false);
                if (err != "")
                    TraceDbgLog(DateTime.Now, $"CopyDbgLogToServer()...Fail," + err);
                _dbgLogMutex.ReleaseMutex();
            }
        }
 
        /// <summary>
        /// 拷贝本地log到服务器，该方法不考虑文件的互斥，必须在调用前做好互斥处理
        /// </summary>
        /// <param name="overWrite">是否用新的文件名备份</param>
        private static string CopyDbgLogToServerProc(bool isNeedNewName)
        {
            try
            {
                string logFile = _dbgLogLocalDir + "\\" + _dbgLogFileName;
                string destFile = _dbgLogRemoteDir + "\\" + _dbgLogFileName;
 
                // 云端是相对路径，log存放在有权限管控的服务器上， 采用HTS提供的接口进行拷贝 , 
                // 先根据本地Log文件的大小，和服务器上已经存在的log文件，裁决 备份到服务器上的文件名
                HsFile2Srv hsFile2Srv = new HsFile2Srv("LOG", null);
 
                if (isNeedNewName)
                {
                    string destFileNew;
                    for (int i = 1; true; i++)
                    {
                        destFileNew = destFile + "_" + i.ToString() + ".txt";
                        if (!hsFile2Srv.Exist(destFileNew))
                        {
                            destFile = destFileNew;
                            break;
                        }
                    }
                }
                // 云端是相对路径，log存放在有权限管控的服务器上， 采用HTS提供的接口进行拷贝 , 
                string sErrMsg;
                if (1 != hsFile2Srv.Copy(logFile, destFile, out sErrMsg, true))
                {
                    return $"HsFile2Srv: upload fail," + sErrMsg;
                }
                if (isNeedNewName)
                {
                    // 如果当前Log已经增加后缀改名备份后，删除本地log, 当新的log生成时会重新创建该文件
                    File.Delete(logFile);
                }
            }
            catch (Exception ex)
            {
                return "CopyDbgLogToServerProc()... Crash " + ex.Message;
            }
            return "";
        }
*/

        /*
        /// <summary>
        /// 获取HtsDb的调试log大小，HTSDB的调试Log以日期为文件名，且该文件有时相当大，为了合理备份Log的大小，只在出错时保存最后的部分Log，
        /// 旨在开始测试时，获取当时log文件的大小，失败时，仅从该位置提取后面的内容
        /// </summary>
        /// <returns></returns>
        public static long GetHtsDbgLogSize()
        {
            string[] lines = new string[0];
            DateTime dt = DateTime.Now;
            string log = $"c:\\hts\\logs\\log{dt.ToString("yyyyMMdd")}.txt";
 
            if (!File.Exists(log))
                return 0;
 
            FileInfo fileInfo = new FileInfo(log);
            return fileInfo.Length;
        }
        */

        /*
        public static string GetHtsDbgLogLast(long offset)
        {
            string strRet = "";
            DateTime dt = DateTime.Now;
            string log = $"c:\\hts\\logs\\log{dt.ToString("yyyyMMdd")}.txt";
 
            if (!File.Exists(log))
                return "";
 
            try
            {
                using (FileStream fs = new FileStream(log, FileMode.Open, FileAccess.Read))
                {
                    if (offset <= fs.Length)
                    {
                        fs.Seek(offset, SeekOrigin.Begin);
                    }
 
                    using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                    {
                        strRet = sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return "";
            }
            return strRet;
        }
        */
        /*
                public static string GetHtsDbgLog(int size = 50 * 1024)
                {
                    //// 因HTSDB log量比较大，只取最后的 最多300行的log,最多在50KB左右
                    //// HtsDB Log 以日期的为文件名，保存在 c:\hts\logs目录下 ：比如  Log20231022.txt
                    //string[] lines = new string[0];
                    //DateTime dt = DateTime.Now;
                    //string log = $"c:\\hts\\logs\\log{dt.ToString("yyyyMMdd")}.txt";
 
                    //if (!File.Exists(log))
                    //    return "";
                    //string s = File.ReadAllText(log, Encoding.Default);
                    //if (s.Length <= size)
                    //    return s;
 
                    //return s.Substring(s.Length - size);
 
                    // 因HTSDB log量比较大，只取最后的 最多300行的log,最多在50KB左右
                    // HtsDB Log 以日期的为文件名，保存在 c:\hts\logs目录下 ：比如  Log20231022.txt
                    string[] lines = new string[0];
                    DateTime dt = DateTime.Now;
                    string log = $"c:\\hts\\logs\\log{dt.ToString("yyyyMMdd")}.txt";
 
                    if (!File.Exists(log))
                        return "";
 
                    List<string> list = new List<string>();
                    int cnt = 0;
                    string[] ss = File.ReadAllLines(log, Encoding.Default);
                    for (int i = ss.Length - 1; i >= 0 && cnt < 100; i--)
                    {
                        string line = ss[i];
                        // 2023/11/21-09:59:08.376
                        if (line.Length >= 11)
                        {
                            //  2023/11/21-
                            if ((Regex.IsMatch(line.Substring(0, 11), "^[0-9]{4}/[0-9]{2}/[0-9]{2}-$")))
                            {
                                cnt++;
                            }
                        }
                        list.Add(line);
 
                    }
                    list.Reverse();
 
                    string strRet = "";
                    foreach (string line in list)
                    {
                        strRet += line + "\r\n";
                    }
                    return strRet;
                }
        */
        /// <summary>
        /// 取得电脑的硬件信息
        /// </summary>
        /// <param name="hardType"></param>
        /// <param name="propKey"></param>
        /// <returns></returns>
        public static string[] MulGetHardwareInfo(string hardType, string propKey)
        {

            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if (hardInfo.Properties[propKey].Value != null && hardInfo.Properties[propKey].Value.ToString().Contains("COM"))
                        {
                            strs.Add(hardInfo.Properties[propKey].Value.ToString());
                        }
                    }
                    searcher.Dispose();
                }
                return strs.ToArray();
            }
            catch
            {
                return null;
            }
            finally
            { strs = null; }
        }


        /// <summary>
        /// 枚举win32 api
        /// </summary>
        public enum HardwareEnum
        {
            // 硬件
            Win32_Processor, // CPU 处理器
            Win32_PhysicalMemory, // 物理内存条
            Win32_Keyboard, // 键盘
            Win32_PointingDevice, // 点输入设备，包括鼠标。
            Win32_FloppyDrive, // 软盘驱动器
            Win32_DiskDrive, // 硬盘驱动器
            Win32_CDROMDrive, // 光盘驱动器
            Win32_BaseBoard, // 主板
            Win32_BIOS, // BIOS 芯片
            Win32_ParallelPort, // 并口
            Win32_SerialPort, // 串口
            Win32_SerialPortConfiguration, // 串口配置
            Win32_SoundDevice, // 多媒体设置，一般指声卡。
            Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
            Win32_USBController, // USB 控制器
            Win32_NetworkAdapter, // 网络适配器
            Win32_NetworkAdapterConfiguration, // 网络适配器设置
            Win32_Printer, // 打印机
            Win32_PrinterConfiguration, // 打印机设置
            Win32_PrintJob, // 打印机任务
            Win32_TCPIPPrinterPort, // 打印机端口
            Win32_POTSModem, // MODEM
            Win32_POTSModemToSerialPort, // MODEM 端口
            Win32_DesktopMonitor, // 显示器
            Win32_DisplayConfiguration, // 显卡
            Win32_DisplayControllerConfiguration, // 显卡设置
            Win32_VideoController, // 显卡细节。
            Win32_VideoSettings, // 显卡支持的显示模式。

            // 操作系统
            Win32_TimeZone, // 时区
            Win32_SystemDriver, // 驱动程序
            Win32_DiskPartition, // 磁盘分区
            Win32_LogicalDisk, // 逻辑磁盘
            Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
            Win32_LogicalMemoryConfiguration, // 逻辑内存配置
            Win32_PageFile, // 系统页文件信息
            Win32_PageFileSetting, // 页文件设置
            Win32_BootConfiguration, // 系统启动配置
            Win32_ComputerSystem, // 计算机信息简要
            Win32_OperatingSystem, // 操作系统信息
            Win32_StartupCommand, // 系统自动启动程序
            Win32_Service, // 系统安装的服务
            Win32_Group, // 系统管理组
            Win32_GroupUser, // 系统组帐号
            Win32_UserAccount, // 用户帐号
            Win32_Process, // 系统进程
            Win32_Thread, // 系统线程
            Win32_Share, // 共享
            Win32_NetworkClient, // 已安装的网络客户端
            Win32_NetworkProtocol, // 已安装的网络协议
            Win32_PnPEntity,//all device
        }
        /// <summary>
        /// WMI取硬件信息
        /// </summary>
        /// <param name="hardType"></param>
        /// <param name="propKey"></param>
        /// <returns></returns>
        public static string[] MulGetHardwareInfo(HardwareEnum hardType, string propKey)
        {

            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if (hardInfo.Properties[propKey].Value != null)
                        {

                            //if (hardInfo.Properties[propKey].Value.ToString().Contains("COM"))
                            {
                                strs.Add(hardInfo.Properties[propKey].Value.ToString());
                            }
                        }
                    }
                    searcher.Dispose();
                }
                return strs.ToArray();
            }
            catch
            {
                return null;
            }
            finally
            { strs = null; }
        }



        #endregion

        public static void KillProcess(string proceName)
        {
            if (proceName == null)
                return;
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process p in myProcesses)
            {
                //if (p.ProcessName.ToUpper().Contains(proceName.ToUpper()))
                if (p.ProcessName.ToUpper() == proceName.ToUpper())
                {
                    p.Kill();
                    p.WaitForExit();
                }
            }
        }
        public static string GetHostIp()
        {
            return GetLocalIp();
        }
        public static string GetLocalIp()
        {
            string hostname = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            //IPAddress localaddr = localhost.AddressList[0];
            for (int i = 0; i < localhost.AddressList.Length; i++)
            {
                //从IP地址列表中筛选出IPv4类型的IP地址
                //AddressFamily.InterNetwork // IPv4,
                //AddressFamily.InterNetworkV6 // IPv6
                if (localhost.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return localhost.AddressList[i].ToString();
                }
            }
            return "";
        }

        public static string GetHostMac()
        {
            //List<string> macList = new List<string>();
            ManagementClass mc;
            mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["IPEnabled"].ToString() == "True")
                    return mo["MacAddress"].ToString();
            }
            return "";
        }
        public static string GetHostName()
        {
            string hostname = Dns.GetHostName();
            return hostname;
        }
        public static int GetOSBit()
        {
            try
            {
                string addressWidth = "";
                ConnectionOptions connectionOptions = new ConnectionOptions();
                ManagementScope managementScope = new ManagementScope(@"\\localhost", connectionOptions);
                ObjectQuery objectQuery = new ObjectQuery("select AddressWidth from Win32_Processor");
                ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery);
                ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
                foreach (ManagementObject managementObject in managementObjectCollection)
                {
                    addressWidth = managementObject["AddressWidth"].ToString();
                }

                return int.Parse(addressWidth);
            }
            catch
            {
                throw;
            }
        }

        public static int GetIso8601WeekOfYear(DateTime date)
        {
            var calendar = CultureInfo.InvariantCulture.Calendar;
            var rule = CalendarWeekRule.FirstFourDayWeek;
            var firstDayOfWeek = DayOfWeek.Monday;

            return calendar.GetWeekOfYear(date, rule, firstDayOfWeek);
        }

        /// <summary>
        /// 查找第N次子字符串出现的位置
        /// </summary>
        /// <param name="str"></param>
        /// <param name="substr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int IndexOfSubStr(string str, string substr, int count)
        {
            int posS = -1;
            int offset = 0;
            while ((count--) > 0)
            {
                posS = str.IndexOf(substr, offset);
                if (posS == -1)
                    break;
                offset = posS + substr.Length;
            }
            return posS;
        }
        public static string GetNextStrBy34(string lastStr)
        {
            if (string.IsNullOrEmpty(lastStr))
                return string.Empty;

            // 34进制递增
            char[] cs = lastStr.ToCharArray();
            for (int i = cs.Length - 1; i >= 0; i--)
            {
                char c;
                bool ret = GetNextCharBy34(cs[i], out c);
                cs[i] = c;
                if (!ret)
                {
                    return new string(cs);
                }
            }

            // 溢出
            return string.Empty;
        }

        /// <summary>
        /// 按34进制递增，返回下一个字符，去除字母O,I
        /// </summary>
        /// <param name="last"></param>
        /// <param name="next"></param>
        /// <returns>true:有进位；false:无进位</returns>
        static bool GetNextCharBy34(char last, out char next)
        {
            bool isOver = false;

            char c = last;
            if (last == '9')
                c = 'A';
            else if (last == 'Z')
            {
                c = '0';
                isOver = true;
            }
            else
            {
                c++;
                if (c == 'I' || c == 'O')
                    c++;
            }
            next = c;

            return isOver;
        }
        public static string GetNextStrBy10(string lastStr)
        {
            if (string.IsNullOrEmpty(lastStr))
                return string.Empty;

            // 10进制递增
            char[] cs = lastStr.ToCharArray();
            for (int i = cs.Length - 1; i >= 0; i--)
            {
                char c;
                bool ret = GetNextCharBy10(cs[i], out c);
                if (c == ' ')
                    return "";
                cs[i] = c;
                if (!ret)
                {
                    return new string(cs);
                }
            }

            // 溢出
            return string.Empty;
        }

        /// <summary>
        /// 取得连号得下一个字串，统一以大写字符为准
        /// </summary>
        /// <param name="opt">10:10进制；16:16进制；34:34进制;24:24进制, 36:36进制</param>
        /// <param name="lastStr"></param>
        /// <returns>非空：下一个字串，空：出错</returns>
        public static string GetNextStr(string opt, string lastStr)
        {
            if (string.IsNullOrEmpty(lastStr))
                return string.Empty;


            char[] cs = lastStr.ToCharArray();
            for (int i = cs.Length - 1; i >= 0; i--)
            {
                char c;
                bool isOver = GetNextChar(opt, cs[i], out c);

                if (c == ' ')
                    return string.Empty; ;
                cs[i] = c;

                if (!isOver)
                {
                    return new string(cs);
                }
            }

            // 溢出
            return string.Empty;
        }


        /// <summary>
        /// 按10进制递增，返回下一个字符
        /// </summary>
        /// <param name="last"></param>
        /// <param name="next"></param>
        /// <returns>true:有进位；false:无进位</returns>
        static bool GetNextCharBy10(char last, out char next)
        {
            bool isOver = false;
            char c = last;
            if (last == '9')
            {
                c = '0';
                isOver = true;
            }
            else if (last >= '0' && last < '9')
            {
                c++;
            }
            else
                c = ' ';

            next = c;

            return isOver;
        }



        /// <summary>
        /// 按连号规则返回下一个字符
        /// </summary>
        /// <param name="last">上一个字符</param>
        /// <param name="next">下一个字符</param>
        /// <returns>true: 有进位;false:无进位</returns>
        static bool GetNextChar(string opt, char last, out char next)
        {
            next = ' ';

            bool isOver = false;

            char[] cs;

            if (opt == "10")
            {
                cs = "0123456789".ToCharArray();
            }
            else if (opt == "16")
            {
                cs = "0123456789ABCDEF".ToCharArray();
            }
            else if (opt == "34")
            {
                cs = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
            }
            else if (opt == "36")
            {
                cs = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            }
            else if (opt == "26")
            {
                cs = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            }
            else if (opt == "24")
            {
                cs = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
            }
            else
            {
                return false;
            }

            for (int i = 0; i < cs.Length; i++)
            {
                if (cs[i] == last)
                {
                    if (i == cs.Length - 1)
                    {
                        next = cs[0];
                        isOver = true;
                    }
                    else
                    {
                        next = cs[i + 1];
                    }
                    break;
                }
            }
            return isOver;
        }




        /// <summary>
        /// 按照当前年月日产生 符合 SN 规范的 4位日期代码
        /// 年份代码 0~Z ： 34进制，  比如 表示 2020 ~ 2052年，每34年重复
        /// </summary>
        /// <returns></returns>
        public static string CreatDateCode()
        {
            // "H24DB0024090001" // 15位  机种 4 + 板别 3 + 日期 4 + 流水 4
            string sn;
            DateTime tm = DateTime.Now;
            sn = "";

            int y = (tm.Year - 2020) % 34;
            if (y >= 0 && y <= 33) // ABCDEFGH ==> 2030 ~ 2037 ， 共 8个
            {
                sn = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ".Substring(y, 1);
            }

            int m = tm.Month;
            if (m >= 1 && m <= 12)
            {
                sn += "123456789XYZ".Substring(m - 1, 1);
            }
            else
            {
                return "";
            }
            int d = tm.Day;
            if (d >= 1 && d <= 31)
            {
                sn += d.ToString("D2");
            }

            return sn;
        }

        /// <summary>
        /// 按照当前年月产生日期编码
        /// 用年的最后两位数字，如2014=14表示
        /// 月份，1-JAN，2-FEB，3-MAR，4-APR，5-MAY，6-JUN，7-JUL，8-AUG，9-SEP，X-OCT，Y-NOV，Z-DEC
        /// </summary>
        /// <returns>4位编码</returns>
        public static string CreatDateCodeYYM()
        {
            DateTime tm = DateTime.Now;

            string s = (tm.Year % 100).ToString("D2");

            int m = tm.Month;

            if (m >= 1 && m <= 12)
            {
                s = "123456789XYZ".Substring(m - 1, 1);
            }
            else
            {
                return "";
            }

            return s;
        }


        public static bool isMac(string str)
        {
            Regex regex = new Regex("^[A-F0-9]{12}$");

            return (regex.Match(str).Success);
        }

        // 箱号或SN
        public static bool isSN(string str)
        {
            // AAAABBCYMDDXXXX
            //Regex regex = new Regex("^[A-Z0-9]{15}$");
            //Regex regex = new Regex("^[A-HJ-NP-Z0-9]{8}[1-9XYZ]{1}[0-9]{2}[A-HJ-NP-Z0-9]{4}$");
            //return (regex.Match(str).Success);

            //return new Regex("^[A-HJ-NP-Z0-9]{8}[1-9XYZ]{1}[0-9]{2}[A-HJ-NP-Z0-9]{4}$").Match(str).Success;

            // 2024/10/9 为了兼容 TNG项目，TNG 客户需要支持字母 I 和 O ，所以 以后放宽对SN格式的检查
            return new Regex("^[A-Z0-9]{8}[1-9XYZ]{1}[0-9]{2}[A-Z0-9]{4}$").Match(str).Success;


        }

        public static bool isSn(string str)
        {
            // AAAABBCYMDDXXXX
            // 输入的仅仅是一个普通的sn ，(非box)

            //if (new Regex("^[A-HJ-NP-Z0-9]{8}[1-9XYZ]{1}[0-9]{2}[A-HJ-NP-Z0-9]{4}$").Match(str).Success)
            // 2024/10/9 为了兼容 TNG项目，TNG 客户需要支持字母 I 和 O ，所以 以后放宽对SN格式的检查
            if (new Regex("^[A-Z0-9]{8}[1-9XYZ]{1}[0-9]{2}[A-Z0-9]{4}$").Match(str).Success)
            {
                if (!isBox(str))
                    return true;
            }
            return false;
        }

        // AAAABBVYMDDSSSS
        public static bool isBox(string str)
        {
            //return new Regex("^[A-HJ-NP-Z0-9]{4}11[A-HJ-NP-Z0-9]{2}[1-9XYZ]{1}[0-9]{2}[A-HJ-NP-Z0-9]{4}$").Match(str).Success;
            // 2024/10/9 为了兼容 TNG项目，TNG 客户需要支持字母 I 和 O ，所以 以后放宽对SN格式的检查
            return new Regex("^[A-Z0-9]{4}11[A-Z0-9]{2}[1-9XYZ]{1}[0-9]{2}[A-Z0-9]{4}$").Match(str).Success;
        }
        public static bool isBox111(string str)
        {
            //return new Regex("^[A-HJ-NP-Z0-9]{4}111[A-HJ-NP-Z0-9]{1}[1-9XYZ]{1}[0-9]{2}[A-HJ-NP-Z0-9]{4}$").Match(str).Success;
            // 2024/10/9 为了兼容 TNG项目，TNG 客户需要支持字母 I 和 O ，所以 以后放宽对SN格式的检查
            return new Regex("^[A-Z0-9]{4}111[A-Z0-9]{1}[1-9XYZ]{1}[0-9]{2}[A-Z0-9]{4}$").Match(str).Success;
        }

        public static bool isBox116(string str)
        {
            //return new Regex("^[A-HJ-NP-Z0-9]{4}116[A-HJ-NP-Z0-9]{1}[1-9XYZ]{1}[0-9]{2}[A-HJ-NP-Z0-9]{4}$").Match(str).Success;
            // 2024/10/9 为了兼容 TNG项目，TNG 客户需要支持字母 I 和 O ，所以 以后放宽对SN格式的检查
            return new Regex("^[A-Z0-9]{4}116[A-Z0-9]{1}[1-9XYZ]{1}[0-9]{2}[A-Z0-9]{4}$").Match(str).Success;
        }

        public static bool isSN(string str, string typeCode)
        {
            if (isSN(str))
            {
                if (str.IndexOf(typeCode) == 0)
                {
                    return true;
                }
            }
            return false;
        }
        public static string KeyVal2Json(string key, string val)
        {
            return $"[{{\"Name\":\"{key}\", \"Value\":\"{val}\"}}]";
        }
        public static string KeyVal2Json(string key, string val, string key2, string val2)
        {
            string josn = $"[" +
                $"{{\"Name\":\"{key}\",\"Value\":\"{val}\"}}" +
                $",{{\"Name\":\"{key2}\",\"Value\":\"{val2}\"}}" +
                $"]";
            return josn;
        }
        /*
          0   普通用户
          1   产线管理员
          2   管理员 - TE
          3   管理员 - PE
          4   管理员 - FPM
          5   特殊角色
          99  系统管理员 - AE
        */
        public static string GetUserRoleName(int iRrole)
        {
            switch (iRrole)
            {
                case 0:
                    return T("普通用户");
                case 1:
                    return T("产线管理员");
                case 2:
                    return T("管理员");
                case 5:
                    return T("维修管理员");
                case 6:
                    return T("维修员");
                case 99:
                    return T("系统管理员");
                default:
                    return T("未知角色");
            }
        }

        public static string[] InputTypes2NameKeyType(string inputTypes)
        {
            string[] ret = new string[4];
            // csn,csn,var,^H[0-9]{2}[A-Z0-9]{7,8}[0-9]{4}$
            try
            {
                string s = inputTypes;
                string[] ss;
                ss = s.Split(',');
                if (ss.Length < 4)
                    return null;

                ret[0] = ss[0].Trim();
                ret[1] = ss[1].Trim();
                ret[2] = ss[2].Trim();
                ret[3] = "";
                int pos = s.IndexOf(",", 0);
                if (pos != -1)
                {
                    pos = s.IndexOf(",", pos + 1);
                    if (pos != -1)
                    {
                        pos = s.IndexOf(",", pos + 1);
                        if (pos != -1)
                            ret[3] = s.Substring(pos + 1).Trim();
                    }
                }
                if (ret[3] == "")
                    return null;
            }
            catch
            {
                return null;
            }
            return ret;
        }

        /// <summary>
        /// 对输入内容做预处理
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="skip">跳转到指定字符串之后</param>
        /// <param name="replaces">替换，可以为多组替换，每组用逗号分隔，前面的替换成后面的</param>
        /// <param name="prefix">加上前缀</param>
        /// <param name="postfix">加上后缀</param>
        /// <returns></returns>
        public static string InputPreProc(string input, string skip, string[] replaces, string prefix, string postfix)
        {
            if (skip != "")
            {
                int pos = input.IndexOf(skip);
                if (pos >= 0)
                {
                    input = input.Substring(pos + skip.Length);
                }
            }
            for (int i = 0; i < replaces.Length; i++)
            {
                string s = replaces[i];
                string[] ss = s.Split(',');

                input = input.Replace(ss[0], ss[1]);
            }
            return prefix + input + postfix;
        }

        public static string InputPreProc(string input, string skip, string replace, string prefix, string postfix)
        {
            if (skip != "")
            {
                int pos = input.IndexOf(skip);
                if (pos >= 0)
                {
                    input = input.Substring(pos + skip.Length);
                }
            }
            if (replace != "")
            {
                string[] ss = replace.Split(',');
                int items = ss.Length / 2;

                for (int i = 0; i < items; i += 2)
                {
                    input = input.Replace(ss[i], ss[i + 1]);
                }
            }

            return prefix + input + postfix;
        }
    }

    #region Windows API
    /// <summary>
    /// WinApi 类 整理了一些 windown 提供的函数
    /// </summary>
    public static class WindowsApi
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, uint hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);


        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);

        //[DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
        [DllImport("User32.dll")]
        private static extern Int32 SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
        //[DllImport("user32.dll ", CharSet = CharSet.Unicode)]
        public static extern IntPtr PostMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
        public static extern void SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpfn, int lParam);

        public delegate bool CallBack(IntPtr hwnd, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        public static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SwitchToThisWindow(IntPtr hWnd, bool fAltTab);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int nMaxCount);

        public const int WM_SETTEXT = 0x000C; // 设置文本内容的消息
        public const int BM_CLICK = 0x00F5; // 鼠标点击事件


        public const int WM_CLOSE = 0x10; //关闭命令
        public const int WM_KEYDOWN = 0x0100;//按下键
        public const int WM_KEYUP = 0x0101;//按键起来
        public const int VK_RETURN = 0x0D;//回车键
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x0202;
        public const short HWND_TOP = 0;//           { 在前面}
        public const short HWND_BOTTOM = 1; //       { 在后面}
        public const short HWND_TOPMOST = (-1);// HWND(-1); // { 在前面, 位于任何顶部窗口的前面}
        public const short HWND_NOTOPMOST = (-2); // HWND(-2); //{ 在前面, 位于其他顶部窗口的后面}

        public const short SWP_NOMOVE = 0X2;
        public const short SWP_NOSIZE = 1;
        public const short SWP_NOZORDER = 0X4;
        public const int SWP_SHOWWINDOW = 0x0040;

        public const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        //模拟鼠标滚轮滚动操作，必须配合dwData参数
        public const int MOUSEEVENTF_WHEEL = 0x0800;

        public struct RECT
        {
            public int left;                             //最左坐标
            public int top;                             //最上坐标
            public int right;                           //最右坐标
            public int bottom;                        //最下坐标
        }



        private static class NativeMethods
        {
            internal const uint GW_OWNER = 4;

            internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern int GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern bool IsWindowVisible(IntPtr hWnd);


        }

        public static IntPtr GetMainWindowHandle(int processId)
        {
            IntPtr MainWindowHandle = IntPtr.Zero;

            NativeMethods.EnumWindows(new NativeMethods.EnumWindowsProc((hWnd, lParam) =>
            {
                IntPtr PID;
                NativeMethods.GetWindowThreadProcessId(hWnd, out PID);

                if (PID == lParam &&
                    NativeMethods.IsWindowVisible(hWnd) &&
                    NativeMethods.GetWindow(hWnd, NativeMethods.GW_OWNER) == IntPtr.Zero)
                {
                    MainWindowHandle = hWnd;
                    return false;
                }

                return true;

            }), new IntPtr(processId));

            return MainWindowHandle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SendMouseEvent(IntPtr hWnd, int x, int y)
        {
            if (hWnd != IntPtr.Zero)
            {
                WindowsApi.RECT rect = new WindowsApi.RECT();
                WindowsApi.GetWindowRect(hWnd, ref rect);
                x += rect.left;
                y += rect.top;
            }
            SwitchToThisWindow(hWnd, true);

            Point curPos;
            curPos = Control.MousePosition;

            WindowsApi.SetCursorPos(x, y);
            Thread.Sleep(20);
            //Thread.Sleep(5000); //for debug

            //移动鼠标 

            WindowsApi.mouse_event(WindowsApi.MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            //     SendMessage(hWnd, WM_LBUTTONDBLCLK, (IntPtr)MK_LBUTTON, (IntPtr)(y * 65536 + x));
            Thread.Sleep(20);
            WindowsApi.mouse_event(WindowsApi.MOUSEEVENTF_LEFTUP, x, y, 0, 0);

            WindowsApi.SetCursorPos(curPos.X, curPos.Y);
        }


        /// <summary>
        /// 查找窗体上控件句柄
        /// </summary>
        /// <param name="hwnd">父窗体句柄</param>
        /// <param name="lpszWindow">控件标题(Text)</param>
        /// <param name="bChild">设定是否在子窗体中查找</param>
        /// <returns>控件句柄，没找到返回IntPtr.Zero</returns>
        public static IntPtr FindWindowEx(IntPtr hwnd, string lpszWindow, bool bChild)
        {
            IntPtr iResult = IntPtr.Zero;
            // 首先在父窗体上查找控件
            iResult = WindowsApi.FindWindowEx(hwnd, 0, null, lpszWindow);
            // 如果找到直接返回控件句柄
            if (iResult != IntPtr.Zero) return iResult;

            // 如果设定了不在子窗体中查找
            if (!bChild) return iResult;

            // 枚举子窗体，查找控件句柄
            int i = WindowsApi.EnumChildWindows(
            hwnd,
            (h, l) =>
            {
                IntPtr f1 = WindowsApi.FindWindowEx(h, 0, null, lpszWindow);
                if (f1 == IntPtr.Zero)
                    return true;
                else
                {
                    iResult = f1;
                    return false;
                }
            },
            0);
            // 返回查找结果
            return iResult;
        }



        // 枚举桌面所有窗口句柄和名称
        // http://t.zoukankan.com/chencidi-p-1907377.html

        private delegate bool WNDENUMPROC(IntPtr hWnd, int lParam);

        //用来遍历所有窗口 
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, int lParam);

        //获取窗口Text 
        [DllImport("user32.dll")]
        private static extern int GetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpString, int nMaxCount);

        //获取窗口类名 
        [DllImport("user32.dll")]
        private static extern int GetClassNameW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpString, int nMaxCount);

        //自定义一个类，用来保存句柄信息，在遍历的时候，随便也用空上句柄来获取些信息，呵呵 
        public struct WindowInfo
        {
            public IntPtr hWnd;
            public string szWindowName;
            public string szClassName;
        }

        public static WindowInfo[] GetAllDesktopWindows()
        {
            //用来保存窗口对象 列表
            List<WindowInfo> wndList = new List<WindowInfo>();

            //enum all desktop windows 
            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                WindowInfo wnd = new WindowInfo();
                StringBuilder sb = new StringBuilder(256);

                //get hwnd 
                wnd.hWnd = hWnd;

                //get window name  
                GetWindowTextW(hWnd, sb, sb.Capacity);
                wnd.szWindowName = sb.ToString();

                //get window class 
                GetClassNameW(hWnd, sb, sb.Capacity);
                wnd.szClassName = sb.ToString();

                //add it into list 
                wndList.Add(wnd);
                return true;
            }, 0);

            return wndList.ToArray();
        }

        /// <summary>
        /// 模糊查询窗口句柄
        /// </summary>
        /// <param name="winName"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static IntPtr GetWin(string winName, string className)
        {
            WindowInfo[] wins = GetAllDesktopWindows();
            foreach (WindowInfo win in wins)
            {
                if (win.szWindowName.Contains(winName) && win.szClassName.Contains(className))
                    return win.hWnd;
            }
            return IntPtr.Zero;
        }


        [DllImport("user32.dll")]
        internal extern static int SetWindowLong(IntPtr hwnd, int index, int value);

        [DllImport("user32.dll")]
        internal extern static int GetWindowLong(IntPtr hwnd, int index);

        internal static void HideMinimizeAndMaximizeButtons(IntPtr hwnd)
        {
            const int GWL_STYLE = -16;
            const long WS_MINIMIZEBOX = 0x00020000L;
            const long WS_MAXIMIZEBOX = 0x00010000L;

            long value = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX));
        }
        internal static void HideMinimizeButtons(IntPtr hwnd)
        {
            const int GWL_STYLE = -16;
            const long WS_MINIMIZEBOX = 0x00020000L;
            //const long WS_MAXIMIZEBOX = 0x00010000L;

            long value = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MINIMIZEBOX /*& ~WS_MAXIMIZEBOX*/));
        }
    }

    public class WindowHandleInfo
    {
        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        private IntPtr _MainHandle;

        public WindowHandleInfo(IntPtr handle)
        {
            this._MainHandle = handle;
        }

        public List<IntPtr> GetAllChildHandles()
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }
    }

    public class MyProfile
    {
        private string CfgFile1 { set; get; }
        private string CfgSection1 { set; get; }
        private string CfgFile2 { set; get; }
        private string CfgSection2 { set; get; }

        public MyProfile(string cfgFile1, string cfgSection1, string cfgFile2 = "", string cfgSection2 = "")
        {
            CfgFile1 = cfgFile1;
            CfgSection1 = cfgSection1;
            CfgFile2 = cfgFile2;
            CfgSection2 = cfgSection2;
        }

        public string ReadItemTrim(string key, string def)
        {
            return TrimSpaceByComma(ReadItem(key, def));
        }
        public string ReadItem(string key, string def)
        {
            string s;
            if (null == (s = Misc.GetProfile(CfgFile1, CfgSection1, key, null)))
            {
                if (CfgFile2 == "" || (null == (s = Misc.GetProfile(CfgFile2, CfgSection2, key, null))))
                {
                    s = def;
                }
            }
            return s;
        }

        public bool WriteItem(string key, string val)
        {
            return Misc.WriteProfile(CfgFile1, CfgSection1, key, val);
        }
        public string TrimSpaceByComma(string input)
        {
            if (input == null)
                return null;
            string[] ss = input.Split(',');
            string s = ss[0].Trim();
            for (int k = 1; k < ss.Length; k++)
                s += ("," + ss[k].Trim());
            return s;
        }
    }
    #endregion
    public class DbgLog
    {
        private string LogFileName { set; get; }
        private string LocalLogFile { set; get; }
        private string RemoteLogFile { set; get; }
        private int DbgLevel { set; get; }
        bool _isDbgTrace;
        public DbgLog(string station, int dbgLevel)
        {
            //_dbgLogRemoteDir = remoteDir;
            //按机型分开保存debug log
            //优点是不用担心同一个目录下的文件太多，不用经常清理log文件
            //缺点是无法在一个目录下，通过时间排序能看出最近有哪些工具在使用，不过这个功能可以通过后续的系统查询的完善而改善
            DbgLevel = dbgLevel;

            HsFile2Srv hsFile2Srv = new HsFile2Srv("LOG", null);
            _isDbgTrace = hsFile2Srv.Exist("StnDbgLog\\DbgTrace.ini");

            string toolName = System.AppDomain.CurrentDomain.FriendlyName;
            string hostname = Dns.GetHostName();
            string hostMac = "";

            do
            {
                ManagementClass mc;
                mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (mo["IPEnabled"].ToString() == "True")
                        hostMac = mo["MacAddress"].ToString().Replace(":", "").ToUpper();
                }
            } while (false);

            //string dbgLogName = $"dbglog_{station}_{toolName}_({hostMac})_{hostname}.txt";
            string dbgLogName = $"dbglog_{station}_{toolName}_{hostname}_({hostMac}).txt";

            string localDbgLogDir = @"c:\hts\logs";
            string remoteDbgLogDir = @"StnDbgLog";
            if (station == "")
                remoteDbgLogDir += "\\other";
            else if (station.Length > 4)
                remoteDbgLogDir += ("\\" + station.Substring(0, 4));
            else
                remoteDbgLogDir += ("\\" + station);

            LocalLogFile = localDbgLogDir + "\\" + dbgLogName;
            RemoteLogFile = remoteDbgLogDir + "\\" + dbgLogName;
            LogFileName = dbgLogName;

            InitLogQueue();
            WriteLine($"\r\n##############################");
            WriteLine($"localDbgLogDir={localDbgLogDir}");
            WriteLine($"remoteDbgLogDir={remoteDbgLogDir}");
            WriteLine($"dbgLogName={dbgLogName}");
            WriteLine($"DbgLevel={DbgLevel}");

            InitHtsDbgLogOffset();

            // 先根据本地Log文件的大小，和服务器上已经存在的log文件，裁决 备份到服务器上的文件名
        }

        public void WriteLine(string s)
        {
            if (string.IsNullOrEmpty(s))
                return;
            if (DbgLevel >= 0)
                Post2Task(s);
        }
        public void SetLevel(int level)
        {
            DbgLevel = level;
        }
        public void Info(string s)
        {
            if (DbgLevel >= 0)
                Post2Task("INFO " + s);
        }
        public void Err(string s)
        {
            if (DbgLevel >= 1)
                Post2Task("ERR " + s);
        }
        public void Dbg(string s)
        {
            if (DbgLevel >= 2)
                Post2Task("DBG " + s);
        }
        public void Show(string s)
        {
            if (DbgLevel >= 3)
                Post2Task("SHOW " + s);
        }
        public void CopyToServer()
        {
            Post2Task("");
        }

        private long _htsDbgLogOffset = 0;
        public void InitHtsDbgLogOffset()
        {
            try
            {
                string log = $"c:\\hts\\logs\\log{DateTime.Now.ToString("yyyyMMdd")}.txt";
                if (!File.Exists(log))
                    _htsDbgLogOffset = 0;
                else
                {
                    FileInfo fileInfo = new FileInfo(log);
                    _htsDbgLogOffset = fileInfo.Length;
                }
            }
            catch
            {
                _htsDbgLogOffset = 0;
            }
        }

        public string GetHtsDbgLogLast()
        {
            string strRet = "";

            try
            {
                string log = $"c:\\hts\\logs\\log{DateTime.Now.ToString("yyyyMMdd")}.txt";
                if (!File.Exists(log))
                    return "";
                using (FileStream fs = new FileStream(log, FileMode.Open, FileAccess.Read))
                {
                    if (_htsDbgLogOffset <= fs.Length)
                    {
                        fs.Seek(_htsDbgLogOffset, SeekOrigin.Begin);
                    }
                    _htsDbgLogOffset = fs.Length;

                    using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                    {
                        strRet = sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return "";
            }
            return strRet;
        }


        private BlockingCollection<Action> _logActionQueue = new BlockingCollection<Action>();
        private Task _logWorkTask;
        private CancellationTokenSource _logCancellationTokenSource;

        private void InitLogQueue()
        {
            // 重新启动队列的执行
            _logActionQueue = new BlockingCollection<Action>();
            _logCancellationTokenSource = new CancellationTokenSource();
            _logWorkTask = Task.Run(() => LogProcessQueue(_logCancellationTokenSource.Token));
        }

        /// <summary>
        /// 将调试log推送到消息队列
        /// </summary>
        /// <param name="s">当s为空时表示自动备份</param>
        private void Post2Task(string s)
        {
            // 将要执行的操作加入队列
            _logActionQueue.Add(() => LogWorkTaskDoWork(s));
        }

        //private void LogCleanWorkTask()
        //{
        //    //_isNeedClearWorkTaskQue = false;

        //    //// 取消后台任务并清理资源
        //    //cancellationTokenSource.Cancel();
        //    //actionQueue.CompleteAdding();
        //    //_workTask.Wait();

        //    //actionQueue.Dispose();
        //    //cancellationTokenSource.Dispose();

        //    //InitializeWorkTask();
        //}
        //private void LogCancelWorkTask()
        //{
        //    // 取消后台任务并清理资源
        //    _logCancellationTokenSource.Cancel();
        //    _logActionQueue.CompleteAdding();
        //    _logWorkTask.Wait();

        //    _logActionQueue.Dispose();
        //    _logCancellationTokenSource.Dispose();

        //}
        //private void LogCloseWorkTask()
        //{
        //    // 取消后台任务并清理资源
        //    _logCancellationTokenSource.Cancel();
        //    _logActionQueue.CompleteAdding();
        //    _logWorkTask.Wait();
        //    _logActionQueue.Dispose();
        //    _logCancellationTokenSource.Dispose();
        //}

        private void LogProcessQueue(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var action in _logActionQueue.GetConsumingEnumerable(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    action.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                // 当取消操作时退出循环
            }
        }

        private void LogWorkTaskDoWork(string s)
        {
            if (string.IsNullOrEmpty(s))
                CopyToServerDo(false);
            else
                WriteLineDo(s);
        }

        private void WriteLineDo(string msg)
        {
            DateTime dt = DateTime.Now;
            const long MAX_SIZE = 10 * 1024 * 1024;
            string logFile = LocalLogFile;
            bool isNeedBackup = false;

            string err = "";
            for (int k = 1; k <= 100; k++)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(logFile, true, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine(dt.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + msg);
                        long len = new System.IO.FileInfo(logFile).Length;
                        if (len >= MAX_SIZE)
                        {
                            isNeedBackup = true;
                            writer.WriteLine($"AutoBackupDbgLog({len})");
                        }
                        writer.Close();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    err += $"\r\nCrash {k}: " + ex.Message;
                    Thread.Sleep(10);
                }
            }
            if (_isDbgTrace && err != "")
            {
                err = $"DbgLog.WriteLineDo({msg})..." + err;
                TraceDbgLog(dt, err);
            }

            if (isNeedBackup)
            {
                // 自动备份调试日志
                CopyToServerDo(true);
            }
        }

        public string ChkFileLocked(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return "";
                //尝试打开文件以检查是否被锁定
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // 文件未被锁定，可以访问
                    //stream.Close();
                    //stream.Dispose();
                    return "";
                }
            }
            catch (IOException ex)
            {
                // 文件被锁定，出现IO异常
                //Console.WriteLine($"文件 '{filePath}' 正在被其他程序使用：{ex.Message}");
                return "ChkFileLocked: " + ex.Message;
            }
        }


        /// <summary>
        /// 拷贝本地log到服务器
        /// </summary>
        /// <param name="isNeedNewName">是否用新的文件名备份</param>
        private void CopyToServerDo(bool isNeedNewName)
        {
            DateTime dt = DateTime.Now;
            string localLogFile = LocalLogFile;
            string remoteLogFile = RemoteLogFile;

            string err = "";
            for (int k = 1; k <= 500; k++)
            {
                try
                {
                    // 云端是相对路径，log存放在有权限管控的服务器上， 采用HTS提供的接口进行拷贝 , 
                    // 先根据本地Log文件的大小，和服务器上已经存在的log文件，裁决 备份到服务器上的文件名
                    HsFile2Srv hsFile2Srv = new HsFile2Srv("LOG", null);

                    if (isNeedNewName)
                    {
                        string destFileNew;
                        for (int i = 1; true; i++)
                        {
                            destFileNew = remoteLogFile + "-" + i.ToString() + ".txt";
                            if (!hsFile2Srv.Exist(destFileNew))
                            {
                                remoteLogFile = destFileNew;
                                break;
                            }
                        }
                    }
                    // 云端是相对路径，log存放在有权限管控的服务器上， 采用HTS提供的接口进行拷贝 , 
                    string sErrMsg;
                    if (1 != hsFile2Srv.Copy(localLogFile, remoteLogFile, out sErrMsg, true))
                    {
                        err += $"\r\nFile2Srv.Copy({localLogFile},{remoteLogFile})...\r\nFail {k} :" + sErrMsg;
                        Thread.Sleep(10);
                        continue;
                    }
                    else if (isNeedNewName)
                    {
                        // 如果当前Log已经增加后缀改名备份后，删除本地log, 当新的log生成时会重新创建该文件
                        File.Delete(localLogFile);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    err += $"\r\nCrash {k}: " + ex.Message;
                    Thread.Sleep(10);
                }
            }

            if (_isDbgTrace && err != "")
            {
                err = $"DbgLog.CopyToServerDo({isNeedNewName})..." + err;
                TraceDbgLog(dt, err);
            }
        }

        private void TraceDbgLog(DateTime dt, string s)
        {
            try
            {
                // 下面这个log 是为了追踪是否还存在写调试日志失败的异常，这个log 单独拷贝在 根目录下
                string strName = LogFileName + "_" + dt.ToString("yyyyMMddTHHmmss.fff") + ".txt";

                string f = $"c:\\hts\\logs\\__tmp_" + strName;
                if (File.Exists(f))
                    File.Delete(f);
                File.WriteAllText(f, strName + "\r\n" + s);

                string fRemoteDir = "StnDbgLog\\" + strName;
                string sErrMsg;
                HsFile2Srv hsFile2Srv = new HsFile2Srv("LOG", null);
                hsFile2Srv.Copy(f, fRemoteDir, out sErrMsg, true);
                File.Delete(f);
            }
            catch/* (Exception ex)*/
            {
                //string s1 = ex.Message;
            }
        }
    }
}