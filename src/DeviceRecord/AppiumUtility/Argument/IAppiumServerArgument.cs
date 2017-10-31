using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppiumUtility.Argument
{
	public interface IAppiumServerArgument
	{
		string CmdSwitch { get; }
		string AssembleCommandLine();
	}
}
