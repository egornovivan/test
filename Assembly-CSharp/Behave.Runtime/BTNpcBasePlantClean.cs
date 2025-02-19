using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBasePlantClean), "NpcBasePlantClean")]
public class BTNpcBasePlantClean : BTNormal
{
	private class Data
	{
		[Behave]
		public string cleanAnim;

		[Behave]
		public float cleanTime;

		[Behave]
		public float cleanEndTime;

		[Behave]
		public float cleanRadius;

		[Behave]
		public float cleanWaitTime;

		public bool m_Clean;

		public bool m_Start;

		public float m_StartTime;
	}

	private FarmWorkInfo m_Work;

	private FarmWorkInfo m_CurWork;

	private Vector3 WalkPos;

	private bool mCanWater;

	private Data m_Data;

	private void CleanMoveTo(Vector3 pos, SpeedState state = SpeedState.Walk, bool avoid = true)
	{
		MoveToPosition(pos, state, avoid);
	}

	private bool Reached(FarmWorkInfo work)
	{
		if (PEUtil.SqrMagnitudeH(work.m_Pos, base.position) < m_Data.cleanRadius * m_Data.cleanRadius)
		{
			return true;
		}
		return false;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Farmer)
		{
			return BehaveResult.Failure;
		}
		if (!ContainsTitle(ENpcTitle.Manage))
		{
			return BehaveResult.Failure;
		}
		if (!CSUtils.FarmCleanReady(base.entity))
		{
			return BehaveResult.Failure;
		}
		m_Work = CSUtils.FindPlantToClean(base.entity);
		if (m_Work == null)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_Clean = false;
		m_Data.m_Start = false;
		m_Data.m_StartTime = 0f;
		mCanWater = false;
		m_CurWork = m_Work;
		SetNpcState(ENpcState.Prepare);
		return BehaveResult.Success;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Farmer)
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Failure;
		}
		if (!ContainsTitle(ENpcTitle.Manage))
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Failure;
		}
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		if (!mCanWater)
		{
			mCanWater = Reached(m_CurWork);
			CleanMoveTo(m_CurWork.m_Pos, SpeedState.Run, avoid: false);
			if (!Stucking())
			{
				return BehaveResult.Running;
			}
			if (!IsReached(base.position, m_CurWork.m_Pos, Is3D: false, m_Data.cleanRadius))
			{
				SetPosition(m_CurWork.m_Pos);
			}
			mCanWater = true;
		}
		else
		{
			CleanMoveTo(Vector3.zero, SpeedState.Walk, avoid: false);
			if (!m_Data.m_Start)
			{
				if (!CSUtils.FarmCleanEnough(base.entity, m_CurWork.m_Plant))
				{
					return BehaveResult.Failure;
				}
				SetBool(m_Data.cleanAnim, value: true);
				m_Data.m_Start = true;
				m_Data.m_StartTime = Time.time;
				SetNpcState(ENpcState.Weeding);
			}
			else if (Time.time - m_Data.m_StartTime >= m_Data.cleanTime)
			{
				if (!m_Data.m_Clean)
				{
					m_Data.m_Clean = CSUtils.TryClean(m_CurWork, base.entity);
				}
				if (!m_Data.m_Clean)
				{
					SetNpcState(ENpcState.UnKnown);
					return BehaveResult.Failure;
				}
				if (Time.time - m_Data.m_StartTime >= m_Data.cleanEndTime)
				{
					m_Data.m_Clean = false;
					m_Data.m_Start = false;
					m_Data.m_StartTime = 0f;
					mCanWater = false;
					m_CurWork = null;
					m_Work = CSUtils.FindPlantToClean(base.entity);
					m_CurWork = m_Work;
					if (m_CurWork == null)
					{
						SetNpcState(ENpcState.UnKnown);
						return BehaveResult.Success;
					}
				}
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_Work != null)
		{
			CSUtils.ReturnCleanPlant(base.entity, m_Work);
			m_Work = null;
		}
	}
}
