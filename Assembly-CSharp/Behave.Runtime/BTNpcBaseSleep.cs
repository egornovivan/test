using System.Collections.Generic;
using Pathea;
using Pathea.Operate;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBaseSleep), "NpcBaseSleep")]
public class BTNpcBaseSleep : BTNormal
{
	private class Data
	{
		[Behave]
		public float minComfort;

		[Behave]
		public float maxComfort;

		[Behave]
		public float minSleepTime;

		[Behave]
		public float maxSleepTime;

		[Behave]
		public string sleepTimeSlots;

		[Behave]
		public int buffId;

		[Behave]
		public string Anim;

		public List<CheckSlot> slots = new List<CheckSlot>();

		public float m_StartSleepTime;

		public float m_CurSleepTime;

		public PESleep m_Sleep;

		private bool m_Init;

		public void Init(PeEntity npc)
		{
			if (m_Init)
			{
				return;
			}
			if (npc.NpcCmpt != null)
			{
				npc.NpcCmpt.npcCheck.ClearSleepSlots();
			}
			if (sleepTimeSlots != string.Empty)
			{
				string[] array = PEUtil.ToArrayString(sleepTimeSlots, ',');
				string[] array2 = array;
				foreach (string str in array2)
				{
					float[] array3 = PEUtil.ToArraySingle(str, '_');
					if (array3.Length == 2)
					{
						CheckSlot checkSlot = new CheckSlot(array3[0], array3[1]);
						slots.Add(checkSlot);
						if (npc.NpcCmpt != null)
						{
							npc.NpcCmpt.npcCheck.AddSleepSlots(checkSlot.minTime, checkSlot.maxTime);
						}
					}
				}
			}
			m_Init = true;
		}

		public bool IsTimeSlot(float timeSlot)
		{
			return slots.Find((CheckSlot ret) => ret.InSlot(timeSlot)) != null;
		}
	}

	private Data m_Data;

	private bool Iscomfort;

	private EThinkingType mSleepTh = EThinkingType.Sleep;

	private bool StopSleepAction()
	{
		SetNpcState(ENpcState.UnKnown);
		if (base.Operator != null && !base.Operator.Equals(null) && base.Operator.Operate != null && !base.Operator.Operate.Equals(null) && base.Operator.Operate.ContainsOperator(base.Operator))
		{
			return base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Sleep);
		}
		return true;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.Init(base.entity);
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Sleep))
		{
			return BehaveResult.Failure;
		}
		if (!NpcThinkDb.CanDo(base.entity, mSleepTh))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.lineType != ELineType.TeamSleep)
		{
			return BehaveResult.Failure;
		}
		if (base.Sleep == null || base.Sleep.Equals(null) || base.Operator == null || base.Operator.Equals(null))
		{
			return BehaveResult.Failure;
		}
		if (!base.Sleep.CanOperateMask(EOperationMask.Sleep))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt != null && !base.entity.NpcCmpt.IsUncomfortable && base.entity.NpcCmpt.lineType != ELineType.TeamSleep)
		{
			base.entity.NpcCmpt.ThinkAgent.RemoveThink(mSleepTh);
			return BehaveResult.Failure;
		}
		PEBed pEBed = base.Sleep as PEBed;
		m_Data.m_Sleep = pEBed.GetStartOperate(EOperationMask.Sleep) as PESleep;
		if (m_Data.m_Sleep == null || m_Data.Equals(null))
		{
			return BehaveResult.Failure;
		}
		Iscomfort = base.entity.NpcCmpt.IsUncomfortable;
		base.entity.NpcCmpt.AddTalkInfo(ENpcTalkType.BaseNpc_strike_sleep, ENpcSpeakType.Both, canLoop: true);
		SetNpcState(ENpcState.Rest);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase || base.Sleep == null || base.Sleep.Equals(null) || base.Operator == null || base.Operator.Equals(null) || m_Data.m_Sleep == null || m_Data.Equals(null))
		{
			if (StopSleepAction())
			{
				return BehaveResult.Failure;
			}
			return BehaveResult.Running;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Sleep))
		{
			if (StopSleepAction())
			{
				return BehaveResult.Failure;
			}
			return BehaveResult.Running;
		}
		if (!NpcThinkDb.CanDoing(base.entity, mSleepTh))
		{
			if (StopSleepAction())
			{
				return BehaveResult.Failure;
			}
			return BehaveResult.Running;
		}
		SetNpcAiType(ENpcAiType.NpcBaseSleep);
		if (!base.Sleep.CanOperateMask(EOperationMask.Sleep) && !base.Sleep.ContainsOperator(base.Operator))
		{
			if (StopSleepAction())
			{
				return BehaveResult.Failure;
			}
			return BehaveResult.Running;
		}
		if (!base.Sleep.ContainsOperator(base.Operator))
		{
			if (!base.Sleep.CanOperate(base.transform))
			{
				bool flag = PEUtil.IsUnderBlock(base.entity);
				bool flag2 = PEUtil.IsForwardBlock(base.entity, base.entity.peTrans.forward, 2f);
				if (flag)
				{
					SetPosition(m_Data.m_Sleep.Trans.position);
				}
				else
				{
					MoveToPosition(m_Data.m_Sleep.Trans.position, SpeedState.Run, avoid: false);
				}
				if (flag2)
				{
					SetPosition(m_Data.m_Sleep.Trans.position);
				}
				if (Stucking())
				{
					SetPosition(m_Data.m_Sleep.Trans.position);
				}
				if (IsReached(base.position, m_Data.m_Sleep.Trans.position))
				{
					SetPosition(m_Data.m_Sleep.Trans.position);
				}
			}
			else
			{
				MoveToPosition(Vector3.zero);
				m_Data.m_StartSleepTime = Time.time;
				m_Data.m_CurSleepTime = Random.Range(m_Data.minSleepTime, m_Data.maxSleepTime);
				PEBed pEBed = base.Sleep as PEBed;
				m_Data.m_Sleep = pEBed.GetStartOperate(EOperationMask.Sleep) as PESleep;
				if (m_Data.m_Sleep != null && !m_Data.Equals(null))
				{
					SetPosition(m_Data.m_Sleep.Trans.position);
					m_Data.m_Sleep.StartOperate(base.Operator, EOperationMask.Sleep);
					SetNpcState(ENpcState.Rest);
				}
			}
		}
		else
		{
			if (Iscomfort)
			{
				float attribute = GetAttribute(AttribType.Comfort);
				float attribute2 = GetAttribute(AttribType.ComfortMax);
				if (attribute >= attribute2 * 0.9f)
				{
					m_Data.m_Sleep.StopOperate(base.Operator, EOperationMask.Sleep);
					base.entity.NpcCmpt.ThinkAgent.RemoveThink(mSleepTh);
					return BehaveResult.Success;
				}
			}
			if (!Iscomfort && base.entity.NpcCmpt.lineType != ELineType.TeamSleep)
			{
				if (StopSleepAction())
				{
					base.entity.NpcCmpt.ThinkAgent.RemoveThink(mSleepTh);
					return BehaveResult.Success;
				}
				return BehaveResult.Running;
			}
			if (base.Operator.Operate == null || base.Operator.Operate.Equals(null) || !base.Operator.IsActionRunning(PEActionType.Sleep))
			{
				return BehaveResult.Failure;
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (base.Creater != null && base.Creater.Assembly != null && base.NpcJobStae == ENpcState.Rest && base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator))
		{
			base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Sleep);
		}
		SetNpcState(ENpcState.UnKnown);
	}
}
