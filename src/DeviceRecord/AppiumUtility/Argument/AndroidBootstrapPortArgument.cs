
namespace AppiumUtility.Argument
{
    public sealed class AndroidBootstrapPortArgument : AppiumServerUintArgument
    {
        private const string CMD_SWITCH = "--bootstrap-port";

        public AndroidBootstrapPortArgument(uint arguments)
        {
            Init(CMD_SWITCH, arguments);
        }
    }
}
