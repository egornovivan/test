using UnityEngine;

namespace SkillSystem;

public static class SystemExt
{
	public static Vector3 Pos(this Collider col)
	{
		return col.bounds.center;
	}
}
