using SkillAsset;
using UnityEngine;

public class CommonNetworkObject : CommonInterface
{
	public override ESkillTargetType GetTargetType()
	{
		return ESkillTargetType.TYPE_SkillRunner;
	}

	public override Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
