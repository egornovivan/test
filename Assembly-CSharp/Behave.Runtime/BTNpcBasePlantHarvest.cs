using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBasePlantHarvest), "NpcBasePlantHarvest")]
public class BTNpcBasePlantHarvest : BTNormal
{
	private class Data
	{
		[Behave]
		public string harvestAnim;

		[Behave]
		public float harvestTime;

		[Behave]
		public float harvestEndTime;

		[Behave]
		public float harvestRadius;

		[Behave]
		public float harvestWaitTime;

		public bool m_Harvest;

		public bool m_Start;

		public float m_StartTime;
	}

	private FarmWorkInfo m_Work;

	private FarmWorkInfo m_CurWork;

	private Vector3 WalkPos;

	private bool mCanWater;

	private Data m_Data;

	private bool Reached(FarmWorkInfo work)
	{
		if (PEUtil.SqrMagnitudeH(work.m_Pos, base.position) < m_Data.harvestRadius * m_Data.harvestRadius)
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
		if (!ContainsTitle(ENpcTitle.Harvest))
		{
			return BehaveResult.Failure;
		}
		m_Work = CSUtils.FindPlantGet(base.entity);
		if (m_Work == null)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_Harvest = false;
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
		if (!ContainsTitle(ENpcTitle.Harvest))
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
			MoveToPosition(m_CurWork.m_Pos, SpeedState.Run, avoid: false);
			if (!Stucking())
			{
				return BehaveResult.Running;
			}
			if (!IsReached(base.position, m_CurWork.m_Pos, Is3D: false, m_Data.harvestRadius))
			{
				SetPosition(m_CurWork.m_Pos);
			}
			mCanWater = true;
		}
		else
		{
			MoveToPosition(Vector3.zero, SpeedState.Walk, avoid: false);
			if (!m_Data.m_Start)
			{
				SetBool(m_Data.harvestAnim, value: true);
				m_Data.m_Start = true;
				m_Data.m_StartTime = Time.time;
				SetNpcState(ENpcState.Gain);
			}
			else if (Time.time - m_Data.m_StartTime >= m_Data.harvestTime)
			{
				if (!m_Data.m_Harvest)
				{
					bool flag = CSUtils.TryHarvest(m_CurWork, base.entity);
					m_Data.m_Harvest = true;
					if (!flag)
					{
						SetNpcState(ENpcState.UnKnown);
						return BehaveResult.Failure;
					}
				}
				if (Time.time - m_Data.m_StartTime >= m_Data.harvestEndTime)
				{
					m_Data.m_Harvest = false;
					m_Data.m_Start = false;
					m_Data.m_StartTime = 0f;
					mCanWater = false;
					m_CurWork = null;
					m_Work = CSUtils.FindPlantGet(base.entity);
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
			CSUtils.ReturnHarvestPlant(base.entity, m_Work);
			m_Work = null;
		}
	}
}
