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

namespace AppiumUtility.AppiumProcess
{
    /// <summary>
    /// Appium进程管理
    /// </summary>
    public class AppiumProcessManager
    {
        #region 单例模式
        private static Lazy<AppiumProcessManager> _Instance = new Lazy<AppiumProcessManager>(() => new AppiumProcessManager());
        /// <summary>
        /// Appium进程管理单例
        /// </summary>
        public static AppiumProcessManager Instane { get { return _Instance.Value; } }
        #endregion
        #region 静态成员列表
        /// <summary>
        /// 设备Appium进程数据池
        /// </summary>
        private static List<AppiumProcessData> _DeviceAppiumProcessPool = new List<AppiumProcessData>();

        /// <summary>
        /// Appium进程池
        /// </summary>
        private static Dictionary<string, Process> _AppiumProcessPool = new Dictionary<string, Process>();

        /// <summary>
        /// 本地Appium程序是否可用
        /// </summary>
        public static bool LocalAppiumEnable { get; private set; }
        #endregion

        #region Appium程序检测及处理
        /// <summary>
        /// 初始化Appium进程管理
        /// </summary>
        public void InitializeAppiumProcess()
        {
            CheckForAppiumRootFolder();      //检查Appium程序目录
        }

        /// <summary>
        /// 检查Appium程序目录
        /// </summary>
        public void CheckForAppiumRootFolder()
        {
            string appiumFolder = AppConfigManager.Instance.AppiumRootFolder;
            string nodeModulesFolder = AppConfigManager.Instance.NodeModulesFolder;
            //检查appium程序目录以及node modules目录是否存在
            if (Directory.Exists(appiumFolder) && Directory.Exists(nodeModulesFolder))
            {
                LocalAppiumEnable = true;
                return;
            }
            //不存在的情况下安装appium程序到指定目录
            string installPackagePath = AppConfigManager.Instance.AppiumInstallFilePath;                      //获取Appium安装包文件路径
            //检测安装包文件与解压程序是否存在
            if (!File.Exists(AppConfigManager.Instance.CompressToolPath) || !File.Exists(installPackagePath))
            {
                AppLog.CreateAppLog().OutputLogData(string.Format("Appium安装包文件[{0}]或解压程序文件[{1}]不存在，无法重新安装Appium程序！",
                    AppConfigManager.Instance.CompressToolPath, installPackagePath));
                return;
            }
            //解压安装包到Appium程序目录上的一个目录
            string compressFolder = Directory.GetParent(appiumFolder).FullName;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = AppConfigManager.Instance.CompressToolPath;
            startInfo.Arguments = string.Format("-c \"{0}\" \"{1}\"", installPackagePath, compressFolder);
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = true;
            Process compressProcess = new Process();
            compressProcess.StartInfo = startInfo;
            compressProcess.Start();
            compressProcess.WaitForExit();
            //再次检查appium程序目录以及node modules目录是否存在
            if (Directory.Exists(appiumFolder) && Directory.Exists(nodeModulesFolder))
            {
                LocalAppiumEnable = true;
                return;
            }
        }
        #endregion

        /// <summary>
        /// 创建设备Appium进程
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        public void CreateDeviceAppiumProcess(string deviceSerialno)
        {
            //判断设备序列号在Appium进程池中是否已存在
            bool isExist = _DeviceAppiumProcessPool.Exists(s => s.Serialno == deviceSerialno);
            if (isExist) return;
            AppiumProcessData session = GenerateAppiumProcessData(deviceSerialno);          //生成本次Appium进程数据
            StartDeviceAppiumProcess(session);            //创建并启动设备Appium进程
            _DeviceAppiumProcessPool.Add(session);        //添加进程数据到设备Appium进程数据池
        }

        /// <summary>
        /// 获取设备Appium进程数据
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        /// <returns></returns>
        public AppiumProcessData GetDeviceAppiumProcess(string deviceSerialno)
        {
            AppiumProcessData session = _DeviceAppiumProcessPool.Find(ds => ds == deviceSerialno);
            return session;
        }

        /// <summary>
        /// 判断设备是否存在Appium进程
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        /// <returns></returns>
        public bool ExistAppiumProcess(string deviceSerialno)
        {
            bool result = _DeviceAppiumProcessPool.Exists(ds => ds == deviceSerialno);
            return result;
        }

        /// <summary>
        /// 关闭设备Appium进程
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        public void CloseDeviceAppiumProcess(string deviceSerialno)
        {
            //删除Appium进程数据
            _DeviceAppiumProcessPool.RemoveAll(ds => ds == deviceSerialno);
            //关闭及移除设备Appium进程
            CloseProcess(_AppiumProcessPool[deviceSerialno], deviceSerialno);
            _AppiumProcessPool.Remove(deviceSerialno);
            AppLog.CreateAppLog().OutputLogData(string.Format("设备({0})Appium进程已关闭！", deviceSerialno));
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
                AppLog.CreateAppLog().OutputLogData(string.Format("关闭设备[{0}]Appium进程失败，{1}", deviceSerialno, ex.Message), ex);
            }
        }

