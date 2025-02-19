using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExtNpcPackage;
using PETools;
using UnityEngine;

namespace ItemAsset;

public class EvaluateMgr
{
	private static List<EvaluateInfo> infos = new List<EvaluateInfo>();

	private static Stack<EvaluateInfo> infoPool = new Stack<EvaluateInfo>();

	private static EvaluateInfo GetEvaluateInfo()
	{
		EvaluateInfo evaluateInfo = null;
		if (infoPool.Count > 0)
		{
			evaluateInfo = infoPool.Pop();
		}
		if (evaluateInfo == null)
		{
			evaluateInfo = new EvaluateInfo();
		}
		return evaluateInfo;
	}

	public static void Recyle(EvaluateInfo info)
	{
		infoPool.Push(info);
	}

	public static EvaluateInfo Evaluate(PeEntity npc, Enemy enemy, ItemObject obj)
	{
		float num = 5f;
		ItemProto.WeaponInfo weaponInfo = obj.protoData.weaponInfo;
		AttackMode attackMode = obj.protoData.weaponInfo.attackModes[0];
		EvaluateInfo evaluateInfo = new EvaluateInfo();
		if (npc.motionEquipment.PEHoldAbleEqObj == obj)
		{
			evaluateInfo.setEquipment(1.2f);
		}
		else
		{
			evaluateInfo.setEquipment(1f);
		}
		if (attackMode.type == AttackType.Ranged)
		{
			evaluateInfo.SetRangeFir(2f);
		}
		else
		{
			evaluateInfo.SetRangeFir(1f);
		}
		float attribute = npc.GetAttribute(AttribType.Hp);
		float attribute2 = npc.GetAttribute(AttribType.Def);
		float attribute3 = enemy.entityTarget.GetAttribute(AttribType.Atk);
		bool flag = ((attribute3 - attribute2 > attribute * 0.1f && attackMode.type == AttackType.Ranged) ? true : false);
		if (enemy.entityTarget.Field == MovementField.Sky || enemy.entityTarget.IsBoss || flag)
		{
			if (attackMode.type == AttackType.Ranged)
			{
				evaluateInfo.SetLongRange(2f);
			}
			else
			{
				evaluateInfo.SetLongRange(0f);
			}
		}
		else
		{
			evaluateInfo.SetLongRange(1f);
		}
		float num2 = PEUtil.MagnitudeH(npc.position, enemy.position);
		if (attackMode.IsInRange(num2))
		{
			evaluateInfo.SetRangeValue(2f);
		}
		else if (num2 < attackMode.minRange && num2 + num > attackMode.minRange)
		{
			evaluateInfo.SetRangeValue(1f + Mathf.Clamp01(num2 / attackMode.minRange));
		}
		else if (num2 > attackMode.maxRange && attackMode.maxRange + num > num2)
		{
			evaluateInfo.SetRangeValue(1f + Mathf.Clamp01(num - (num2 - attackMode.maxRange) / num));
		}
		else if (attackMode.type == AttackType.Ranged)
		{
			evaluateInfo.SetRangeValue(1.8f);
		}
		else
		{
			evaluateInfo.SetRangeValue(1f);
		}
		evaluateInfo.SetDPS(attackMode.damage / ((!(attackMode.frequency < float.Epsilon)) ? attackMode.frequency : 1f));
		if (weaponInfo.useEnergry)
		{
			GunAmmo cmpt = obj.GetCmpt<GunAmmo>();
			if (cmpt != null)
			{
				int num3 = cmpt.count / Mathf.Max(weaponInfo.costPerShoot, 1);
				if (num3 > 3)
				{
					evaluateInfo.SetSurplusCnt(1f);
				}
				else if (num3 >= 1 || npc.GetAttribute(AttribType.Energy) > float.Epsilon)
				{
					evaluateInfo.SetSurplusCnt(0.5f);
				}
				else
				{
					evaluateInfo.SetSurplusCnt(0f);
				}
			}
			else
			{
				evaluateInfo.SetSurplusCnt(0f);
			}
		}
		else if (weaponInfo.costItem > 0)
		{
			GunAmmo cmpt2 = obj.GetCmpt<GunAmmo>();
			if (cmpt2 == null)
			{
				if (npc.GetItemCount(weaponInfo.costItem) < 3)
				{
					evaluateInfo.SetSurplusCnt(0.5f);
				}
				else
				{
					evaluateInfo.SetSurplusCnt(1f);
				}
			}
			else if (cmpt2.count < 3)
			{
				if (npc.GetItemCount(weaponInfo.costItem) < 3)
				{
					evaluateInfo.SetSurplusCnt(0.5f);
				}
				else
				{
					evaluateInfo.SetSurplusCnt(1f);
				}
			}
			else
			{
				evaluateInfo.SetSurplusCnt(1f);
			}
		}
		else
		{
			evaluateInfo.SetSurplusCnt(1f);
		}
		evaluateInfo.SetObj(obj);
		return evaluateInfo;
	}

	public static List<EvaluateInfo> Evaluates(PeEntity npc, Enemy enemy, List<ItemObject> objs)
	{
		infos.Clear();
		for (int i = 0; i < objs.Count; i++)
		{
			EvaluateInfo evaluateInfo = Evaluate(npc, enemy, objs[i]);
			if (evaluateInfo.EvaluateValue > 0f)
			{
				infos.Add(evaluateInfo);
			}
		}
		return infos;
	}
}
