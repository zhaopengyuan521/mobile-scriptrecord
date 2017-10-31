
namespace AppiumUtility.Argument
{
    public class AndroidWaitForPackageArgument : AppiumServerStringArgument
    {
        private const string CMD_SWITCH = "--app-wait-package";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="waitForPackageName">Wait for Package name</param>
        public AndroidWaitForPackageArgument(string waitForPackageName)
        {
            Init(CMD_SWITCH, waitForPackageName);
        }
    }
}
