﻿using AppiumUtility.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Logger
{
    /// <summary>
    /// 调试日志（只针对Debug模式编译的程序才输出）
    /// </summary>
    public class DebugLog
    {
        /// <summary>
        /// 日志目录
        /// </summary>
        private string _LogFilePath = string.Empty;

        /// <summary>
        /// 初始化Debug日志
        /// </summary>
        public DebugLog()
        {
            Init();
        }

        /// <summary>
        /// 创建Debug日志对象
        /// </summary>
        /// <returns></returns>
        public static DebugLog CreateDebugLog()
        {
            return new DebugLog();
        }

        /// <summary>
        /// 初始化日志
        /// </summary>
        private void Init()
        {
            //获取日志保存路径
            string logFileName = string.Format("Debug_{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
            string logFileDirectory = Path.Combine(AppConfigManager.Instance.AppRootFolder, "runLog\\app\\");
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
#if DEBUG
            FileStream fs = null;
            try
            {
                fs = new FileStream(_LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                string logContent = string.Format("{0}>\t{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log);
                byte[] datas = Encoding.UTF8.GetBytes(logContent);
                fs.Write(datas, 0, datas.Length);
                fs.Flush();
            }
            catch (Exception) { }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
            }
#endif
        }

        /// <summary>
        /// 输出日志数据
        /// </summary>
        /// <param name="log">日志信息</param>
        /// <param name="ex">异常信息</param>
        public void OutputLogData(string log, Exception ex)
        {
#if DEBUG
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
            catch (Exception) { }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
            }
#endif
        }
    }
}
