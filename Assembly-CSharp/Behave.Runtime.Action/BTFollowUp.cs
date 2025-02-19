using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTFollowUp), "FollowUp")]
public class BTFollowUp : BTNormal
{
	private class Data
	{
		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float radius;

		public Vector3 m_Local;

		public Vector3 m_Patrol;

		private float m_LastRandomTime;

		private float m_CurCDTime;

		private bool m_InitRandom;

		public bool Cooldown()
		{
			if (Time.time - m_LastRandomTime > m_CurCDTime)
			{
				m_CurCDTime = PERandom.BehaveSeed.Next(1, 5);
				m_LastRandomTime = Time.time;
				return true;
			}
			return false;
		}

		public bool IsReached(Vector3 pos, Vector3 target, float Radius)
		{
			float num = PEUtil.SqrMagnitudeH(pos, target + m_Local);
			return num <= Radius * Radius;
		}

		public bool IsReached(Enemy enmy)
		{
			return enmy.SqrDistanceXZ >= 0.001f;
		}

		public void ResetInitRandom()
		{
			m_InitRandom = false;
		}

		public Vector3 GetRandomPosition(Vector3 target)
		{
			return target + m_Local;
		}

		public Vector3 GetPatrolPosition(Vector3 target)
		{
			return target + m_Local + m_Patrol;
		}

		public void RandomPosition(float argRadius)
		{
			if (!m_InitRandom)
			{
				m_InitRandom = true;
				m_Local = PEUtil.GetRandomPosition(Vector3.zero, argRadius + minRange, argRadius + maxRange);
				m_Local = new Vector3(m_Local.x, 0f, m_Local.z);
			}
		}

		public void RandomPatrol(Vector3 dir)
		{
			m_Patrol = PEUtil.GetRandomPosition(Vector3.zero, dir, 0f, radius, -90f, 90f);
			m_Patrol = new Vector3(m_Patrol.x, 0f, m_Patrol.z);
		}
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		float argRadius = base.radius + base.attackEnemy.radius;
		Vector3 vector = base.attackEnemy.position - base.position;
		vector.y = 0f;
		if (base.Weapon != null && base.Weapon is IAimWeapon aimWeapon && base.attackEnemy.CenterBone != null)
		{
			aimWeapon.SetTarget(base.attackEnemy.CenterBone);
		}
		if (!m_Data.IsReached(base.attackEnemy))
		{
			m_Data.RandomPosition(argRadius);
			SetMoveMode(MoveMode.ForwardOnly);
			FaceDirection(base.attackEnemy.position - base.position);
			MoveToPosition(m_Data.GetRandomPosition(base.attackEnemy.position), SpeedState.Run);
			return BehaveResult.Failure;
		}
		m_Data.ResetInitRandom();
		if (m_Data.Cooldown())
		{
			m_Data.RandomPatrol(-vector);
		}
		SetMoveMode(MoveMode.EightAaxis);
		FaceDirection(vector);
		return BehaveResult.Success;
	}
}
