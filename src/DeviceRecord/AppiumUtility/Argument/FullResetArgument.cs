﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppiumUtility.Argument
{
	public sealed class FullResetArgument : AppiumServerArgument
	{
		private const string CMD_SWITCH = "--full-reset";

		public FullResetArgument()
		{
			_cmdSwitch = CMD_SWITCH;
		}
	}
}
