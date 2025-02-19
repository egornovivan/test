using ItemAsset;
using Pathea;
using Pathea.Effect;
using PETools;
using SkillSystem;
using UnityEngine;

public class PEGun : PEAimAbleEquip, IWeapon, IAimWeapon
{
	public AttackMode[] m_AttackMode;

	public string[] m_Idles;

	[Header("Reload")]
	public string m_ReloadAnim;

	public GameObject m_ChargeEffectGo;

	public int m_ShellCaseEffectID;

	[HideInInspector]
	public Transform m_ShellCaseTrans;

	public GameObject m_MagazineObj;

	public Transform m_MagazinePos;

	public int m_MagazineEffectID;

	public ShootMode m_ShootMode;

	public AmmoType m_AmmoType;

	public Magazine m_Magazine;

	[Header("Ammo gun")]
	public int[] m_AmmoItemIDList = new int[1] { 11000001 };

	private int m_CurAmmoItemIndex;

	[Header("Energy gun")]
	public float m_ChargeEnergySpeed = 0.5f;

	public float m_RechargeEnergySpeed = 3f;

	public float m_RechargeDelay = 1.5f;

	public float[] m_ChargeTime = new float[2] { 0.8f, 1.5f };

	public float m_EnergyPerShoot = 1f;

	public int[] m_SkillIDList = new int[1] { 20219924 };

	public int m_MeleeSkill;

	public float m_FireRate = 0.3f;

	[Header("SEID")]
	public int m_ChargeSoundID;

	public int m_ChargeLevelUpSoundID;

	public int m_ChargeLevelUpEffectID;

	public int m_DryFireSoundID;

	public int m_ShootSoundID;

	public int m_ReloadSoundID;

	private GunAmmo m_ItemAmmoAttr;

	public virtual float magazineCost => 1f;

	public virtual float magazineSize => m_Magazine.m_Size;

	public virtual float magazineValue
	{
		get
		{
			if (m_ItemAmmoAttr != null && Mathf.Abs((float)m_ItemAmmoAttr.count - m_Magazine.m_Value) > 0.8f)
			{
				m_Magazine.m_Value = m_ItemAmmoAttr.count;
			}
			return m_Magazine.m_Value;
		}
		set
		{
			m_Magazine.m_Value = value;
			if (m_ItemAmmoAttr != null)
			{
				m_ItemAmmoAttr.count = Mathf.RoundToInt(value);
			}
		}
	}

	public int curItemID => (m_AmmoItemIDList.Length > 0) ? m_AmmoItemIDList[m_CurAmmoItemIndex] : 0;

	public int curAmmoItemIndex
	{
		get
		{
			return m_CurAmmoItemIndex;
		}
		set
		{
			m_CurAmmoItemIndex = Mathf.Min(value, m_AmmoItemIDList.Length - 1);
			if (m_ItemAmmoAttr != null)
			{
				m_ItemAmmoAttr.index = m_CurAmmoItemIndex;
			}
		}
	}

	public ItemObject ItemObj => m_ItemObj;

	public bool HoldReady => m_MotionMgr.GetMaskState(m_HandChangeAttr.m_HoldActionMask);

	public bool UnHoldReady => !m_MotionMgr.IsActionRunning(m_HandChangeAttr.m_ActiveActionType);

	public virtual bool Aimed
	{
		get
		{
			if (null != m_IKCmpt)
			{
				return m_IKCmpt.aimed;
			}
			return false;
		}
	}

