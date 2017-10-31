using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Exceptions
{
    /// <summary>
    /// 安装应用异常
    /// </summary>
    public class InstallException : Exception
    {
        /// <summary>
        /// 安装应用异常
        /// </summary>
        public InstallException()
            : base()
        {
        }

        /// <summary>
        /// 安装应用异常
        /// </summary>
        /// <param name="message">异常信息</param>
        public InstallException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 安装应用异常
        /// </summary>
        /// <param name="message">异常信息</param>
        /// <param name="innerException">导致当前异常的异常</param>
        public InstallException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
