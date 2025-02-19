using System;
using System.Collections.Generic;
using Behave.Runtime;
using Pathea;
using UnityEngine;

public class BehaveGroup : MonoBehaviour, IAgent, IBehave
{
	private EntityGrp m_Grp;

	private PeEntity m_Leader;

	private int m_MaxCount;

	private List<PeEntity> m_Entities = new List<PeEntity>();

	private List<Vector3> m_Locals;

	private List<Vector3> m_LocalUse;

	public int atkMin => m_Grp._atkMin;

	public int atkMax => m_Grp._atkMax;

	public List<PeEntity> Entities => m_Entities;

	public float AlivePercent
	{
		get
		{
			int num = 0;
			for (int i = 0; i < m_Entities.Count; i++)
			{
				if (m_Entities[i] != null && !m_Entities[i].IsDeath())
				{
					num++;
				}
			}
			return (float)num / (float)m_MaxCount;
		}
	}

	public float EscapePercent
	{
		get
		{
			int num = 0;
			for (int i = 0; i < m_Entities.Count; i++)
			{
				if (m_Entities[i] != null && m_Entities[i].target != null && m_Entities[i].target.GetEscapeEnemyUnit() != null)
				{
					num++;
				}
			}
			return (float)num / (float)m_MaxCount;
		}
	}

	public PeEntity Leader
	{
		get
		{
			return m_Leader;
		}
		set
		{
			m_Leader = value;
		}
	}

	public Enemy EscapeEnemy
	{
		get
		{
			for (int i = 0; i < m_Entities.Count; i++)
			{
				if (!(m_Entities[i] == null) && m_Entities[i].target != null)
				{
					Enemy escapeEnemyUnit = m_Entities[i].target.GetEscapeEnemyUnit();
					if (escapeEnemyUnit != null)
					{
						return escapeEnemyUnit;
					}
				}
			}
			return null;
		}
	}

	public bool BehaveActive => true;

	public Vector3 FollowLeader(PeEntity entity)
	{
		if (Leader == null || entity == null)
		{
			return Vector3.zero;
		}
		CalculateLocal(entity);
		Vector3 position = entity.GroupLocal * 5f * entity.maxRadius;
		if (entity.Field == MovementField.Sky)
		{
			position += UnityEngine.Random.Range(-10f, 10f) * Vector3.up;
		}
		else if (entity.Field == MovementField.water)
		{
			Vector3 position2 = Leader.position;
			float num = VFVoxelWater.self.UpToWaterSurface(position2.x, position2.y, position2.z);
			if (num > 0f)
			{
				position += UnityEngine.Random.Range(-5f, Mathf.Max(0f, num - entity.maxHeight)) * Vector3.up;
			}
		}
		return Leader.tr.TransformPoint(position);
	}

	public Vector3 FollowEnemy(PeEntity entity, float radius)
	{
		if (Leader == null || entity == null || entity.attackEnemy == null)
		{
			return Vector3.zero;
		}
		CalculateLocal(entity, isForce: true);
		return entity.attackEnemy.modelTrans.TransformPoint(entity.GroupLocal * radius);
	}

	private void Awake()
	{
		m_Grp = GetComponent<EntityGrp>();
		if (m_Grp != null)
		{
			EntityGrp grp = m_Grp;
			grp.handlerMonsterCreated = (Action<PeEntity>)Delegate.Combine(grp.handlerMonsterCreated, new Action<PeEntity>(OnMemberCreated));
		}
	}

	private void Start()
	{
		InitLocals();
	}

	private void Update()
	{
		if (m_Leader == null || m_Leader.IsDeath() || !m_Leader.hasView)
		{
			m_Leader = m_Entities.Find((PeEntity ret) => ret != null && !ret.IsDeath() && ret.hasView);
		}
	}

