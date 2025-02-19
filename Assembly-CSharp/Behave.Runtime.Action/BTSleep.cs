using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTSleep), "Sleep")]
public class BTSleep : BTNormal
{
	private class Target
	{
		public PeEntity m_Entity;

		public float m_Time;

		public Target(PeEntity entity, float time)
		{
			m_Entity = entity;
			m_Time = time;
		}
	}

	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float checkTime;

		public float m_Time;

		public float m_StartTime;

		public float m_LastCDTime = -1000f;

		public float m_LastCheckTime = -1000f;

		public float m_WakeupTime;

		public float m_LastWakeupTime;

		public float m_AwakenedTime;

		public float m_LastAwakenedTime;
	}

	private Data m_Data;

	private List<Target> m_Targets = new List<Target>();

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!GameConfig.IsNight)
		{
			return BehaveResult.Failure;
		}
		if (RandomDunGenUtil.IsInDungeon(base.entity))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_LastCDTime < m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_LastCheckTime < m_Data.checkTime)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Leader == null || base.entity.IsLeader)
		{
			if (Random.value > m_Data.prob)
			{
				return BehaveResult.Failure;
			}
		}
		else if (base.entity.Leader.animCmpt == null || !base.entity.Leader.animCmpt.GetBool("Sleep"))
		{
			return BehaveResult.Failure;
		}
		StopMove();
		SetBool("Sleep", value: true);
		m_Data.m_StartTime = Time.time;
		m_Data.m_LastCheckTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime < 0.5f)
		{
			return BehaveResult.Running;
		}
		if (GetBool("Sleep"))
		{
			if (Time.time - m_Data.m_LastWakeupTime > m_Data.m_WakeupTime)
			{
				if (!GameConfig.IsNight)
				{
					SetBool("Sleep", value: false);
				}
				m_Data.m_WakeupTime = Random.Range(60f, 120f);
			}
			if (base.attackEnemy != null && base.attackEnemy.ThreatDamage > 0f)
			{
				SetBool("Sleep", value: false);
			}
			if (base.escapeEnemy != null && base.escapeEnemy.ThreatDamage > 0f)
			{
				SetBool("Sleep", value: false);
			}
			if (Time.time - m_Data.m_LastAwakenedTime > m_Data.m_AwakenedTime)
			{
				m_Data.m_LastAwakenedTime = Time.time;
				m_Data.m_AwakenedTime = Random.Range(1f, 5f);
				if (base.attackEnemy != null && base.attackEnemy.SqrDistanceXZ < 25f)
				{
					SetBool("Sleep", value: false);
				}
				if (base.escapeEnemy != null && base.escapeEnemy.SqrDistanceXZ < 25f)
				{
					SetBool("Sleep", value: false);
				}
				if (base.entity.Leader != null && !base.entity.IsLeader && base.entity.Leader.animCmpt != null && !base.entity.Leader.animCmpt.GetBool("Sleep"))
				{
					SetBool("Sleep", value: false);
				}
			}
			for (int num = m_Targets.Count - 1; num >= 0; num--)
			{
				if (m_Targets[num].m_Entity == null)
				{
					m_Targets.RemoveAt(num);
				}
				else
				{
					if (Time.time - m_Targets[num].m_Time > 15f)
					{
						SetBool("Sleep", value: false);
						return BehaveResult.Running;
					}
					float num2 = base.radius + m_Targets[num].m_Entity.maxRadius;
					if (PEUtil.SqrMagnitude(base.position, m_Targets[num].m_Entity.position, is3D: false) > num2 * num2)
					{
						m_Targets.RemoveAt(num);
					}
				}
			}
			List<PeEntity> ents = PeSingleton<EntityMgr>.Instance.GetEntities(base.position, base.radius + 10f, isDeath: false);
			ents.Remove(base.entity);
			for (int i = 0; i < ents.Count; i++)
			{
				Target target = m_Targets.Find((Target ret) => ret.m_Entity == ents[i]);
				if (target == null)
				{
					m_Targets.Add(new Target(ents[i], Time.time));
				}
			}
		}
		if (GetBool("Sleeping"))
		{
			return BehaveResult.Running;
		}
		m_Data.m_LastCDTime = Time.time;
		return BehaveResult.Success;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			if (GetBool("Sleep"))
			{
				SetBool("Sleep", value: false);
			}
			m_Data.m_StartTime = 0f;
		}
	}
}