        /// <summary>
        /// 释放设备Appium进程
        /// </summary>
        public void DisposeDeviceAppiumProcess()
        {
            AppLog.CreateAppLog().OutputLogData("正在释放Appium进程...");
            List<string> currentDeviceList = (from s in _DeviceAppiumProcessPool
                                              select s.Serialno).ToList();
            foreach (var deviceSerialno in currentDeviceList)
            {
                CloseDeviceAppiumProcess(deviceSerialno);
            }
            _DeviceAppiumProcessPool.Clear();
            _AppiumProcessPool.Clear();
            AppLog.CreateAppLog().OutputLogData("释放Appium进程完成！");
        }

        /// <summary>
        /// 启动设备Appium进程
        /// </summary>
        /// <param name="processData">进程数据</param>
        private void StartDeviceAppiumProcess(AppiumProcessData processData)
        {
            //Appium服务启动参数
            List<IAppiumServerArgument> args = new List<IAppiumServerArgument>();
            args.Add(new ServerRunnerArgument());
            args.Add(new ServerAddressArgument(processData.ServerAddress));
            args.Add(new ServerPortArgument(processData.AppiumPort));
            args.Add(new AndroidBootstrapPortArgument(processData.BootstrapPort));
            args.Add(new SelendroidPortArgument(processData.SelendroidPort));
            args.Add(new ChromeDriverPortArgument(processData.ChromedriverPort));
            args.Add(new OverrideExistingSessionArgument());
            args.Add(new CommandTimeoutArgument(120));          //默认所有会话的接收命令超时时间为120秒
            args.Add(new LogToFileArgument(AppiumLog.GetAppiumLogFilePath(processData.Serialno)));
            string argumentsCmd = String.Join<IAppiumServerArgument>(" ", args);
            //创建服务进程启动信息
            var appiumServerProcessStartInfo = new ProcessStartInfo();
            appiumServerProcessStartInfo.WorkingDirectory = AppConfigManager.Instance.AppiumRootFolder;
            appiumServerProcessStartInfo.FileName = AppConfigManager.Instance.NodePath;
            appiumServerProcessStartInfo.Arguments = argumentsCmd + " --log-no-color";
            appiumServerProcessStartInfo.CreateNoWindow = true;
            appiumServerProcessStartInfo.UseShellExecute = false;
            //创建服务进行并启动
            Process appiumServerProcess = new Process();
            appiumServerProcess.StartInfo = appiumServerProcessStartInfo;
            appiumServerProcess.Start();
            DebugLog.CreateDebugLog().OutputLogData(string.Format("设备({0})Appium进程启动成功！Appium端口号：{1}；Bootstrap端口号：{2}；SelendroidPort：{3}；ChromedriverPort：{4}；", processData.Serialno, processData.AppiumPort, processData.BootstrapPort, processData.SelendroidPort, processData.ChromedriverPort));
            AppLog.CreateAppLog().OutputLogData(string.Format("设备[{0}]Appium进程启动成功，启动参数：{1}", processData.Serialno, argumentsCmd));
            _AppiumProcessPool.Add(processData.Serialno, appiumServerProcess);
        }

        /// <summary>
        /// 生成设备Appium进程数据
        /// </summary>
        /// <param name="deviceSerialno">设备序列号</param>
        /// <returns></returns>
        private AppiumProcessData GenerateAppiumProcessData(string deviceSerialno)
        {
            string serverAddress = AppConfigManager.Instance.AppiumServerAddress;
            uint beginPort = GetBeginPort();       //获取服务起始端口号
            uint appiumPort = NetworkHelper.GetUsablePort(beginPort);      //获取未使用的端口号
            uint bootstrapPort = NetworkHelper.GetUsablePort(appiumPort + 1);
            uint selendroidPort = NetworkHelper.GetUsablePort(bootstrapPort + 1);
            uint chromedriverPort = NetworkHelper.GetUsablePort(selendroidPort + 1);
            AppiumProcessData session = new AppiumProcessData(serverAddress, appiumPort, deviceSerialno);
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
            if (_DeviceAppiumProcessPool.Count == 0)
            {
                port = 4723;
                return port;
            }
            uint poolMaxPort = (from dp in _DeviceAppiumProcessPool
                                select dp.MaxPort()).Max();
            port = poolMaxPort + 1;
            return port;
        }
    }
}
