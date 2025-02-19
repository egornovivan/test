using ItemAsset;
using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcFollowerEat), "NpcFollowerEat")]
public class BTNpcFollowerEat : BTNormal
{
	private class Data
	{
		[Behave]
		public string eatAnim;
	}

	private Data m_Data;

	private ItemObject mEatItem;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!NpcEatDb.CanEat(base.entity))
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!NpcEatDb.IsContinueEat(base.entity, out mEatItem))
		{
			return BehaveResult.Failure;
		}
		if (GetCdByProtoId(mEatItem.protoId) >= float.Epsilon)
		{
			return BehaveResult.Failure;
		}
		if (mEatItem != null)
		{
			UseItem(mEatItem);
			if (CanDoAction(PEActionType.Eat) && !IsActionRunning(PEActionType.Eat))
			{
				PEActionParamS param = PEActionParamS.param;
				param.str = m_Data.eatAnim;
				DoAction(PEActionType.Eat, param);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		EndAction(PEActionType.Eat);
	}
}
