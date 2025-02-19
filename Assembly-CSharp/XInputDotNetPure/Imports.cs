using System;
using System.Runtime.InteropServices;

namespace XInputDotNetPure;

internal class Imports
{
	internal const string DLLName = "XInputInterface";

	[DllImport("XInputInterface")]
	public static extern uint XInputGamePadGetState(uint playerIndex, IntPtr state);

	[DllImport("XInputInterface")]
	public static extern void XInputGamePadSetState(uint playerIndex, float leftMotor, float rightMotor);
}
