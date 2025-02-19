using System.Collections.Generic;
using UnityEngine;

namespace Pathea;

public abstract class PEAction
{
	[HideInInspector]
	public List<PEActionType> m_PauseActions = new List<PEActionType>();

	private AnimatorCmpt m_Anim;

	public abstract PEActionType ActionType { get; }

	public bool pauseAction => m_PauseActions.Count > 0;

	public MotionMgrCmpt motionMgr { get; set; }

	protected PeEntity entity => motionMgr.Entity;

	protected Motion_Move move => entity.motionMove;

	protected PeTrans trans => entity.peTrans;

	protected SkAliveEntity skillCmpt => entity.aliveEntity;

	protected EquipmentCmpt equipCmpt => entity.equipmentCmpt;

	protected PackageCmpt packageCmpt => entity.packageCmpt;

	protected BiologyViewCmpt viewCmpt => entity.biologyViewCmpt;

	protected IKCmpt ikCmpt => entity.IKCmpt;

	protected AnimatorCmpt anim
	{
		get
		{
			if (null == m_Anim)
			{
				m_Anim = entity.animCmpt;
				if (null != m_Anim)
				{
					m_Anim.AnimEvtString += OnAnimEvent;
				}
			}
			return m_Anim;
		}
	}

	public virtual void PreDoAction()
	{
	}

	public virtual void DoAction(PEActionParam para)
	{
	}

	public virtual void ResetAction(PEActionParam para)
	{
	}

	public virtual void PauseAction()
	{
	}

	public virtual void ContinueAction()
	{
	}

	public virtual void OnModelBuild()
	{
	}

	public virtual void OnModelDestroy()
	{
		motionMgr.EndImmediately(ActionType);
	}

	public virtual void EndAction()
	{
		motionMgr.EndImmediately(ActionType);
	}

	public virtual void EndImmediately()
	{
	}

	public virtual bool CanDoAction(PEActionParam para)
	{
		return true;
	}

	public virtual bool Update()
	{
		return true;
	}

	protected virtual void OnAnimEvent(string eventParam)
	{
	}
}
