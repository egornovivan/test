using System;
using System.Collections.Generic;
using ItemAsset;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class Motion_Equip : PeCmpt, IPeMsg
{
	public const string GlovesPrefabPath = "Prefab/Item/Equip/Weapon/Other/Gloves";

	[HideInInspector]
	[SerializeField]
	private Action_HandChangeEquipHold m_HandChangeHold;

	[SerializeField]
	[HideInInspector]
	private Action_HandChangeEquipPutOff m_HandChangePutOff;

	[SerializeField]
	private Action_SwordAttack m_SwordAttack;

	[SerializeField]
	[HideInInspector]
	private Action_TwoHandWeaponHold m_TwoHandWeaponHold;

	[SerializeField]
	[HideInInspector]
	private Action_TwoHandWeaponPutOff m_TwoHandWeaponPutOff;

	[HideInInspector]
	[SerializeField]
	private Action_TwoHandWeaponAttack m_TwoHandWeaponAttack;

	[HideInInspector]
	[SerializeField]
	private Action_SheildHold m_SheildHold;

	[SerializeField]
	[HideInInspector]
	private Action_GunHold m_GunHold;

	[HideInInspector]
	[SerializeField]
	private Action_GunPutOff m_GunPutOff;

	[SerializeField]
	private Action_GunFire m_GunFire;

	[SerializeField]
	private Action_GunReload m_GunReload;

	[SerializeField]
	private Action_GunMelee m_GunMelee;

	[SerializeField]
	private Action_BowHold m_BowHold;

	[SerializeField]
	[HideInInspector]
	private Action_BowPutOff m_BowPutOff;

	[SerializeField]
	private Action_BowShoot m_BowShoot;

	[SerializeField]
	private Action_BowReload m_BowReload;

	[SerializeField]
	private Action_AimEquipHold m_AimEquipHold;

	[SerializeField]
	private Action_AimEquipPutOff m_AimEquipPutOff;

	[SerializeField]
	private Action_DigTerrain m_DigTerrain;

	[SerializeField]
	private Action_Fell m_Fell;

	[SerializeField]
	private Action_JetPack m_JetPackAction;

	[SerializeField]
	private Action_Parachute m_ParachuteAction;

	[SerializeField]
	private Action_Glider m_GliderAction;

	[SerializeField]
	private Action_DrawWater m_DrawWater;

	[SerializeField]
	private Action_PumpWater m_PumpWater;

	[SerializeField]
	private Action_Throw m_ThrowGrenade;

	[SerializeField]
	private Action_FlashLight m_FlashLightAction;

	[SerializeField]
	private Action_RopeGunShoot m_RopeGunAction;

	private MotionMgrCmpt m_MotionMgr;

	private PeSword m_Sword;

	private PEAxe m_Axe;

	private PETwoHandWeapon m_TwoHandWeapon;

	private PESheild m_Sheild;

	private PEEnergySheildLogic m_EnergySheild;

	private PEGun m_Gun;

	private PEBow m_Bow;

	private PEDigTool m_DigTool;

	private PEParachute m_Parachute;

	private PEGlider m_Glider;

	private PEGloves m_Gloves;

	private PEWaterPitcher m_WaterPitcher;

	private IKAimCtrl m_IKAimCtrl;

	private PeTrans m_Trans;

	private SkAliveEntity m_Skill;

	private BiologyViewCmpt m_View;

	private EquipmentCmpt m_EquipCmpt;

	private PackageCmpt m_Package;

	private NpcCmpt m_NPC;

	private AnimatorCmpt m_Anim;

	private PEHoldAbleEquipment m_ActiveableEquipment;

	private int m_WeaponID = -1;

	private IWeapon m_Weapon;

	private bool m_SwitchWeapon;

	private bool m_PutOnNewWeapon;

	private PEHoldAbleEquipment m_OldWeapon;

	private PEHoldAbleEquipment m_NewWeapon;

	private HeavyEquipmentCtrl m_HeavyEquipmentCtrl = new HeavyEquipmentCtrl();

	private List<IRechargeableEquipment> m_RechangeableEquipments = new List<IRechargeableEquipment>();

	private Dictionary<Type, Action<PEEquipment>> m_SetEquipmentFunc = new Dictionary<Type, Action<PEEquipment>>();

	private float m_CheckIgnorCostTime;

	private List<IWeapon> retList = new List<IWeapon>();

	private float accuracyScale = 0.5f;

	private float accuracyRangeAdd = 3f;

	public MotionMgrCmpt motionMgr => m_MotionMgr;

	public PEEnergySheildLogic energySheild => m_EnergySheild;

	public PEGun gun => m_Gun;

	public PEBow bow => m_Bow;

	public PESheild sheild => m_Sheild;

	public PEAxe axe => m_Axe;

	public PEDigTool digTool => m_DigTool;

	public PEGloves gloves => m_Gloves;

	public ItemObject PEHoldAbleEqObj => (!(null != m_ActiveableEquipment)) ? null : m_ActiveableEquipment.m_ItemObj;

	public PEHoldAbleEquipment ActiveableEquipment => m_ActiveableEquipment;

	public IWeapon Weapon
	{
		get
		{
			if (m_Weapon != null && !m_Weapon.Equals(null))
			{
				PEHoldAbleEquipment pEHoldAbleEquipment = m_Weapon as PEHoldAbleEquipment;
				if (null != pEHoldAbleEquipment && !motionMgr.IsActionRunning(pEHoldAbleEquipment.m_HandChangeAttr.m_ActiveActionType))
				{
					m_Weapon = null;
					m_WeaponID = -1;
				}
				return m_Weapon;
			}
			return null;
		}
	}

	public float jetPackEnCurrent
	{
		get
		{
			if (null != m_JetPackAction.jetPackLogic)
			{
				return m_JetPackAction.jetPackLogic.enCurrent;
			}
			return 0f;
		}
	}

	public float jetPackEnMax
	{
		get
		{
			if (null != m_JetPackAction.jetPackLogic)
			{
				return m_JetPackAction.jetPackLogic.enMax;
			}
			return 0f;
		}
	}

	private bool isMainPlayer => PeSingleton<MainPlayer>.Instance.entity == base.Entity;

	public EquipType EquipedWeaponType
	{
		get
		{
			if (null != m_Sword)
			{
				return m_Sword.equipType;
			}
			if (null != m_Gun)
			{
				return m_Gun.equipType;
			}
			if (null != m_Bow)
			{
				return m_Bow.equipType;
			}
			return EquipType.Null;
		}
	}

	public bool ISAimWeapon
	{
		get
		{
			if (null != m_ActiveableEquipment && m_ActiveableEquipment is PEAimAbleEquip)
			{
				return m_ActiveableEquipment.m_HandChangeAttr.m_CamMode.camModeIndex3rd == 1;
			}
			return false;
		}
	}

	public event Action OnActiveWeapon;

	public event Action OnDeactiveWeapon;

	private void InitAction()
	{
		m_Trans = base.Entity.peTrans;
		m_Skill = base.Entity.aliveEntity;
		m_Skill.onSheildReduce += OnSheildReduce;
		m_View = base.Entity.biologyViewCmpt;
		m_EquipCmpt = base.Entity.equipmentCmpt;
		m_Package = base.Entity.packageCmpt;
		m_NPC = base.Entity.NpcCmpt;
		m_Anim = base.Entity.animCmpt;
		m_MotionMgr = base.Entity.motionMgr;
		Invoke("CheckGloves", 0.5f);
		m_HeavyEquipmentCtrl.moveCmpt = base.Entity.motionMove as Motion_Move_Human;
		m_HeavyEquipmentCtrl.ikCmpt = base.Entity.IKCmpt;
		m_HeavyEquipmentCtrl.motionMgr = m_MotionMgr;
		m_SwordAttack.m_UseStamina = isMainPlayer;
		m_TwoHandWeaponAttack.m_UseStamina = isMainPlayer;
		m_GunFire.m_gunHold = m_GunHold;
		m_HandChangeHold.onActiveEvt += OnActiveEquipment;
		m_HandChangeHold.onDeactiveEvt += OnDeactiveEquipment;
		m_TwoHandWeaponHold.onActiveEvt += OnActiveEquipment;
		m_TwoHandWeaponHold.onDeactiveEvt += OnDeactiveEquipment;
		m_GunHold.onActiveEvt += OnActiveEquipment;
		m_GunHold.onDeactiveEvt += OnDeactiveEquipment;
		m_BowHold.onActiveEvt += OnActiveEquipment;
		m_BowHold.onDeactiveEvt += OnDeactiveEquipment;
		m_AimEquipHold.onActiveEvt += OnActiveEquipment;
		m_AimEquipHold.onDeactiveEvt += OnDeactiveEquipment;
		if (null != m_MotionMgr)
		{
			m_MotionMgr.onActionEnd += OnActionEnd;
			m_MotionMgr.AddAction(m_HandChangeHold);
			m_MotionMgr.AddAction(m_HandChangePutOff);
			m_MotionMgr.AddAction(m_SwordAttack);
			m_MotionMgr.AddAction(m_TwoHandWeaponHold);
			m_MotionMgr.AddAction(m_TwoHandWeaponPutOff);
			m_MotionMgr.AddAction(m_TwoHandWeaponAttack);
			m_MotionMgr.AddAction(m_SheildHold);
			m_MotionMgr.AddAction(m_GunHold);
			m_MotionMgr.AddAction(m_GunPutOff);
			m_MotionMgr.AddAction(m_GunFire);
			m_MotionMgr.AddAction(m_GunReload);
			m_MotionMgr.AddAction(m_GunMelee);
			m_MotionMgr.AddAction(m_BowHold);
			m_MotionMgr.AddAction(m_BowPutOff);
			m_MotionMgr.AddAction(m_BowShoot);
			m_MotionMgr.AddAction(m_BowReload);
			m_MotionMgr.AddAction(m_AimEquipHold);
			m_MotionMgr.AddAction(m_AimEquipPutOff);
			m_MotionMgr.AddAction(m_DigTerrain);
			m_MotionMgr.AddAction(m_Fell);
			m_MotionMgr.AddAction(m_JetPackAction);
			m_MotionMgr.AddAction(m_ParachuteAction);
			m_MotionMgr.AddAction(m_GliderAction);
			m_MotionMgr.AddAction(m_DrawWater);
			m_MotionMgr.AddAction(m_PumpWater);
			m_MotionMgr.AddAction(m_ThrowGrenade);
			m_MotionMgr.AddAction(m_FlashLightAction);
			m_MotionMgr.AddAction(m_RopeGunAction);
		}
	}

	private void InitEquipment()
	{
		m_SetEquipmentFunc[typeof(PeSword)] = SetSword;
		m_SetEquipmentFunc[typeof(PETorch)] = SetSword;
		m_SetEquipmentFunc[typeof(PETwoHandWeapon)] = SetTwoHandWeapon;
		m_SetEquipmentFunc[typeof(PEAxe)] = SetAxe;
		m_SetEquipmentFunc[typeof(PEChainSaw)] = SetChainSaw;
		m_SetEquipmentFunc[typeof(PEJetPack)] = SetJetPack;
		m_SetEquipmentFunc[typeof(PEParachute)] = SetParachute;
		m_SetEquipmentFunc[typeof(PEGlider)] = SetGlider;
		m_SetEquipmentFunc[typeof(PEWaterPitcher)] = SetWaterPitcher;
		m_SetEquipmentFunc[typeof(PEWaterPump)] = SetWaterPump;
		m_SetEquipmentFunc[typeof(PESheild)] = SetSheild;
		m_SetEquipmentFunc[typeof(PEGun)] = SetGun;
		m_SetEquipmentFunc[typeof(PEPujaGun)] = SetGun;
		m_SetEquipmentFunc[typeof(PEConversionGun)] = SetGun;
		m_SetEquipmentFunc[typeof(PEBow)] = SetBow;
		m_SetEquipmentFunc[typeof(PEDigTool)] = SetDigTool;
		m_SetEquipmentFunc[typeof(PEGrenade)] = SetGrenade;
		m_SetEquipmentFunc[typeof(PEFlashLight)] = SetFlashLight;
		m_SetEquipmentFunc[typeof(PECrusher)] = SetCrusher;
		m_SetEquipmentFunc[typeof(PERopeGun)] = SetRopeGun;
	}

	public override void Start()
	{
		base.Start();
		InitAction();
		InitEquipment();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		UpdateHeavyEquipment();
		UpdateEnergeRecharge();
		UpdateAutoUseEquipment();
		UpdateSwitchWeapon();
		UpdateItemCostState();
	}

	private void UpdateEnergeRecharge()
	{
		if (!(null != m_Skill) || !(m_Skill.GetAttribute(AttribType.Energy) > float.Epsilon))
		{
			return;
		}
		for (int num = m_RechangeableEquipments.Count - 1; num >= 0; num--)
		{
			IRechargeableEquipment rechargeableEquipment = m_RechangeableEquipments[num];
			if (rechargeableEquipment != null && !rechargeableEquipment.Equals(null))
			{
				if (Time.time > rechargeableEquipment.lastUsedTime + rechargeableEquipment.rechargeDelay && !(rechargeableEquipment.enMax - rechargeableEquipment.enCurrent <= float.Epsilon))
				{
					float attribute = m_Skill.GetAttribute(AttribType.Energy);
					float num2 = Mathf.Clamp(rechargeableEquipment.rechargeSpeed * Time.deltaTime, 0f, rechargeableEquipment.enMax - rechargeableEquipment.enCurrent);
					if (num2 > attribute)
					{
						num2 = attribute;
					}
					rechargeableEquipment.enCurrent += num2;
					m_Skill.SetAttribute(AttribType.Energy, attribute - num2);
				}
			}
			else
			{
				m_RechangeableEquipments.RemoveAt(num);
			}
		}
	}

	private void UpdateHeavyEquipment()
	{
		m_HeavyEquipmentCtrl.Update();
	}

	private void UpdateAutoUseEquipment()
	{
		if (null != m_Parachute)
		{
			m_MotionMgr.DoAction(PEActionType.Parachute);
		}
		if (null != m_Glider)
		{
			m_MotionMgr.DoAction(PEActionType.Glider);
		}
		if (null != m_DigTool)
		{
			m_DigTerrain.UpdateDigPos();
		}
	}

	private void UpdateSwitchWeapon()
	{
		if (m_SwitchWeapon)
		{
			if (null == m_OldWeapon || m_OldWeapon.Equals(null) || null == m_NewWeapon || m_NewWeapon.Equals(null))
			{
				m_SwitchWeapon = false;
			}
			else if (m_PutOnNewWeapon && m_MotionMgr.IsActionRunning(m_NewWeapon.m_HandChangeAttr.m_ActiveActionType))
			{
				m_SwitchWeapon = false;
			}
			else if (m_MotionMgr.IsActionRunning(m_OldWeapon.m_HandChangeAttr.m_ActiveActionType))
			{
				ActiveEquipment(m_OldWeapon, active: false);
			}
			else if (!m_MotionMgr.IsActionRunning(m_NewWeapon.m_HandChangeAttr.m_ActiveActionType))
			{
				m_PutOnNewWeapon = true;
				ActiveEquipment(m_NewWeapon, active: true);
			}
		}
	}

	private void UpdateItemCostState()
	{
		if (!isMainPlayer)
		{
			if (base.Entity.proto == EEntityProto.Monster)
			{
				m_GunFire.m_IgnoreItem = true;
				m_GunReload.m_IgnoreItem = true;
				m_BowShoot.m_IgnoreItem = true;
				m_BowHold.m_IgnoreItem = true;
				m_BowReload.m_IgnoreItem = true;
			}
			else if (null != m_NPC)
			{
				bool hasConsume = m_NPC.HasConsume;
				m_GunFire.m_IgnoreItem = !hasConsume;
				m_GunReload.m_IgnoreItem = !hasConsume;
				m_BowShoot.m_IgnoreItem = !hasConsume;
				m_BowHold.m_IgnoreItem = !hasConsume;
				m_BowReload.m_IgnoreItem = !hasConsume;
			}
		}
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.Skill_CheckLoop:
			if (args[0] is SkFuncInOutPara { _para: not null, _para: string para } skFuncInOutPara && para == "SwordAttack")
			{
				if (null != m_TwoHandWeaponAttack.sword)
				{
					skFuncInOutPara._ret = m_TwoHandWeaponAttack.CheckContinueAttack();
				}
				else if (null != m_SwordAttack.sword)
				{
					skFuncInOutPara._ret = m_SwordAttack.CheckContinueAttack();
				}
			}
			break;
		case EMsg.View_Prefab_Build:
		{
			BiologyViewCmpt biologyViewCmpt = args[0] as BiologyViewCmpt;
			HumanPhyCtrl monoPhyCtrl = biologyViewCmpt.monoPhyCtrl;
			m_SwordAttack.phyMotor = monoPhyCtrl;
			m_TwoHandWeaponAttack.phyMotor = monoPhyCtrl;
			m_JetPackAction.m_PhyCtrl = monoPhyCtrl;
			m_ParachuteAction.m_PhyCtrl = monoPhyCtrl;
			m_GliderAction.m_PhyCtrl = monoPhyCtrl;
			m_HeavyEquipmentCtrl.phyCtrl = monoPhyCtrl;
			m_RopeGunAction.phyCtrl = monoPhyCtrl;
			m_IKAimCtrl = biologyViewCmpt.monoIKAimCtrl;
			m_GunFire.ikAim = m_IKAimCtrl;
			m_BowShoot.ikAim = m_IKAimCtrl;
			CheckGloves();
			Invoke("ResetWeapon", 0.5f);
			break;
		}
		case EMsg.View_Prefab_Destroy:
			DeletGloves();
			break;
		case EMsg.Battle_OnShoot:
			if (null != m_Gun)
			{
				m_GunHold.OnFire();
			}
			if (null != m_Bow)
			{
				m_BowHold.OnFire();
			}
			break;
		case EMsg.View_FirstPerson:
			m_SwordAttack.firstPersonAttack = (bool)args[0];
			m_TwoHandWeaponAttack.firstPersonAttack = (bool)args[0];
			break;
		}
	}

	public bool IsSwitchWeapon()
	{
		return m_SwitchWeapon;
	}

	public IWeapon GetHoldWeapon()
	{
		if (null != m_EquipCmpt && m_EquipCmpt._Weapons != null)
		{
			return m_EquipCmpt._Weapons.Find((IWeapon ret) => ret != null && !ret.Equals(null) && ret.HoldReady);
		}
		return null;
	}

	public List<IWeapon> GetWeaponList()
	{
		retList.Clear();
		if (null != m_EquipCmpt)
		{
			retList.AddRange(m_EquipCmpt._Weapons);
		}
		bool flag = null == m_HeavyEquipmentCtrl.heavyEquipment;
		if (flag)
		{
			for (int i = 0; i < retList.Count; i++)
			{
				if (retList[i] is PeSword)
				{
					flag = false;
					break;
				}
			}
		}
		if (null != m_Gloves && flag)
		{
			retList.Add(m_Gloves);
		}
		return retList;
	}

	public List<IWeapon> GetCanUseWeaponList(PeEntity entity)
	{
		retList.Clear();
		if (null != m_EquipCmpt)
		{
			retList.AddRange(m_EquipCmpt._Weapons);
		}
		for (int i = 0; i < retList.Count; i++)
		{
			if (PE.WeaponCanCombat(entity, retList[i]))
			{
				return retList;
			}
		}
		retList.Clear();
		bool flag = null == m_HeavyEquipmentCtrl.heavyEquipment;
		if (flag)
		{
			for (int j = 0; j < retList.Count; j++)
			{
				if (retList[j] is PeSword)
				{
					flag = false;
					break;
				}
			}
		}
		if (null != m_Gloves && flag)
		{
			retList.Add(m_Gloves);
		}
		return retList;
	}

	public void UpdateMoveDir(Vector3 moveDir, Vector3 localDir)
	{
		m_ParachuteAction.SetMoveDir(moveDir.normalized);
		m_GliderAction.SetMoveDir(localDir.normalized);
		m_JetPackAction.SetMoveDir(moveDir);
	}

	public void SetEquipment(PEEquipment equipment, bool isPutOn)
	{
		ActiveGloves(active: false, immediately: true);
		Type type = equipment.GetType();
		foreach (Type key in m_SetEquipmentFunc.Keys)
		{
			if (key == type)
			{
				if (isPutOn)
				{
					m_SetEquipmentFunc[key](equipment);
				}
				else
				{
					m_SetEquipmentFunc[key](null);
				}
			}
		}
		ResetAccuracy(equipment);
		if (null != m_Anim && equipment.m_EquipAnim != string.Empty)
		{
			m_Anim.SetBool(equipment.m_EquipAnim, isPutOn);
		}
		CheckGloves();
	}

	private void ResetAccuracy(PEEquipment equipment)
	{
		if (!isMainPlayer && base.Entity.proto != EEntityProto.Monster)
		{
			PEAimAbleEquip pEAimAbleEquip = equipment as PEAimAbleEquip;
			if (null != pEAimAbleEquip)
			{
				pEAimAbleEquip.m_AimAttr.m_FireStability *= accuracyScale;
				pEAimAbleEquip.m_AimAttr.m_AccuracyMin += accuracyRangeAdd;
				pEAimAbleEquip.m_AimAttr.m_AccuracyMax += accuracyRangeAdd;
				pEAimAbleEquip.m_AimAttr.m_AccuracyPeriod *= 1f / accuracyScale;
				pEAimAbleEquip.m_AimAttr.m_AccuracyDiffusionRate += 2f;
				pEAimAbleEquip.m_AimAttr.m_AccuracyShrinkSpeed *= accuracyScale;
			}
		}
	}

	public void SetWeapon(PEEquipment equip)
	{
		m_Weapon = equip as IWeapon;
		m_WeaponID = ((m_Weapon == null || m_Weapon.ItemObj == null) ? (-1) : m_Weapon.ItemObj.instanceId);
	}

	private void ResetWeapon()
	{
		if (m_WeaponID != -1 && null != m_ActiveableEquipment && m_ActiveableEquipment.m_ItemObj != null && m_ActiveableEquipment.m_ItemObj.instanceId == m_WeaponID)
		{
			m_Weapon = m_ActiveableEquipment as IWeapon;
		}
		else if (null != m_Gloves && null != m_MotionMgr && m_MotionMgr.IsActionRunning(m_Gloves.m_HandChangeAttr.m_ActiveActionType))
		{
			m_Weapon = m_Gloves;
		}
	}

	private void SetSword(PEEquipment sword)
	{
		if (null != m_Sword && null != sword)
		{
			m_HandChangeHold.handChangeEquipment = null;
			m_SwordAttack.sword = null;
			m_ActiveableEquipment = null;
		}
		m_Sword = sword as PeSword;
		m_HandChangeHold.handChangeEquipment = m_Sword;
		m_SwordAttack.sword = m_Sword;
		m_ActiveableEquipment = m_Sword;
	}

	private void SetTwoHandWeapon(PEEquipment weapon)
	{
		m_TwoHandWeapon = weapon as PETwoHandWeapon;
		m_TwoHandWeaponHold.twoHandWeapon = m_TwoHandWeapon;
		m_TwoHandWeaponAttack.sword = m_TwoHandWeapon;
		m_ActiveableEquipment = m_TwoHandWeapon;
	}

	private void SetAxe(PEEquipment equipment)
	{
		m_Axe = equipment as PEAxe;
		m_Fell.m_Axe = m_Axe;
		m_HandChangeHold.handChangeEquipment = m_Axe;
		m_ActiveableEquipment = m_Axe;
	}

	private void SetChainSaw(PEEquipment chainSaw)
	{
		SetAxe(chainSaw);
		m_HeavyEquipmentCtrl.heavyEquipment = chainSaw as IHeavyEquipment;
	}

	private void SetJetPack(PEEquipment jetPack)
	{
		m_JetPackAction.jetPack = jetPack as PEJetPack;
	}

	public void SetJetPackLogic(PEJetPackLogic jetPackLogic)
	{
		m_JetPackAction.jetPackLogic = jetPackLogic;
		if (null != jetPackLogic)
		{
			m_RechangeableEquipments.Add(jetPackLogic);
		}
	}

	private void SetParachute(PEEquipment parachute)
	{
		m_Parachute = parachute as PEParachute;
		m_ParachuteAction.parachute = m_Parachute;
	}

	private void SetGlider(PEEquipment glider)
	{
		m_Glider = glider as PEGlider;
		m_GliderAction.glider = m_Glider;
	}

	private void SetWaterPitcher(PEEquipment waterPitcher)
	{
		m_WaterPitcher = waterPitcher as PEWaterPitcher;
		m_DrawWater.waterPitcher = m_WaterPitcher;
	}

	private void SetWaterPump(PEEquipment equipment)
	{
		PEWaterPump pEWaterPump = equipment as PEWaterPump;
		m_GunHold.aimAbleEquip = pEWaterPump;
		m_PumpWater.waterPump = pEWaterPump;
		m_ActiveableEquipment = pEWaterPump;
	}

	private void SetSheild(PEEquipment sheild)
	{
		m_Sheild = sheild as PESheild;
		m_SheildHold.sheild = m_Sheild;
	}

	public void SetEnergySheild(PEEnergySheildLogic energySheild)
	{
		m_EnergySheild = energySheild;
		if (null != m_EnergySheild)
		{
			m_RechangeableEquipments.Add(m_EnergySheild);
		}
	}

	private void SetGun(PEEquipment gun)
	{
		m_Gun = gun as PEGun;
		m_GunFire.gun = m_Gun;
		m_GunHold.aimAbleEquip = m_Gun;
		m_GunReload.gun = m_Gun;
		m_GunMelee.gun = m_Gun;
		m_ActiveableEquipment = m_Gun;
	}

	public void SetEnergyGunLogic(PEEnergyGunLogic gun)
	{
		m_GunFire.energyGunLogic = gun;
		if (null != gun)
		{
			m_RechangeableEquipments.Add(gun);
		}
	}

	private void SetBow(PEEquipment bow)
	{
		m_Bow = bow as PEBow;
		m_BowHold.bow = m_Bow;
		m_BowReload.bow = m_Bow;
		m_BowShoot.bow = m_Bow;
		m_ActiveableEquipment = m_Bow;
	}

	private void SetDigTool(PEEquipment digTool)
	{
		m_DigTool = digTool as PEDigTool;
		m_HandChangeHold.handChangeEquipment = m_DigTool;
		m_DigTerrain.digTool = m_DigTool;
		m_ActiveableEquipment = m_DigTool;
	}

	private void SetGrenade(PEEquipment equipment)
	{
		PEGrenade pEGrenade = equipment as PEGrenade;
		m_GunHold.aimAbleEquip = pEGrenade;
		m_ThrowGrenade.grenade = pEGrenade;
		m_ActiveableEquipment = pEGrenade;
	}

	private void SetFlashLight(PEEquipment flashLight)
	{
		m_FlashLightAction.flashLight = flashLight as PEFlashLight;
	}

	private void SetCrusher(PEEquipment crusher)
	{
		m_DigTool = crusher as PEDigTool;
		m_AimEquipHold.aimAbleEquip = m_DigTool;
		m_DigTerrain.digTool = m_DigTool;
		m_ActiveableEquipment = m_DigTool;
		m_HeavyEquipmentCtrl.heavyEquipment = m_DigTool as IHeavyEquipment;
	}

	private void SetRopeGun(PEEquipment ropeGun)
	{
		m_RopeGunAction.ropeGun = ropeGun as PERopeGun;
		m_GunHold.aimAbleEquip = m_RopeGunAction.ropeGun;
		m_ActiveableEquipment = m_RopeGunAction.ropeGun;
	}

	public bool WeaponCanUse(IWeapon weapon)
	{
		if (!isMainPlayer && null != m_NPC && !m_NPC.HasConsume)
		{
			return true;
		}
		PeSword peSword = weapon as PeSword;
		if (null != peSword)
		{
			return true;
		}
		PEGun pEGun = weapon as PEGun;
		if (null != pEGun)
		{
			if (m_GunFire.m_IgnoreItem)
			{
				return true;
			}
			if (pEGun.m_AmmoType == AmmoType.Bullet)
			{
				return pEGun.durability > float.Epsilon && (pEGun.magazineValue > float.Epsilon || null == m_Package || m_Package.GetItemCount(pEGun.curItemID) > 0);
			}
			return pEGun.durability > float.Epsilon && (pEGun.magazineValue > float.Epsilon || base.Entity.GetAttribute(AttribType.Energy) > float.Epsilon);
		}
		PEBow pEBow = weapon as PEBow;
		if (null != pEBow)
		{
			if (m_BowShoot.m_IgnoreItem)
			{
				return true;
			}
			return (pEBow.durability > float.Epsilon && null == m_Package) || m_Package.GetItemCount(pEBow.curItemID) > 0;
		}
		return true;
	}

	private bool EquipmentCanUse()
	{
		if (null != m_ActiveableEquipment)
		{
			if (m_ActiveableEquipment is IWeapon)
			{
				return WeaponCanUse(m_ActiveableEquipment as IWeapon);
			}
			return m_ActiveableEquipment.durability > float.Epsilon;
		}
		return false;
	}

	public bool CheckEquipmentDurability()
	{
		if (null != m_ActiveableEquipment)
		{
			return m_ActiveableEquipment.durability > float.Epsilon;
		}
		return false;
	}

	public bool CheckEquipmentAmmunition()
	{
		if (!(m_ActiveableEquipment is IWeapon))
		{
			return true;
		}
		IWeapon weapon = m_ActiveableEquipment as IWeapon;
		PEGun pEGun = weapon as PEGun;
		if (null != pEGun)
		{
			if (pEGun.m_AmmoType == AmmoType.Bullet)
			{
				return null != m_Package && m_Package.GetItemCount(pEGun.curItemID) > 0;
			}
			return pEGun.magazineValue > float.Epsilon || base.Entity.GetAttribute(AttribType.Energy) > float.Epsilon;
		}
		PEBow pEBow = weapon as PEBow;
		if (null != pEBow)
		{
			return null != m_Package && m_Package.GetItemCount(pEBow.curItemID) > 0;
		}
		return true;
	}

	public bool IsWeaponActive()
	{
		if (null != m_ActiveableEquipment && m_ActiveableEquipment is IWeapon)
		{
			return m_MotionMgr.IsActionRunning(m_ActiveableEquipment.m_HandChangeAttr.m_ActiveActionType);
		}
		return false;
	}

	public void SetTarget(SkEntity skEntity)
	{
		m_SwordAttack.targetEntity = skEntity;
		m_TwoHandWeaponAttack.targetEntity = skEntity;
		m_BowShoot.targetEntity = skEntity;
		m_ThrowGrenade.targetEntity = skEntity;
		m_GunFire.targetEntity = skEntity;
		m_GunMelee.targetEntity = skEntity;
	}

	public void SwordAttack(Vector3 dir, int attackModeIndex = 0, int time = 0)
	{
		if (null != m_SwordAttack.sword && m_SwordAttack.sword == m_HandChangeHold.handChangeEquipment && m_TwoHandWeapon == null)
		{
			PEActionParamVVNN param = PEActionParamVVNN.param;
			param.vec1 = m_Trans.position;
			param.vec2 = dir;
			param.n1 = time;
			param.n2 = attackModeIndex;
			m_MotionMgr.DoAction(PEActionType.SwordAttack, param);
		}
	}

	public void TwoHandWeaponAttack(Vector3 dir, int attackModeIndex = 0, int time = 0)
	{
		if (null != m_TwoHandWeapon)
		{
			PEActionParamVVNN param = PEActionParamVVNN.param;
			param.vec1 = m_Trans.position;
			param.vec2 = dir;
			param.n1 = time;
			param.n2 = attackModeIndex;
			m_MotionMgr.DoAction(PEActionType.TwoHandSwordAttack, param);
		}
	}

	public void ActiveWeapon(bool active)
	{
		if (null != m_WaterPitcher)
		{
			return;
		}
		if (null != m_ActiveableEquipment)
		{
			if (EquipmentCanUse())
			{
				if (m_HandChangeHold.handChangeEquipment == m_Gloves && motionMgr.IsActionRunning(PEActionType.EquipmentHold))
				{
					motionMgr.EndImmediately(PEActionType.SwordAttack);
					motionMgr.EndImmediately(PEActionType.EquipmentHold);
				}
				else
				{
					ActiveEquipment(m_ActiveableEquipment, active);
				}
			}
			else
			{
				if (m_ActiveableEquipment != m_Axe || m_HandChangeHold.handChangeEquipment == m_Axe)
				{
					ActiveEquipment(m_ActiveableEquipment, active: false);
				}
				ActiveGloves(active);
			}
		}
		else
		{
			ActiveGloves(active);
		}
	}

	private void ActiveGloves(bool active, bool immediately = false)
	{
		if (!(null != m_Gloves))
		{
			return;
		}
		if (active)
		{
			if (m_HandChangeHold.handChangeEquipment != m_Gloves)
			{
				m_HandChangeHold.handChangeEquipment = null;
				m_HandChangeHold.handChangeEquipment = m_Gloves;
			}
			if (m_SwordAttack.sword != m_Gloves)
			{
				m_SwordAttack.sword = null;
				m_SwordAttack.sword = m_Gloves;
			}
		}
		else
		{
			if (m_HandChangeHold.handChangeEquipment == m_Gloves && null != m_ActiveableEquipment && m_ActiveableEquipment.m_HandChangeAttr.m_ActiveActionType == PEActionType.EquipmentHold)
			{
				m_HandChangeHold.handChangeEquipment = null;
				m_HandChangeHold.handChangeEquipment = m_ActiveableEquipment;
			}
			if (m_SwordAttack.sword == m_Gloves)
			{
				m_SwordAttack.sword = null;
			}
			if (null != m_Sword)
			{
				m_SwordAttack.sword = m_Sword;
			}
		}
		ActiveEquipment(m_Gloves, active, immediately);
	}

	private void ActiveEquipment(PEHoldAbleEquipment equipment, bool active, bool immediately = false)
	{
		if (active)
		{
			if (!m_MotionMgr.IsActionRunning(equipment.m_HandChangeAttr.m_ActiveActionType) && m_MotionMgr.DoAction(equipment.m_HandChangeAttr.m_ActiveActionType))
			{
				m_Weapon = equipment as IWeapon;
				if (m_Weapon != null && equipment.m_ItemObj != null)
				{
					m_WeaponID = equipment.m_ItemObj.instanceId;
				}
				else
				{
					m_WeaponID = -1;
				}
			}
		}
		else if ((immediately && m_MotionMgr.EndImmediately(equipment.m_HandChangeAttr.m_ActiveActionType)) || (m_MotionMgr.IsActionRunning(equipment.m_HandChangeAttr.m_ActiveActionType) && m_MotionMgr.DoAction(equipment.m_HandChangeAttr.m_UnActiveActionType)))
		{
			m_Weapon = null;
			m_WeaponID = -1;
		}
	}

	public void ActiveWeapon(PEHoldAbleEquipment handChangeEquipment, bool active, bool immediately = false)
	{
		if (null != m_Gloves && handChangeEquipment == m_Gloves)
		{
			ActiveGloves(active);
			ActiveEquipment(m_Gloves, active);
			return;
		}
		if (null != m_Gloves && Weapon == m_Gloves)
		{
			ActiveGloves(active: false);
		}
		if (!(null != handChangeEquipment))
		{
			return;
		}
		if (immediately)
		{
			if (active)
			{
				if (!m_MotionMgr.IsActionRunning(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType))
				{
					m_MotionMgr.DoActionImmediately(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType);
					m_Weapon = handChangeEquipment as IWeapon;
					if (m_Weapon != null && handChangeEquipment.m_ItemObj != null)
					{
						m_WeaponID = handChangeEquipment.m_ItemObj.instanceId;
					}
					else
					{
						m_WeaponID = -1;
					}
				}
			}
			else if (m_MotionMgr.IsActionRunning(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType))
			{
				m_MotionMgr.EndImmediately(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType);
				m_WeaponID = -1;
				m_Weapon = null;
			}
		}
		else if (active)
		{
			if (!m_MotionMgr.IsActionRunning(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType) && m_MotionMgr.DoAction(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType))
			{
				m_Weapon = handChangeEquipment as IWeapon;
				if (m_Weapon != null && handChangeEquipment.m_ItemObj != null)
				{
					m_WeaponID = handChangeEquipment.m_ItemObj.instanceId;
				}
				else
				{
					m_WeaponID = -1;
				}
			}
		}
		else if (m_MotionMgr.IsActionRunning(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType) && m_MotionMgr.DoAction(handChangeEquipment.m_HandChangeAttr.m_UnActiveActionType))
		{
			m_Weapon = null;
			m_WeaponID = -1;
		}
	}

	public void HoldSheild(bool hold)
	{
		if (null != m_Sheild)
		{
			if (hold)
			{
				m_MotionMgr.DoAction(PEActionType.HoldShield);
			}
			else
			{
				m_MotionMgr.EndImmediately(PEActionType.HoldShield);
			}
		}
	}

	public bool SwitchHoldWeapon(IWeapon oldWeapon, IWeapon newWeapon)
	{
		if (m_SwitchWeapon)
		{
			return false;
		}
		if (oldWeapon != null && !oldWeapon.Equals(null) && newWeapon != null && !newWeapon.Equals(null))
		{
			PEHoldAbleEquipment pEHoldAbleEquipment = oldWeapon as PEHoldAbleEquipment;
			if (null != pEHoldAbleEquipment && m_MotionMgr.DoAction(pEHoldAbleEquipment.m_HandChangeAttr.m_UnActiveActionType))
			{
				m_SwitchWeapon = true;
				m_PutOnNewWeapon = false;
				m_OldWeapon = oldWeapon as PEHoldAbleEquipment;
				m_NewWeapon = newWeapon as PEHoldAbleEquipment;
				return true;
			}
		}
		else
		{
			Debug.LogError("SwitchHoldWeapon is null");
		}
		return false;
	}

	public void Reload()
	{
		if (null != m_Gun)
		{
			PEActionParamN param = PEActionParamN.param;
			param.n = m_Gun.curAmmoItemIndex;
			m_MotionMgr.DoAction(PEActionType.GunReload, param);
		}
	}

	public float GetAimPointScale()
	{
		if (null != m_Gun)
		{
			return m_GunHold.GetAimPointScale();
		}
		if (null != m_Bow)
		{
			return m_BowHold.GetAimPointScale();
		}
		return 0f;
	}

	private void CheckGloves()
	{
		if (null == m_Gloves && null != m_View && m_View.hasView && null != m_View.GetModelTransform("mountMain"))
		{
			UnityEngine.Object @object = AssetsLoader.Instance.LoadPrefabImm("Prefab/Item/Equip/Weapon/Other/Gloves");
			if (null != @object)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
				if (null != gameObject)
				{
					m_Gloves = gameObject.GetComponent<PEGloves>();
					m_Gloves.InitEquipment(base.Entity, null);
				}
			}
		}
		if (null != m_Gloves && m_HeavyEquipmentCtrl.heavyEquipment == null)
		{
			if (m_HandChangeHold.handChangeEquipment == null)
			{
				m_HandChangeHold.handChangeEquipment = m_Gloves;
			}
			if (m_SwordAttack.sword == null)
			{
				m_SwordAttack.sword = m_Gloves;
			}
			m_MotionMgr.EndImmediately(PEActionType.SwordAttack);
			m_MotionMgr.EndImmediately(PEActionType.Fell);
			m_MotionMgr.EndImmediately(PEActionType.Dig);
			m_MotionMgr.EndImmediately(m_Gloves.m_HandChangeAttr.m_ActiveActionType);
		}
	}

	private void DeletGloves()
	{
		if (null != m_Gloves)
		{
			if (m_Sword == m_Gloves)
			{
				m_Sword = null;
			}
			if (m_HandChangeHold.handChangeEquipment == m_Gloves)
			{
				m_HandChangeHold.handChangeEquipment = null;
			}
			if (m_SwordAttack.sword == m_Gloves)
			{
				m_SwordAttack.sword = null;
			}
			if (null != m_View)
			{
				m_View.DetachObject(m_Gloves.gameObject);
			}
			UnityEngine.Object.Destroy(m_Gloves.gameObject);
			m_Gloves = null;
		}
	}

	private void OnSheildReduce()
	{
		if (null != m_EnergySheild)
		{
			m_EnergySheild.lastUsedTime = Time.time;
		}
	}

	private void OnActiveEquipment()
	{
		if (this.OnActiveWeapon != null)
		{
			this.OnActiveWeapon();
		}
	}

	private void OnDeactiveEquipment()
	{
		if (this.OnDeactiveWeapon != null)
		{
			this.OnDeactiveWeapon();
		}
	}

	private void OnActionEnd(PEActionType type)
	{
		if (Weapon != null)
		{
			PEHoldAbleEquipment pEHoldAbleEquipment = m_Weapon as PEHoldAbleEquipment;
			if (null != pEHoldAbleEquipment && pEHoldAbleEquipment.m_HandChangeAttr.m_ActiveActionType == type)
			{
				m_Weapon = null;
				m_WeaponID = -1;
			}
		}
	}
}
