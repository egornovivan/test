using NaturalResAsset;
using UnityEngine;

public class GroundItemTarget
{
	internal Vector3 Pos;

	internal int TypeIndex;

	internal float HP = 255f;

	internal float MaxHP = 255f;

	internal float Height;

	internal float Width;

	public GroundItemTarget(Vector3 pos, int typeIndex, float height, float width)
	{
		Pos = pos;
		TypeIndex = typeIndex;
		Height = height;
		Width = width;
	}

	public float GetDestroyed(float damage, float durDec)
	{
		HP -= damage * durDec;
		return HP;
	}

	public ESkillTargetType GetTargetType()
	{
		return NaturalRes.GetTerrainResData(TypeIndex + 1000).m_type switch
		{
			9 => ESkillTargetType.TYPE_Wood, 
			10 => ESkillTargetType.TYPE_Herb, 
			_ => ESkillTargetType.TYPE_Herb, 
		};
	}
}
