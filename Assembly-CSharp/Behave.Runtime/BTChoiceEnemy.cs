namespace Behave.Runtime;

[BehaveAction(typeof(BTChoiceEnemy), "ChoiceEnemy")]
public class BTChoiceEnemy : BTNormal
{
	private class Data
	{
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (base.entity.NpcCmpt == null)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.BattleMgr.CanChoiceEnemy(base.Enemies))
		{
			SetCambat(value: true);
			return BehaveResult.Success;
		}
		SetCambat(value: false);
		ClearEnemy();
		return BehaveResult.Failure;
	}
}
