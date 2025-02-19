using Pathea;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTCheckStandPos), "CheckStandPos")]
public class BTCheckStandPos : BTNormal
{
	private class Data
	{
		[Behave]
		public float Radius;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (PeGameMgr.IsTutorial)
		{
			return BehaveResult.Success;
		}
		bool hasNearleague = base.entity.NpcCmpt.HasNearleague;
		bool flag = AiUtil.CheckBlockBrush(base.entity);
		bool flag2 = PeSingleton<PeCreature>.Instance != null && AiUtil.CheckDig(base.entity, PeSingleton<PeCreature>.Instance.mainPlayer);
		bool flag3 = AiUtil.CheckDraging(base.entity);
		bool flag4 = AiUtil.CheckCreation(base.entity);
		if (hasNearleague || flag2 || flag || flag3 || flag4)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
