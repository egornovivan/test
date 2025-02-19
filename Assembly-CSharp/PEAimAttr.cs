using System;
using UnityEngine;

[Serializable]
public class PEAimAttr
{
	public const float AccuracyDis = 100f;

	public Transform m_AimTrans;

	public UISightingTelescope.SightingType m_AimPointType;

	[Range(0.1f, 1f)]
	public float m_FireStability = 0.5f;

	public float m_AccuracyMin = 1f;

	public float m_AccuracyMax = 5f;

	public float m_AccuracyPeriod = 5f;

	public float m_AccuracyDiffusionRate = 1f;

	public float m_AccuracyShrinkSpeed = 3f;

	public float m_CenterUpDisMax = 10f;

	public float m_CenterUpDisPerShoot = 3f;

	public float m_CenterUpShrinkSpeed = 3f;

	public bool m_ApplyAimIK = true;

	public bool m_SyncIKWhenAnim = true;
}
