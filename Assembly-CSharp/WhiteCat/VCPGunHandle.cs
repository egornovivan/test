using Pathea;
using UnityEngine;

namespace WhiteCat;

public class VCPGunHandle : VCPart
{
	public Transform m_PivotPoint;

	public Transform m_FirstHandPoint;

	public Transform m_SecondHandPoint;

	[Header("Additional Attributes")]
	public PEActionType[] m_RemoveEndAction;

	public ActiveAttr m_HandChangeAttr = new ActiveAttr();

	[Space(8f)]
	public float m_AccuracyMin = 1f;

	public float m_AccuracyMax = 5f;

	public float m_AccuracyPeriod = 5f;

	public float m_AccuracyShrinkSpeed = 3f;

	public float m_CenterUpDisMax = 10f;

	public float m_CenterUpShrinkSpeed = 3f;

	[Space(8f)]
	public string m_ReloadAnim;

	public GameObject m_MagazineObj;

	public Transform m_MagazinePos;

	public int m_MagazineEffectID;

	public int m_MagazineSize = 30;

	public int m_ReloadSoundID;

	[Header("Original Attributes")]
	public bool DualHand;

	public int GunType;

	public bool showOnVehicle;

	public void CopyTo(PEGun target)
	{
		target.m_RemoveEndAction = m_RemoveEndAction;
		target.m_HandChangeAttr = m_HandChangeAttr;
		target.m_AimAttr.m_AccuracyMin = m_AccuracyMin;
		target.m_AimAttr.m_AccuracyMax = m_AccuracyMax;
		target.m_AimAttr.m_AccuracyPeriod = m_AccuracyPeriod;
		target.m_AimAttr.m_AccuracyShrinkSpeed = m_AccuracyShrinkSpeed;
		target.m_AimAttr.m_CenterUpDisMax = m_CenterUpDisMax;
		target.m_AimAttr.m_CenterUpShrinkSpeed = m_CenterUpShrinkSpeed;
		target.m_ReloadAnim = m_ReloadAnim;
		target.m_MagazineObj = m_MagazineObj;
		target.m_MagazinePos = m_MagazinePos;
		target.m_MagazineEffectID = m_MagazineEffectID;
		target.m_Magazine = new Magazine(30f, 30f);
		target.m_Magazine.m_Size = m_MagazineSize;
		target.m_Magazine.m_Value = m_MagazineSize;
		target.m_ReloadSoundID = m_ReloadSoundID;
		target.showOnVehicle = showOnVehicle;
	}
}
