using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTSquat), "Squat")]
public class BTSquat : BTNormal
{
	private float m_LastCheckTime;

	private float m_LastSquatTimeCD = -20f;

	private float m_StartSquatTime;

	private float m_CurSquatTime = 10f;

	private bool m_Squat;

	private BehaveResult Tick(Tree sender)
	{
		if (base.Weapon == null || base.attackEnemy == null)
		{
			return BehaveResult.Success;
		}
		if (base.attackEnemy.GroupAttack == EAttackGroup.Threat)
		{
			return BehaveResult.Success;
		}
		if (base.attackEnemy.velocity.sqrMagnitude > 0.0225f)
		{
			return BehaveResult.Success;
		}
		bool @bool = GetBool("Squat");
		bool flag = GetBool("Bazooka_Aim") || GetBool("Rifle_Aim");
		bool flag2 = PEUtil.IsScopeAngle(base.transform.forward, base.attackEnemy.Direction, Vector3.up, -75f, 75f);
		if (m_Squat != @bool)
		{
			m_Squat = @bool;
			if (!m_Squat)
			{
				m_LastSquatTimeCD = Time.time;
			}
		}
		if (!m_Squat)
		{
			if (Time.time - m_LastSquatTimeCD > 15f && Time.time - m_LastCheckTime > 5f)
			{
				if (Random.value < 0.5f && flag2 && flag)
				{
					SetBool("Squat", value: true);
					m_StartSquatTime = Time.time;
				}
				m_LastCheckTime = Time.time;
			}
		}
		else if (Time.time - m_StartSquatTime > m_CurSquatTime || !flag)
		{
			SetBool("Squat", value: false);
			m_CurSquatTime = Random.Range(10f, 20f);
		}
		return BehaveResult.Success;
	}

	private void Reset(Tree sender)
	{
		if (GetBool("Squat") && base.attackEnemy == null)
		{
			m_Squat = false;
			SetBool("Squat", value: false);
		}
	}
}
