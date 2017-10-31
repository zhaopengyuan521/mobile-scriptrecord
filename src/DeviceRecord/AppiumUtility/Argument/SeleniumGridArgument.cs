
namespace AppiumUtility.Argument
{
    public sealed class SeleniumGridArgument : AppiumServerStringArgument
    {
        private const string CMD_SWITCH = "--nodeconfig";

        public SeleniumGridArgument(string arguments)
        {
            Init(CMD_SWITCH, arguments);
        }
    }
}
