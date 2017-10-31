using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Session
{
    /// <summary>
    /// Appium会话信息
    /// </summary>
    public struct AppiumSession
    {
        /// <summary>
        /// Appium服务地址
        /// </summary>
        public string ServerAddress { get; private set; }

        /// <summary>
        /// Appium服务端口号
        /// </summary>
        public uint AppiumPort { get; private set; }

        /// <summary>
        /// Bootstrap服务端口号
        /// </summary>
        public uint BootstrapPort { get; set; }

        /// <summary>
        /// Selendroid服务端口号
        /// </summary>
        public uint SelendroidPort { get; set; }

        /// <summary>
        /// Chromedriver服务端口号
        /// </summary>
        public uint ChromedriverPort { get; set; }

        /// <summary>
        /// 设备序列号
        /// </summary>
        public string Serialno { get; private set; }

        public AppiumSession(string serverAddress, uint port, string serialno)
            : this()
        {
            ServerAddress = serverAddress;
            AppiumPort = port;
            Serialno = serialno;
        }

        /// <summary>
        /// 当前会话占用端口号中最大的端口号
        /// </summary>
        /// <returns></returns>
        public uint MaxPort()
        {
            uint maxPort = ChromedriverPort;
            if (SelendroidPort > maxPort) maxPort = SelendroidPort;
            if (BootstrapPort > maxPort) maxPort = BootstrapPort;
            if (AppiumPort > maxPort) maxPort = AppiumPort;
            return maxPort;
        }

        #region 重载运算符
        public static bool operator ==(AppiumSession lhs, AppiumSession rhs)
        {
            return lhs.Serialno == rhs.Serialno;
        }

        public static bool operator ==(AppiumSession lhs, string rhs)
        {
            return lhs.Serialno == rhs;
        }

        public static bool operator !=(AppiumSession lhs, AppiumSession rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator !=(AppiumSession lhs, string rhs)
        {
            return !(lhs == rhs);
        }


        public static bool operator ==(string rhs,AppiumSession lhs)
        {
            return lhs.Serialno == rhs;
        }

        public static bool operator !=(string rhs, AppiumSession lhs)
        {
            return !(lhs == rhs);
        }
        #endregion
    }
}
