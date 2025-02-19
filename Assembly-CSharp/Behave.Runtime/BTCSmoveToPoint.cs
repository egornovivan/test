using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCSmoveToPoint), "CSmoveToPoint")]
public class BTCSmoveToPoint : BTNormal
{
	private class Data
	{
		[Behave]
		public int LineType;
	}

	private Data m_Data;

	private ChatTeamDb m_chatTeamDb;

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		m_chatTeamDb = TeamDb.LoadchatTeamDb(base.entity);
		if (m_chatTeamDb == null)
		{
			return BehaveResult.Failure;
		}
		if (m_chatTeamDb.TRMovePos == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.lineType == ELineType.IDLE)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (IsReached(base.position, m_chatTeamDb.TRMovePos))
		{
			SetPosition(m_chatTeamDb.TRMovePos);
			StopMove();
			return BehaveResult.Success;
		}
		if (Stucking())
		{
			SetPosition(m_chatTeamDb.TRMovePos);
		}
		if (base.entity.NpcCmpt.lineType == ELineType.IDLE)
		{
			return BehaveResult.Failure;
		}
		MoveToPosition(m_chatTeamDb.TRMovePos);
		return BehaveResult.Running;
	}
}
