using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Config
{
    /// <summary>
    /// 应用配置管理
    /// </summary>
    public class AppConfigManager
    {
        #region 单例
        private static AppConfigManager _configuration;
        /// <summary>
        /// 单例Appium配置信息对象
        /// </summary>
        public static AppConfigManager Instance
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new AppConfigManager();
                    _configuration.Load();
                }
                return _configuration;
            }
        }
        #endregion
        #region 私用成员
        private Configuration AppConfig = null;

        /// <summary>android sdk路径</summary>
        private string _AndroidSDKPath = Environment.GetEnvironmentVariable("ANDROID_HOME");

        /// <summary>应用程序 文件夹根目录路径</summary>
        private Lazy<string> __AppRootFolder = new Lazy<string>(() => Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory));

        /// <summary>自动化平台URL</summary>
        private string _ATFSURL = string.Empty;

        /// <summary>node.exe文件路径</summary>
        private string _NodePath = string.Empty;

        /// <summary>Appium程序根目录</summary>
        private string _AppiumRootFolder = string.Empty;

        /// <summary>Nodel Modules目录</summary>
        private string _NodeModuleFolder = string.Empty;

        /// <summary>Appium安装文件名</summary>
        private string _AppiumInstallFileName = string.Empty;

        /// <summary>Appium启动参数</summary>
        private string _AppiumRunnerArgument = string.Empty;

        /// <summary>设备状态检查间隔时间（单位：秒）</summary>
        private int _CheckStateInterval = 0;
        #endregion
        #region Android配置信息
        /// <summary>
        /// 当前 Android SDK 路径
        /// </summary>
        public string AndroidSDKPath
        {
            get { return _AndroidSDKPath; }
        }
        #endregion
        #region 通用配置信息
        /// <summary>
        /// 应用程序根目录
        /// </summary>
        public string AppRootFolder
        {
            get { return __AppRootFolder.Value; }
        }

        /// <summary>
        /// 应用程序数据根目录
        /// </summary>
        public string AppDataRootFolder
        {
            get
            {
                return Path.Combine(AppRootFolder, "AppData");
            }
        }

        /// <summary>
        /// 日志根目录
        /// </summary>
        public string LogRootFolder
        {
            get
            {
                return Path.Combine(AppRootFolder, "Log");
            }
        }

        /// <summary>
        /// 应用日志目录
        /// </summary>
        public string AppLogFolder
        {
            get
            {
                return Path.Combine(LogRootFolder, "AppLog");
            }
        }

        /// <summary>
        /// 设备日志目录
        /// </summary>
        public string DeviceLogFolder
        {
            get
            {
                return Path.Combine(LogRootFolder, "DeviceLog");
            }
        }

        /// <summary>
        /// 录制脚本目录
        /// </summary>
        public string RecrodScriptFolder
        {
            get
            {
                return Path.Combine(AppDataRootFolder, "record_script");
            }
        }

        /// <summary>
        /// 设备屏幕图像存储目录
        /// </summary>
        public string DeviceMonitorFolder
        {
            get
            {
                return Path.Combine(AppDataRootFolder, "device_monitor");
            }
        }

        /// <summary>
        /// node.exe文件路径
        /// </summary>
        public string NodePath
        {
            get
            {
                if (_NodePath == string.Empty)
                    _NodePath = Path.Combine(AppiumRootFolder, "node.exe");
                return _NodePath;
            }
        }

        /// <summary>
        /// appium程序根目录
        /// </summary>
        public string AppiumRootFolder
        {
            get
            {
                if (_AppiumRootFolder == string.Empty)
                    _AppiumRootFolder = Path.Combine(AppRootFolder, "appium-master");
                return _AppiumRootFolder;
            }
        }

        /// <summary>
        /// Node Module目录
        /// </summary>
        public string NodeModulesFolder
        {
            get
            {
                if (_NodeModuleFolder == string.Empty)
                    _NodeModuleFolder = Path.Combine(AppiumRootFolder, GetUserSettingValue("NodeModule"));
                return _NodeModuleFolder;
            }
        }

        /// <summary>
        /// Appium服务启动参数
        /// </summary>
        public string AppiumRunnerArgument
        {
            get
            {
                if (_AppiumRunnerArgument == string.Empty)
                    _AppiumRunnerArgument = GetUserSettingValue("AppiumRunner");
                return _AppiumRunnerArgument;
            }
        }

        /// <summary>
        /// Appium服务地址
        /// </summary>
        public string AppiumServerAddress
        {
            get
            {
                return "127.0.0.1";
            }
        }

        /// <summary>
        /// Appium安装文件路径
        /// </summary>
        public string AppiumInstallFilePath
        {
            get
            {
                if (_AppiumInstallFileName == string.Empty)
                    _AppiumInstallFileName = GetUserSettingValue("AppiumInstallFileName");
                return Path.Combine(AppRootFolder, _AppiumInstallFileName);
            }
        }

        /// <summary>
        /// 解压程序路径
        /// </summary>
        public string CompressToolPath
        {
            get
            {
                return Path.Combine(AppRootFolder, @"CompressTool\CompressTool.exe");
            }
        }

        /// <summary>
        /// APK目录
        /// </summary>
        public string ApkFolder
        {
            get
            {
                return Path.Combine(AppDataRootFolder, "app_apk");
            }
        }

        /// <summary>
        /// ATFS平台地址
        /// </summary>
        public string ATFSURL
        {
            get
            {
                if (string.IsNullOrEmpty(_ATFSURL))
                    _ATFSURL = GetUserSettingValue("ATFSURL");
                return _ATFSURL;
            }
        }

        /// <summary>
        /// 检查设备状态间隔（单位：秒）
        /// </summary>
        public int CheckStateInterval
        {
            get
            {
                if (_CheckStateInterval == 0)
                {
                    _CheckStateInterval = GetUserSettingValue<int>("CheckStateInterval");
                    _CheckStateInterval = (_CheckStateInterval == 0) ? 10 : _CheckStateInterval;        //设置默认间隔10秒
                }
                return _CheckStateInterval;
            }
        }
        #endregion

        /// <summary>
        /// 加载配置文件配置信息
        /// </summary>
        private void Load()
        {
            this._NodePath = GetUserSettingValue("NodePath");
        }

        #region 配置文件相关操作
        /// <summary>
        /// 获取配置文件用户设置值
        /// </summary>
        /// <param name="name">设置名称</param>
        /// <returns>设置值</returns>
        private string GetUserSettingValue(string name)
        {
            string settingValue = "";
            if (AppConfig == null) AppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var userSettings = AppConfig.GetSectionGroup("userSettings");
            var appSection = userSettings.Sections.Get("AppSettings") as ClientSettingsSection;
            settingValue = appSection.Settings.Get(name).Value.ValueXml.InnerText;
            return settingValue;
        }

        /// <summary>
        /// 获取配置文件用户设置值
        /// </summary>
        /// <typeparam name="T">设置值类型</typeparam>
        /// <param name="name">设置名称</param>
        /// <returns>设置值</returns>
        public T GetUserSettingValue<T>(string name)
        {
            T settingValue = default(T);
            if (AppConfig == null) AppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var userSettings = AppConfig.GetSectionGroup("userSettings");
            var appiumSection = userSettings.Sections.Get("Appium.Properties.Settings") as ClientSettingsSection;
            settingValue = (T)((object)appiumSection.Settings.Get(name).Value.ValueXml.InnerText);
            return settingValue;
        }
        #endregion

        /// <summary>
        /// 释放相关资源
        /// </summary>
        public void Dispose()
        {
            if (AppConfig != null)
            {
                AppConfig = null;
            }
        }
    }
}
