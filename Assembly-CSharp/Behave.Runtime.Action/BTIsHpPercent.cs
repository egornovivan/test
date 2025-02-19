namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsHpPercent), "IsHpPercent")]
public class BTIsHpPercent : BTNormal
{
	private class Data
	{
		[Behave]
		public float minHpPercent;

		[Behave]
		public float maxHpPercent;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.HpPercent >= m_Data.minHpPercent && base.HpPercent <= m_Data.maxHpPercent)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
