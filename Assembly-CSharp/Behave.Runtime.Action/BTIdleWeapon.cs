using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIdleWeapon), "IdleWeapon")]
public class BTIdleWeapon : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		[Behave]
		public float relaxProb;

		[Behave]
		public float relaxTime;

		public float m_StartIdleTime;

		public float m_CurrentIdleTime;

		public float m_StartRestTime;
	}

	private Data m_Data;

	private float m_StartTime;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_StartTime < float.Epsilon)
		{
			m_StartTime = Time.time;
		}
		if (Time.time - m_StartTime < 10f)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || base.entity.Food != null || base.entity.IsDarkInDaytime || base.entity.Chat != null)
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
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartRestTime = Time.time;
		m_Data.m_StartIdleTime = Time.time;
		m_Data.m_CurrentIdleTime = Random.Range(m_Data.minTime, m_Data.maxTime);
		StopMove();
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
		{
			return BehaveResult.Running;
		}
		SetNpcAiType(ENpcAiType.FieldNpcIdle_Idle);
		if (!Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || base.entity.Food != null || base.entity.IsDarkInDaytime || base.entity.Chat != null)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartIdleTime > m_Data.m_CurrentIdleTime)
		{
			return BehaveResult.Success;
		}
		if (Time.time - m_Data.m_StartRestTime > m_Data.relaxTime)
		{
			m_Data.m_StartRestTime = Time.time;
			if (Random.value < m_Data.relaxProb)
			{
				List<IWeapon> weaponlist = base.entity.GetWeaponlist();
				if (weaponlist != null && weaponlist.Count > 0)
				{
					IWeapon weapon = weaponlist[Random.Range(0, weaponlist.Count)];
					if (weapon != null && !weapon.Equals(null) && weapon.leisures != null && weapon.leisures.Length > 0)
					{
						SetBool(weapon.leisures[Random.Range(0, weapon.leisures.Length)], value: true);
					}
				}
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartIdleTime > float.Epsilon)
		{
			m_Data.m_StartRestTime = 0f;
			m_Data.m_StartIdleTime = 0f;
			m_Data.m_CurrentIdleTime = 0f;
		}
	}
}
