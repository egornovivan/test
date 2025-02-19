using System;

namespace AiAsset;

[Serializable]
public class AiPrefab : IComparable<AiPrefab>
{
	public LifeHabit habit;

	public Nature nature;

	public string prefabName;

	public float radius;

	public float height;

	public float rate;

	public int CompareTo(AiPrefab other)
	{
		return rate.CompareTo(other.rate);
	}
}
