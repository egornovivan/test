using UnityEngine;

public class OperatableItemRepairMachine : OperatableItem
{
	public override bool Operate()
	{
		if (!base.Operate())
		{
			return false;
		}
		if (!TutorialData.AddActiveTutorialID(8))
		{
			CSRepairObject component = GetComponent<CSRepairObject>();
			if (null != component && !GameUI.Instance.mRepair.isShow)
			{
				GameUI.Instance.mRepair.OpenWnd(component);
			}
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
		CSRepairObject component = base.gameObject.GetComponent<CSRepairObject>();
		component.transform.localScale = Vector3.one;
		return 4 == component.Init(id, CSMain.GetCreator(10000), bFight: false);
	}

	public void PostInit()
	{
		CSRepairObject component = base.gameObject.GetComponent<CSRepairObject>();
		component.transform.localScale = Vector3.one;
		component.Init(m_id, CSMain.GetCreator(10000), bFight: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CSMain.InitOperatItemEvent -= PostInit;
	}
}
