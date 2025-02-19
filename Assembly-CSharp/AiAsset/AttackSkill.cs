using System;

namespace AiAsset;

public class AttackSkill : IComparable<AttackSkill>
{
	public string name;

	public int id;

	public float probability;

	public int CompareTo(AttackSkill other)
	{
		return probability.CompareTo(other.probability);
	}
}
