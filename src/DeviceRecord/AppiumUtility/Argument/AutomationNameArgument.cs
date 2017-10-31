
namespace AppiumUtility.Argument
{
    public sealed class AutomationNameArgument : AppiumServerStringArgument
    {
        private const string _CommandSwitch = "--automation-name";

        public AutomationNameArgument(string automationName)
        {
            Init(_CommandSwitch, automationName);
        }
    }
}
