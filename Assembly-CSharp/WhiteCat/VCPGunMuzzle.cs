using System;
using UnityEngine;

namespace WhiteCat;

public class VCPGunMuzzle : VCPart
{
	public Transform Start;

	public Transform End;

	public Transform MuzzleEffect;

	public int SkillId;

	[Header("Additional Attributes")]
	public Transform m_AimTrans;

	public UISightingTelescope.SightingType m_AimPointType;

	[Range(0.1f, 1f)]
	public float m_FireStability = 0.5f;

	public float m_AccuracyDiffusionRate = 1f;

	public float m_CenterUpDisPerShoot = 3f;

	[Space(8f)]
	public GameObject m_ChargeEffectGo;

	public int m_ShellCaseEffectID;

	public ShootMode m_ShootMode;

	public AmmoType m_AmmoType;

	public int[] m_AmmoItemIDList = new int[1] { 11000001 };

	[Space(8f)]
	public float m_ChargeEnergySpeed = 0.5f;

	public float m_RechargeEnergySpeed = 3f;

	public float m_RechargeDelay = 1.5f;

	public float[] m_ChargeTime = new float[2] { 0.8f, 1.5f };

	public float m_EnergyPerShoot = 1f;

	[Space(8f)]
	public int[] m_SkillIDList = new int[1] { 20219924 };

	public float m_FireRate = 0.3f;

	[Space(8f)]
	public int m_ChargeSoundID;

	public int m_ChargeLevelUpSoundID;

	public int m_DryFireSoundID;

	public int m_ShootSoundID;

	public int m_ChargeLevelUpEffectID;

	public AttackMode[] m_AttackMode;

	[Header("Original Attributes")]
	public int GunType;

	public bool Multishot;

	public float FireInterval = 0.5f;

	public int CostItemId;

	public EArmType ArmType;

	public int ProjectileId;

	public int SoundId;

	public float Attack;

	public int AttackType = 3;

	public float Accuracy = 0.5f;

	public void CopyTo(PEGun target)
	{
		target.m_AimAttr.m_AimTrans = m_AimTrans;
		target.m_AimAttr.m_AimPointType = m_AimPointType;
		target.m_AimAttr.m_FireStability = m_FireStability;
		target.m_AimAttr.m_AccuracyDiffusionRate = m_AccuracyDiffusionRate;
		target.m_AimAttr.m_CenterUpDisPerShoot = m_CenterUpDisPerShoot;
		target.m_ChargeEffectGo = m_ChargeEffectGo;
		target.m_ShellCaseEffectID = m_ShellCaseEffectID;
		target.m_ShootMode = m_ShootMode;
		target.m_AmmoType = m_AmmoType;
		target.m_AmmoItemIDList = m_AmmoItemIDList;
		target.m_ChargeEnergySpeed = m_ChargeEnergySpeed;
		target.m_RechargeEnergySpeed = m_RechargeEnergySpeed;
		target.m_RechargeDelay = m_RechargeDelay;
		target.m_ChargeTime = m_ChargeTime;
		target.m_EnergyPerShoot = m_EnergyPerShoot;
		target.m_SkillIDList = m_SkillIDList;
		target.m_FireRate = m_FireRate;
		target.m_ChargeSoundID = m_ChargeSoundID;
		target.m_ChargeLevelUpSoundID = m_ChargeLevelUpSoundID;
		target.m_DryFireSoundID = m_DryFireSoundID;
		target.m_ShootSoundID = m_ShootSoundID;
		target.m_ChargeLevelUpEffectID = m_ChargeLevelUpEffectID;
		target.m_AttackMode = new AttackMode[m_AttackMode.Length];
		Array.Copy(m_AttackMode, target.m_AttackMode, m_AttackMode.Length);
	}

	public void PlayMuzzleEffect()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(MuzzleEffect.gameObject);
		gameObject.transform.parent = MuzzleEffect.transform.parent;
		gameObject.transform.localPosition = MuzzleEffect.transform.localPosition;
		gameObject.transform.localRotation = MuzzleEffect.transform.localRotation;
		gameObject.transform.localScale = MuzzleEffect.transform.localScale;
		gameObject.SetActive(value: true);
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.enabled = true;
		}
		ParticleSystem[] componentsInChildren2 = gameObject.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array2 = componentsInChildren2;
		foreach (ParticleSystem particleSystem in array2)
		{
			particleSystem.Play();
		}
	}
}
