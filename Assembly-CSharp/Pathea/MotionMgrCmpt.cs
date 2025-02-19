using System;
using System.Collections.Generic;

namespace Pathea;

public class MotionMgrCmpt : PeCmpt, IPeMsg
{
	private class RunningAction
	{
		private MotionMgrCmpt m_MotionMgr;

		private PEAction m_Action;

		private ActionRelationData m_RelationData;

		private PEActionParam m_Para;

		private bool m_WaitRelation;

		private bool m_DoAction;

		public RunningAction(MotionMgrCmpt mmc)
		{
			m_MotionMgr = mmc;
		}

		public void Do(PEAction action, ActionRelationData relationData, PEActionParam para, bool doImmediately = false)
		{
			m_Action = action;
			m_RelationData = relationData;
			m_Para = para;
			m_WaitRelation = !doImmediately;
			m_DoAction = true;
			action.PreDoAction();
			CheckRelation(doImmediately);
			TryDoAction();
		}

		public bool UpdateAction()
		{
			if (m_WaitRelation)
			{
				if (!CheckExecution(m_RelationData))
				{
					return false;
				}
				if (m_Action.pauseAction)
				{
					return false;
				}
				m_WaitRelation = false;
			}
			if (m_DoAction)
			{
				m_Action.DoAction(m_Para);
				m_DoAction = false;
			}
			if (!m_Action.Update())
			{
				return false;
			}
			for (int i = 0; i < m_RelationData.m_PauseAction.Count; i++)
			{
				m_MotionMgr.ContinueAction(m_RelationData.m_PauseAction[i], m_Action.ActionType);
			}
			m_MotionMgr.OnActionEnd(m_Action.ActionType);
			return true;
		}

		public void EndAction()
		{
			m_Action.EndImmediately();
			for (int i = 0; i < m_RelationData.m_PauseAction.Count; i++)
			{
				m_MotionMgr.ContinueAction(m_RelationData.m_PauseAction[i], m_Action.ActionType);
			}
		}

		private void CheckRelation(bool doImmediately)
		{
			for (int i = 0; i < m_RelationData.m_PauseAction.Count; i++)
			{
				m_MotionMgr.PauseAction(m_RelationData.m_PauseAction[i], m_Action.ActionType);
			}
			for (int j = 0; j < m_RelationData.m_EndAction.Count; j++)
			{
				if (doImmediately)
				{
					m_MotionMgr.EndImmediately(m_RelationData.m_EndAction[j]);
				}
				else
				{
					m_MotionMgr.EndAction(m_RelationData.m_EndAction[j]);
				}
			}
			for (int k = 0; k < m_RelationData.m_EndImmediately.Count; k++)
			{
				m_MotionMgr.EndImmediately(m_RelationData.m_EndImmediately[k]);
			}
		}

		private void TryDoAction()
		{
			if (m_WaitRelation)
			{
				if (!CheckExecution(m_RelationData) || m_Action.pauseAction)
				{
					return;
				}
				m_WaitRelation = false;
			}
			m_Action.DoAction(m_Para);
			m_DoAction = false;
		}

		private bool CheckExecution(ActionRelationData data)
		{
			for (int i = 0; i < data.m_EndAction.Count; i++)
			{
				if (m_MotionMgr.IsActionRunning(data.m_EndAction[i]))
				{
					return false;
				}
			}
			return true;
		}
	}

	private PEAction[] m_ActionDic;

	private bool[] m_MaskArray;

	private SkAliveEntity m_SkEntity;

	private BiologyViewCmpt m_View;

	private RunningAction[] m_RunningAction;

	private Stack<RunningAction> m_RunningActionPool;

	private bool m_HasActiveAction;

	private bool m_FreezeCol;

	private List<Type> mFreezePhyRequestType;

	private bool mFreezePhyStateForSystem;

	private bool mFreezePhyByNet;

	public bool FreezeCol
	{
		get
		{
			return m_FreezeCol;
		}
		set
		{
			m_FreezeCol = value;
			if (null != m_View)
			{
				m_View.ActivateCollider(!m_FreezeCol);
			}
		}
	}

	public bool freezePhyState { get; set; }

	public bool freezePhyStateForSystem => mFreezePhyStateForSystem;

	public bool isInAimState => IsActionRunning(PEActionType.AimEquipHold) || IsActionRunning(PEActionType.GunHold) || IsActionRunning(PEActionType.BowHold);

	public event Action<PEActionType> onActionStart;

