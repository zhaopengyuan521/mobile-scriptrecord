
namespace AppiumUtility.Argument
{
    public sealed class LogToWebHookArgument : AppiumServerStringArgument
    {
        private const string CMD_SWITCH = "--webhook";

        public LogToWebHookArgument(string arguments)
        {
            Init(CMD_SWITCH, arguments);
        }
    }
}

