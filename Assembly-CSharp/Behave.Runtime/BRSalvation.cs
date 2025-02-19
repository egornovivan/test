using Pathea;
using PEIK;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BRSalvation), "RSalvation")]
public class BRSalvation : BTNormal
{
	private class Data
	{
		[Behave]
		public string animName = string.Empty;

		[Behave]
		public float startCarryUpTime;

		[Behave]
		public float carryUpTime;

		[Behave]
		public float startCarryDownTime;

		[Behave]
		public float carryDownTime;

		public bool m_BeCarry;

		public float m_StartCarryUpTime;

		public float m_StartCarryDownTime;
	}

	private Data m_Data;

	private RQSalvation m_Salvation;

	private Vector3 SalvatPos;

	private Interaction_Carry carry;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_Salvation == null)
		{
			m_Salvation = GetRequest(EReqType.Salvation) as RQSalvation;
		}
		if (m_Salvation == null)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_BeCarry = false;
		m_Data.m_StartCarryUpTime = 0f;
		m_Data.m_StartCarryDownTime = 0f;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!m_Salvation.CanRun())
		{
			return BehaveResult.Failure;
		}
		SetNpcAiType(ENpcAiType.RSalvation);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_Salvation.id);
		if (peEntity == null)
		{
			return BehaveResult.Failure;
		}
		PeTrans peTrans = peEntity.peTrans;
		if (peTrans == null)
		{
			return BehaveResult.Failure;
		}
		NpcCmpt npcCmpt = peEntity.NpcCmpt;
		if (npcCmpt == null)
		{
			return BehaveResult.Failure;
		}
		Motion_Move motionMove = peEntity.motionMove;
		if (motionMove == null)
		{
			return BehaveResult.Failure;
		}
		if (GetBool("SquatTreat"))
		{
			SetBool("SquatTreat", value: false);
			return BehaveResult.Running;
		}
		if (m_Salvation.carry)
		{
			if (PEUtil.SqrMagnitudeH(base.position, peTrans.position) > 1f)
			{
				MoveToPosition(peTrans.position, SpeedState.Run, avoid: false);
				if (Stucking())
				{
					SetPosition(peTrans.position);
				}
				return BehaveResult.Running;
			}
			if (!m_Data.m_BeCarry)
			{
				MoveToPosition(peTrans.position + peTrans.trans.forward * 0.3f, SpeedState.Walk, avoid: false);
			}
			if (m_Salvation.m_Direction == Vector3.zero)
			{
				m_Salvation.m_Direction = base.transform.position - peTrans.trans.position;
				m_Salvation.m_Direction.y = 0f;
			}
			if (m_Salvation.m_Direction == Vector3.zero)
			{
				m_Salvation.m_Direction = peTrans.trans.forward;
				m_Salvation.m_Direction.y = 0f;
			}
			peEntity.motionMgr.SetMaskState(PEActionMask.Cutscene, state: false);
			FaceDirection(m_Salvation.m_Direction);
			motionMove.RotateTo(m_Salvation.m_Direction);
			if (Vector3.Angle(peTrans.trans.forward, base.transform.forward) < 5f)
			{
				if (m_Data.m_StartCarryUpTime < float.Epsilon)
				{
					m_Data.m_StartCarryUpTime = Time.time;
					StopMove();
					SetBool("Carry", value: true);
					npcCmpt.MountID = base.entity.Id;
					npcCmpt.Battle = ENpcBattle.Passive;
					carry = new Interaction_Carry();
					carry.Init(base.entity.transform, peTrans.transform);
				}
				if (Time.time - m_Data.m_StartCarryUpTime > m_Data.startCarryUpTime && !m_Data.m_BeCarry)
				{
					m_Data.m_BeCarry = true;
					if (carry != null)
					{
						carry.StartInteraction();
					}
				}
				if (Time.time - m_Data.m_StartCarryUpTime > m_Data.carryUpTime)
				{
					RemoveRequest(m_Salvation);
					base.entity.target.SetEnityCanAttack(canAttackOrNot: false);
					return BehaveResult.Success;
				}
			}
		}
		else
		{
			if (m_Data.m_StartCarryDownTime < float.Epsilon)
			{
				m_Data.m_StartCarryDownTime = Time.time;
				npcCmpt.MountID = 0;
				npcCmpt.Battle = ENpcBattle.Defence;
				SetBool("Carry", value: false);
				if (carry != null)
				{
					carry.EndInteraction();
				}
			}
			if (Time.time - m_Data.m_StartCarryDownTime > m_Data.startCarryDownTime)
			{
			}
			if (Time.time - m_Data.m_StartCarryDownTime > m_Data.carryDownTime)
			{
				RemoveRequest(m_Salvation);
				base.entity.target.SetEnityCanAttack(canAttackOrNot: true);
				return BehaveResult.Success;
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		m_Salvation = null;
	}
}
