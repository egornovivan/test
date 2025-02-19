using Pathea;
using UnityEngine;

namespace WhiteCat;

public class VCPAxeHilt : VCPart
{
	[Header("Original Attributes")]
	public float AttackInc;

	public float DurabilityInc;

	public float AttackEnh = 1f;

	public float DurabilityEnh = 1f;

	[Header("Additional Attributes")]
	public PEActionType[] m_RemoveEndAction;

	public ActiveAttr m_HandChangeAttr = new ActiveAttr();

	public bool showOnVehicle;

	public int m_FellSkillID;

	public float m_StaminaCost = 5f;

	public void CopyTo(PEAxe target, CreationData data)
	{
		target.m_RemoveEndAction = m_RemoveEndAction;
		target.m_HandChangeAttr = m_HandChangeAttr;
		target.showOnVehicle = showOnVehicle;
		target.m_FellSkillID = m_FellSkillID;
		target.m_StaminaCost = m_StaminaCost;
	}
}
