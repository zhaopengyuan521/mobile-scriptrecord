using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace CustomLogger
{
    /// <summary>
    /// 动态创建Logger工厂类
    /// </summary>
    public static class CustomRollingFileLogger
    {
        private static readonly ConcurrentDictionary<string, ILog> loggerContainer = new ConcurrentDictionary<string, ILog>();

        private static readonly Dictionary<string, ReadParamAppender> appenderContainer = new Dictionary<string, ReadParamAppender>();

        private static object lockObj = new object();

        #region 默认配置
        private const int MAX_SIZE_ROLL_BACKUPS = 20;
        private const string LAYOUT_PATTERN = "%d [%t] %-5p - %m%n";
        private const string DATE_PATTERN = "yyyyMMdd\".txt\"";
        private const string MAXIMUM_FILE_SIZE = "50MB";
        private const string LEVEL = "debug";
        #endregion

        /// <summary>
        /// 静态构造函数（读取配置文件并缓存）
        /// </summary>
        static CustomRollingFileLogger()
        {
            IAppender[] appenders = LogManager.GetRepository().GetAppenders();
            for (int i = 0; i < appenders.Length; i++)
            {
                if (appenders[i] is ReadParamAppender)
                {
                    ReadParamAppender appender = (ReadParamAppender)appenders[i];
                    if (appender.MaxSizeRollBackups == 0)
                        appender.MaxSizeRollBackups = MAX_SIZE_ROLL_BACKUPS;
                    if (appender.Layout != null && appender.Layout is log4net.Layout.PatternLayout)
                        appender.LayoutPattern = ((log4net.Layout.PatternLayout)appender.Layout).ConversionPattern;
                    if (string.IsNullOrEmpty(appender.LayoutPattern))
                        appender.LayoutPattern = LAYOUT_PATTERN;
                    if (string.IsNullOrEmpty(appender.DatePattern))
                        appender.DatePattern = DATE_PATTERN;
                    if (string.IsNullOrEmpty(appender.MaximumFileSize))
                        appender.MaximumFileSize = MAXIMUM_FILE_SIZE;
                    if (string.IsNullOrEmpty(appender.Level))
                        appender.Level = LEVEL;
                    lock (lockObj)
                    {
                        appenderContainer[appenders[i].Name] = appender;
                    }
                }
            }
        }

        public static ILog GetCustomLogger(string loggerName, string category = null, bool additivity = false)
        {
            return loggerContainer.GetOrAdd(loggerName, delegate(string name)
            {
                RollingFileAppender newAppender = null;
                ReadParamAppender appender = null;
                if (appenderContainer.ContainsKey(loggerName))
                {
                    appender = appenderContainer[loggerName];
                    newAppender = GetNewFileAppender(loggerName, string.IsNullOrEmpty(appender.File) ? GetFile(category, loggerName) : appender.File,
                        appender.MaxSizeRollBackups, appender.AppendToFile, true, appender.MaximumFileSize, RollingFileAppender.RollingMode.Composite,
                        appender.DatePattern, appender.LayoutPattern);
                }
                else
                {
                    newAppender = GetNewFileAppender(loggerName, GetFile(category, loggerName), MAX_SIZE_ROLL_BACKUPS, true, true, MAXIMUM_FILE_SIZE,
                        RollingFileAppender.RollingMode.Composite, DATE_PATTERN, LAYOUT_PATTERN);
                }
                Hierarchy repository = (Hierarchy)LogManager.GetRepository();
                Logger logger = repository.LoggerFactory.CreateLogger(repository, loggerName);
                logger.Hierarchy = repository;
                logger.Parent = repository.Root;
                logger.Level = GetLoggerLevel(appender == null ? LEVEL : appender.Level);
                logger.Additivity = additivity;
                logger.AddAppender(newAppender);
                logger.Repository.Configured = true;
                return new LogImpl(logger);
            });
        }

        private static string GetFile(string category, string loggerName)
        {
            if (string.IsNullOrEmpty(category))
            {
                return string.Format(@"runlog\{0}.txt", loggerName);
            }
            else
            {
                return string.Format(@"runlog\{0}\{1}.txt", category, loggerName);
            }
        }

        private static Level GetLoggerLevel(string level)
        {
            if (!string.IsNullOrEmpty(level))
            {
                switch (level.ToLower().Trim())
                {
                    case "debug":
                        return Level.Debug;
                    case "info":
                        return Level.Info;
                    case "warn":
                        return Level.Warn;
                    case "error":
                        return Level.Error;
                    case "fatal":
                        return Level.Fatal;
                }
            }
            return Level.Debug;
        }

        private static RollingFileAppender GetNewFileAppender(string appenderName, string file, int maxSizeRollBackups,
            bool appendToFile = true, bool staticLogFileName = false, string maximumFileSize = "5MB", RollingFileAppender.RollingMode rollingMode = RollingFileAppender.RollingMode.Composite,
            string datePattern = "yyyyMMdd\".txt\"", string layoutPattern = "%d [%t] %-5p  - %m%n")
        {
            RollingFileAppender appender = new RollingFileAppender
            {
                LockingModel=new FileAppender.MinimalLock(),
                Name=appenderName,
                File=file,
                AppendToFile=appendToFile,
                MaxSizeRollBackups=maxSizeRollBackups,
                StaticLogFileName=staticLogFileName,
                RollingStyle=rollingMode,
                DatePattern=datePattern
            };
            PatternLayout layout = new PatternLayout(layoutPattern);
            appender.Layout = layout;
            layout.ActivateOptions();
            appender.ActivateOptions();
            return appender;
        }
    }
}
