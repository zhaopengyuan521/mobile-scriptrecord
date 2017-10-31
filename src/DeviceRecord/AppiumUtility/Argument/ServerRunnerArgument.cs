
using AppiumUtility.Config;
namespace AppiumUtility.Argument
{
    public sealed class ServerRunnerArgument : AppiumServerArgument
    {
        private static readonly string CMD_SWITCH = AppConfigManager.Instance.AppiumRunnerArgument;

        public ServerRunnerArgument()
        {
            _cmdSwitch = CMD_SWITCH;
        }
    }
}
