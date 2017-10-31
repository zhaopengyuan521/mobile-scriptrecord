using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Text.RegularExpressions;
using AppiumUtility.Exceptions;
using AppiumUtility.Logger;
using System.Text;
using AppiumUtility.Models;
using AppiumUtility.Config;

namespace AppiumUtility.Utility
{
    /// <summary>
    /// Android SDK命令
    /// </summary>
    public class AndroidSDKCommands
    {
        /// <summary>VBoxManage path</summary>
        private const string _VBoxManagePath = "Oracle\\VirtualBox\\VBoxManage.exe";

        #region Public Methods
        /// <summary>
        /// 获取当前Android设备列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDeviceList()
        {
            var deviceList = new List<string>();
            try
            {
                ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
                deviceDetectionProcessInfo.FileName = PathToAndroidBinary("adb.exe");
                if (File.Exists(deviceDetectionProcessInfo.FileName))
                {
                    deviceDetectionProcessInfo.Arguments = " devices";
                    deviceDetectionProcessInfo.UseShellExecute = false;
                    deviceDetectionProcessInfo.CreateNoWindow = true;
                    deviceDetectionProcessInfo.RedirectStandardOutput = true;
                    var avdDetectionProcess = Process.Start(deviceDetectionProcessInfo);
                    avdDetectionProcess.WaitForExit();

                    // 读取输出
                    var output = string.Empty;
                    using (var myOutput = avdDetectionProcess.StandardOutput)
                    {
                        output = myOutput.ReadToEnd();
                    }
                    string[] deviceParse = null;
                    foreach (var line in output.Split(new char[] { '\r', '\n' }))
                    {
                        deviceParse = line.Split(new char[] { '\t' });
                        if (deviceParse.Length != 2) continue;
                        if (string.IsNullOrEmpty(deviceParse[0])) continue;
                        deviceList.Add(deviceParse[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.CreateAppLog().OutputLogData("通过adb获取当前Android设备列表失败，" + ex.Message, ex);
            }
            return deviceList;
        }

        /// <summary>
        /// 安装应用
        /// </summary>
        /// <param name="apkPath">应用文件路径</param>
        /// <param name="serialNo">设备编号</param>
        /// <returns>安装耗时时间(秒)</returns>
        public static double InstallAPK(string apkPath, string serialNo)
        {
            double timeConsuming = 0;       //安装应用耗时
            ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
            deviceDetectionProcessInfo.FileName = PathToAndroidBinary("adb.exe");
            if (!File.Exists(deviceDetectionProcessInfo.FileName)) throw new FileNotFoundException("获取adb文件失败！");
            //设置启动参数
            deviceDetectionProcessInfo.Arguments = string.Format("-s {0} install -r {1}", serialNo, apkPath);
            deviceDetectionProcessInfo.UseShellExecute = false;
            deviceDetectionProcessInfo.CreateNoWindow = true;
            deviceDetectionProcessInfo.RedirectStandardOutput = true;
            deviceDetectionProcessInfo.RedirectStandardError = true;
            Process adbProcess = new Process();
            adbProcess.StartInfo = deviceDetectionProcessInfo;
            adbProcess.Start();
            adbProcess.WaitForExit();
            // 读取输出
            var output = string.Empty;
            using (var myOutput = adbProcess.StandardError)
            {
                output = myOutput.ReadToEnd();
            }
            using (var myOutput = adbProcess.StandardOutput)
            {
                output += myOutput.ReadToEnd();
            }
            string timeConsumingPatten = @"bytes in\s*(?<ts>.*)s\)";            //获取应用安装耗时正则表达式
            //获取安装结果
            string[] resultParse = null;
            Regex regex = new Regex(timeConsumingPatten, RegexOptions.IgnoreCase);
            Match matchResult = null;
            foreach (var line in output.Split(new char[] { '\r', '\n' }))
            {
                //解析安装耗时
                matchResult = regex.Match(line);
                if (matchResult.Success) double.TryParse(matchResult.Groups["ts"].Value, out timeConsuming);
                resultParse = line.Split(new char[] { '\t' });
                if (resultParse.Length < 1) continue;
                //判断安装是否失败
                if (resultParse[0].Trim().StartsWith("Failure", StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception(resultParse.Length > 1 ? resultParse[1] : resultParse[0]);
            }
            return timeConsuming;
        }

        /// <summary>
        /// 卸载应用
        /// </summary>
        /// <param name="apkPath">应用文件路径</param>
        /// <param name="serialNo">设备编号</param>
        public static void UnInstallAPK(string apkPath, string serialNo)
        {
            List<string> activtyList;
            List<string> packageList;
            //获取安装文件的包名
            GetActivitiesAndPackages(apkPath, out activtyList, out packageList);
            string apkPackageName = packageList.FirstOrDefault();
            if (string.IsNullOrEmpty(apkPackageName))
                throw new Exception(string.Format("卸载应用失败，未获取到安装程序[{0}]包名。", Path.GetFileName(apkPath)));
            //启动adb程序卸载应用
            ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
            deviceDetectionProcessInfo.FileName = PathToAndroidBinary("adb.exe");
            if (!File.Exists(deviceDetectionProcessInfo.FileName)) throw new FileNotFoundException("获取adb文件失败！");
            //设置启动参数
            deviceDetectionProcessInfo.Arguments = string.Format(" -s {0} uninstall {1}", serialNo, apkPackageName);
            deviceDetectionProcessInfo.UseShellExecute = false;
            deviceDetectionProcessInfo.CreateNoWindow = true;
            deviceDetectionProcessInfo.RedirectStandardOutput = true;
            Process adbProcess = new Process();
            adbProcess.StartInfo = deviceDetectionProcessInfo;
            adbProcess.Start();
            adbProcess.WaitForExit();
            // 读取输出
            var output = string.Empty;
            using (var myOutput = adbProcess.StandardOutput)
            {
                output = myOutput.ReadToEnd();
            }
            //获取安装结果
            string[] resultParse = null;
            foreach (var line in output.Split(new char[] { '\r', '\n' }))
            {
                resultParse = line.Split(new char[] { '\t' });
                if (resultParse.Length < 1) continue;
                //判断安装是否失败
                if (resultParse[0].Trim().StartsWith("Failure", StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception(resultParse.Length > 1 ? resultParse[1] : resultParse[0]);
            }
        }

        /// <summary>
        /// 启动Android程序
        /// </summary>
        /// <param name="serialNo">设备编号</param>
        /// <param name="appPackageName">启动的应用包名</param>
        /// <param name="appMainActivityName">启动的应用Active名</param>
        /// <returns>返回结果：-1(未安装)，0(启动成功)，1(已启动)</returns>
        public static int RunAndroidApp(string serialNo, string appPackageName, string appMainActivityName)
        {
            int result = 0;
            ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
            deviceDetectionProcessInfo.FileName = AndroidSDKCommands.PathToAndroidBinary("adb.exe");
            if (!File.Exists(deviceDetectionProcessInfo.FileName)) throw new FileNotFoundException("获取adb文件失败！");
            //设置启动参数
            deviceDetectionProcessInfo.Arguments = string.Format(" -s {0} shell am start -n {1}/{2}",
                serialNo, appPackageName, appMainActivityName);
            deviceDetectionProcessInfo.UseShellExecute = false;
            deviceDetectionProcessInfo.CreateNoWindow = true;
            deviceDetectionProcessInfo.RedirectStandardOutput = true;
            Process adbProcess = new Process();
            adbProcess.StartInfo = deviceDetectionProcessInfo;
            adbProcess.Start();
            adbProcess.WaitForExit();
            // 读取输出
            var output = string.Empty;
            using (var myOutput = adbProcess.StandardOutput)
            {
                output = myOutput.ReadToEnd();
            }
            //获取启动结果
            foreach (var line in output.Split(new char[] { '\r', '\n' }))
            {
                //判断是否有警告信息
                if (line.StartsWith("warning:", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (line.Contains("its current task has been brought to the front"))        //判断应用是否已启动
                    {
                        result = 1;
                        break;
                    }
                }
                else if (line.StartsWith("error:", StringComparison.CurrentCultureIgnoreCase))      //判断是否有错误信息
                {
                    if (line.Contains("does not exist"))
                    {
                        result = -1;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 结束Android程序
        /// </summary>
        /// <param name="serialNo">设备编号</param>
        /// <param name="appPackageName">结束的应用包名</param>
        public static void StopAndroidApp(string serialNo, string appPackageName)
        {
            ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
            deviceDetectionProcessInfo.FileName = AndroidSDKCommands.PathToAndroidBinary("adb.exe");
            if (!File.Exists(deviceDetectionProcessInfo.FileName)) throw new FileNotFoundException("获取adb文件失败！");
            //设置启动参数
            deviceDetectionProcessInfo.Arguments = string.Format(" -s {0} shell am force-stop {1}",
                serialNo, appPackageName);
            deviceDetectionProcessInfo.UseShellExecute = false;
            deviceDetectionProcessInfo.CreateNoWindow = true;
            deviceDetectionProcessInfo.RedirectStandardOutput = true;
            Process adbProcess = new Process();
            adbProcess.StartInfo = deviceDetectionProcessInfo;
            adbProcess.Start();
            adbProcess.WaitForExit();
            // 读取输出
            var output = string.Empty;
            using (var myOutput = adbProcess.StandardOutput)
            {
                output = myOutput.ReadToEnd();
            }
        }

        /// <summary>
        /// 启动本地端口与设备的端口映射
        /// </summary>
        /// <param name="serialNo">设备编号</param>
        /// <param name="localPort">本地端口</param>
        /// <param name="devicePort">设备端口</param>
        public static void StartDevicePortMapping(string serialNo, uint localPort, uint devicePort)
        {
            ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
            deviceDetectionProcessInfo.FileName = AndroidSDKCommands.PathToAndroidBinary("adb.exe");
            if (!File.Exists(deviceDetectionProcessInfo.FileName)) throw new FileNotFoundException("获取adb文件失败！");
            //设置启动参数
            deviceDetectionProcessInfo.Arguments = string.Format(" -s {0} forward tcp:{1} tcp:{2}",
                serialNo, localPort, devicePort);
            deviceDetectionProcessInfo.UseShellExecute = false;
            deviceDetectionProcessInfo.CreateNoWindow = true;
            deviceDetectionProcessInfo.RedirectStandardOutput = true;
            deviceDetectionProcessInfo.RedirectStandardError = true;
            Process adbProcess = new Process();
            adbProcess.StartInfo = deviceDetectionProcessInfo;
            adbProcess.Start();
            adbProcess.WaitForExit();
            // 读取输出
            var output = "";
            var errorOutput = "";
            using (var myOutput = adbProcess.StandardOutput)
            {
                output = myOutput.ReadToEnd();
            }
            using (var myOutput = adbProcess.StandardError)
            {
                errorOutput = adbProcess.StandardError.ReadToEnd();
            }
            if (!string.IsNullOrEmpty(errorOutput) && errorOutput.Length > 0)
            {
                string errorInfo = string.Format("建立本地端口：{0}与设备端口：{1}映射关系失败！\r\n adb程序输出错误信息：{2}", localPort, devicePort, errorOutput);
                DeviceLog.CreateDeviceLog(serialNo).OutputLogData(errorInfo);
                throw new Exception(errorInfo);
            }
            DeviceLog.CreateDeviceLog(serialNo).OutputLogData(string.Format("建立本地端口：{0}与设备端口：{1}映射关系！\r\n adb程序输出信息：{2}", localPort, devicePort, output + errorOutput));
        }

        /// <summary>
        /// 尝试启动本地端口与设备的端口映射
        /// </summary>
        /// <param name="serialNo">设备编号</param>
        /// <param name="localPort">本地端口</param>
        /// <param name="devicePort">设备端口</param>
        public static bool TryStartDevicePortMapping(string serialNo, uint localPort, uint devicePort)
        {
            bool result = false;
            ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
            deviceDetectionProcessInfo.FileName = AndroidSDKCommands.PathToAndroidBinary("adb.exe");
            if (!File.Exists(deviceDetectionProcessInfo.FileName)) throw new FileNotFoundException("获取adb文件失败！");
            //设置启动参数
            deviceDetectionProcessInfo.Arguments = string.Format(" -s {0} forward tcp:{1} tcp:{2}",
                serialNo, localPort, devicePort);
            deviceDetectionProcessInfo.UseShellExecute = false;
            deviceDetectionProcessInfo.CreateNoWindow = true;
            deviceDetectionProcessInfo.RedirectStandardOutput = true;
            deviceDetectionProcessInfo.RedirectStandardError = true;
            Process adbProcess = new Process();
            adbProcess.StartInfo = deviceDetectionProcessInfo;
            adbProcess.Start();
            adbProcess.WaitForExit();
            // 读取输出
            var output = "";
            var errorOutput = "";
            using (var myOutput = adbProcess.StandardOutput)
            {
                output = myOutput.ReadToEnd();
            }
            using (var myOutput = adbProcess.StandardError)
            {
                errorOutput = adbProcess.StandardError.ReadToEnd();
            }
            if (!string.IsNullOrEmpty(errorOutput) && errorOutput.Length > 0)
            {
                result = false;
            }
            else
            {
                result = true;
                DeviceLog.CreateDeviceLog(serialNo).OutputLogData(string.Format("建立本地端口：{0}与设备端口：{1}映射关系！\r\n adb程序输出信息：{2}", localPort, devicePort, output + errorOutput));
            }
            return result;
        }

        /// <summary>
        /// 结束本地端口与设备的端口映射
        /// </summary>
        /// <param name="serialNo">设备编号</param>
        /// <param name="localPort">本地端口</param>
        public static void StopDevicePortMapping(string serialNo, uint localPort)
        {
            ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
            deviceDetectionProcessInfo.FileName = AndroidSDKCommands.PathToAndroidBinary("adb.exe");
            if (!File.Exists(deviceDetectionProcessInfo.FileName)) throw new FileNotFoundException("获取adb文件失败！");
            //设置启动参数
            deviceDetectionProcessInfo.Arguments = string.Format(" -s {0} forward --remove tcp:{1}",
                serialNo, localPort);
            deviceDetectionProcessInfo.UseShellExecute = false;
            deviceDetectionProcessInfo.CreateNoWindow = true;
            deviceDetectionProcessInfo.RedirectStandardOutput = true;
            Process adbProcess = new Process();
            adbProcess.StartInfo = deviceDetectionProcessInfo;
            adbProcess.Start();
            adbProcess.WaitForExit();
            // 读取输出
            var output = string.Empty;
            using (var myOutput = adbProcess.StandardOutput)
            {
                output = myOutput.ReadToEnd();
            }
            DeviceLog.CreateDeviceLog(serialNo).OutputLogData(string.Format("关闭本地端口：{0}与设备端口的映射关系！\r\n adb程序输出信息:{1}", localPort, output));
        }

        /// <summary>
        /// 关闭所有打开的adb程序
        /// </summary>
        public static void KillAdb()
        {
            Process adbProcess = Process.GetProcessesByName("adb").FirstOrDefault();
            if (adbProcess == null) return;
            try
            {
                adbProcess.Kill();
                adbProcess.Dispose();
                KillAdb();
            }
            catch (Exception ex)
            {
                AppLog.CreateAppLog().OutputLogData(string.Format("关闭adb进程[{0}]失败，{1}", adbProcess.Id, ex.Message));
            }
        }

        ///<summary>
        ///获取设备信息
        /// </summary>
        /// <param name="serialNo">设备编号</param>
        public static DeviceInfo GetDeviceInfo(string serialNo)
        {
            DeviceInfo deviceInfo = new DeviceInfo() { Serialno = serialNo, IsRemote = false };
            try
            {
                ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
                deviceDetectionProcessInfo.FileName = PathToAndroidBinary("adb.exe");
                if (File.Exists(deviceDetectionProcessInfo.FileName))
                {
                    #region 读取build.prop信息内容
                    deviceDetectionProcessInfo.Arguments = string.Format("-s {0} shell cat /system/build.prop", serialNo);
                    deviceDetectionProcessInfo.UseShellExecute = false;
                    deviceDetectionProcessInfo.CreateNoWindow = true;
                    deviceDetectionProcessInfo.RedirectStandardOutput = true;
                    var avdDetectionProcess = Process.Start(deviceDetectionProcessInfo);
                    var output = GetProcessOutputData(avdDetectionProcess);
                    StringBuilder regexPatten = new StringBuilder(@"ro.build.version.sdk=(?<level>[\d]*)");
                    //匹配SDK版本
                    Regex regex = new Regex(regexPatten.ToString(), RegexOptions.IgnoreCase);
                    Match matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["level"].Value))
                            deviceInfo.SDKLevel = matchResult.Groups["level"].Value.Replace("\r", "");
                    }
                    //匹配系统版本
                    regexPatten = new StringBuilder(@"ro.build.version.release=(?<version>[\d\.]*)");
                    regex = new Regex(regexPatten.ToString(), RegexOptions.IgnoreCase);
                    matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["version"].Value))
                            deviceInfo.SystemVersion = matchResult.Groups["version"].Value.Replace("\r", "");
                    }
                    //匹配手机型号
                    regexPatten = new StringBuilder(@"ro.product.model=(?<type>.*)");
                    regex = new Regex(regexPatten.ToString(), RegexOptions.IgnoreCase);
                    matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["type"].Value))
                            deviceInfo.Type = matchResult.Groups["type"].Value.Replace("\r", "");
                    }
                    //匹配手机品牌
                    regexPatten = new StringBuilder(@"ro.product.brand=(?<brand>.*)");
                    regex = new Regex(regexPatten.ToString(), RegexOptions.IgnoreCase);
                    matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["brand"].Value))
                            deviceInfo.Brand = matchResult.Groups["brand"].Value.Replace("\r", "");
                    }
                    //匹配手机名称
                    regexPatten = new StringBuilder(@"ro.product.name=(?<name>.*)");
                    regex = new Regex(regexPatten.ToString(), RegexOptions.IgnoreCase);
                    matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["name"].Value))
                            deviceInfo.Name = matchResult.Groups["name"].Value.Replace("\r", "");
                    }
                    //匹配手机主板
                    regexPatten = new StringBuilder(@"ro.product.board=(?<board>.*)");
                    regex = new Regex(regexPatten.ToString(), RegexOptions.IgnoreCase);
                    matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["board"].Value))
                            deviceInfo.Board = matchResult.Groups["board"].Value.Replace("\r", "");
                    }
                    avdDetectionProcess.Close();
                    avdDetectionProcess.Dispose();
                    avdDetectionProcess = null;
                    //额外解析屏幕密度信息
                    regexPatten = new StringBuilder(@"ro.sf.lcd_density=(?<dpi>[\d]*)");
                    regex = new Regex(regexPatten.ToString(), RegexOptions.IgnoreCase);
                    matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["dpi"].Value))
                            deviceInfo.DPI = int.Parse(matchResult.Groups["dpi"].Value);
                    }
                    #endregion
                    #region 读取CPU信息
                    deviceDetectionProcessInfo.Arguments = string.Format("-s {0} shell cat /proc/cpuinfo", serialNo);
                    avdDetectionProcess = Process.Start(deviceDetectionProcessInfo);
                    output = GetProcessOutputData(avdDetectionProcess);
                    regexPatten = new StringBuilder(@"Hardware.*:(?<cpu>.*)");
                    regex = new Regex(regexPatten.ToString(), RegexOptions.IgnoreCase);
                    matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["cpu"].Value))
                        {
                            deviceInfo.CPU = matchResult.Groups["cpu"].Value.Replace("\r", "");
                            //解析CPU信息
                            string regexExpression = @", Inc (?<cpu>.*)";
                            regex = new Regex(regexExpression, RegexOptions.IgnoreCase);
                            Match cpuResult = regex.Match(deviceInfo.CPU);
                            if (cpuResult.Success)
                            {
                                deviceInfo.CPU = cpuResult.Groups["cpu"].Value;
                            }
                        }
                    }
                    avdDetectionProcess.Close();
                    avdDetectionProcess.Dispose();
                    avdDetectionProcess = null;
                    #endregion
                    #region 读取内存信息
                    deviceDetectionProcessInfo.Arguments = string.Format("-s {0} shell cat proc/meminfo", serialNo);
                    avdDetectionProcess = Process.Start(deviceDetectionProcessInfo);
                    output = GetProcessOutputData(avdDetectionProcess);
                    regexPatten = new StringBuilder(@"^MemTotal:\s*(?<mem>\d*)");
                    regex = new Regex(regexPatten.ToString(), RegexOptions.Singleline);
                    matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["mem"].Value))
                        {
                            double memory = double.Parse(matchResult.Groups["mem"].Value);
                            //kB转换为G
                            memory = (memory / 1024) / 1024;
                            memory = Math.Round(memory, 0);
                            deviceInfo.Memory = ((int)memory).ToString() + "G";
                        }
                    }
                    avdDetectionProcess.Close();
                    avdDetectionProcess.Dispose();
                    avdDetectionProcess = null;
                    #endregion
                    #region 读取屏幕分辨率
                    deviceDetectionProcessInfo.Arguments = string.Format("-s {0} shell wm size", serialNo);
                    avdDetectionProcess = Process.Start(deviceDetectionProcessInfo);
                    output = GetProcessOutputData(avdDetectionProcess);
                    regexPatten = new StringBuilder(@"^Physical size:\s*(?<size>\d*[x|*]\d*)");
                    regex = new Regex(regexPatten.ToString(), RegexOptions.Singleline);
                    matchResult = regex.Match(output);
                    if (matchResult.Success)
                    {
                        if (!string.IsNullOrEmpty(matchResult.Groups["size"].Value))
                        {
                            deviceInfo.Resolution = matchResult.Groups["size"].Value;
                        }
                    }
                    avdDetectionProcess.Close();
                    avdDetectionProcess.Dispose();
                    avdDetectionProcess = null;
                    #endregion
                    #region 读取设备网卡地址
                    deviceDetectionProcessInfo.Arguments = string.Format("-s {0} shell cat /sys/class/net/wlan0/address", serialNo);
                    avdDetectionProcess = Process.Start(deviceDetectionProcessInfo);
                    output = GetProcessOutputData(avdDetectionProcess);
                    if (!string.IsNullOrEmpty(output))
                    {
                        deviceInfo.MAC = output.Replace("\r", "").Replace("\n", "");
                    }
                    avdDetectionProcess.Close();
                    avdDetectionProcess.Dispose();
                    avdDetectionProcess = null;
                    #endregion
                }
            }
            catch
            { }
            return deviceInfo;
        }

        /// <summary>
        /// 获取进度数据输出
        /// </summary>
        /// <param name="avdDetectionProcess">进程对象</param>
        /// <returns></returns>
        private static string GetProcessOutputData(Process avdDetectionProcess)
        {
            // 读取输出
            var output = string.Empty;
            while (string.IsNullOrEmpty(output))
            {
                using (var myOutput = avdDetectionProcess.StandardOutput)
                {
                    output = myOutput.ReadToEnd();
                }
                System.Threading.Thread.Sleep(500);
            }
            return output;
        }

        /// <summary>
        /// 获取设备应用进程ID
        /// </summary>
        /// <param name="packageName">应用包名</param>
        /// <param name="serialNo">设备编号</param>
        public static int GetDeviceProcessID(string packageName, string serialNo)
        {
            ProcessStartInfo deviceDetectionProcessInfo = new ProcessStartInfo();
            deviceDetectionProcessInfo.FileName = PathToAndroidBinary("adb.exe");
            if (!File.Exists(deviceDetectionProcessInfo.FileName)) throw new FileNotFoundException("获取adb文件失败！");
            //设置启动参数
            deviceDetectionProcessInfo.Arguments = string.Format(" -s {0} shell ps", serialNo);
            deviceDetectionProcessInfo.UseShellExecute = false;
            deviceDetectionProcessInfo.CreateNoWindow = true;
            deviceDetectionProcessInfo.RedirectStandardOutput = true;
            Process adbProcess = new Process();
            adbProcess.StartInfo = deviceDetectionProcessInfo;
            adbProcess.Start();
            adbProcess.WaitForExit(1000);
            // 读取输出
            var output = string.Empty;
            using (var myOutput = adbProcess.StandardOutput)
            {
                output = myOutput.ReadToEnd();
            }
            //获取进程ID
            int pid = 0;
            //解析进程信息正则表达式
            string pidPatten = @"(?<user>\w*)\s*(?<pid>\d*)\s*(?<ppid>\d*)\s*(?<vsize>\d*)\s*(?<rss>\d*)\s*(?<wchan>\w*\s\w*)\s*(?<pc>\w*)\s*(?<name>.*)";
            Regex rexExpression = new Regex(pidPatten, RegexOptions.IgnoreCase);
            Match matchResult;
            foreach (var line in output.Split(new char[] { '\r', '\n' }))
            {
                matchResult = rexExpression.Match(line);
                if (!matchResult.Success) continue;
                if (string.IsNullOrEmpty(matchResult.Groups["name"].Value)) continue;
                if (!matchResult.Groups["name"].Value.Equals(packageName, StringComparison.CurrentCultureIgnoreCase)) continue;
                if (string.IsNullOrEmpty(matchResult.Groups["pid"].Value)) continue;
                pid = int.Parse(matchResult.Groups["pid"].Value);
                break;
            }
            return pid;
        }
        /// <summary>
        /// Get the list of AVD's by running the android.bat file in tools
        /// </summary>
        /// <returns>list of avd's found, empty list if none found</returns>
        public static List<string> GetAvdList()
        {
            var avdList = new List<string>();
            try
            {
                // use the android command to list the avds
                ProcessStartInfo avdDetectionProcessInfo = new ProcessStartInfo();
                //avdDetectionProcessInfo.FileName = Path.Combine(androidSdkPath, "tools", "android.bat");
                avdDetectionProcessInfo.FileName = PathToAndroidBinary("android.bat");

                if (File.Exists(avdDetectionProcessInfo.FileName))
                {
                    avdDetectionProcessInfo.Arguments = "list avd -c";
                    avdDetectionProcessInfo.UseShellExecute = false;
                    avdDetectionProcessInfo.CreateNoWindow = true;
                    avdDetectionProcessInfo.RedirectStandardOutput = true;
                    var avdDetectionProcess = Process.Start(avdDetectionProcessInfo);
                    avdDetectionProcess.WaitForExit();

                    // read the output
                    var output = string.Empty;
                    using (var myOutput = avdDetectionProcess.StandardOutput)
                    {
                        output = myOutput.ReadToEnd();
                    }

                    foreach (var line in output.Split(new char[] { '\r', '\n' }))
                    {
                        if (line.Length > 0)
                        {
                            avdList.Add(line);
                        }
                    }
                }

                // get genymotion avds
                var genymotionAvdDetectionProc = new ProcessStartInfo();
                var vboxPath = (String)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion", "ProgramW6432Dir", "");
                if (vboxPath.Length == 0)
                    vboxPath = Environment.GetEnvironmentVariable("ProgramFiles");
                vboxPath = Path.Combine(vboxPath, _VBoxManagePath);
                genymotionAvdDetectionProc.FileName = vboxPath;
                if (File.Exists(genymotionAvdDetectionProc.FileName))
                {
                    genymotionAvdDetectionProc.Arguments = "list vms";
                    genymotionAvdDetectionProc.UseShellExecute = false;
                    genymotionAvdDetectionProc.CreateNoWindow = true;
                    genymotionAvdDetectionProc.RedirectStandardOutput = true;
                    var genymotionAvdDetectionProcess = Process.Start(genymotionAvdDetectionProc);
                    genymotionAvdDetectionProcess.WaitForExit();

                    // read the output
                    var output = string.Empty;
                    using (var myOutput = genymotionAvdDetectionProcess.StandardOutput)
                    {
                        output = myOutput.ReadToEnd().TrimEnd();
                    }

                    foreach (var line in output.Split(new char[] { '\r', '\n' }))
                    {
                        var startQuote = line.IndexOf('"');
                        var endQuote = line.LastIndexOf('"');
                        if (startQuote != -1 && endQuote != -1 && startQuote < endQuote)
                        {
                            avdList.Add(line.Substring(startQuote + 1, endQuote - startQuote - 1));
                        }
                    }
                }
            }
            catch
            {
            }
            return avdList;
        }

        /// <summary>
        /// return a list of packages and activities that are associated with the android application
        /// </summary>
        /// <param name="appPath">application path</param>
        /// <param name="activityList">Activity List - count 0 if not found</param>
        /// <param name="packageList">Package List - count 0 if not found</param>
        /// <exception cref="AppiumUtility.Exceptions.PackageParseException"></exception>
        public static void GetActivitiesAndPackages(string appPath, out List<string> activityList, out List<string> packageList)
        {
            activityList = new List<string>();
            packageList = new List<string>();

            if (string.IsNullOrWhiteSpace(appPath))
            {
                return;
            }
            appPath = appPath.Trim();
            if (!File.Exists(appPath))
            {
                return;
            }

            try
            {
                var aaptPath = new ProcessStartInfo();
                aaptPath.FileName = PathToAndroidBinary("aapt.exe");

                if (File.Exists(aaptPath.FileName))
                {
                    aaptPath.Arguments = string.Format("dump xmltree {0} AndroidManifest.xml", appPath);
                    aaptPath.UseShellExecute = false;
                    aaptPath.CreateNoWindow = true;
                    aaptPath.RedirectStandardOutput = true;
                    var aaptProcess = Process.Start(aaptPath);

                    // read the output
                    string output;
                    var isElementNodeActivity = false;
                    while (null != (output = aaptProcess.StandardOutput.ReadLine()))
                    {
                        output = output.Trim();
                        // determine when an activity element has started or ended
                        if (output.StartsWith("E:"))
                        {
                            isElementNodeActivity = output.StartsWith("E: activity (line=");
                        }
                        // determine when the activity name has appeared
                        else if (isElementNodeActivity && output.StartsWith("A: android:name("))
                        {
                            var arr = output.Split('"');
                            if (3 <= arr.Length)
                            {
                                activityList.Add(arr[1]);
                            }
                        }
                        else if (output.StartsWith("A: package="))
                        {
                            var arr = output.Split('"');
                            if (3 <= arr.Length)
                            {
                                packageList.Add(arr[1]);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PackageParseException(appPath, ex);
            }
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Search Android SDK directory to find the binary needed
        /// </summary>
        /// <param name="binaryName">binary file to find</param>
        /// <returns>path to the binary file (filename included)</returns>
        public static string PathToAndroidBinary(string binaryName)
        {
            var sdkPath = AppConfigManager.Instance.AndroidSDKPath;
            string binaryPath = null;
            try
            {
                var tmpPath = string.Empty;
                // check platform folder
                if (File.Exists(tmpPath = Path.Combine(sdkPath, "platform-tools", binaryName)))
                {
                    binaryPath = tmpPath;
                }
                // check tools folder
                else if (File.Exists(tmpPath = Path.Combine(sdkPath, "tools", binaryName)))
                {
                    binaryPath = tmpPath;
                }
                // check the build-tools subdirectories
                else
                {
                    binaryPath = _RecursiveFind(Path.Combine(sdkPath, "build-tools"), binaryName);
                }
            }
            catch
            {
            }
            finally
            {
                if (string.IsNullOrEmpty(binaryPath))
                {
                    AppLog.CreateAppLog().OutputLogData(string.Format("未找到 {0} 文件所在的路径，请检查[ANDROID_HOME]环境变量！", binaryName));
                }
            }
            return binaryPath;
        }

        /// <summary>
        /// Recursively search directories until file is found
        /// </summary>
        /// <param name="dir">directory to seach</param>
        /// <param name="fileToFind">file to find (i.e. "aapt.exe" or "*.bat" or "*.exe")</param>
        /// <returns>path to file</returns>
        private static string _RecursiveFind(string dir, string fileToFind)
        {
            string pathFound = null;
            try
            {
                foreach (var d in Directory.GetDirectories(dir))
                {
                    var matchingFiles = Directory.GetFiles(d, fileToFind);
                    if (0 < matchingFiles.Length)
                    {
                        pathFound = matchingFiles[0];
                        break;
                    }
                    _RecursiveFind(d, fileToFind);
                }
            }
            catch { }

            return pathFound;
        }
        #endregion Private Methods
    }
}
