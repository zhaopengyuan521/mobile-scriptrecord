using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Exceptions
{
    /// <summary>
    /// 下载异常
    /// </summary>
    public class DownloadException : Exception
    {
        /// <summary>
        /// 下载异常
        /// </summary>
        public DownloadException()
            : base()
        {
        }

        /// <summary>
        /// 下载异常
        /// </summary>
        /// <param name="message">异常信息</param>
        public DownloadException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 下载异常
        /// </summary>
        /// <param name="message">异常信息</param>
        /// <param name="innerException">导致当前异常的异常</param>
        public DownloadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
