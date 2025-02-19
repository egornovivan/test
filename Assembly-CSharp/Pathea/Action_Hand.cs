using System;
using PEIK;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Hand : PEAction
{
	private float startDis = 0.3f;

	private float maxDis = 1f;

	private float startAngle = 5f;

	private Vector3 offset = new Vector3(-0.4f, 0f, -0.2f);

	[SerializeField]
	private float tryHandTime = 5f;

	[SerializeField]
	private float rotateScale = 0.2f;

	[SerializeField]
	private int tryHandCount = 2;

	[SerializeField]
	private float tryRotateAngle = 60f;

	private MotionMgrCmpt m_TargetMotion;

	private Action_Handed m_TargetAction;

	private PeTrans m_TargetTrans;

	private bool m_AnimMatch;

	private Interaction_Hand m_Interaction;

	private Action_Move m_MoveAction;

	private float m_StartTime;

	private bool m_EndAction;

	private int m_CurTryCount;

	public override PEActionType ActionType => PEActionType.Hand;

	public bool moveable => m_AnimMatch && m_TargetAction != null && m_TargetAction.standAnimEnd;

	public override void DoAction(PEActionParam para = null)
	{
		if (null == base.trans || null == base.move)
		{
			return;
		}
		PEActionParamN pEActionParamN = para as PEActionParamN;
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(pEActionParamN.n);
		if (!(null != peEntity))
		{
			return;
		}
		m_TargetMotion = peEntity.motionMgr;
		if (!(null != m_TargetMotion))
		{
			return;
		}
		m_TargetAction = m_TargetMotion.GetAction<Action_Handed>();
		m_TargetTrans = peEntity.peTrans;
		if (!(null == m_TargetTrans))
		{
			if (m_MoveAction == null)
			{
				m_MoveAction = base.motionMgr.GetAction<Action_Move>();
			}
			m_AnimMatch = false;
			base.motionMgr.SetMaskState(PEActionMask.Hand, state: true);
			PEActionParamN param = PEActionParamN.param;
			param.n = base.motionMgr.Entity.Id;
			m_TargetMotion.DoActionImmediately(PEActionType.Handed, param);
			m_EndAction = false;
			m_StartTime = Time.time;
			m_CurTryCount = 0;
			if (PeGameMgr.IsMulti && base.entity == PeSingleton<PeCreature>.Instance.mainPlayer)
			{
				peEntity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp, 13, base.entity.Id);
			}
		}
	}

	public override bool Update()
	{
		if (null == base.trans || m_TargetAction == null || null == m_TargetMotion)
		{
			return true;
		}
		Vector3 vector = m_TargetTrans.position + m_TargetTrans.rotation * offset;
		float num = Vector3.Angle(m_TargetTrans.forward, base.trans.forward);
		if (m_AnimMatch)
		{
			if (Vector3.SqrMagnitude(base.trans.position - vector) > maxDis * maxDis)
			{
				EndAction();
				return true;
			}
		}
		else
		{
			if (Vector3.SqrMagnitude(base.trans.position - vector) > startDis * startDis)
			{
				base.move.MoveTo(vector);
			}
			else if (num > startAngle)
			{
				base.move.RotateTo(m_TargetTrans.forward);
			}
			else
			{
				m_AnimMatch = true;
				m_TargetAction.OnHand();
				m_Interaction = new Interaction_Hand();
				m_Interaction.Init(base.motionMgr.Entity.transform, m_TargetMotion.Entity.transform);
				m_Interaction.StartInteraction();
				if (m_MoveAction != null)
				{
					m_MoveAction.rotateSpeedScale = rotateScale;
				}
			}
			if (Time.time - m_StartTime > tryHandTime)
			{
				if (m_CurTryCount > tryHandCount)
				{
					EndAction();
				}
				else
				{
					m_StartTime = Time.time;
					m_CurTryCount++;
					m_TargetMotion.Entity.motionMove.RotateTo(Quaternion.AngleAxis(tryRotateAngle, m_TargetTrans.existent.up) * m_TargetTrans.forward);
				}
			}
		}
		return m_EndAction;
	}

	private void OnEndAction(bool immediately)
	{
		base.motionMgr.SetMaskState(PEActionMask.Hand, state: false);
		if (m_Interaction != null)
		{
			m_Interaction.EndInteraction(immediately);
			m_Interaction = null;
		}
		if (null != m_TargetMotion)
		{
			m_TargetMotion.EndAction(PEActionType.Handed);
		}
		if (m_MoveAction != null)
		{
			m_MoveAction.rotateSpeedScale = 1f;
		}
	}

	public override void EndAction()
	{
		OnEndAction(immediately: false);
		m_EndAction = true;
	}

	public override void EndImmediately()
	{
		OnEndAction(immediately: true);
	}
}
