using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCheckLine), "CheckLine")]
public class BTCheckLine : BTNormal
{
	private class Data
	{
		[Behave]
		public int LineType;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.entity == null || base.entity.NpcCmpt == null || !base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.lineType == (ELineType)m_Data.LineType)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
