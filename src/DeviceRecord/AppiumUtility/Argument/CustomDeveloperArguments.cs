﻿
namespace AppiumUtility.Argument
{
    public class CustomDeveloperArguments : AppiumServerStringArgument
    {
        private const string CMD_SWITCH = "";

        public CustomDeveloperArguments(string arguments)
        {
            Init(CMD_SWITCH, arguments);
        }
    }
}