	public event Action<PEActionType> onActionEnd;

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
			UpdatePhy();
			FreezeCol = m_FreezeCol;
			Invoke("UpdateActionStateWhenBuildMode", (!PeGameMgr.IsMulti) ? 0.1f : 1f);
			break;
		case EMsg.View_Prefab_Destroy:
			UpdateActionStateWhenModeDestroy();
			break;
		case EMsg.View_FirstPerson:
			HideEquipFirstPerson((bool)args[0]);
			break;
		}
	}

	private RunningAction GetRunningAction()
	{
		RunningAction runningAction = null;
		if (m_RunningActionPool.Count > 0)
		{
			runningAction = m_RunningActionPool.Pop();
		}
		if (runningAction == null)
		{
			runningAction = new RunningAction(this);
		}
		return runningAction;
	}

	public void FreezePhyByNet(bool v)
	{
		mFreezePhyByNet = v;
		UpdatePhy();
	}

	public void FreezePhyState(Type type, bool v)
	{
		if (v)
		{
			if (!mFreezePhyRequestType.Contains(type))
			{
				mFreezePhyRequestType.Add(type);
			}
		}
		else
		{
			mFreezePhyRequestType.Remove(type);
		}
		UpdatePhy();
	}

	public void FreezePhySteateForSystem(bool v)
	{
		mFreezePhyStateForSystem = v;
		UpdatePhy();
	}

	private void UpdatePhy()
	{
		freezePhyState = mFreezePhyRequestType.Count > 0 || mFreezePhyStateForSystem || mFreezePhyByNet;
		if (null != m_View)
		{
			m_View.ActivatePhysics(!freezePhyState);
			m_View.ActivateRagdollPhysics(!mFreezePhyStateForSystem);
		}
	}

	public override void Awake()
	{
		base.Awake();
		mFreezePhyStateForSystem = true;
		FreezeCol = false;
		m_ActionDic = new PEAction[63];
		m_RunningAction = new RunningAction[63];
		m_RunningActionPool = new Stack<RunningAction>();
		mFreezePhyRequestType = new List<Type>();
		m_MaskArray = new bool[56];
		for (int i = 0; i < m_MaskArray.Length; i++)
		{
			m_MaskArray[i] = false;
		}
	}

	public override void Start()
	{
		base.Start();
		m_SkEntity = base.Entity.aliveEntity;
		m_View = base.Entity.biologyViewCmpt;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_HasActiveAction = false;
		for (int i = 0; i < m_RunningAction.Length; i++)
		{
			if (m_RunningAction[i] != null)
			{
				m_HasActiveAction = true;
				if (m_RunningAction[i].UpdateAction())
				{
					m_RunningActionPool.Push(m_RunningAction[i]);
					m_RunningAction[i] = null;
				}
			}
		}
	}

	public void AddAction(PEAction action)
	{
		if (!HasAction(action.ActionType))
		{
			m_ActionDic[(int)action.ActionType] = action;
			action.motionMgr = this;
		}
	}

	public void RemoveAction(PEActionType type)
	{
		if (HasAction(type))
		{
			EndImmediately(type);
			m_ActionDic[(int)type] = null;
		}
	}

	public T GetAction<T>() where T : PEAction
	{
		for (int i = 0; i < m_ActionDic.Length; i++)
		{
			if (m_ActionDic[i] != null && m_ActionDic[i].GetType() == typeof(T))
			{
				return m_ActionDic[i] as T;
			}
		}
		return (T)null;
	}

	public void SetMaskState(PEActionMask mask, bool state)
	{
		m_MaskArray[(int)mask] = state;
	}

	public bool GetMaskState(PEActionMask mask)
	{
		return m_MaskArray[(int)mask];
	}

	public bool HasAction(PEActionType type)
	{
		return null != m_ActionDic[(int)type];
	}

	public bool IsActionRunning()
	{
		return m_HasActiveAction;
	}

	public bool IsActionRunning(PEActionType type)
	{
		return null != m_RunningAction[(int)type];
	}

	public bool IsActionPause(PEActionType type)
	{
		return HasAction(type) && m_ActionDic[(int)type].pauseAction;
	}

	public bool CanDoAction(PEActionType type, PEActionParam para = null)
	{
		if (HasAction(type))
		{
			if (IsActionRunning(type))
			{
				return true;
			}
			if (m_ActionDic[(int)type].CanDoAction(para))
			{
				ActionRelationData data = ActionRelationData.GetData(type);
				return CheckDepend(data);
			}
		}
		return false;
	}

	public bool DoAction(PEActionType type, PEActionParam para = null)
	{
		if (HasAction(type))
		{
			if (IsActionRunning(type))
			{
				m_ActionDic[(int)type].ResetAction(para);
				return true;
			}
			if (m_ActionDic[(int)type].CanDoAction(para))
			{
				ActionRelationData data = ActionRelationData.GetData(type);
				if (data == null)
				{
					return false;
				}
				if (CheckDepend(data))
				{
					if (null != m_SkEntity._net)
					{
						((SkNetworkInterface)m_SkEntity._net).SendDoAction(type, para);
					}
					if (this.onActionStart != null)
					{
						this.onActionStart(type);
					}
					RunningAction runningAction = GetRunningAction();
					runningAction.Do(m_ActionDic[(int)type], data, para);
					m_RunningAction[(int)type] = runningAction;
					return true;
				}
			}
		}
		return false;
	}

	public void DoActionImmediately(PEActionType type, PEActionParam para = null)
	{
		if (!HasAction(type))
		{
			return;
		}
		if (IsActionRunning(type))
		{
			m_ActionDic[(int)type].ResetAction(para);
			return;
		}
		if (null != m_SkEntity._net)
		{
			((SkNetworkInterface)m_SkEntity._net).SendDoAction(type, para);
		}
		ActionRelationData data = ActionRelationData.GetData(type);
		if (this.onActionStart != null)
		{
			this.onActionStart(type);
		}
		RunningAction runningAction = GetRunningAction();
		runningAction.Do(m_ActionDic[(int)type], data, para, doImmediately: true);
		m_RunningAction[(int)type] = runningAction;
	}

	private bool CheckDepend(ActionRelationData data)
	{
		if (data == null)
		{
			return false;
		}
		for (int i = 0; i < data.m_DependMask.Count; i++)
		{
			if (data.m_DependMask[i].maskValue != m_MaskArray[(int)data.m_DependMask[i].maskType])
			{
				return false;
			}
		}
		return true;
	}

	public void PauseAction(PEActionType beType, PEActionType tryPauseType)
	{
		if (HasAction(beType))
		{
			if (IsActionRunning(beType) && !m_ActionDic[(int)beType].pauseAction)
			{
				m_ActionDic[(int)beType].PauseAction();
			}
			if (!m_ActionDic[(int)beType].m_PauseActions.Contains(tryPauseType))
			{
				m_ActionDic[(int)beType].m_PauseActions.Add(tryPauseType);
			}
		}
	}

	public void ContinueAction(PEActionType beType, PEActionType tryPauseType)
	{
		if (HasAction(beType))
		{
			m_ActionDic[(int)beType].m_PauseActions.Remove(tryPauseType);
			if (IsActionRunning(beType) && !m_ActionDic[(int)beType].pauseAction)
			{
				m_ActionDic[(int)beType].ContinueAction();
			}
		}
	}

	public bool EndAction(PEActionType type)
	{
		if (IsActionRunning(type))
		{
			m_ActionDic[(int)type].EndAction();
			if (null != m_SkEntity._net && type != PEActionType.Wentfly && type != PEActionType.Knocked)
			{
				((SkNetworkInterface)m_SkEntity._net).SendEndAction(type);
			}
			return true;
		}
		return false;
	}

	public bool EndImmediately(PEActionType type)
	{
		if (IsActionRunning(type))
		{
			m_RunningActionPool.Push(m_RunningAction[(int)type]);
			m_RunningAction[(int)type].EndAction();
			m_RunningAction[(int)type] = null;
			if (null != m_SkEntity._net && type != PEActionType.Wentfly && type != PEActionType.Knocked)
			{
				((SkNetworkInterface)m_SkEntity._net).SendEndImmediately(type);
			}
			OnActionEnd(type);
			return true;
		}
		return false;
	}

	private void HideEquipFirstPerson(bool hide)
	{
		for (int i = 0; i < 63; i++)
		{
			if (m_ActionDic[i] != null && m_ActionDic[i] is iEquipHideAbleAction iEquipHideAbleAction2)
			{
				iEquipHideAbleAction2.hideEquipInactive = hide;
			}
		}
	}

	private void UpdateActionStateWhenBuildMode()
	{
		for (int i = 0; i < 63; i++)
		{
			if (m_RunningAction[i] != null && m_ActionDic[i] != null)
			{
				m_ActionDic[i].OnModelBuild();
			}
		}
	}

	private void UpdateActionStateWhenModeDestroy()
	{
		for (int i = 0; i < 63; i++)
		{
			if (m_RunningAction[i] != null && m_ActionDic[i] != null)
			{
				m_ActionDic[i].OnModelDestroy();
			}
		}
	}

	public void OnActionEnd(PEActionType type)
	{
		if (this.onActionEnd != null)
		{
			this.onActionEnd(type);
		}
	}
}
