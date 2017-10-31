using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace CompressTool
{
    class Program
    {
        /// <summary>
        /// 程序入口方法
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            bool parseResult = ParseArgument(args);
            //判断解析是否成功
            if (!parseResult)
            {
                PrintHelper();
                return;
            }
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }

        /// <summary>
        /// 解析入参
        /// </summary>
        private static bool ParseArgument(string[] args)
        {
            bool result = false;
            if (args == null || args.Length < 2)
            {
                Console.WriteLine("参数不能为空，或使用了无效的参数！");
                return result;
            }
            switch (args[0].ToLower())
            {
                case "-c":
                    //解压文件操作
                    if (!string.IsNullOrEmpty(args[1]) && !string.IsNullOrEmpty(args[2]))
                    {
                        CompressData.OperationType = OperationType.Compress;
                        CompressData.CompressFilePath = PathFormatParse(args[1]);
                        CompressData.CompressToDirectory = PathFormatParse(args[2]);
                        result = true;
                    }
                    break;
                case "-d":
                    //删除文件操作
                    if (!string.IsNullOrEmpty(args[1]))
                    {
                        CompressData.OperationType = OperationType.Delete;
                        string deleteDir = "";
                        #region 路径解析
                        deleteDir = PathFormatParse(args[1]);
                        #endregion
                        CompressData.DeletePath = deleteDir;
                        result = true;
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// 路径格式解析
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        private static string PathFormatParse(string sourcePath)
        {
            string targetPath = sourcePath;
            if (sourcePath.StartsWith(@"..\"))
            {
                targetPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, sourcePath.Replace(@"..\", ""));
            }
            else if (sourcePath.StartsWith(@".\"))
            {
                targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourcePath.Replace(@".\", ""));
            }
            else if (!sourcePath.Contains(":"))
            {
                targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourcePath);
            }
            return targetPath;
        }

        /// <summary>
        /// 输出帮助信息
        /// </summary>
        private static void PrintHelper()
        {
            Console.WriteLine("使用说明：");
            Console.WriteLine("\t-c :解压文件操作");
            Console.WriteLine("\t-d :删除文件或目录操作");
            Console.WriteLine("示例(路径中获取空格时使用\"号包含路径)：");
            Console.WriteLine("解压文件示例：\r\n CompressTool.exe -c D:\\test\appium.zip \"C:\\Program Files\\\"");
            Console.WriteLine("删除文件或目录示例：\r\n CompressTool.exe -d D:\\data\\appium\\");
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
