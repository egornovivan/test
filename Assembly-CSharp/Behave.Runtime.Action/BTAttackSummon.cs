using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAttackSummon), "AttackSummon")]
public class BTAttackSummon : BTAttackBase
{
	private class Data : IAttack
	{
		[Behave]
		public string anim = string.Empty;

		[Behave]
		public float prob;

		[Behave]
		public int count;

		[Behave]
		public int protoID;

		[Behave]
		public float hpPercent;

		[Behave]
		public float delayTime;

		[Behave]
		public Vector3 center = Vector3.zero;

		[Behave]
		public Vector3 extend = Vector3.zero;

		public bool m_Summoned;

		public float m_StartTime;

		public float Weight => prob;

		public bool IsRunning(Enemy enemy)
		{
			return m_StartTime > 0f && Time.time - m_StartTime <= delayTime;
		}

		public bool IsReadyCD(Enemy enemy)
		{
			return true;
		}

		public bool ReadyAttack(Enemy enemy)
		{
			return !m_Summoned && enemy.entity.HPPercent <= hpPercent;
		}

		public bool CanAttack(Enemy enemy)
		{
			return true;
		}

		public bool IsProbability(float randomValue)
		{
			return true;
		}

		public bool IsBlocked(Enemy enemy)
		{
			return false;
		}

		public bool CanInterrupt()
		{
			return false;
		}
	}

	private Data m_Data;

	private float m_StartTime;

	private Vector3 GetRandomPos(Transform root)
	{
		if (root != null)
		{
			float x = Random.Range(0f - Mathf.Abs(m_Data.extend.x), Mathf.Abs(m_Data.extend.x));
			float y = Random.Range(0f - Mathf.Abs(m_Data.extend.y), Mathf.Abs(m_Data.extend.y));
			float z = Random.Range(0f - Mathf.Abs(m_Data.extend.z), Mathf.Abs(m_Data.extend.z));
			return root.TransformPoint(m_Data.center + new Vector3(x, y, z));
		}
		return Vector3.zero;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.Attack == null || !base.attackEnemy.Attack.Equals(m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.attackEnemy.Attack.ReadyAttack(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.m_StartTime < float.Epsilon)
		{
			m_Data.m_StartTime = Time.time;
			SetBool(m_Data.anim, value: true);
		}
		if (Time.time - m_Data.m_StartTime < m_Data.delayTime)
		{
			return BehaveResult.Running;
		}
		for (int i = 0; i < m_Data.count; i++)
		{
			SceneMan.AddSceneObj(MonsterEntityCreator.CreateAgent(GetRandomPos(base.transform), m_Data.protoID));
		}
		m_Data.m_Summoned = true;
		return BehaveResult.Success;
	}
}
