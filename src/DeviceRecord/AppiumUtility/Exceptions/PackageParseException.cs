using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Exceptions
{
    /// <summary>
    /// APK包解析异常
    /// </summary>
    public class PackageParseException : Exception
    {
        /// <summary>
        /// 包解析异常
        /// </summary>
        public PackageParseException()
            : base()
        {
        }

        /// <summary>
        /// 包解析异常
        /// </summary>
        /// <param name="message">异常信息</param>
        public PackageParseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 包解析异常
        /// </summary>
        /// <param name="message">异常信息</param>
        /// <param name="innerException">导致当前异常的异常</param>
        public PackageParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
