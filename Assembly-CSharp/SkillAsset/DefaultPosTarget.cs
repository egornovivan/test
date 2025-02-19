using UnityEngine;

namespace SkillAsset;

public class DefaultPosTarget : ISkillTarget
{
	private Vector3 m_vPos;

	public DefaultPosTarget(Vector3 position)
	{
		m_vPos = position;
	}

	public ESkillTargetType GetTargetType()
	{
		return ESkillTargetType.TYPE_Building;
	}

	public Vector3 GetPosition()
	{
		return m_vPos;
	}
}
