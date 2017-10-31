using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace DeviceRecord
{
    class Program
    {
        /// <summary>
        /// 程序入口方法
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Process process = RuningInstance();     //获取已运行的程序
            if (process != null)
            {

                HandleRunningInstance(process);         //激活新闻该程序，并前端显示
                Process.GetCurrentProcess().CloseMainWindow();
            }
            else
            {
                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

        /// <summary>
        /// 获取运行实例
        /// </summary>
        /// <returns></returns>
        private static Process RuningInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process result = null;
            Process[] processList = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (var process in processList)
            {
                if (process.Id != currentProcess.Id)
                {
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == currentProcess.MainModule.FileName)
                    {
                        result = process;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 将指定进显示到最前端
        /// </summary>
        /// <param name="instance">进程实例</param>
        private static void HandleRunningInstance(Process instance)
        {
            ShowWindowAsync(instance.MainWindowHandle, SW_SHOWNOMAL);   //显示窗口
            SetForegroundWindow(instance.MainWindowHandle);//顶置
        }

        /// <summary>
        /// 设置由不同线程产生的窗口的显示状态
        /// </summary>
        /// <param name="hWd">窗口句柄</param>
        /// <param name="cmdShow">指定窗口如何显示。</param>
        /// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWd, int cmdShow);

        /// <summary>
        /// 将指定窗口的线程设置到前台，并激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。
        /// </summary>
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static int SW_SHOWNOMAL = 1;
    }
}
