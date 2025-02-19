using UnityEngine;

namespace AiAsset;

public class DamageMsgArg
{
	public AiObject attacker;

	public GameObject hurter;

	public float damage;

	public DamageMsgArg(AiObject attacker, GameObject hurter, float damage)
	{
		this.attacker = attacker;
		this.hurter = hurter;
		this.damage = damage;
	}
}
