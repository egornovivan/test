public class OperatableItemPowerPlantSolar : OperatableItem
{
	public override bool Operate()
	{
		if (!base.Operate())
		{
			return false;
		}
		if (!TutorialData.AddActiveTutorialID(9))
		{
			GameUI.Instance.mPowerPlantSolar.OpenWnd(GetComponent<CSPowerPlantObject>());
		}
		return true;
	}

	public override bool Init(int id)
	{
		if (!base.Init(id))
		{
			return false;
		}
		if (CSMain.GetCreator(10000) == null)
		{
			CSMain.InitOperatItemEvent += PostInit;
			return true;
		}
		CSPowerPlantObject component = GetComponent<CSPowerPlantObject>();
		return 4 == component.Init(id, CSMain.GetCreator(10000), bFight: false);
	}

	public void PostInit()
	{
		CSPowerPlantObject component = GetComponent<CSPowerPlantObject>();
		component.Init(m_id, CSMain.GetCreator(10000), bFight: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CSMain.InitOperatItemEvent -= PostInit;
	}
}
