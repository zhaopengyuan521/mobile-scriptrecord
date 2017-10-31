using AppiumUtility.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Logger
{
    /// <summary>
    /// Appium会话数据日志
    /// </summary>
    public class AppiumLog
    {
        /// <summary>
        /// 设备序列号
        /// </summary>
        private string _SerialNo;

        /// <summary>
        /// 日志目录
        /// </summary>
        private string _LogFilePath = string.Empty;

        /// <summary>
        /// Appium会话数据日志
        /// </summary>
        /// <param name="serialNo">设备序列号</param>
        public AppiumLog(string serialNo)
        {
            if (string.IsNullOrEmpty(serialNo)) throw new Exception("设备序列号无效！");
            _SerialNo = serialNo;
            Init();
        }

        /// <summary>
        /// 获取Appium日志文件路径
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public static string GetAppiumLogFilePath(string serialNo)
        {
            string logDir = Path.Combine(AppConfigManager.Instance.DeviceLogFolder, serialNo);
            //判断目录是否存在
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            string logFilePath = Path.Combine(logDir, string.Format("appium_session_{0}.log",DateTime.Now.ToString("yyyy-MM-dd")));
            return logFilePath;
        }

        /// <summary>
        /// Appium会话数据日志
        /// </summary>
        /// <param name="serialNo">设备序列号</param>
        /// <returns></returns>
        public static AppiumLog CreateAppiumLog(string serialNo)
        {
            return new AppiumLog(serialNo);
        }

        /// <summary>
        /// 初始化日志
        /// </summary>
        private void Init()
        {
            //获取日志保存路径
            string logFileName = string.Format("appium_serssion_{0}_part1.log", DateTime.Now.ToString("yyyy-MM-dd"));
            string logFileDirectory = Path.Combine(AppConfigManager.Instance.AppRootFolder, "runLog\\devices\\" + _SerialNo);
            _LogFilePath = Path.Combine(logFileDirectory, logFileName);
            //判断日志保存目录是否存在
            if (!Directory.Exists(logFileDirectory))
                Directory.CreateDirectory(logFileDirectory);
        }

        /// <summary>
        /// 查检日志文件大小
        /// </summary>
        private void CheckLogFileSize()
        {
            int maxFileSize = 50;       //单个日志文件不超过50MB
            //获取当前文件大小
            long fileSize = this.GetFileSize(this._LogFilePath);
            if (fileSize < maxFileSize) return;
            //获取当前文件索引
            string partStr = "_part";
            string currentFileName = Path.GetFileNameWithoutExtension(this._LogFilePath);
            int partIndex = currentFileName.IndexOf(partStr) + partStr.Length;
            string partNumberStr = currentFileName.Substring(partIndex);
            int partNumber = int.Parse(partNumberStr);
            partNumber += 1;
            string newFileName = currentFileName.Substring(0, partIndex) + partNumber;
            //判断新的文件大小是否超过50MB
            string newFilePath = this._LogFilePath.Replace(currentFileName, newFileName);
            while (GetFileSize(newFilePath) >= maxFileSize)
            {
                currentFileName = newFileName;
                partNumber += 1;
                newFileName = currentFileName.Substring(0, partIndex) + partNumber;
                newFilePath = newFilePath.Replace(currentFileName, newFileName);
            }
            this._LogFilePath = newFilePath;
        }

        /// <summary>
        /// 获取文件大小(MB)
        /// </summary>
        private long GetFileSize(string filePath)
        {
            long size = 0;
            if (File.Exists(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                size = fi.Length;
                size = ((size / 1024) / 1024);  //转换单位为MB
            }
            return size;
        }

        /// <summary>
        /// 输出日志数据
        /// </summary>
        /// <param name="log">日志信息</param>
        public void OutputLogData(string log)
        {
            CheckLogFileSize();
            FileStream fs = null;
            try
            {
                fs = new FileStream(_LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                string logContent = string.Format("{0}>\t{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log);
                byte[] datas = Encoding.UTF8.GetBytes(logContent);
                fs.Write(datas, 0, datas.Length);
                fs.Flush();
            }
            catch (Exception ex)
            {
                DebugLog.CreateDebugLog().OutputLogData(string.Format("设备{0}会话数据日志信息输出失败，{1}", _SerialNo, ex.Message));
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
            }
        }

        /// <summary>
        /// 输出日志数据
        /// </summary>
        /// <param name="log">日志信息</param>
        /// <param name="ex">异常信息</param>
        public void OutputLogData(string log, Exception ex)
        {
            CheckLogFileSize();
            FileStream fs = null;
            try
            {
                fs = new FileStream(_LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                string logContent = string.Format("{0}>\t{1}\r\n异常信息：{2}\r\n堆栈信息：{3}\r\n",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log, ex.Message, ex.StackTrace);
                byte[] datas = Encoding.UTF8.GetBytes(logContent);
                fs.Write(datas, 0, datas.Length);
                fs.Flush();
            }
            catch (Exception e)
            {
                DebugLog.CreateDebugLog().OutputLogData(string.Format("设备{0}会话数据日志信息输出失败，{1}", _SerialNo, e.Message));
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
            }
        }
    }
}
