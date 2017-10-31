using AppiumUtility.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.AppiumProcess
{
    /// <summary>
    /// Appium进程运行日志数据接收
    /// </summary>
    public class AppiumProcessDataReceived
    {
        /// <summary>
        /// 设备序列号
        /// </summary>
        public string SerialNo { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="serialNo">设备序列号</param>
        public AppiumProcessDataReceived(string serialNo)
        {
            this.SerialNo = serialNo;
        }

        /// <summary>
        /// Node进程数据输出接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (null != e && null != e.Data)
            {
                AppiumLog appiumLog = new AppiumLog(SerialNo);
                appiumLog.OutputLogData(e.Data);
                appiumLog = null;
            }
        }
    }
}