	public string[] leisures => m_Idles;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_ItemAmmoAttr = itemObj.GetCmpt<GunAmmo>();
		if (m_ItemAmmoAttr != null)
		{
			if (m_AmmoItemIDList != null && m_AmmoItemIDList.Length > m_ItemAmmoAttr.index)
			{
				m_CurAmmoItemIndex = m_ItemAmmoAttr.index;
			}
			if (m_Magazine != null)
			{
				if (m_ItemAmmoAttr.count < 0)
				{
					m_ItemAmmoAttr.count = (int)magazineSize;
				}
				m_Magazine.m_Value = m_ItemAmmoAttr.count;
			}
		}
		m_ShellCaseTrans = PEUtil.GetChild(base.transform, "ShellCase");
		m_View = entity.biologyViewCmpt;
		if (null != m_View && null != m_MagazineObj && null != m_View)
		{
			m_View.AttachObject(m_MagazineObj, "mountOff");
			m_MagazineObj.transform.localPosition = Vector3.zero;
			m_MagazineObj.transform.localRotation = Quaternion.identity;
		}
		if (null != m_ChargeEffectGo)
		{
			EffectLateupdateHelper effectLateupdateHelper = m_ChargeEffectGo.AddComponent<EffectLateupdateHelper>();
			effectLateupdateHelper.Init(m_ChargeEffectGo.transform.parent);
			m_ChargeEffectGo.transform.parent = Singleton<EffectBuilder>.Instance.transform;
		}
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if (null != m_View && null != m_MagazineObj && null != m_View)
		{
			m_View.DetachObject(m_MagazineObj);
			Object.Destroy(m_MagazineObj);
		}
		if (null != m_ChargeEffectGo)
		{
			Object.Destroy(m_ChargeEffectGo);
		}
	}

	public override void ResetView()
	{
		base.ResetView();
		if (null != m_View)
		{
			Object.Destroy(m_MagazineObj);
		}
	}

	public int GetSkillID(int chargeLevel = 0)
	{
		if (m_SkillIDList.Length == 0 || chargeLevel >= m_SkillIDList.Length)
		{
			return 0;
		}
		return m_SkillIDList[chargeLevel];
	}

	public virtual void HoldWeapon(bool hold)
	{
		m_MotionEquip.ActiveWeapon(this, hold);
	}

	public AttackMode[] GetAttackMode()
	{
		return m_AttackMode;
	}

	public bool CanAttack(int index = 0)
	{
		return m_MotionMgr.CanDoAction(PEActionType.GunFire);
	}

	public void Attack(int index = 0, SkEntity targetEntity = null)
	{
		if (!(null == m_MotionMgr))
		{
			if (null != m_MotionEquip)
			{
				m_MotionEquip.SetTarget(targetEntity);
			}
			if (index > 0)
			{
				PEActionParamN param = PEActionParamN.param;
				param.n = index;
				m_MotionMgr.DoAction(PEActionType.GunMelee, param);
			}
			else
			{
				PEActionParamB param2 = PEActionParamB.param;
				param2.b = true;
				m_MotionMgr.DoAction(PEActionType.GunFire, param2);
			}
			if (m_AttackMode != null && m_AttackMode.Length > index)
			{
				m_AttackMode[index].ResetCD();
			}
		}
	}

	public bool AttackEnd(int index = 0)
	{
		if (null == m_MotionMgr)
		{
			return true;
		}
		if (index > 0)
		{
			return !m_MotionMgr.IsActionRunning(PEActionType.GunMelee);
		}
		return !m_MotionMgr.IsActionRunning(PEActionType.GunFire);
	}

	public virtual bool IsInCD(int index = 0)
	{
		if (m_AttackMode != null && m_AttackMode.Length > index)
		{
			return m_AttackMode[index].IsInCD();
		}
		return false;
	}

	public virtual void SetAimState(bool aimState)
	{
		if (aimState)
		{
			m_MotionMgr.ContinueAction(m_HandChangeAttr.m_ActiveActionType, PEActionType.GunFire);
		}
		else
		{
			m_MotionMgr.PauseAction(m_HandChangeAttr.m_ActiveActionType, PEActionType.GunFire);
		}
	}

	public void SetTarget(Vector3 aimPos)
	{
		if (null != m_IKCmpt)
		{
			m_IKCmpt.aimTargetPos = aimPos;
		}
	}

	public void SetTarget(Transform trans)
	{
		if (null != m_IKCmpt)
		{
			m_IKCmpt.aimTargetTrans = trans;
		}
	}
}
