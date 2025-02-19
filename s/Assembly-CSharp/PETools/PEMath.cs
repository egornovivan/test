using UnityEngine;

namespace PETools;

public static class PEMath
{
	public struct DrawTarget
	{
		public RaycastHit rch;

		public IntVector3 snapto;

		public IntVector3 cursor;
	}

	public const float Epsilon = float.Epsilon;

	public const int MC_ISO_VALUE = 128;

	public const float MC_ISO_VALUEF = 127.5f;

	public static bool IsNumeral(string tmp)
	{
		try
		{
			int.Parse(tmp);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
