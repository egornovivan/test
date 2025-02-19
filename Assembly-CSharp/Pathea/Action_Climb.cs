using System;
using PETools;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Climb : PEAction
{
	private const float UpperHeight = 2f;

	private const float LowerHeight = -0.2f;

	private const float AnimUpDis = 1.75f;

	private const float HumanWith = 0.37f;

	private const float StepHeight = 0.8f;

	private const float DisToLadder = 0.37f;

	public HumanPhyCtrl m_PhyCtrl;

	private float m_ClimbDir;

	private bool m_EndClimb;

	private bool m_CheckLadder;

	private ItemScript_ClimbLadder m_CurrentLadder;

	private ItemScript_ClimbLadder m_LowerLadder;

	private ItemScript_ClimbLadder m_UpperLadder;

	public override PEActionType ActionType => PEActionType.Climb;

	public void SetMoveDir(float moveDir, bool checkLadder = false)
	{
		m_ClimbDir = moveDir;
		m_CheckLadder = checkLadder;
	}

	public override void DoAction(PEActionParam para = null)
	{
		m_ClimbDir = 0f;
		base.motionMgr.SetMaskState(PEActionMask.Climb, state: true);
		if (null != base.trans)
		{
			PEActionParamVQN pEActionParamVQN = para as PEActionParamVQN;
			Quaternion q = pEActionParamVQN.q;
			switch ((ItemScript_ClimbLadder.OpSide)pEActionParamVQN.n)
			{
			case ItemScript_ClimbLadder.OpSide.Both:
				if (Vector3.Angle(base.trans.forward, q * Vector3.forward) > 90f)
				{
					base.trans.rotation = Quaternion.AngleAxis(180f, Vector3.up) * q;
				}
				else
				{
					base.trans.rotation = q;
				}
				break;
			case ItemScript_ClimbLadder.OpSide.Backward:
				base.trans.rotation = Quaternion.AngleAxis(180f, Vector3.up) * q;
				break;
			case ItemScript_ClimbLadder.OpSide.Forward:
				base.trans.rotation = q;
				break;
			}
			base.trans.position = pEActionParamVQN.vec;
		}
		if (null != base.anim)
		{
			base.anim.ResetTrigger("ResetFullBody");
			base.anim.ResetTrigger("LadderUpEnd");
			base.anim.SetTrigger("LadderClimb");
		}
		base.motionMgr.FreezePhyState(GetType(), v: true);
		if (null != m_PhyCtrl)
		{
			m_PhyCtrl.velocity = Vector3.zero;
			m_PhyCtrl.CancelMoveRequest();
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = false;
		}
		m_EndClimb = false;
	}

	public override bool Update()
	{
		UpdateLadderState();
		return m_EndClimb;
	}

	public override void EndAction()
	{
		base.motionMgr.SetMaskState(PEActionMask.Climb, state: false);
		if (null != base.trans)
		{
			base.trans.position += -0.37f * base.trans.forward;
		}
		base.motionMgr.FreezePhyState(GetType(), v: false);
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = true;
		}
		m_EndClimb = true;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Climb, state: false);
		if (null != base.trans)
		{
			base.trans.position += -0.37f * base.trans.forward;
		}
		base.motionMgr.FreezePhyState(GetType(), v: false);
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = true;
		}
	}

	public void UpdateLadderState()
	{
		if (null != base.trans)
		{
			base.trans.position += base.anim.m_LastMove;
		}
		if (!(null != base.anim))
		{
			return;
		}
		if (m_CheckLadder)
		{
			if (m_ClimbDir < -0.5f)
			{
				RaycastHit[] array = Physics.RaycastAll(base.trans.position - 0.2f * base.trans.forward, Vector3.down, 0.8f, -1, QueryTriggerInteraction.Ignore);
				for (int i = 0; i < array.Length; i++)
				{
					if (null == array[i].collider.transform.GetComponentInChildren<ItemScript_ClimbLadder>())
					{
						base.motionMgr.EndAction(ActionType);
						return;
					}
				}
				m_CurrentLadder = GetLadder(base.trans.position, base.trans.forward);
				m_LowerLadder = GetLadder(base.trans.position + -0.2f * Vector3.up, base.trans.forward);
				if (null == m_CurrentLadder && null == m_LowerLadder)
				{
					base.motionMgr.EndAction(ActionType);
				}
			}
			else if (m_ClimbDir > 0.5f)
			{
				RaycastHit[] array2 = Physics.RaycastAll(base.trans.position + 1.75f * Vector3.up - 0.2f * base.trans.forward, Vector3.up, 0.8f, -1, QueryTriggerInteraction.Ignore);
				for (int j = 0; j < array2.Length; j++)
				{
					if (!array2[j].collider.isTrigger && null == array2[j].collider.transform.GetComponentInChildren<ItemScript_ClimbLadder>())
					{
						base.anim.SetFloat("LadderClimbDir", 0f);
						return;
					}
				}
				m_CurrentLadder = GetLadder(base.trans.position + 1.75f * Vector3.up, base.trans.forward);
				m_UpperLadder = GetLadder(base.trans.position + 2f * Vector3.up, base.trans.forward);
				if (null == m_CurrentLadder && null == m_UpperLadder && null != base.anim)
				{
					base.anim.SetTrigger("LadderUpEnd");
				}
			}
			else
			{
				m_CurrentLadder = GetLadder(base.trans.position, base.trans.forward);
				m_UpperLadder = GetLadder(base.trans.position + 2f * Vector3.up, base.trans.forward);
				if (null == m_CurrentLadder && null == m_UpperLadder)
				{
					base.motionMgr.EndImmediately(ActionType);
				}
			}
		}
		base.anim.SetFloat("LadderClimbDir", m_ClimbDir);
	}

	private ItemScript_ClimbLadder GetLadder(Vector3 pos, Vector3 dir)
	{
		ItemScript_ClimbLadder itemScript_ClimbLadder = null;
		RaycastHit[] hits = Physics.RaycastAll(pos - 0.5f * dir, dir, 1f);
		hits = PEUtil.SortHitInfo(hits);
		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit raycastHit = hits[i];
			if (raycastHit.distance < 1f)
			{
				itemScript_ClimbLadder = raycastHit.collider.transform.GetComponent<ItemScript_ClimbLadder>();
				if (null != itemScript_ClimbLadder)
				{
					break;
				}
			}
		}
		return itemScript_ClimbLadder;
	}

	private void FixedHeight()
	{
		if (null != base.trans)
		{
			m_CurrentLadder = GetLadder(base.trans.position + 0.4f * Vector3.up, base.trans.forward);
			if (null != m_CurrentLadder && null != m_CurrentLadder)
			{
				Vector3 position = m_CurrentLadder.transform.position - base.trans.forward * m_CurrentLadder.m_LadderWith;
				float num = (base.trans.position.y - m_CurrentLadder.transform.position.y + 0.4f) % 0.8f - 0.4f;
				position.y = base.trans.position.y - num;
				base.trans.position = position;
			}
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType))
		{
			switch (eventParam)
			{
			case "EndClimb":
				base.motionMgr.EndAction(ActionType);
				break;
			case "OnHandLadder":
				FixedHeight();
				break;
			}
		}
	}
}
