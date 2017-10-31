using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Argument
{
    /// <summary>
    /// 日志输出级别
    /// </summary>
    public class LogLevelArgument : AppiumServerStringArgument
    {
        private const string _CommandSwitch = "--log-level";

        public LogLevelArgument(string level)
        {
            Init(_CommandSwitch, level);
        }
    }
}
