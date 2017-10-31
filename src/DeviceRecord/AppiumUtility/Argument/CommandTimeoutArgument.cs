using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Argument
{
    /// <summary>
    /// 所有会话的接收命令超时时间
    /// </summary>
    public sealed class CommandTimeoutArgument : AppiumServerUintArgument
    {
        private const string CMD_SWITCH = "--command-timeout";

        public CommandTimeoutArgument(uint timeout)
        {
            Init(CMD_SWITCH, timeout);
        }
    }
}
