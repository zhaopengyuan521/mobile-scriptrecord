
namespace AppiumUtility.Argument
{
    public sealed class OverrideExistingSessionArgument : AppiumServerArgument
    {
        private const string CMD_SWITCH = "--session-override";

        public OverrideExistingSessionArgument()
        {
            _cmdSwitch = CMD_SWITCH;
        }

    }
}
