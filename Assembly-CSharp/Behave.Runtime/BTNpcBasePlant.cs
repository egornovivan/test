using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBasePlant), "NpcBasePlant")]
public class BTNpcBasePlant : BTNormal
{
	private class Data
	{
		[Behave]
		public string plantAnim;

		[Behave]
		public float plantTime;

		[Behave]
		public float plantEndTime;

		[Behave]
		public float plantRadius;

		[Behave]
		public float plantWaitTime;

		public bool m_Plant;

		public bool m_Start;

		public float m_StartTime;
	}

	private FarmWorkInfo m_Work;

	private FarmWorkInfo m_CurWork;

	private Vector3 WalkPos;

	private Data m_Data;

	private bool Reached(FarmWorkInfo work)
	{
		if (PEUtil.SqrMagnitudeH(work.m_Pos, base.position) < m_Data.plantRadius * m_Data.plantRadius)
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
		if (!ContainsTitle(ENpcTitle.Plant))
		{
			return BehaveResult.Failure;
		}
		if (!CSUtils.FarmPlantReady(base.entity))
		{
			return BehaveResult.Failure;
		}
		m_Work = CSUtils.FindPlantPosNewChunk(base.entity);
		if (m_Work == null)
		{
			return BehaveResult.Failure;
		}
		m_CurWork = m_Work;
		m_Data.m_Plant = false;
		m_Data.m_Start = false;
		m_Data.m_StartTime = 0f;
		SetNpcState(ENpcState.Prepare);
		return BehaveResult.Running;
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
		if (!ContainsTitle(ENpcTitle.Plant))
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Failure;
		}
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		SetNpcAiType(ENpcAiType.NpcBaseJobFarmer_Plant);
		if (!Reached(m_CurWork))
		{
			MoveToPosition(m_CurWork.m_Pos, SpeedState.Walk, avoid: false);
			if (!Stucking() && !IsReached(m_CurWork.m_Pos, base.position))
			{
				return BehaveResult.Running;
			}
			SetPosition(m_CurWork.m_Pos);
		}
		else
		{
			MoveToPosition(Vector3.zero);
			if (!m_Data.m_Start)
			{
				if (CSUtils.CheckFarmPlantAround(m_CurWork.m_Pos, base.entity))
				{
					if (!CSUtils.FarmPlantReady(base.entity))
					{
						return BehaveResult.Failure;
					}
					SetBool(m_Data.plantAnim, value: true);
					m_Data.m_Start = true;
					m_Data.m_StartTime = Time.time;
					SetNpcState(ENpcState.Plant);
				}
				else
				{
					m_Work = CSUtils.FindPlantPosNewChunk(base.entity);
					m_CurWork = m_Work;
					if (m_Work == null)
					{
						return BehaveResult.Failure;
					}
				}
			}
			else if (Time.time - m_Data.m_StartTime >= m_Data.plantTime)
			{
				if (!m_Data.m_Plant)
				{
					m_Data.m_Plant = CSUtils.TryPlant(m_CurWork, base.entity);
				}
				if (!m_Data.m_Plant)
				{
					SetNpcState(ENpcState.UnKnown);
					return BehaveResult.Failure;
				}
				if (Time.time - m_Data.m_StartTime >= m_Data.plantEndTime)
				{
					m_Data.m_Plant = false;
					m_Data.m_Start = false;
					m_Data.m_StartTime = 0f;
					m_CurWork = null;
					if (CSUtils.FarmPlantReady(base.entity))
					{
						if (m_Work != null)
						{
							m_CurWork = CSUtils.FindPlantPosSameChunk(m_Work, base.entity);
						}
						if (m_CurWork == null)
						{
							CSUtils.ReturnCleanChunk(m_Work, base.entity);
							m_Work = CSUtils.FindPlantPosNewChunk(base.entity);
							m_CurWork = m_Work;
						}
					}
					if (m_CurWork == null)
					{
						SetNpcState(ENpcState.UnKnown);
						return BehaveResult.Failure;
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
			CSUtils.ReturnCleanChunk(m_Work, base.entity);
			m_Work = null;
		}
	}
}
