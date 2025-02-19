using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTSwitchWeapon), "SwitchWeapon")]
public class BTSwitchWeapon : BTNormal
{
	private List<IWeapon> m_Weapons = new List<IWeapon>();

	private bool CanAttackWeapon(IWeapon weapon, Enemy enemy)
	{
		AttackMode[] attackMode = weapon.GetAttackMode();
		for (int i = 0; i < attackMode.Length; i++)
		{
			if (enemy.DistanceXZ > attackMode[i].minRange && enemy.DistanceXZ < attackMode[i].maxRange)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsRangedWeapon(IWeapon weapon)
	{
		AttackMode[] attackMode = weapon.GetAttackMode();
		for (int i = 0; i < attackMode.Length; i++)
		{
			if (attackMode[i].type == AttackType.Ranged)
			{
				return true;
			}
		}
		return false;
	}

	private float GetWeaponScale(IWeapon weapon, Enemy enemy)
	{
		float result = float.PositiveInfinity;
		AttackMode[] attackMode = weapon.GetAttackMode();
		for (int i = 0; i < attackMode.Length; i++)
		{
			result = Mathf.Min(Mathf.Abs(enemy.DistanceXZ - attackMode[i].minRange), Mathf.Abs(enemy.DistanceXZ - attackMode[i].maxRange));
		}
		return result;
	}

	private IWeapon GetWeapon()
	{
		IWeapon weapon = null;
		m_Weapons.Clear();
		m_Weapons = base.entity.target.GetCanUseWeaponList(base.attackEnemy);
		for (int i = 0; i < m_Weapons.Count; i++)
		{
			if (CanAttackWeapon(m_Weapons[i], base.attackEnemy))
			{
				weapon = m_Weapons[i];
				if (IsRangedWeapon(m_Weapons[i]))
				{
					break;
				}
			}
		}
		if (weapon == null)
		{
			float num = float.PositiveInfinity;
			for (int j = 0; j < m_Weapons.Count; j++)
			{
				float weaponScale = GetWeaponScale(m_Weapons[j], base.attackEnemy);
				if (weaponScale < num)
				{
					num = weaponScale;
					weapon = m_Weapons[j];
				}
			}
		}
		return weapon;
	}

	private BehaveResult Tick(Tree sender)
	{
		IWeapon weapon = GetWeapon();
		if (weapon != null && !weapon.Equals(null))
		{
			if (base.Weapon == null || base.Weapon.Equals(null))
			{
				if (!weapon.HoldReady)
				{
					weapon.HoldWeapon(hold: true);
				}
			}
			else if (!base.Weapon.Equals(weapon))
			{
				base.entity.motionEquipment.SwitchHoldWeapon(base.Weapon, weapon);
			}
		}
		if (base.Weapon == null || base.Weapon.Equals(null))
		{
			return BehaveResult.Failure;
		}
		if (!base.Weapon.HoldReady)
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
