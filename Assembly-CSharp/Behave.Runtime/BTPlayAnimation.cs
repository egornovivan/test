using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTPlayAnimation), "PlayAnimation")]
public class BTPlayAnimation : BTNormal
{
	private class Data
	{
		[Behave]
		public string PlayState;

		[Behave]
		public string AnimName;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Work))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		SetBool(m_Data.PlayState, value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Work))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		SetBool(m_Data.AnimName, value: true);
		return BehaveResult.Success;
	}
}
