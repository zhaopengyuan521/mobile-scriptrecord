using AppiumUtility.Exceptions;
using AppiumUtility.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Utility
{
    /// <summary>
    /// 网络信息
    /// </summary>
    public static class NetworkHelper
    {
        private static string _LocalMac = string.Empty;
        private static string _LocalIP = string.Empty;

        /// <summary>
        /// 本机网卡地址
        /// </summary>
        public static string LocalMac
        {
            get
            {
                if (_LocalMac == string.Empty)
                {
                    _LocalMac = GetLocalMac();
                }
                return _LocalMac;
            }
        }

        /// <summary>
        /// 本机IP地址
        /// </summary>
        public static string LocalIP
        {
            get
            {
                if (_LocalIP == string.Empty)
                {
                    _LocalIP = GetLocalIP();
                }
                return _LocalIP;
            }
        }

        /// <summary>
        /// 获取本机网卡地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalMac()
        {
            string mac = "";
            List<string> macs = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                macs.Add(ni.GetPhysicalAddress().ToString());
            }
            if (macs.Count() > 0) mac = macs.FirstOrDefault();
            return mac;
        }

        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostEntry(hostName);
            string ip = string.Empty;
            var queryResult = (from a in localhost.AddressList
                               where a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                               select a);
            foreach (var item in queryResult)
            {
                ip = item.ToString();
                if (string.IsNullOrEmpty(ip) || !ip.Contains("."))
                {
                    ip = string.Empty;
                    continue;
                }
                break;
            }
            //判断是否获取到本机IP地址
            if (string.IsNullOrEmpty(ip)) throw new Exception("无法获取到本地IP地址！");
            AppLog.CreateAppLog().OutputLogData("设置检测到执行机IP：" + ip);
            return ip;
        }


        /// <summary>
        /// 获取未使用的端口号
        /// </summary>
        /// <param name="startPort">起始端口号</param>
        /// <returns></returns>
        public static uint GetUsablePort(uint startPort)
        {
            int port = (int)startPort;
            int endPort = 65535;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            List<int> ipEndPoints = ipProperties.GetActiveTcpListeners().Select<IPEndPoint, int>(p => p.Port).ToList();
            ipEndPoints.AddRange(ipProperties.GetActiveUdpListeners().Select<IPEndPoint, int>(p => p.Port));
            for (port = (int)startPort; port <= endPort; port++)
            {
                if (!ipEndPoints.Contains(port)) break;
            }
            if (port > 65535) port = -1;
            return (uint)port;
        }

        /// <summary>
        /// 检测端口号是否打开
        /// </summary>
        /// <param name="port">要检测的端口号</param>
        /// <returns></returns>
        public static bool CheckPortOpen(uint port)
        {
            bool result = false;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            result = (from ip in ipProperties.GetActiveTcpListeners()
                      where ip.Port == port
                      select ip).Count() > 0;
            return result;
        }


        /// <summary>
        /// 从网络上下载文件
        /// </summary>
        /// <param name="downloadUrl">下载地址</param>
        /// <param name="saveDirectory">保存目录</param>
        /// <exception cref="AppiumUtility.Exceptions.DownloadException"></exception>
        /// <returns></returns>
        public static string DownloadFile(string downloadUrl, string saveDirectory)
        {
            var webClient = new WebClient();
            string filePath = string.Empty;
            try
            {
                var responseData = webClient.DownloadData(downloadUrl);
                //解析下载的文件名
                string contentDisposition = webClient.ResponseHeaders["Content-Disposition"];
                if (contentDisposition == null) throw new DownloadException("资源文件不存在！");
                string fileName = contentDisposition.Substring(contentDisposition.IndexOf('=') + 1);
                if (string.IsNullOrEmpty(fileName)) throw new Exception("解析下载资源文件名失败！");
                filePath = System.IO.Path.Combine(saveDirectory, fileName);
                System.IO.File.WriteAllBytes(filePath, responseData);
            }
            catch (DownloadException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return filePath;
        }

        /// <summary>
        /// 检测网络连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool CheckNetworkConnect(string ip, int port)
        {
            bool result = false;
            TcpClient client = null;
            try
            {
                client = new TcpClient();
                client.SendTimeout = 5;
                client.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
                client.Close();
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
    }
}
