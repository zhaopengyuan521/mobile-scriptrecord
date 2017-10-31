﻿
namespace AppiumUtility.Argument
{
    public sealed class PlatformNameArgument : AppiumServerStringArgument
    {
        private const string _CommandSwitch = "--platform-name";

        public PlatformNameArgument(string platformName)
        {
            Init(_CommandSwitch, platformName);
        }
    }
}
