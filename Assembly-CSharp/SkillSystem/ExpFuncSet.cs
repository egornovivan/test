using System;
using System.Collections.Generic;
using Pathea;
using Pathea.Projectile;
using PETools;
using UnityEngine;

namespace SkillSystem;

public class ExpFuncSet : IExpFuncSet
{
	private const float ShieldToHP = 10f;

	private ISkAttribs _parent;

	public ExpFuncSet(ISkAttribs parent)
	{
		_parent = parent;
	}

	public bool RandIn(float prob)
	{
		return UnityEngine.Random.value < prob;
	}

	public void GetHurt(float dmg)
	{
		if (SkRuntimeInfo.Current is SkInst { _colInfo: not null } skInst && skInst._colInfo.damageScale > float.Epsilon)
		{
			dmg *= skInst._colInfo.damageScale;
		}
		SkEntity caster = SkRuntimeInfo.Current.Caster;
		SkEntity target = SkRuntimeInfo.Current.Target;
		IList<float> raws;
		IList<float> list = (raws = _parent.raws);
		int index;
		int index2 = (index = 1);
		float num = raws[index];
		list[index2] = num - dmg;
		IList<float> sums;
		IList<float> list2 = (sums = _parent.sums);
		int index3 = (index = 1);
		num = sums[index];
		list2[index3] = num - dmg;
		_parent.modflags[1] = false;
		float time = Time.time;
		if (caster != null)
		{
			caster._lastestTimeOfHurtingSb = time;
		}
		if (target != null)
		{
			target._lastestTimeOfGettingHurt = time;
		}
		if (SkRuntimeInfo.Current is SkInst skInst2)
		{
			skInst2.Caster.OnHurtSb(skInst2, dmg);
			skInst2.Target.OnGetHurt(skInst2, dmg);
		}
	}

	public void TryGetHurt(float dmg, float exp = 0f)
	{
		if (SkRuntimeInfo.Current is SkInst { _colInfo: not null } skInst && skInst._colInfo.damageScale > float.Epsilon)
		{
			dmg *= skInst._colInfo.damageScale;
		}
		SkEntity caster = SkRuntimeInfo.Current.Caster;
		SkEntity target = SkRuntimeInfo.Current.Target;
		if (CanDamage(caster, target))
		{
			if (caster is SkProjectile)
			{
				SkProjectileDamageScale component = caster.GetComponent<SkProjectileDamageScale>();
				if (null != component)
				{
					dmg *= component.damageScale;
				}
			}
			float num = _parent.sums[31];
			if (num > 0f && caster is SkProjectile)
			{
				if (num * 10f < dmg)
				{
					_parent.sums[31] = 0f;
					IList<float> raws;
					IList<float> list = (raws = _parent.raws);
					int index;
					int index2 = (index = 1);
					float num2 = raws[index];
					list[index2] = num2 - (dmg - num * 10f);
					IList<float> sums;
					IList<float> list2 = (sums = _parent.sums);
					int index3 = (index = 1);
					num2 = sums[index];
					list2[index3] = num2 - (dmg - num * 10f);
					_parent.modflags[1] = false;
				}
				else
				{
					IList<float> sums2;
					IList<float> list3 = (sums2 = _parent.sums);
					int index;
					int index4 = (index = 31);
					float num2 = sums2[index];
					list3[index4] = num2 - dmg / 10f;
				}
			}
			else
			{
				IList<float> raws2;
				IList<float> list4 = (raws2 = _parent.raws);
				int index;
				int index5 = (index = 1);
				float num2 = raws2[index];
				list4[index5] = num2 - dmg;
				IList<float> sums3;
				IList<float> list5 = (sums3 = _parent.sums);
				int index6 = (index = 1);
				num2 = sums3[index];
				list5[index6] = num2 - dmg;
				_parent.modflags[1] = false;
			}
			float time = Time.time;
			if (caster != null)
			{
				caster._lastestTimeOfHurtingSb = time;
			}
			if (target != null)
			{
				target._lastestTimeOfGettingHurt = time;
			}
			if (SkRuntimeInfo.Current is SkInst skInst2)
			{
				skInst2.Caster.OnHurtSb(skInst2, dmg);
				skInst2.Target.OnGetHurt(skInst2, dmg);
			}
		}
		if (exp > float.Epsilon)
		{
			SkEntity skEntity = caster;
			if (caster is SkProjectile)
			{
				skEntity = (skEntity as SkProjectile).parentSkEntity;
			}
			skEntity.SetAttribute(74, skEntity.GetAttribute(74) + exp, eventOff: false);
		}
	}

	public bool InCoolingForGettingHurt(float cooltime)
	{
		return Time.time >= SkRuntimeInfo.Current.Target._lastestTimeOfGettingHurt + cooltime;
	}

	public bool InCoolingForConsumingStamina(float cooltime)
	{
		return Time.time > SkRuntimeInfo.Current.Target._lastestTimeOfConsumingStamina + Mathf.Max(cooltime, 0.1f);
	}

	private static bool HasReputation(int p1, int p2)
	{
		if (!PeSingleton<ReputationSystem>.Instance.GetActiveState(p1))
		{
			return false;
		}
		return p2 == 4 || p2 == 5;
	}

	private static bool CanDamage(SkEntity e1, SkEntity e2)
	{
		SkProjectile skProjectile = e1 as SkProjectile;
		if (skProjectile != null)
		{
			SkEntity skEntityCaster = skProjectile.GetSkEntityCaster();
			if (skEntityCaster == null)
			{
				return false;
			}
			e1 = skProjectile.GetSkEntityCaster();
		}
		int num = Convert.ToInt32(e1.GetAttribute(91));
		int num2 = Convert.ToInt32(e2.GetAttribute(91));
		int src = Convert.ToInt32(e1.GetAttribute(95));
		int dst = Convert.ToInt32(e2.GetAttribute(95));
		return PEUtil.CanDamageReputation(num, num2) && Singleton<ForceSetting>.Instance.Conflict(num, num2) && DamageData.GetValue(src, dst) != 0;
	}
}
