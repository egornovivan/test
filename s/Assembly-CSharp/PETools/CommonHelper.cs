using UnityEngine;

namespace PETools;

public static class CommonHelper
{
	public static void AdjustPos(ref Vector3 pos)
	{
		pos.y = Mathf.Floor(pos.y) + 1f;
	}
}
