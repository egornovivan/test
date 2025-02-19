using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using PatheaScript;
using UnityEngine;

namespace PatheaScriptExt;

public class ActionModifyPlayerItem : ActionImmediate
{
	protected override bool Exec()
	{
		VarRef playerId = PeType.GetPlayerId(mInfo, mTrigger);
		VarRef itemId = PeType.GetItemId(mInfo, mTrigger);
		VarRef varRefOrValue = PatheaScript.Util.GetVarRefOrValue(mInfo, "count", VarValue.EType.Int, mTrigger);
		Functor functor = mFactory.GetFunctor(mInfo, "modify");
		List<PeEntity> player = PeType.GetPlayer((int)playerId.Value);
		int num = (int)itemId.Value;
		foreach (PeEntity item in player)
		{
			PlayerPackageCmpt cmpt = item.GetCmpt<PlayerPackageCmpt>();
			ItemPackage playerPak = cmpt.package._playerPak;
			int count = playerPak.GetCount(num);
			functor.Set(new Variable(count), varRefOrValue.Var);
			functor.Do();
			int num2 = (int)functor.Target.Value;
			if (num2 == count)
			{
				continue;
			}
			if (num2 > count)
			{
				int num3 = num2 - count;
				if (!playerPak.Add(num, num3))
				{
					UnityEngine.Debug.LogError(string.Concat("Add item:", num, " to player:", item, " failed."));
				}
				else
				{
					UnityEngine.Debug.Log(string.Concat("Add item:", num, " count:", num3, " to player:", item, " succeed."));
				}
			}
			else
			{
				int num4 = count - num2;
				playerPak.Destroy(num, num4);
				UnityEngine.Debug.Log(string.Concat("remove item:", num, " count:", num4, "from player:", item, " succeed."));
			}
		}
		return true;
	}

	public override string ToString()
	{
		return $"ActionPlayerAnimation";
	}
}
