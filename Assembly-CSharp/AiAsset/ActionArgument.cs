using System;
using UnityEngine;

namespace AiAsset;

[Serializable]
public class ActionArgument : IComparable<ActionArgument>
{
	public bool enable;

	[HideInInspector]
	public bool canDamage = true;

	public float probability;

	public float coolTime;

	public int CompareTo(ActionArgument other)
	{
		return probability.CompareTo(other.probability);
	}
}
