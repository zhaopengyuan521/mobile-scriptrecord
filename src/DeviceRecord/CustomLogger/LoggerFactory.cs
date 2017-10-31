using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CustomLogger
{
    /// <summary>
    /// 日志工厂类
    /// </summary>
    public static class LoggerFactory
    {
        /// <summary>
        /// 获取应用日志记录对象
        /// </summary>
        /// <returns></returns>
        public static ILog GetApplicationLogger()
        {
            return CustomRollingFileLogger.GetCustomLogger("app");
        }

        /// <summary>
        /// 获取脚本运行期间日志记录对象
        /// </summary>
        /// <param name="tag">标识（不为空：移动设备执行日志；为空：PC脚本执行日志）</param>
        /// <returns></returns>
        public static ILog GetScriptRunningLogger(string tag)
        {
            
            string category = string.Format("scriptRunning\\{0}", DateTime.Now.ToString("yyyy-MM-dd"));
            
            tag = string.IsNullOrEmpty(tag) ? "PCScriptRunning" : tag;
            return CustomRollingFileLogger.GetCustomLogger(tag, category);
        }

        /// <summary>
        /// 对象转Json字符串
        /// </summary>
        /// <param name="obj">转换对象</param>
        /// <returns></returns>
        public static string ConvertToJson(object obj)
        {
            string result = string.Empty;
            //参数验证
            if (obj == null)
            {
                result = "";
                return result;
            }
            JavaScriptSerializer jss = new JavaScriptSerializer();
            result = jss.Serialize(obj);
            return result;
        }
    }
}
