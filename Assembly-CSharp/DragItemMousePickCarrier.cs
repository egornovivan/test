using Pathea;

public class DragItemMousePickCarrier : DragItemMousePickCreation
{
	protected override string tipsText => base.tipsText + "\n" + PELocalization.GetString(8000130);

	public override bool CanCmd()
	{
		if (!base.CanCmd())
		{
			return false;
		}
		if (PeGameMgr.IsMulti)
		{
			ItemScript script = GetScript();
			if (null != script && null != script.netLayer && Singleton<ForceSetting>.Instance.Conflict(BaseNetwork.MainPlayer.Id, script.netLayer.TeamId))
			{
				return false;
			}
		}
		ItemScript_Carrier component = GetComponent<ItemScript_Carrier>();
		if (component == null)
		{
			return false;
		}
		return !component.IsPlayerOnCarrier();
	}

	protected override void InitCmd(CmdList cmdList)
	{
		cmdList.Add("Get On", GetOnCarrier);
		cmdList.Add("Get", OnGetBtn);
	}

	public override void DoGetItem()
	{
		ItemScript_Carrier component = GetComponent<ItemScript_Carrier>();
		if (!(component == null) && component.PassengerCountOnSeat() <= 0)
		{
			base.DoGetItem();
		}
	}

	private void GetOnCarrier()
	{
		ItemScript_Carrier component = GetComponent<ItemScript_Carrier>();
		if (component != null)
		{
			component.GetOn();
		}
		HideItemOpGui();
	}

	private void OnRepair()
	{
		ItemScript_Carrier component = GetComponent<ItemScript_Carrier>();
		if (component != null)
		{
			component.Repair();
		}
		HideItemOpGui();
	}

	private void OnCharge()
	{
		ItemScript_Carrier component = GetComponent<ItemScript_Carrier>();
		if (component != null)
		{
			component.Charge();
		}
		HideItemOpGui();
	}
}
