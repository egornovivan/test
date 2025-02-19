using System;
using System.Collections.Generic;
using System.IO;
using Pathea.Projectile;
using SkillSystem;
using UnityEngine;
using WhiteCat;

namespace Pathea;

public class TowerCmpt : PeCmpt, ITowerOpration, IPeMsg
{
	private PEBarrelController m_Barrel;

	private PeTrans m_Trans;

	private SkEntity m_SkEntity;

	private ECostType m_CostType;

	private int m_ConsumeItem;

	private int m_ConsumeCost;

	private int m_ConsumeCount;

	private int m_ConsumeCountMax;

	private int m_ConsumeEnergy;

	private int m_ConsumeEnergyCost;

	private int m_ConsumeEnergyMax;

	private int m_SkillID;

	private bool m_NeedVoxel = true;

	private bool m_OnBlock;

	private Transform m_Target;

	private BillBoard m_BillBoard;

	private List<TowerEffect> m_TowerEffect;

	public ECostType CostType
	{
		get
		{
			return m_CostType;
		}
		set
		{
			m_CostType = value;
		}
	}

	public int ConsumeItem
	{
		get
		{
			return m_ConsumeItem;
		}
		set
		{
			m_ConsumeItem = value;
		}
	}

	public int ConsumeCost
	{
		get
		{
			return m_ConsumeCost;
		}
		set
		{
			m_ConsumeCost = value;
		}
	}

	public int ConsumeCountMax
	{
		get
		{
			return m_ConsumeCountMax;
		}
		set
		{
			m_ConsumeCountMax = value;
		}
	}

	public int ConsumeEnergyCost
	{
		get
		{
			return m_ConsumeEnergyCost;
		}
		set
		{
			m_ConsumeEnergyCost = value;
		}
	}

	public int ConsumeEnergyMax
	{
		get
		{
			return m_ConsumeEnergyMax;
		}
		set
		{
			m_ConsumeEnergyMax = value;
		}
	}

	public int SkillID
	{
		get
		{
			return m_SkillID;
		}
		set
		{
			m_SkillID = value;
		}
	}

	public bool NeedVoxel
	{
		get
		{
			return m_NeedVoxel;
		}
		set
		{
			m_NeedVoxel = value;
		}
	}

	public int ConsumeCount
	{
		get
		{
			return m_ConsumeCount;
		}
		set
		{
			m_ConsumeCount = value;
			m_ConsumeCount = Mathf.Clamp(m_ConsumeCount, 0, m_ConsumeCountMax);
		}
	}

	public int ConsumeEnergy
	{
		get
		{
			return m_ConsumeEnergy;
		}
		set
		{
			m_ConsumeEnergy = value;
			m_ConsumeEnergy = Mathf.Clamp(m_ConsumeEnergy, 0, m_ConsumeEnergyMax);
		}
	}

	public Transform Target
	{
		get
		{
			return m_Target;
		}
		set
		{
			if (m_Target != value)
			{
				m_Target = value;
				if (m_Barrel != null)
				{
					m_Barrel.AimTarget = m_Target;
				}
			}
		}
	}

	public float ChassisY => (!(null == m_Barrel)) ? m_Barrel.ChassisY : 0f;

	public Vector3 PitchEuler => (!(null == m_Barrel)) ? m_Barrel.PitchEuler : Vector3.zero;

	public bool IsEnable
	{
		get
		{
			if (m_NeedVoxel && !m_OnBlock)
			{
				return false;
			}
			return true;
		}
	}

	public int ItemID => m_ConsumeItem;

	public int ItemCount
	{
		get
		{
			return m_ConsumeCount;
		}
		set
		{
			ConsumeCount = value;
		}
	}

	public int ItemCountMax => m_ConsumeCountMax;

	public int EnergyCount
	{
		get
		{
			return m_ConsumeEnergy;
		}
		set
		{
			ConsumeEnergy = value;
		}
	}

	public int EnergyCountMax => m_ConsumeEnergyMax;

