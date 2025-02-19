using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BRIdle), "RIdle")]
public class BRIdle : BTNormal
{
	private class Data
	{
		[Behave]
		public string idle;

		[Behave]
		public int Type;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		[Behave]
		public float startTime;

		[Behave]
		public float endTime;

		[Behave]
		public string[] relax;

		private float m_LastRelaxTime;

		private float m_CurRelaxTime;

		public bool m_End;

		public float m_StartTime;

		public float m_EndTime;

		public RQIdle m_Idle;

		public RQIdle.RQidleType RqType => (RQIdle.RQidleType)Type;

		public bool CanRelax()
		{
			return relax.Length > 0;
		}

		public bool CheckRelax()
		{
			if (Time.time - m_LastRelaxTime > m_CurRelaxTime)
			{
				m_LastRelaxTime = Time.time;
				m_CurRelaxTime = Random.Range(minTime, maxTime);
				return true;
			}
			return false;
		}

		public string RandomRelax()
		{
			return relax[Random.Range(0, relax.Length)];
		}

		public void InitRelax()
		{
			m_LastRelaxTime = 0f;
			m_CurRelaxTime = Random.Range(minTime, maxTime);
		}
	}

	private Data m_Data;

	private void InitIdletypeData()
	{
		if (m_Data.m_End)
		{
			return;
		}
		switch (m_Data.RqType)
		{
		case RQIdle.RQidleType.BeCarry:
			if (base.entity.biologyViewCmpt != null)
			{
				base.entity.biologyViewCmpt.ActivateCollider(value: false);
			}
			if (base.entity.motionMgr != null)
			{
				base.entity.motionMgr.FreezePhyState(GetType(), v: true);
			}
			if (base.entity.biologyViewCmpt != null)
			{
				base.entity.biologyViewCmpt.ActivateInjured(value: false);
			}
			if (base.entity.enityInfoCmpt != null)
			{
				base.entity.enityInfoCmpt.ShowName(show: false);
				base.entity.enityInfoCmpt.ShowMissionMark(show: false);
			}
			break;
		case RQIdle.RQidleType.Idle:
			break;
		case RQIdle.RQidleType.InjuredRest:
			if (base.entity.biologyViewCmpt != null)
			{
				base.entity.biologyViewCmpt.ActivateInjured(value: false);
			}
			if (base.entity.target != null)
			{
				base.entity.target.SetEnityCanAttack(canAttackOrNot: false);
			}
			if (base.entity.motionMgr != null)
			{
				base.entity.motionMgr.FreezePhyState(GetType(), v: true);
			}
			if (base.entity.enityInfoCmpt != null)
			{
				base.entity.enityInfoCmpt.ShowName(show: false);
				base.entity.enityInfoCmpt.ShowMissionMark(show: false);
			}
			SetBool("BeCarry", value: false);
			break;
		case RQIdle.RQidleType.InjuredSit:
			base.entity.motionMgr.SetMaskState(PEActionMask.Cutscene, state: true);
			break;
		case RQIdle.RQidleType.InjuredSitEX:
			break;
		case RQIdle.RQidleType.Lie:
			if (base.entity.biologyViewCmpt != null)
			{
				base.entity.biologyViewCmpt.ActivateInjured(value: false);
			}
			if (base.entity.target != null)
			{
				base.entity.target.SetEnityCanAttack(canAttackOrNot: false);
			}
			break;
		}
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.m_Idle = GetRequest(EReqType.Idle) as RQIdle;
		if (m_Data.m_Idle == null)
		{
			return BehaveResult.Failure;
		}
		if (!m_Data.m_Idle.CanRun())
		{
			return BehaveResult.Failure;
		}
		if (!m_Data.m_Idle.state.Equals(m_Data.idle))
		{
			return BehaveResult.Failure;
		}
		StopMove();
		m_Data.m_End = false;
		m_Data.m_StartTime = Time.time;
		m_Data.m_EndTime = Time.time;
		m_Data.InitRelax();
		SetBool(m_Data.m_Idle.state, value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (GetRequest(EReqType.Idle) is RQIdle rQIdle && !rQIdle.CanRun())
		{
			return BehaveResult.Success;
		}
		InitIdletypeData();
		if (!GameConfig.IsMultiMode && base.IsOnVCCarrier)
		{
			GetOff();
		}
		if (m_Data.m_Idle != null && Time.time - m_Data.m_StartTime > m_Data.startTime)
		{
			if (!(GetRequest(EReqType.Idle) is RQIdle obj) || !m_Data.m_Idle.Equals(obj))
			{
				if (!m_Data.m_End)
				{
					SetBool(m_Data.m_Idle.state, value: false);
					m_Data.m_EndTime = Time.time;
					m_Data.m_End = true;
					if (m_Data.RqType == RQIdle.RQidleType.InjuredRest && base.entity.target != null)
					{
						base.entity.target.SetEnityCanAttack(canAttackOrNot: true);
					}
					if (m_Data.RqType == RQIdle.RQidleType.InjuredSit)
					{
						base.entity.motionMgr.SetMaskState(PEActionMask.Cutscene, state: false);
					}
					if (m_Data.RqType == RQIdle.RQidleType.Lie)
					{
						if (base.entity.biologyViewCmpt != null)
						{
							base.entity.biologyViewCmpt.ActivateInjured(value: true);
						}
						if (base.entity.target != null)
						{
							base.entity.target.SetEnityCanAttack(canAttackOrNot: true);
						}
					}
				}
				else
				{
					if (Time.time - m_Data.m_EndTime > m_Data.endTime)
					{
						if (m_Data.RqType == RQIdle.RQidleType.InjuredRest)
						{
							if (base.entity.motionMgr != null)
							{
								base.entity.motionMgr.FreezePhyState(GetType(), v: false);
							}
							if (base.entity.enityInfoCmpt != null)
							{
								base.entity.enityInfoCmpt.ShowName(show: true);
								base.entity.enityInfoCmpt.ShowMissionMark(show: true);
								base.entity.peTrans.SetModel(base.existent);
							}
						}
						RemoveRequest(m_Data.m_Idle);
						m_Data.m_Idle = null;
						return BehaveResult.Success;
					}
					StopMove();
				}
			}
			else if (m_Data.CanRelax() && m_Data.CheckRelax())
			{
				SetBool(m_Data.RandomRelax(), value: true);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return;
		}
		RQIdle rQIdle = GetRequest(EReqType.Idle) as RQIdle;
		if (rQIdle == null || rQIdle.CanRun())
		{
			if (rQIdle == null && m_Data.m_Idle != null && GetBool(m_Data.m_Idle.state))
			{
				SetBool(m_Data.m_Idle.state, value: false);
			}
			m_Data.m_Idle = null;
		}
	}
}
