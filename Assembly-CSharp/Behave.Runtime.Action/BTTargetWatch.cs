namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTargetWatch), "TargetWatch")]
public class BTTargetWatch : BTNormal
{
	private class Data
	{
		[Behave]
		public bool isWatch;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.isWatch && base.attackEnemy != null)
		{
			SetIKAim(base.attackEnemy.CenterBone);
		}
		else
		{
			SetIKAim(null);
		}
		return BehaveResult.Success;
	}
}
