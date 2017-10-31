using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppiumUtility.Argument
{
	public sealed class NoResetArgument : AppiumServerArgument
	{
		private const string CMD_SWITCH = "--no-reset";

		public NoResetArgument()
		{
			_cmdSwitch = CMD_SWITCH;
		}
	}
}
