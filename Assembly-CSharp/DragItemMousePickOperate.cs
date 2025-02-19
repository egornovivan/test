using Pathea;
using Pathea.Operate;
using UnityEngine;

public class DragItemMousePickOperate : DragItemMousePick
{
	[SerializeField]
	private EOperationMask m_Mask = EOperationMask.Sleep;

	[SerializeField]
	private PEActionType m_ActionType = PEActionType.Sleep;

	[SerializeField]
	private string m_ButtonName = "Sleep";

	[SerializeField]
	private int m_AddTipsID = 8000120;

	private Operation m_Operation;

	protected Operation operation
	{
		get
		{
			if (m_Operation == null)
			{
				m_Operation = GetComponent<Operation>();
			}
			return m_Operation;
		}
	}

	protected IOperator operater
	{
		get
		{
			if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				return PeSingleton<PeCreature>.Instance.mainPlayer.operateCmpt;
			}
			return null;
		}
	}

	protected override string tipsText
	{
		get
		{
			if (Operatable())
			{
				return base.tipsText + "\n" + PELocalization.GetString(m_AddTipsID);
			}
			return string.Empty;
		}
	}

	protected virtual void AddOperateCmd(CmdList cmdList)
	{
		cmdList.Add(m_ButtonName, OnDoOperate);
	}

	protected virtual void OnDoOperate()
	{
		if (Operatable())
		{
			operation.StartOperate(operater, m_Mask);
			GameUI.Instance.mItemOp.Hide();
		}
	}

	protected override void InitCmd(CmdList cmdList)
	{
		cmdList.Add("Turn", Turn90Degree);
		cmdList.Add("Get", OnGetBtn);
		if (Operatable())
		{
			AddOperateCmd(cmdList);
		}
	}

	public override bool CanCmd()
	{
		return base.CanCmd() && Operatable();
	}

	protected bool Operatable()
	{
		if (operation == null)
		{
			return false;
		}
		if (!operation.CanOperateMask(m_Mask))
		{
			return false;
		}
		OperateCmpt operateCmpt = PeSingleton<MainPlayer>.Instance.entity.operateCmpt;
		if (null != operateCmpt && operateCmpt.HasOperate)
		{
			return false;
		}
		MotionMgrCmpt motionMgr = PeSingleton<MainPlayer>.Instance.entity.motionMgr;
		if (null != motionMgr && (motionMgr.IsActionRunning(m_ActionType) || !motionMgr.CanDoAction(m_ActionType)))
		{
			return false;
		}
		return true;
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd() && Operatable())
		{
			OnDoOperate();
		}
	}

	protected virtual void Update()
	{
		if (operater != null && !operater.IsActionRunning(m_ActionType))
		{
			operation.StopOperate(operater, m_Mask);
		}
	}
}
