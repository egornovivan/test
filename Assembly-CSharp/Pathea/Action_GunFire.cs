using System;
using Pathea.Effect;
using SkillSystem;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_GunFire : PEAction
{
	public Action_GunHold m_gunHold;

	public float m_ChargeEffectDelayTime = 0.8f;

	private bool m_EndFire;

	private AudioController m_Audio;

	private float m_HoldFireTime;

	private float m_LastShootTime;

	private Vector3 m_IKAimDirWorld;

	private Vector3 m_IKAimDirLocal;

	private Quaternion m_IK;

	private PEGun m_Gun;

	public PEEnergyGunLogic energyGunLogic;

	private bool m_EndAfterShoot;

	public bool m_IgnoreItem;

	public override PEActionType ActionType => PEActionType.GunFire;

	public IKAimCtrl ikAim { get; set; }

	public SkEntity targetEntity { get; set; }

	public PEGun gun
	{
		get
		{
			return m_Gun;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_Gun = value;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null == base.skillCmpt || null == gun || null == base.entity)
		{
			return false;
		}
		if (gun.durability <= float.Epsilon)
		{
			base.entity.SendMsg(EMsg.Action_DurabilityDeficiency);
			return false;
		}
		if (m_IgnoreItem)
		{
			return true;
		}
		if (gun.m_AmmoType == AmmoType.Bullet)
		{
			return true;
		}
		if (gun.magazineValue >= gun.m_EnergyPerShoot)
		{
			return true;
		}
		return false;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (null == base.skillCmpt || null == gun)
		{
			return;
		}
		PEActionParamB pEActionParamB = para as PEActionParamB;
		m_EndAfterShoot = pEActionParamB.b;
		m_EndFire = false;
		m_HoldFireTime = 0f;
		ShootMode shootMode = gun.m_ShootMode;
		if (shootMode != 0 && shootMode != ShootMode.MultiShoot)
		{
			return;
		}
		if (gun.m_AmmoType == AmmoType.Bullet)
		{
			if (gun.magazineValue > 0f)
			{
				if (Time.time - m_LastShootTime > gun.m_FireRate)
				{
					OnFire(1f);
				}
			}
			else if (m_IgnoreItem || (null != base.packageCmpt && base.packageCmpt.GetItemCount(gun.curItemID) > 0))
			{
				PEActionParamN param = PEActionParamN.param;
				param.n = gun.curAmmoItemIndex;
				base.motionMgr.DoAction(PEActionType.GunReload, param);
			}
			else
			{
				base.skillCmpt.StartSkill(base.skillCmpt, gun.m_DryFireSoundID);
			}
		}
		else if (gun.magazineValue >= gun.m_EnergyPerShoot)
		{
			if (Time.time - m_LastShootTime > gun.m_FireRate)
			{
				OnFire(gun.m_EnergyPerShoot);
			}
		}
		else
		{
			AudioManager.instance.Create(gun.m_AimAttr.m_AimTrans.position, gun.m_DryFireSoundID, gun.m_AimAttr.m_AimTrans);
		}
	}

	public override bool Update()
	{
		if (null == gun)
		{
			return true;
		}
		if (null != energyGunLogic && !energyGunLogic.Equals(null))
		{
			energyGunLogic.lastUsedTime = Time.time;
		}
		switch (gun.m_ShootMode)
		{
		case ShootMode.SingleShoot:
			return true;
		case ShootMode.MultiShoot:
			if (gun.m_AmmoType == AmmoType.Bullet)
			{
				if (gun.magazineValue < 1f)
				{
					m_EndFire = true;
				}
				else if (Time.time - m_LastShootTime > gun.m_FireRate)
				{
					OnFire(1f);
				}
			}
			else if (gun.magazineValue < gun.m_EnergyPerShoot)
			{
				m_EndFire = true;
			}
			else if (Time.time - m_LastShootTime > gun.m_FireRate)
			{
				OnFire(gun.m_EnergyPerShoot);
			}
			if (m_EndFire)
			{
				if (null != m_Audio)
				{
					m_Audio.Delete(0.1f);
					m_Audio = null;
				}
				return true;
			}
			break;
		case ShootMode.ChargeShoot:
			if (Time.time - m_LastShootTime > gun.m_FireRate)
			{
				if (m_EndAfterShoot)
				{
					m_EndFire = true;
				}
				if (m_HoldFireTime > m_ChargeEffectDelayTime)
				{
					if (null != gun.m_ChargeEffectGo && !gun.m_ChargeEffectGo.activeSelf)
					{
						m_Audio = AudioManager.instance.Create(gun.m_AimAttr.m_AimTrans.position, gun.m_ChargeSoundID, gun.m_AimAttr.m_AimTrans, isPlay: true, isDelete: false);
						gun.m_ChargeEffectGo.SetActive(value: true);
						gun.magazineValue -= gun.m_EnergyPerShoot;
					}
					if (gun.magazineValue <= gun.m_ChargeEnergySpeed * Time.deltaTime)
					{
						m_EndFire = true;
						gun.magazineValue = 0f;
					}
					else
					{
						gun.magazineValue -= gun.m_ChargeEnergySpeed * Time.deltaTime;
					}
					if (m_EndFire)
					{
						OnFire(0f, GetChargeLevel(m_HoldFireTime));
						base.skillCmpt.StartSkill(base.skillCmpt, gun.m_ShootSoundID);
						if (null != m_Audio)
						{
							m_Audio.Delete();
							m_Audio = null;
						}
						if (null != gun.m_ChargeEffectGo)
						{
							gun.m_ChargeEffectGo.SetActive(value: false);
						}
						return true;
					}
				}
				else if (m_EndFire)
				{
					OnFire(gun.m_EnergyPerShoot, GetChargeLevel(m_HoldFireTime));
					base.skillCmpt.StartSkill(base.skillCmpt, gun.m_ShootSoundID);
					return true;
				}
				int chargeLevel = GetChargeLevel(m_HoldFireTime);
				m_HoldFireTime += Time.deltaTime;
				if (GetChargeLevel(m_HoldFireTime) > chargeLevel)
				{
					AudioManager.instance.Create(gun.transform.position, gun.m_ChargeLevelUpSoundID, gun.transform);
					Singleton<EffectBuilder>.Instance.Register(gun.m_ChargeLevelUpEffectID, null, gun.m_AimAttr.m_AimTrans);
				}
			}
			else if (m_EndFire)
			{
				EndImmediately();
				return true;
			}
			break;
		}
		return false;
	}

	public override void EndAction()
	{
		m_EndFire = true;
	}

	public override void EndImmediately()
	{
		if (null != m_Audio)
		{
			m_Audio.Delete();
			m_Audio = null;
		}
		if (null != gun && gun.m_ShootMode == ShootMode.ChargeShoot && null != gun.m_ChargeEffectGo)
		{
			gun.m_ChargeEffectGo.SetActive(value: false);
		}
		if (null != energyGunLogic && !energyGunLogic.Equals(null))
		{
			energyGunLogic.lastUsedTime = Time.time;
		}
	}

	private int GetChargeLevel(float chargeTime)
	{
		if (null == gun)
		{
			return 0;
		}
		for (int i = 0; i < gun.m_ChargeTime.Length; i++)
		{
			if (chargeTime < gun.m_ChargeTime[i])
			{
				return i;
			}
		}
		return gun.m_ChargeTime.Length;
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (!(null != gun))
		{
			return;
		}
		switch (eventParam)
		{
		case "ShellCase":
			if (null != gun && gun.m_ShellCaseEffectID != 0 && null != gun.m_ShellCaseTrans)
			{
				Singleton<EffectBuilder>.Instance.Register(gun.m_ShellCaseEffectID, null, gun.m_ShellCaseTrans);
			}
			break;
		}
	}

	private void OnFire(float magazineCost, int skillIndex = 0)
	{
		if (!(null != gun) || gun.m_AimAttr == null || !(null != gun.m_AimAttr.m_AimTrans) || gun.ItemObj == null || !(null != base.entity) || !(null != base.skillCmpt) || gun.m_AttackMode == null)
		{
			return;
		}
		base.skillCmpt.StartSkill(base.skillCmpt, gun.m_ShootSoundID);
		ShootTargetPara shootTargetPara = new ShootTargetPara();
		if (null != ikAim)
		{
			shootTargetPara.m_TargetPos = ikAim.targetPos;
		}
		else
		{
			shootTargetPara.m_TargetPos = base.entity.position + base.entity.forward;
		}
		base.skillCmpt.StartSkill(targetEntity, gun.GetSkillID(skillIndex), shootTargetPara);
		if (PeGameMgr.IsSingle)
		{
			if (!m_IgnoreItem || gun.m_AmmoType != AmmoType.Energy)
			{
				gun.magazineValue -= magazineCost;
			}
		}
		else if (!m_IgnoreItem && null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RequestAttrChanged(base.entity.Id, gun.ItemObj.instanceId, magazineCost, gun.curItemID);
		}
		m_LastShootTime = Time.time;
		if (m_EndAfterShoot && null != base.motionMgr)
		{
			base.motionMgr.EndAction(ActionType);
		}
		base.entity.SendMsg(EMsg.Battle_EquipAttack, gun.ItemObj);
		if (!PeGameMgr.IsMulti && gun.m_AttackMode.Length > 0)
		{
			base.entity.SendMsg(EMsg.Battle_OnAttack, gun.m_AttackMode[0], gun.transform, gun.curItemID);
		}
	}
}
