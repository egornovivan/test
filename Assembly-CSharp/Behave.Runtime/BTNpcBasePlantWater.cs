using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBasePlantWater), "NpcBasePlantWater")]
public class BTNpcBasePlantWater : BTNormal
{
	private class Data
	{
		[Behave]
		public string waterAnim;

		[Behave]
		public float waterTime;

		[Behave]
		public float waterEndTime;

		[Behave]
		public float waterRadius;

		[Behave]
		public float waterWaitTime;

		public bool m_Water;

		public bool m_Start;

		public float m_StartTime;
	}

	private FarmWorkInfo m_Work;

	private FarmWorkInfo m_CurWork;

	private Vector3 WalkPos;

	private Data m_Data;

	private bool mCanWater;

	private bool Reached(FarmWorkInfo work)
	{
		if (PEUtil.SqrMagnitudeH(work.m_Pos, base.position) < m_Data.waterRadius * m_Data.waterRadius)
		{
			return true;
		}
		return false;
	}

	private void PlantMoveTo(Vector3 pos, SpeedState state = SpeedState.Walk, bool avoid = true)
	{
		MoveToPosition(pos, state, avoid);
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
		if (!CSUtils.FarmWaterReady(base.entity))
		{
			return BehaveResult.Failure;
		}
		m_Work = CSUtils.FindPlantToWater(base.entity);
		if (m_Work == null)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_Water = false;
		m_Data.m_Start = false;
		m_Data.m_StartTime = 0f;
		m_CurWork = m_Work;
		mCanWater = false;
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
			PlantMoveTo(m_CurWork.m_Pos, SpeedState.Run, avoid: false);
			if (!Stucking())
			{
				return BehaveResult.Running;
			}
			if (!IsReached(base.position, m_CurWork.m_Pos, Is3D: false, m_Data.waterRadius))
			{
				SetPosition(m_CurWork.m_Pos);
			}
			mCanWater = true;
		}
		else
		{
			PlantMoveTo(Vector3.zero, SpeedState.Walk, avoid: false);
			if (!m_Data.m_Start)
			{
				if (m_CurWork != null && !CSUtils.FarmWaterEnough(base.entity, m_CurWork.m_Plant))
				{
					return BehaveResult.Failure;
				}
				SetBool(m_Data.waterAnim, value: true);
				m_Data.m_Start = true;
				m_Data.m_StartTime = Time.time;
				SetNpcState(ENpcState.Watering);
			}
			else if (Time.time - m_Data.m_StartTime >= m_Data.waterTime)
			{
				if (!m_Data.m_Water)
				{
					m_Data.m_Water = CSUtils.TryWater(m_CurWork, base.entity);
				}
				if (!m_Data.m_Water)
				{
					SetNpcState(ENpcState.UnKnown);
					return BehaveResult.Failure;
				}
				if (Time.time - m_Data.m_StartTime >= m_Data.waterEndTime)
				{
					m_Data.m_Water = false;
					m_Data.m_Start = false;
					m_Data.m_StartTime = 0f;
					mCanWater = false;
					m_CurWork = null;
					m_Work = CSUtils.FindPlantToWater(base.entity);
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
			CSUtils.ReturnWaterPlant(base.entity, m_Work);
			m_Work = null;
		}
	}
}
