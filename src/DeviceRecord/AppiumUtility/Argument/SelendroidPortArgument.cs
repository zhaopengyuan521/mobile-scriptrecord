
namespace AppiumUtility.Argument
{
    public sealed class SelendroidPortArgument : AppiumServerUintArgument
    {
        private const string CMD_SWITCH = "--selendroid-port";

        public SelendroidPortArgument(uint arguments)
        {
            Init(CMD_SWITCH, arguments);
        }

    }
}
