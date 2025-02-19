using System;
using System.Collections.Generic;
using System.IO;
using Pathea.Effect;
using Pathea.Projectile;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class SkAliveEntity : PESkEntity, IPeCmpt, IPeMsg, ISkSubTerrain, IDigTerrain
{
	private const int Version = 3;

	private ViewCmpt m_View;

	private PeTrans m_Trans;

	private AnimatorCmpt m_Animator;

	private Motion_Beat m_Beat;

	private Motion_Equip m_MotionEquip;

	private Motion_Live m_Live;

	private MotionMgrCmpt m_MotionMgr;

	private bool m_Init;

	private Dictionary<Collider, List<Collider>> m_CollisionEntities;

	private SkEntity m_SkParent;

	private bool[] m_UseParentMasks;

	private PeEntity mEntity;

	public SkEntity SkParent
	{
		get
		{
			return m_SkParent;
		}
		set
		{
			m_SkParent = value;
			m_UseParentMasks = new bool[97];
			for (int i = 0; i < m_UseParentMasks.Length; i++)
			{
				m_UseParentMasks[i] = true;
			}
		}
	}

	public bool[] UseParentMasks => m_UseParentMasks;

	public PeEntity Entity
	{
		get
		{
			if (mEntity == null)
			{
				mEntity = GetComponent<PeEntity>();
			}
			return mEntity;
		}
	}

	public IntVector4 digPosType
	{
		get
		{
			if (null != m_MotionMgr)
			{
				if (m_MotionMgr.IsActionRunning(PEActionType.Dig) && null != m_MotionEquip)
				{
					Action_DigTerrain action = m_MotionMgr.GetAction<Action_DigTerrain>();
					if (action != null)
					{
						return new IntVector4(action.digPos, 1);
					}
				}
				else if (m_MotionMgr.IsActionRunning(PEActionType.SwordAttack))
				{
					Action_SwordAttack action2 = m_MotionMgr.GetAction<Action_SwordAttack>();
					if (action2 != null)
					{
						return new IntVector4(action2.GetHitPos(), 0);
					}
				}
				else
				{
					if (!m_MotionMgr.IsActionRunning(PEActionType.TwoHandSwordAttack))
					{
						if (Entity.peTrans != null)
						{
							return new IntVector4(Entity.peTrans.forwardCenter, 0);
						}
						return new IntVector4(Entity.position, 0);
					}
					Action_TwoHandWeaponAttack action3 = m_MotionMgr.GetAction<Action_TwoHandWeaponAttack>();
					if (action3 != null)
					{
						return new IntVector4(action3.GetHitPos(), 0);
					}
				}
			}
			return IntVector4.Zero;
		}
	}

	public GlobalTreeInfo treeInfo
	{
		get
		{
			if (null != m_MotionMgr)
			{
				if (m_MotionMgr.IsActionRunning(PEActionType.Fell))
				{
					Action_Fell action = m_MotionMgr.GetAction<Action_Fell>();
					if (action != null)
					{
						return action.treeInfo;
					}
				}
				if (null != m_Live && m_MotionMgr.IsActionRunning(PEActionType.Gather))
				{
					return m_Live.gather.treeInfo;
				}
			}
			return null;
		}
	}

	public event Action<int> evtOnBuffAdd;

	public event Action<int> evtOnBuffRemove;

	void IPeCmpt.Serialize(BinaryWriter w)
	{
		Serialize(w);
	}

	void IPeCmpt.Deserialize(BinaryReader r)
	{
		Deserialize(r);
	}

	string IPeCmpt.GetTypeName()
	{
		return GetType().Name;
	}

	public override void ApplyRepelEff(SkRuntimeInfo info)
	{
		base.ApplyRepelEff(info);
		if (info is SkInst inst)
		{
			ApplyForce(inst);
		}
		else
		{
			Debug.LogError("[ApplySpEff] Unsupported SkRuntimeInfo type");
		}
	}

	public override void OnBuffAdd(int buffId)
	{
		if (this.evtOnBuffAdd != null)
		{
			this.evtOnBuffAdd(buffId);
		}
	}

	public override void OnBuffRemove(int buffId)
	{
		if (this.evtOnBuffRemove != null)
		{
			this.evtOnBuffRemove(buffId);
		}
	}

	public override void CondTstFunc(SkFuncInOutPara funcInOut)
	{
		base.CondTstFunc(funcInOut);
		Entity.SendMsg(EMsg.Skill_CheckLoop, funcInOut);
		if (funcInOut._ret && IsController())
		{
			if (funcInOut._inst._target != null)
			{
				SendBLoop(funcInOut._inst.SkillID, funcInOut._inst._target.GetId(), funcInOut._ret);
			}
			else
			{
				SendBLoop(funcInOut._inst.SkillID, 0, funcInOut._ret);
			}
		}
		else
		{
			SetCondRet(funcInOut);
		}
	}

	public override void ApplyEmission(int emitId, SkRuntimeInfo info)
	{
		base.ApplyEmission(emitId, info);
		Singleton<ProjectileBuilder>.Instance.Register(emitId, base.transform, info);
	}

	public override void ApplyEff(int effId, SkRuntimeInfo info)
	{
		base.ApplyEff(effId, info);
		if (null != m_Trans)
		{
			Singleton<EffectBuilder>.Instance.RegisterEffectFromSkill(effId, info, m_Trans.existent);
		}
	}

	public override void ApplySe(int seId, SkRuntimeInfo info)
	{
		base.ApplySe(seId, info);
		if (m_Trans != null && !m_Trans.Equals(null))
		{
			AudioManager.instance.Create(m_Trans.position, seId);
		}
	}

	public override void ApplyCamEff(int camEffId, SkRuntimeInfo info)
	{
		base.ApplyCamEff(camEffId, info);
		PeCamera.ApplySkCameraEffect(camEffId, this);
	}

	public override Collider GetCollider(string name)
	{
		if (m_View is BiologyViewCmpt)
		{
			return (m_View as BiologyViewCmpt).GetModelCollider(name);
		}
		return null;
	}

	protected override void OnPutInPak(List<ItemToPack> itemsToPack)
	{
		base.OnPutInPak(itemsToPack);
		if (!(Entity.packageCmpt != null))
		{
			return;
		}
		foreach (ItemToPack item in itemsToPack)
		{
			if (PeGameMgr.IsMulti)
			{
				if (_net != null)
				{
					_net.RPCServer(EPacketType.PT_Test_AddItem, item._id, item._cnt);
				}
			}
			else
			{
				NpcPackageCmpt npcPackageCmpt = Entity.packageCmpt as NpcPackageCmpt;
				if (npcPackageCmpt != null)
				{
					npcPackageCmpt.AddToHandin(item._id, item._cnt);
				}
				else
				{
					Entity.packageCmpt.Add(item._id, item._cnt);
				}
			}
		}
	}

	public void SetUseParentMasks(AttribType type, bool value)
	{
		if (m_UseParentMasks != null && (int)type < m_UseParentMasks.Length)
		{
			m_UseParentMasks[(int)type] = value;
		}
	}

	private void Start()
	{
		if (!m_Init)
		{
			InitSkEntity();
		}
		Entity.AddMsgListener(this);
		m_CollisionEntities = new Dictionary<Collider, List<Collider>>();
		m_View = Entity.viewCmpt;
		m_Trans = Entity.peTrans;
		m_Animator = Entity.animCmpt;
		m_Beat = Entity.GetCmpt<Motion_Beat>();
		m_MotionMgr = Entity.motionMgr;
		m_MotionEquip = Entity.GetCmpt<Motion_Equip>();
		m_Live = Entity.GetCmpt<Motion_Live>();
		base.onHpChange += OnHpChange;
		base.onSkillEvent += OnTargetSkill;
		base.onTranslate += OnTranslate;
		Invoke("CheckInitAttr", 0.5f);
	}

	public virtual void Serialize(BinaryWriter w)
	{
		w.Write(3);
		_attribs.Serialize(w);
	}

	public virtual void Deserialize(BinaryReader r)
	{
		if (!m_Init)
		{
			InitSkEntity();
		}
		int num = r.ReadInt32();
		if (num == 2)
		{
			_attribs.DisableNumAttribsEvent();
			for (int i = 0; i < 96; i++)
			{
				IList<float> sums = _attribs.sums;
				int index = i;
				float value = r.ReadSingle();
				_attribs.raws[i] = value;
				sums[index] = value;
			}
			_attribs.EnableNumAttribsEvent();
			SetAttribute(AttribType.HPRecover, 0.01f);
		}
		else
		{
			_attribs.Deserialize(r);
		}
		if (num == 1)
		{
			int num2 = r.ReadInt32();
			m_InitBuffList = new int[num2];
			for (int j = 0; j < num2; j++)
			{
				m_InitBuffList[j] = r.ReadInt32();
			}
		}
		EntityProto entityProto = Entity.entityProto;
		if (entityProto == null)
		{
			return;
		}
		switch (entityProto.proto)
		{
		case EEntityProto.Monster:
		{
			MonsterProtoDb.Item item3 = MonsterProtoDb.Get(entityProto.protoId);
			if (item3 != null)
			{
				m_InitBuffList = item3.initBuff;
			}
			break;
		}
		case EEntityProto.Npc:
		{
			NpcProtoDb.Item item = NpcProtoDb.Get(entityProto.protoId);
			if (item != null)
			{
				m_InitBuffList = item.InFeildBuff;
			}
			break;
		}
		case EEntityProto.Player:
		{
			PlayerProtoDb.Item item2 = PlayerProtoDb.Get();
			if (item2 != null)
			{
				m_InitBuffList = item2.initBuff;
			}
			break;
		}
		case EEntityProto.RandomNpc:
		{
			PlayerProtoDb.Item randomNpc = PlayerProtoDb.GetRandomNpc();
			if (randomNpc != null)
			{
				m_InitBuffList = randomNpc.InFeildBuff;
			}
			break;
		}
		}
	}

	public void InitSkEntity()
	{
		m_Init = true;
		InitEntity(m_SkParent, m_UseParentMasks);
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		if (msg == EMsg.State_Revive)
		{
			DispatchReviveEvent();
			SetAutoBuffActive(active: true);
			base.isDead = false;
		}
	}

	public override void ApplyAnim(string anim, SkRuntimeInfo info)
	{
		base.ApplyAnim(anim, info);
		if (anim == "Knock")
		{
			if (info is SkInst skInst)
			{
				if (m_View != null && skInst._colInfo != null && skInst._colInfo.hitTrans != null && skInst._colInfo.hitTrans.GetComponent<Rigidbody>() != null)
				{
					RagdollHitInfo hitInfo = PE.CapsuleHitToRagdollHit(skInst._colInfo);
					if (m_View is BiologyViewCmpt)
					{
						(m_View as BiologyViewCmpt).ActivateRagdoll(hitInfo);
					}
				}
			}
			else
			{
				Debug.LogError("[ApplyAnim] Unsupported SkRuntimeInfo type");
			}
		}
		else if (m_Animator != null)
		{
			m_Animator.SetTrigger(anim);
		}
	}

	private void ApplyForce(SkInst inst)
	{
		if (null != m_Beat && PEUtil.CanDamage(inst.Caster, inst.Target))
		{
			m_Beat.Beat(inst.Target, (inst._colInfo == null) ? null : inst._colInfo.hitTrans, inst._forceDirection, inst._forceMagnitude);
		}
	}

	public override void OnHurtSb(SkInst inst, float dmg)
	{
		base.OnHurtSb(inst, dmg);
		if (m_MotionEquip != null && m_MotionEquip.Weapon != null && m_MotionEquip.Weapon.ItemObj != null)
		{
			Entity.SendMsg(EMsg.Battle_EquipAttack, m_MotionEquip.Weapon.ItemObj);
		}
	}

	public override void OnGetHurt(SkInst inst, float dmg)
	{
		base.OnGetHurt(inst, dmg);
		Entity.SendMsg(EMsg.Battle_BeAttacked, dmg, inst.Caster);
	}

	public override void GetCollisionInfo(out List<KeyValuePair<Collider, Collider>> colPairs)
	{
		colPairs = new List<KeyValuePair<Collider, Collider>>();
		foreach (KeyValuePair<Collider, List<Collider>> collisionEntity in m_CollisionEntities)
		{
			foreach (Collider item in collisionEntity.Value)
			{
				if (collisionEntity.Key != null && item != null)
				{
					colPairs.Add(new KeyValuePair<Collider, Collider>(collisionEntity.Key, item));
				}
			}
		}
	}

	private void EventFunc(string para)
	{
		Entity.SendMsg(EMsg.Skill_Event, para);
	}

	public override Transform GetTransform()
	{
		return (!(m_Trans != null)) ? null : m_Trans.existent;
	}

	private void OnHpChange(SkEntity caster, float hpChange)
	{
		if (null != Entity)
		{
			Entity.SendMsg(EMsg.Battle_HPChange, caster, hpChange);
		}
	}

	private void OnTargetSkill(SkEntity caster)
	{
		if (null != Entity)
		{
			Entity.SendMsg(EMsg.Battle_TargetSkill, caster);
		}
	}

	private void OnTranslate(Vector3 pos)
	{
		if (null != Entity)
		{
			Entity.SendMsg(EMsg.Trans_Pos_set, pos);
		}
	}

	public void OnDeathProcessBuff(SkEntity cur, SkEntity caster)
	{
		if (caster == null || cur == null)
		{
			return;
		}
		SkAliveEntity skAliveEntity = caster as SkAliveEntity;
		if (skAliveEntity == null)
		{
			if (caster is SkProjectile)
			{
				skAliveEntity = (caster as SkProjectile).GetSkEntityCaster() as SkAliveEntity;
			}
			if (skAliveEntity == null)
			{
				return;
			}
		}
		SkAliveEntity skAliveEntity2 = cur as SkAliveEntity;
		if (skAliveEntity2 == null)
		{
			return;
		}
		MonsterProtoDb.Item item = MonsterProtoDb.Get(skAliveEntity2.Entity.entityProto.protoId);
		if (item == null || item.deathBuff.Length == 0)
		{
			return;
		}
		if (skAliveEntity.Entity.NpcCmpt != null && skAliveEntity.Entity.NpcCmpt.Master != null && skAliveEntity.Entity.NpcCmpt.Master.Entity != null && skAliveEntity.Entity.NpcCmpt.Master.Entity.aliveEntity != null)
		{
			skAliveEntity = skAliveEntity.Entity.NpcCmpt.Master.Entity.aliveEntity;
		}
		if (PeGameMgr.IsMulti)
		{
			if (skAliveEntity.IsController())
			{
				string[] array = item.deathBuff.Split(',');
				int buffId = Convert.ToInt32(array[0]);
				List<int> list = new List<int>();
				List<float> list2 = new List<float>();
				for (int i = 1; i < array.Length; i += 2)
				{
					list.Add(Convert.ToInt32(array[i]));
					list2.Add(Convert.ToSingle(array[i + 1]));
				}
				if (list.Count > 0 && list2.Count > 0)
				{
					SkEntity.UnmountBuff(skAliveEntity, buffId);
					SkEntity.MountBuff(skAliveEntity, buffId, list, list2);
				}
			}
		}
		else
		{
			string[] array2 = item.deathBuff.Split(',');
			int buffId2 = Convert.ToInt32(array2[0]);
			List<int> list3 = new List<int>();
			List<float> list4 = new List<float>();
			for (int j = 1; j < array2.Length; j += 2)
			{
				list3.Add(Convert.ToInt32(array2[j]));
				list4.Add(Convert.ToSingle(array2[j + 1]));
			}
			if (list3.Count > 0 && list4.Count > 0)
			{
				SkEntity.UnmountBuff(skAliveEntity, buffId2);
				SkEntity.MountBuff(skAliveEntity, buffId2, list3, list4);
			}
		}
		skAliveEntity2.deathEvent -= OnDeathProcessBuff;
	}
}
