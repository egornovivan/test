using System.Collections.Generic;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTCalculateAttackMode), "CalculateAttackMode")]
public class BTCalculateAttackMode : BTNormal
{
	private float m_LastTime;

	private List<IAttack> m_Attacks = new List<IAttack>();

	private BehaveResult Tick(Tree sender)
	{
		if (base.entity == null || base.entity.target == null || base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		IAttack attack = null;
		List<IAttack> attacks = base.entity.target.Attacks;
		m_Attacks.Clear();
		float num = 0f;
		float num2 = 0f;
		float value = Random.value;
		for (int i = 0; i < attacks.Count; i++)
		{
			if (attacks[i].IsReadyCD(base.attackEnemy) && (attacks[i].ReadyAttack(base.attackEnemy) || attacks[i] is IAttackTop))
			{
				num += attacks[i].Weight;
				m_Attacks.Add(attacks[i]);
			}
		}
		for (int j = 0; j < m_Attacks.Count; j++)
		{
			num2 += m_Attacks[j].Weight / num;
			if (value <= num2)
			{
				attack = m_Attacks[j];
				break;
			}
		}
		if (attack == null && Time.time - m_LastTime > 2f)
		{
			m_Attacks.Clear();
			num = 0f;
			num2 = 0f;
			value = Random.value;
			for (int k = 0; k < attacks.Count; k++)
			{
				if (attacks[k] is IAttackPositive)
				{
					num += attacks[k].Weight;
					m_Attacks.Add(attacks[k]);
				}
			}
			for (int l = 0; l < m_Attacks.Count; l++)
			{
				num2 += m_Attacks[l].Weight / num;
				if (value <= num2)
				{
					attack = m_Attacks[l];
					m_LastTime = Time.time;
					break;
				}
			}
		}
		if (attack != null)
		{
			base.attackEnemy.Attack = attack;
			if (base.attackEnemy.entityTarget.target != null)
			{
				if (attack is BTMelee || attack is BTMeleeAttack)
				{
					base.attackEnemy.entityTarget.target.AddMelee(base.entity);
				}
				if (attack is BTAttackRanged)
				{
					base.attackEnemy.entityTarget.target.RemoveMelee(base.entity);
				}
			}
		}
		if (base.attackEnemy.Attack != null)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
