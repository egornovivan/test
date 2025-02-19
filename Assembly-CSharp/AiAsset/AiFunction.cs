using UnityEngine;

namespace AiAsset;

public class AiFunction
{
	public static void Log(string str)
	{
		Debug.Log(str);
	}

	public static bool Match(AttackSkill skill, string skillName)
	{
		return skill.name == skillName;
	}
}
