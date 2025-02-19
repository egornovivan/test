using System;
using Pathea;
using UnityEngine;

namespace WhiteCat;

public class VCPSwordHilt : VCPart
{
	[Header("Original Attributes")]
	public float AttackInc;

	public float DurabilityInc;

	public float AttackEnh = 1f;

	public float DurabilityEnh = 1f;

	[Header("Additional Attributes")]
	public PEActionType[] m_RemoveEndAction;

	public ActiveAttr m_HandChangeAttr = new ActiveAttr();

	public AttackMode[] m_AttackMode;

	public PeSword.AttackSkill[] m_AttackSkill;

	public float[] m_StaminaCost;

	public bool showOnVehicle;

	[Header("Original DoubleHilt")]
	public string m_LHandPutOnBone = "mountMain";

	public string m_LHandPutOffBone = "mountBack";

	public GameObject m_LHandWeapon;

	[SerializeField]
	private PEAttackTrigger m_peAttacktrigger;

	public PEAttackTrigger Attacktrigger => m_peAttacktrigger;

	public virtual void CopyTo(PeSword target, CreationData data)
	{
		target.m_RemoveEndAction = m_RemoveEndAction;
		target.m_HandChangeAttr = m_HandChangeAttr;
		target.showOnVehicle = showOnVehicle;
		target.m_AttackMode = new AttackMode[m_AttackMode.Length];
		Array.Copy(m_AttackMode, target.m_AttackMode, m_AttackMode.Length);
		target.m_AttackSkill = new PeSword.AttackSkill[m_AttackSkill.Length];
		Array.Copy(m_AttackSkill, target.m_AttackSkill, m_AttackSkill.Length);
		target.m_StaminaCost = new float[m_StaminaCost.Length];
		for (int i = 0; i < m_StaminaCost.Length; i++)
		{
			target.m_StaminaCost[i] = m_StaminaCost[i] * Mathf.Clamp(data.m_Attribute.m_Weight / PEVCConfig.instance.swordStandardWeight, PEVCConfig.instance.minStaminaCostRatioOfWeight, PEVCConfig.instance.maxStaminaCostRatioOfWeight);
		}
		PETwoHandWeapon pETwoHandWeapon = target as PETwoHandWeapon;
		if (pETwoHandWeapon != null)
		{
			pETwoHandWeapon.m_LHandPutOnBone = m_LHandPutOnBone;
			pETwoHandWeapon.m_LHandPutOffBone = m_LHandPutOffBone;
		}
	}
}
