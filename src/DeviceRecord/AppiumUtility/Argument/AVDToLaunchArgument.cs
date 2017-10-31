﻿
namespace AppiumUtility.Argument
{
	public sealed class AVDToLaunchArgument : AppiumServerStringArgument
	{
		private const string CMD_SWITCH = "--avd";

		public AVDToLaunchArgument(string activityName)
		{
			Init(CMD_SWITCH, activityName);
		}
	}
}
