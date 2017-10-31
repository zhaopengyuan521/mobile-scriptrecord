using AppiumUtility.Argument;
using AppiumUtility.Config;
using AppiumUtility.Logger;
using AppiumUtility.Models;
using AppiumUtility.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppiumUtility.Session
{
    /// <summary>
    /// Appium会话管理
    /// </summary>
    public class AppiumSessionManager
    {
        private static Lazy<AppiumSessionManager> _Instance = new Lazy<AppiumSessionManager>(() => new AppiumSessionManager());
        /// <summary>
        /// Appium会话管理单例
        /// </summary>
        public static AppiumSessionManager Instane { get { return _Instance.Value; } }

        /// <summary>
        /// 设备会话池
        /// </summary>
        private static List<AppiumSession> _DeviceSessionPool = new List<AppiumSession>();

        /// <summary>
        /// Appium进程池
        /// </summary>
        private static Dictionary<string, Process> _AppiumProcessPool = new Dictionary<string, Process>();

        /// <summary>
        /// 检查node modules目录
        /// </summary>
        public void CheckForNodeModules()
        {
            string nodeModules = AppConfigManager.Instance.NodeModulesFolder;
            if (Directory.Exists(nodeModules)) return;
            string appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7zr.exe");
            string nodePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "node_modules.7z");
            //检测node压缩文件与解压程序是否存在
            if (!File.Exists(appPath) || !File.Exists(nodePath)) return;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            startInfo.FileName = appPath;
            startInfo.Arguments = "x " + Path.GetFileName(nodePath);
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = true;
            Process compressProcess = new Process();
            compressProcess.StartInfo = startInfo;
            compressProcess.Start();
            compressProcess.WaitForExit();

        }

        /// <summary>
        /// 创建设备会话
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        public void CreateDeviceSession(string deviceSerialno)
        {
            //判断设备序列号在会话池中是否已存在
            bool isExist = _DeviceSessionPool.Exists(s => s.Serialno == deviceSerialno);
            if (isExist) return;
            AppiumSession session = GenerateAppiumSessionData(deviceSerialno);          //生成本次会话数据
            StartDeviceSession(session);            //创建并启动设备会话
            _DeviceSessionPool.Add(session);        //添加数据到会话池
        }

        /// <summary>
        /// 获取设备会话数据
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        /// <returns></returns>
        public AppiumSession GetDeviceSession(string deviceSerialno)
        {
            AppiumSession session = _DeviceSessionPool.Find(ds => ds == deviceSerialno);
            return session;
        }

        /// <summary>
        /// 判断设备是否存在会话
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        /// <returns></returns>
        public bool ExistSession(string deviceSerialno)
        {
            bool result = _DeviceSessionPool.Exists(ds => ds == deviceSerialno);
            return result;
        }

        /// <summary>
        /// 关闭设备会话
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        public void CloseDeviceSession(string deviceSerialno)
        {
            //删除会话数据
            _DeviceSessionPool.RemoveAll(ds => ds == deviceSerialno);
            //删除设备服务进程
            CloseProcess(_AppiumProcessPool[deviceSerialno], deviceSerialno);
            _AppiumProcessPool.Remove(deviceSerialno);
            AppLog.CreateAppLog().OutputLogData(string.Format("设备({0})Appium关联进程已关闭！", deviceSerialno));
        }

        /// <summary>
        /// 关闭进程
        /// </summary>
        /// <param name="process"></param>
        /// <param name="deviceSerialno"></param>
        private void CloseProcess(Process process, string deviceSerialno)
        {
            try
            {
                ProcessStartInfo taskkillInfo = new ProcessStartInfo();
                taskkillInfo.FileName = "cmd.exe";
                taskkillInfo.Arguments = "/c taskkill /f /t /PID " + process.Id;
                taskkillInfo.RedirectStandardOutput = true;
                taskkillInfo.RedirectStandardError = true;
                taskkillInfo.CreateNoWindow = true;
                taskkillInfo.UseShellExecute = false;
                Process taskKillProcess = new Process();
                taskKillProcess.StartInfo = taskkillInfo;
                taskKillProcess.Start();
                taskKillProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                deviceSerialno = string.IsNullOrEmpty(deviceSerialno) ? "未知" : deviceSerialno;
                AppLog.CreateAppLog().OutputLogData(string.Format("关闭设备[{0}]关联的Appium进程失败，{1}", deviceSerialno, ex.Message), ex);
            }
        }

        /// <summary>
        /// 释放设备会话
        /// </summary>
        public void DisposeDeviceSession()
        {
            AppLog.CreateAppLog().OutputLogData("正在释放Appium进程...");
            List<string> currentDeviceList = (from s in _DeviceSessionPool
                                              select s.Serialno).ToList();
            foreach (var deviceSerialno in currentDeviceList)
            {
                CloseDeviceSession(deviceSerialno);
            }
            _DeviceSessionPool.Clear();
            _AppiumProcessPool.Clear();
            AppLog.CreateAppLog().OutputLogData("释放Appium进程完成！");
        }

        /// <summary>
        /// 启动设备会话
        /// </summary>
        /// <param name="session">会话数据</param>
        private void StartDeviceSession(AppiumSession session)
        {
            //Appium服务启动参数
            List<IAppiumServerArgument> args = new List<IAppiumServerArgument>();
            args.Add(new ServerRunnerArgument());
            args.Add(new ServerAddressArgument(session.ServerAddress));
            args.Add(new ServerPortArgument(session.AppiumPort));
            args.Add(new AndroidBootstrapPortArgument(session.BootstrapPort));
            args.Add(new SelendroidPortArgument(session.SelendroidPort));
            args.Add(new ChromeDriverPortArgument(session.ChromedriverPort));
            args.Add(new OverrideExistingSessionArgument());
            args.Add(new PlatformNameArgument(AppConfigManager.Instance.PlatformName));
            args.Add(new CommandTimeoutArgument(120));          //默认所有会话的接收命令超时时间为120秒
            string argumentsCmd = String.Join<IAppiumServerArgument>(" ", args);
            //创建服务进程启动信息
            var appiumServerProcessStartInfo = new ProcessStartInfo();
            appiumServerProcessStartInfo.WorkingDirectory = AppConfigManager.Instance.AppiumWorkingDirectory;
            appiumServerProcessStartInfo.FileName = AppConfigManager.Instance.NodePath;
            appiumServerProcessStartInfo.Arguments = argumentsCmd + " --log-no-color";
            appiumServerProcessStartInfo.RedirectStandardOutput = true;
            appiumServerProcessStartInfo.RedirectStandardError = true;
            appiumServerProcessStartInfo.CreateNoWindow = true;
            appiumServerProcessStartInfo.UseShellExecute = false;
            AppiumDataReceived dataReceived = new AppiumDataReceived(session.Serialno);
            //创建服务进行并启动
            Process appiumServerProcess = new Process();
            appiumServerProcess.StartInfo = appiumServerProcessStartInfo;
            appiumServerProcess.OutputDataReceived += dataReceived.OutputDataReceived;
            appiumServerProcess.ErrorDataReceived += dataReceived.OutputDataReceived;
            appiumServerProcess.Start();
            appiumServerProcess.BeginOutputReadLine();
            appiumServerProcess.BeginErrorReadLine();
            DebugLog.CreateDebugLog().OutputLogData(string.Format("设备({0})会话已开启！Appium端口号：{1}；Bootstrap端口号：{2}；SelendroidPort：{3}；ChromedriverPort：{4}；", session.Serialno, session.AppiumPort, session.BootstrapPort, session.SelendroidPort, session.ChromedriverPort));
            AppLog.CreateAppLog().OutputLogData(string.Format("设备[{0}]Appium服务启动成功，启动参数：{1}", session.Serialno, argumentsCmd));
            _AppiumProcessPool.Add(session.Serialno, appiumServerProcess);
        }

        /// <summary>
        /// 生成设备会话数据
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        /// <returns></returns>
        private AppiumSession GenerateAppiumSessionData(string deviceSerialno)
        {
            string serverAddress = AppConfigManager.Instance.ServerAddress;
            uint beginPort = GetBeginPort();       //获取服务起始端口号
            uint appiumPort = NetworkHelper.GetUsablePort(beginPort);      //获取未使用的端口号
            uint bootstrapPort = NetworkHelper.GetUsablePort(appiumPort + 1);
            uint selendroidPort = NetworkHelper.GetUsablePort(bootstrapPort + 1);
            uint chromedriverPort = NetworkHelper.GetUsablePort(selendroidPort + 1);
            AppiumSession session = new AppiumSession(serverAddress, appiumPort, deviceSerialno);
            session.BootstrapPort = bootstrapPort;
            session.SelendroidPort = selendroidPort;
            session.ChromedriverPort = chromedriverPort;
            return session;
        }

        /// <summary>
        /// 获取起始服务端口号
        /// </summary>
        /// <returns></returns>
        private uint GetBeginPort()
        {
            uint port = 0;
            if (_DeviceSessionPool.Count == 0)
            {
                port = 4723;
                return port;
            }
            uint poolMaxPort = (from dp in _DeviceSessionPool
                                select dp.MaxPort()).Max();
            port = poolMaxPort + 1;
            return port;
        }
    }
}
