using ItemAsset;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCheckEquips), "CheckEquips")]
public class BTCheckEquips : BTNormal
{
	private class Data
	{
		[Behave]
		public int EqType;

		[Behave]
		public int AttackType;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (SelectItem.HasCanEquip(base.entity, (EeqSelect)m_Data.EqType, (AttackType)m_Data.AttackType))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
