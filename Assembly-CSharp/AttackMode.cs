using System;
using UnityEngine;

[Serializable]
public class AttackMode
{
	public AttackType type;

	public float minRange;

	public float maxRange;

	public float minSwitchRange;

	public float maxSwitchRange;

	public float minAngle;

	public float maxAngle;

	public float frequency;

	public float damage;

	public bool ignoreTerrain;

	private float m_LastUseTime = -99999f;

	public bool IsInCD()
	{
		return Time.time - m_LastUseTime <= frequency;
	}

	public void ResetCD()
	{
		m_LastUseTime = Time.time;
	}

	public bool IsInRange(float dis)
	{
		if (minRange < dis && maxRange > dis)
		{
			return true;
		}
		return false;
	}
}
