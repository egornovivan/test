using System.Collections.Generic;
using Pathea;
using PatheaScript;

namespace PatheaScriptExt;

public class CPlayerOwn : Condition
{
	private VarRef mPlayerId;

	private VarRef mItemId;

	private VarRef mItemCount;

	private Compare mCompare;

	public override bool Parse()
	{
		mPlayerId = PeType.GetPlayerId(mInfo, mTrigger);
		mItemId = PeType.GetItemId(mInfo, mTrigger);
		mItemCount = PatheaScript.Util.GetVarRefOrValue(mInfo, "count", VarValue.EType.Int, mTrigger);
		mCompare = mFactory.GetCompare(mInfo, "compare");
		return true;
	}

	public override bool Do()
	{
		List<PeEntity> player = PeType.GetPlayer((int)mPlayerId.Value);
		bool result = true;
		foreach (PeEntity item in player)
		{
			PlayerPackageCmpt cmpt = item.GetCmpt<PlayerPackageCmpt>();
			int count = cmpt.package.GetCount((int)mItemId.Value);
			if (!mCompare.Do(count, mItemCount.Value))
			{
				result = false;
			}
		}
		return result;
	}

	public override string ToString()
	{
		return $"Condition[Is player:{0} item:{mItemId} count:{mItemCount} compare:{mCompare} ref:{mItemCount}]";
	}
}
