using AppiumUtility.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Logger
{
    /// <summary>
    /// 设备日志
    /// </summary>
    public class DeviceLog
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
        /// 初始化设备日志
        /// </summary>
        /// <param name="serialNo">设备序列号</param>
        public DeviceLog(string serialNo)
        {
            if (string.IsNullOrEmpty(serialNo)) throw new Exception("设备序列号无效！");
            _SerialNo = serialNo;
            Init();
        }

        /// <summary>
        /// 创建设备日志
        /// </summary>
        /// <param name="serialNo">设备序列号</param>
        /// <returns></returns>
        public static DeviceLog CreateDeviceLog(string serialNo)
        {
            return new DeviceLog(serialNo);
        }

        /// <summary>
        /// 设备日志输出信息通知
        /// </summary>
        public static Action<string, string> LogMessageNotice = null;

        /// <summary>
        /// 初始化日志
        /// </summary>
        private void Init()
        {
            //获取日志保存路径
            string logFileName = string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
            string logFileDirectory = Path.Combine(AppConfigManager.Instance.AppRootFolder, "runLog\\devices\\" + _SerialNo);
            _LogFilePath = Path.Combine(logFileDirectory, logFileName);
            //判断日志保存目录是否存在
            if (!Directory.Exists(logFileDirectory))
                Directory.CreateDirectory(logFileDirectory);
        }

        /// <summary>
        /// 输出日志数据
        /// </summary>
        /// <param name="log">日志信息</param>
        public void OutputLogData(string log)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(_LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                string logContent = string.Format("{0}>\t{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log);
                byte[] datas = Encoding.UTF8.GetBytes(logContent);
                fs.Write(datas, 0, datas.Length);
                fs.Flush();
                if (LogMessageNotice != null) LogMessageNotice(this._SerialNo, logContent);
            }
            catch (Exception ex)
            {
                DebugLog.CreateDebugLog().OutputLogData(string.Format("设备{0}日志信息输出失败，{1}", _SerialNo, ex.Message));
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
        /// 读取日志数据
        /// </summary>
        /// <returns></returns>
        public string ReadLogData()
        {
            string logData = "";
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(_LogFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
                sr = new StreamReader(fs, Encoding.UTF8);
                logData = sr.ReadToEnd();
                sr = null;
            }
            catch (Exception ex)
            {
                DebugLog.CreateDebugLog().OutputLogData(string.Format("设备{0}日志信息输出失败，{1}", _SerialNo, ex.Message));
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
            return logData;
        }

        /// <summary>
        /// 输出日志数据
        /// </summary>
        /// <param name="log">日志信息</param>
        /// <param name="ex">异常信息</param>
        public void OutputLogData(string log, Exception ex)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(_LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                string logContent = string.Format("{0}>\t{1}\r\n异常信息：{2}\r\n堆栈信息：{3}\r\n",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log, ex.Message, ex.StackTrace);
                byte[] datas = Encoding.UTF8.GetBytes(logContent);
                fs.Write(datas, 0, datas.Length);
                fs.Flush();
                if (LogMessageNotice != null) LogMessageNotice(this._SerialNo, logContent);
            }
            catch (Exception e)
            {
                DebugLog.CreateDebugLog().OutputLogData(string.Format("设备{0}日志信息输出失败，{1}", _SerialNo, e.Message));
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
        /// 脚本进程数据输出接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (null != e && null != e.Data)
            {
                this.OutputLogData(e.Data);
            }
        }
    }
}
