using UnityEngine;

namespace SkillSystem;

public class ShootTargetPara : ISkPara, ISkParaNet
{
	public Vector3 m_TargetPos;

	public int TypeId => 3;

	public ShootTargetPara()
	{
		m_TargetPos = Vector3.zero;
	}

	public float[] ToFloatArray()
	{
		return new float[4]
		{
			(float)TypeId + 0.1f,
			m_TargetPos.x,
			m_TargetPos.y,
			m_TargetPos.z
		};
	}

	public void FromFloatArray(float[] data)
	{
		m_TargetPos.x = data[1];
		m_TargetPos.y = data[2];
		m_TargetPos.z = data[3];
	}
}