	private void InitLocals(bool isGravity = true)
	{
		m_Locals = new List<Vector3>();
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				float x = i;
				float y = 0f;
				float z = j;
				m_Locals.Add(new Vector3(x, y, z));
			}
		}
		m_LocalUse = new List<Vector3>(m_Locals);
	}

	private void CalculateLocal(PeEntity entity, bool isForce = false)
	{
		if ((isForce || !(entity.GroupLocal != Vector3.zero)) && m_LocalUse != null && m_LocalUse.Count != 0)
		{
			Vector3 groupLocal = entity.GroupLocal;
			Vector3 item = (entity.GroupLocal = m_LocalUse[UnityEngine.Random.Range(0, m_LocalUse.Count)]);
			m_LocalUse.Remove(item);
			if (groupLocal != Vector3.zero)
			{
				m_LocalUse.Add(groupLocal);
			}
		}
	}

	public void SetBehavePath(string behavePath)
	{
		Behave.Runtime.Singleton<BTLauncher>.Instance.Instantiate(behavePath, this);
	}

	public void RegisterMember(PeEntity skEntity)
	{
		if (!m_Entities.Contains(skEntity))
		{
			skEntity.Group = this;
			m_Entities.Add(skEntity);
			m_MaxCount++;
		}
	}

	public void RemoveMember(PeEntity skEntity)
	{
		if (m_Entities.Contains(skEntity))
		{
			if (m_Leader != null && m_Leader.Equals(skEntity))
			{
				m_Leader = null;
			}
			skEntity.Group = null;
			m_Entities.Remove(skEntity);
			m_MaxCount--;
		}
	}

	public void PauseMemberBehave(bool value)
	{
		foreach (PeEntity entity in m_Entities)
		{
			if (entity != null)
			{
				BehaveCmpt component = entity.GetComponent<BehaveCmpt>();
				if (component != null)
				{
					component.Pause(value);
				}
			}
		}
	}

	public bool HasAttackEnemy()
	{
		foreach (PeEntity entity in m_Entities)
		{
			if (entity != null && !entity.IsDeath())
			{
				TargetCmpt component = entity.GetComponent<TargetCmpt>();
				if (component != null && component.GetAttackEnemy() != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEscapeEnemy()
	{
		foreach (PeEntity entity in m_Entities)
		{
			if (entity != null && !entity.IsDeath())
			{
				TargetCmpt component = entity.GetComponent<TargetCmpt>();
				if (component != null && component.GetEscapeEnemy() != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Fly(bool value)
	{
		foreach (PeEntity entity in m_Entities)
		{
			if (entity != null && !entity.IsDeath())
			{
				MonsterCmpt component = entity.GetComponent<MonsterCmpt>();
				if (component != null)
				{
					component.Fly(value);
				}
			}
		}
	}

	public void ActivateGravity(bool value)
	{
		foreach (PeEntity entity in m_Entities)
		{
			if (entity != null && !entity.IsDeath())
			{
				MonsterCmpt component = entity.GetComponent<MonsterCmpt>();
				if (component != null)
				{
					component.ActivateGravity(value);
				}
			}
		}
	}

	public void MoveToPosition(Vector3 pos, SpeedState speed = SpeedState.Walk)
	{
		foreach (PeEntity entity in m_Entities)
		{
			if (!(entity != null) || entity.IsDeath())
			{
				continue;
			}
			Motion_Move component = entity.GetComponent<Motion_Move>();
			if (component != null && Leader != null)
			{
				PeTrans component2 = component.GetComponent<PeTrans>();
				PeTrans component3 = Leader.GetComponent<PeTrans>();
				if (component2 != null && component3 != null)
				{
					float num = 3f * UnityEngine.Random.value * Mathf.Max(1f, Mathf.Max(component2.radius, component3.radius));
					component.MoveTo(pos + (component2.position - component3.position).normalized * num, speed);
				}
			}
		}
	}

	public void SetEscape(PeEntity self, PeEntity escapeEntity)
	{
		for (int i = 0; i < m_Entities.Count; i++)
		{
			if (m_Entities[i] != null && m_Entities[i].target != null && !m_Entities.Equals(self))
			{
				m_Entities[i].target.SetEscapeEntity(escapeEntity);
			}
		}
	}

	public void OnTargetDiscover(PeEntity self, PeEntity target)
	{
		if (self == null || target == null)
		{
			return;
		}
		for (int i = 0; i < m_Entities.Count; i++)
		{
			if (m_Entities[i] != null && !m_Entities.Equals(self))
			{
				m_Entities[i].OnTargetDiscover(target);
			}
		}
	}

	public void OnDamageMember(PeEntity self, PeEntity target, float hatred)
	{
		for (int i = 0; i < m_Entities.Count; i++)
		{
			if (m_Entities[i] != null && !m_Entities.Equals(self))
			{
				m_Entities[i].OnDamageMember(target, hatred);
			}
		}
	}

	private void OnMemberCreated(PeEntity e)
	{
		if (e != null)
		{
			RegisterMember(e);
		}
	}

	public void Reset(Behave.Runtime.Tree sender)
	{
	}

	public int SelectTopPriority(Behave.Runtime.Tree sender, params int[] IDs)
	{
		return IDs[0];
	}

	public BehaveResult Tick(Behave.Runtime.Tree sender)
	{
		return BehaveResult.Success;
	}
}
