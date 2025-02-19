namespace Behave.Runtime;

[BehaveAction(typeof(BTCheckProfession), "CheckProfession")]
public class BTCheckProfession : BTNormal
{
	private class Data
	{
		[Behave]
		public int Profession;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.Profession == (int)base.entity.NpcCmpt.Profession)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