	public ECostType ConsumeType => m_CostType;

	public event Action<float> onConsumeChange;

	public void ApplyChassis(float rotY)
	{
		if (null != m_Barrel)
		{
			m_Barrel.ApplyChassis(rotY);
		}
	}

	public void ApplyPitchEuler(Vector3 angleEuler)
	{
		if (null != m_Barrel)
		{
			m_Barrel.ApplyPitchEuler(angleEuler);
		}
	}

	public bool HaveCost()
	{
		switch (m_CostType)
		{
		case ECostType.None:
			return true;
		case ECostType.Item:
			if (m_ConsumeCount - m_ConsumeCost >= 0)
			{
				return true;
			}
			return false;
		case ECostType.Energy:
			if (m_ConsumeEnergy - m_ConsumeEnergyCost >= 0)
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	public void Fire(SkEntity target)
	{
		if (HaveCost() && m_SkEntity != null && m_SkillID > 0)
		{
			Cost();
			m_SkEntity.StartSkill(target, m_SkillID);
		}
	}

	public bool IsSkillRunning()
	{
		if (m_SkEntity != null)
		{
			return m_SkEntity.IsSkillRunning(m_SkillID);
		}
		return false;
	}

	public bool Angle(Vector3 position, float angle)
	{
		if (m_Barrel != null)
		{
			return m_Barrel.Angle(position, angle);
		}
		return false;
	}

	public bool PitchAngle(Vector3 position, float angle)
	{
		if (m_Barrel != null)
		{
			return m_Barrel.PitchAngle(position, angle);
		}
		return false;
	}

	public bool CanAttack(Vector3 position, Transform target = null)
	{
		if (m_Barrel != null)
		{
			return m_Barrel.CanAttack(position, target);
		}
		return false;
	}

	public bool EstimatedAttack(Vector3 position, Transform target = null)
	{
		if (m_Barrel != null)
		{
			return m_Barrel.EstimatedAttack(position, target);
		}
		return false;
	}

	public bool Evaluate(Vector3 position)
	{
		if (m_Barrel != null)
		{
			return m_Barrel.Evaluate(position);
		}
		return false;
	}

	private void Cost()
	{
		if (m_CostType == ECostType.Item)
		{
			ConsumeCount -= m_ConsumeCost;
		}
		if (m_CostType == ECostType.Energy)
		{
			ConsumeEnergy -= m_ConsumeEnergyCost;
		}
		if (this.onConsumeChange != null)
		{
			this.onConsumeChange((m_CostType != ECostType.Item) ? m_ConsumeEnergyCost : m_ConsumeCost);
		}
	}

	private bool CheckOnBuildTerrain()
	{
		if (m_Trans == null)
		{
			return false;
		}
		float s_Scale = BSBlock45Data.s_Scale;
		for (int i = -1; i <= 0; i++)
		{
			for (int j = -1; j <= 0; j++)
			{
				Vector3 vector = m_Trans.trans.TransformPoint((float)i * s_Scale, 0f - s_Scale, (float)j * s_Scale);
				int x = Mathf.FloorToInt(vector.x * (float)BSBlock45Data.s_ScaleInverted);
				int y = Mathf.FloorToInt(vector.y * (float)BSBlock45Data.s_ScaleInverted);
				int z = Mathf.FloorToInt(vector.z * (float)BSBlock45Data.s_ScaleInverted);
				if (Block45Man.self.DataSource.SafeRead(x, y, z).blockType >> 2 == 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void Start()
	{
		base.Start();
		m_Trans = GetComponent<PeTrans>();
		m_SkEntity = GetComponent<SkEntity>();
		base.Entity.peSkEntity.onHpChange += OnHpChange;
		base.Entity.peSkEntity.onHpReduce += OnDamage;
		base.Entity.peSkEntity.attackEvent += OnAttack;
		base.Entity.peSkEntity.onSkillEvent += OnSkillTarget;
		m_TowerEffect = new List<TowerEffect>();
		TowerProtoDb.Item item = TowerProtoDb.Get(base.Entity.ProtoID);
		if (item != null && item.effects != null && item.effects.Count > 0)
		{
			for (int i = 0; i < item.effects.Count; i++)
			{
				m_TowerEffect.Add(new TowerEffect(item.effects[i].hpPercent, item.effects[i].effectID, item.effects[i].audioID));
			}
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (Time.frameCount % 30 != 0)
		{
			return;
		}
		m_OnBlock = CheckOnBuildTerrain();
		if (m_BillBoard != null)
		{
			if (m_NeedVoxel && !m_OnBlock)
			{
				m_BillBoard.gameObject.SetActive(value: true);
			}
			else
			{
				m_BillBoard.gameObject.SetActive(value: false);
			}
		}
		if (base.Entity.hasView)
		{
			for (int i = 0; i < m_TowerEffect.Count; i++)
			{
				m_TowerEffect[i].ActivateEffect(base.Entity);
			}
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (m_TowerEffect != null && m_TowerEffect.Count > 0)
		{
			for (int i = 0; i < m_TowerEffect.Count; i++)
			{
				m_TowerEffect[i].Destroy();
			}
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write(m_NeedVoxel);
		w.Write(m_SkillID);
		w.Write((int)m_CostType);
		w.Write(m_ConsumeItem);
		w.Write(m_ConsumeCost);
		w.Write(m_ConsumeCount);
		w.Write(m_ConsumeCountMax);
		w.Write(m_ConsumeEnergy);
		w.Write(m_ConsumeEnergyCost);
		w.Write(m_ConsumeEnergyMax);
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		m_NeedVoxel = r.ReadBoolean();
		m_SkillID = r.ReadInt32();
		m_CostType = (ECostType)r.ReadInt32();
		m_ConsumeItem = r.ReadInt32();
		m_ConsumeCost = r.ReadInt32();
		m_ConsumeCount = r.ReadInt32();
		m_ConsumeCountMax = r.ReadInt32();
		m_ConsumeEnergy = r.ReadInt32();
		m_ConsumeEnergyCost = r.ReadInt32();
		m_ConsumeEnergyMax = r.ReadInt32();
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
		{
			BiologyViewRoot biologyViewRoot = (BiologyViewRoot)args[1];
			m_Barrel = biologyViewRoot.barrelController;
			m_BillBoard = biologyViewRoot.billBoard;
			if (m_Target != null)
			{
				m_Barrel.AimTarget = m_Target;
			}
			break;
		}
		case EMsg.View_Model_Destroy:
			if (m_TowerEffect != null && m_TowerEffect.Count > 0)
			{
				for (int i = 0; i < m_TowerEffect.Count; i++)
				{
					m_TowerEffect[i].Destroy();
				}
			}
			break;
		}
	}

	private void OnAttack(SkEntity skEntity, float damage)
	{
		PeEntity component = skEntity.GetComponent<PeEntity>();
		if (!(component != null) || !(component != base.Entity))
		{
			return;
		}
		float radius = ((!component.IsBoss) ? 64f : 128f);
		int playerID = (int)base.Entity.GetAttribute(AttribType.DefaultPlayerID);
		bool flag = false;
		if (GameConfig.IsMultiClient)
		{
			if (Singleton<ForceSetting>.Instance.GetForceType(playerID) == EPlayerType.Human)
			{
				flag = true;
			}
			int playerID2 = (int)component.GetAttribute(AttribType.DefaultPlayerID);
			if (Singleton<ForceSetting>.Instance.GetForceType(playerID2) == EPlayerType.Human)
			{
				List<PeEntity> entities = PeSingleton<EntityMgr>.Instance.GetEntities(component.position, radius, playerID2, isDeath: false, component);
				for (int i = 0; i < entities.Count; i++)
				{
					if (!entities[i].Equals(base.Entity) && entities[i].target != null)
					{
						entities[i].target.TransferHatred(base.Entity, damage);
					}
				}
			}
		}
		else if (Singleton<ForceSetting>.Instance.GetForceID(playerID) == 1)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		List<PeEntity> entities2 = PeSingleton<EntityMgr>.Instance.GetEntities(base.Entity.position, radius, playerID, isDeath: false, base.Entity);
		for (int j = 0; j < entities2.Count; j++)
		{
			if (!entities2[j].Equals(component) && entities2[j].target != null)
			{
				entities2[j].target.TransferHatred(component, damage);
			}
		}
	}

	private void OnHpChange(SkEntity entity, float damage)
	{
	}

	private void OnDamage(SkEntity entity, float damage)
	{
		if (null == base.Entity.peSkEntity || null == entity)
		{
			return;
		}
		PeEntity component = entity.GetComponent<PeEntity>();
		if (component == base.Entity)
		{
			return;
		}
		float radius = ((!component.IsBoss) ? 64f : 128f);
		int playerID = (int)base.Entity.peSkEntity.GetAttribute(91);
		bool flag = false;
		if (GameConfig.IsMultiClient)
		{
			if (Singleton<ForceSetting>.Instance.GetForceType(playerID) == EPlayerType.Human)
			{
				flag = true;
			}
			int playerID2 = (int)component.GetAttribute(AttribType.DefaultPlayerID);
			if (Singleton<ForceSetting>.Instance.GetForceType(playerID2) == EPlayerType.Human)
			{
				List<PeEntity> entities = PeSingleton<EntityMgr>.Instance.GetEntities(component.position, radius, playerID2, isDeath: false, component);
				for (int i = 0; i < entities.Count; i++)
				{
					if (!entities[i].Equals(base.Entity) && entities[i].target != null)
					{
						entities[i].target.TransferHatred(base.Entity, damage);
					}
				}
			}
		}
		else if (Singleton<ForceSetting>.Instance.GetForceID(playerID) == 1)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		List<PeEntity> entities2 = PeSingleton<EntityMgr>.Instance.GetEntities(base.Entity.peTrans.position, radius, playerID, isDeath: false, base.Entity);
		for (int j = 0; j < entities2.Count; j++)
		{
			if (!entities2[j].Equals(base.Entity) && entities2[j].target != null)
			{
				entities2[j].target.TransferHatred(component, damage);
			}
		}
	}

	private SkEntity GetCaster(SkEntity entity)
	{
		SkEntity skEntity = entity;
		SkProjectile skProjectile = skEntity as SkProjectile;
		if (skProjectile != null)
		{
			skEntity = skProjectile.GetSkEntityCaster();
		}
		CreationSkEntity creationSkEntity = skEntity as CreationSkEntity;
		if (creationSkEntity != null && creationSkEntity.driver != null)
		{
			skEntity = creationSkEntity.driver.skEntity;
		}
		return skEntity;
	}

	private void OnSkillTarget(SkEntity caster)
	{
		if (null == base.Entity.aliveEntity || null == caster)
		{
			return;
		}
		float num = 5f;
		int playerID = (int)base.Entity.aliveEntity.GetAttribute(91);
		SkEntity caster2 = GetCaster(caster);
		PeEntity component = caster.GetComponent<PeEntity>();
		if (component == base.Entity)
		{
			return;
		}
		float radius = ((!component.IsBoss) ? 64f : 128f);
		bool flag = false;
		if (GameConfig.IsMultiClient)
		{
			if (Singleton<ForceSetting>.Instance.GetForceType(playerID) == EPlayerType.Human)
			{
				flag = true;
			}
		}
		else if (Singleton<ForceSetting>.Instance.GetForceID(playerID) == 1)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		List<PeEntity> entities = PeSingleton<EntityMgr>.Instance.GetEntities(base.Entity.peTrans.position, radius, playerID, isDeath: false, base.Entity);
		for (int i = 0; i < entities.Count; i++)
		{
			if (!(entities[i] == null) && !entities[i].Equals(base.Entity) && entities[i].target != null)
			{
				entities[i].target.OnTargetSkill(component.skEntity);
			}
		}
	}
}
