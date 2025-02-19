using Pathea;
using UnityEngine;

public class DragItemMousePickColony : DragItemMousePick
{
	public CSBuildingLogic csbl;

	protected override string tipsText => base.tipsText + "\n" + PELocalization.GetString(8000141);

	protected override void InitCmd(CmdList cmdList)
	{
		cmdList.Add("Open", OnOpen);
		cmdList.Add("Get", OnGetBtn);
		cmdList.Add("Turn", Turn90Degree);
	}

	public override bool CanCmd()
	{
		if (csbl == null)
		{
			csbl = GetComponentInParent<CSBuildingLogic>();
		}
		if (PeGameMgr.IsMulti && csbl != null && BaseNetwork.MainPlayer.TeamId != csbl.network.TeamId)
		{
			return false;
		}
		return base.CanCmd();
	}

	public override void DoGetItem()
	{
		if (base.pkg == null || base.itemObj == null || !base.pkg.CanAdd(base.itemObj))
		{
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		CSEntityObject component = GetComponent<CSEntityObject>();
		if (component != null)
		{
			float durability = component.m_Entity.BaseData.m_Durability;
			float durability2 = component.m_Entity.m_Info.m_Durability;
			component.m_Entity.BaseData.m_Durability = durability - Mathf.Floor(durability2 * 0.1f);
			if (component.m_Creator.RemoveEntity(component.m_Entity.ID, bRemoveData: false) != null)
			{
				base.DoGetItem();
			}
		}
		if (!PeGameMgr.IsMulti)
		{
			SendMessage("OnRemoveGo", base.itemObjectId, SendMessageOptions.DontRequireReceiver);
		}
	}

	public override void OnGetBtn()
	{
		CSEntityObject component = GetComponent<CSEntityObject>();
		if (!(component == null))
		{
			if (EntityMonsterBeacon.IsRunning())
			{
				PeTipMsg.Register(PELocalization.GetString(8000622), PeTipMsg.EMsgLevel.Warning);
				CloseOn();
			}
			else if (component.m_Entity.BaseData.m_Durability < component.m_Entity.m_Info.m_Durability * 0.15f)
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000084));
			}
			else if (component as CSDwellingsObject != null)
			{
				MessageBox_N.ShowYNBox(PELocalization.GetString(8000085), GetOn, CloseOn);
			}
			else
			{
				MessageBox_N.ShowYNBox(PELocalization.GetString(8000086), GetOn, CloseOn);
			}
		}
	}

	public override void Turn90Degree()
	{
		base.Turn90Degree();
		if (!GameConfig.IsMultiMode)
		{
			CSEntityObject component = GetComponent<CSEntityObject>();
			if (component != null)
			{
				csbl = GetComponentInParent<CSBuildingLogic>();
				if (csbl != null)
				{
					component.Init(csbl, component.m_Creator, bFight: false);
					csbl.m_Entity.AfterTurn90Degree();
				}
			}
		}
		OnItemOpGUIHide();
	}

	public void OnOpen()
	{
		CSEntityObject component = GetComponent<CSEntityObject>();
		if (CSUI_MainWndCtrl.Instance != null && component != null)
		{
			CSUI_MainWndCtrl.Instance.ShowWndPart(component.m_Entity);
		}
		HideItemOpGui();
		OnItemOpGUIHide();
	}

	public void OnItemOpGUIActive()
	{
		if (!(this == null))
		{
			CSCommonObject component = GetComponent<CSCommonObject>();
			if (component != null)
			{
				component.ShowWorkSpaceEffect();
			}
		}
	}

	public void OnItemOpGUIHide()
	{
		if (!(this == null))
		{
			CSCommonObject component = GetComponent<CSCommonObject>();
			if (component != null)
			{
				component.HideWorkSpaceEffect();
			}
		}
	}

	private void CloseOn()
	{
		HideItemOpGui();
		OnItemOpGUIHide();
	}

	private void GetOn()
	{
		base.OnGetBtn();
	}

	protected override void CheckOperate()
	{
		if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && CanCmd())
		{
			GameUI.Instance.mItemOp.ListenEvent(OnItemOpGUIHide, OnItemOpGUIActive);
		}
		base.CheckOperate();
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd())
		{
			OnOpen();
		}
	}
}
