using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBaseClearPlant), "NpcBaseClearPlant")]
public class BTNpcBaseClearPlant : BTNormal
{
	private class Data
	{
		[Behave]
		public string clearAnim;

		[Behave]
		public float clearTime;

		[Behave]
		public float clearEndTime;

		[Behave]
		public float clearRadius;

		[Behave]
		public float clearWaitTime;

		public bool m_Clear;

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
		if (PEUtil.SqrMagnitudeH(work.m_Pos, base.position) < m_Data.clearRadius * m_Data.clearRadius)
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
		m_Work = CSUtils.FindPlantRemove(base.entity);
		if (m_Work == null)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_Clear = false;
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
		if (!mCanWater)
		{
			mCanWater = Reached(m_CurWork);
			MoveToPosition(m_CurWork.m_Pos, SpeedState.Run, avoid: false);
			if (!Stucking())
			{
				return BehaveResult.Running;
			}
			if (!IsReached(base.position, m_CurWork.m_Pos, Is3D: false, m_Data.clearRadius))
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
				SetBool(m_Data.clearAnim, value: true);
				m_Data.m_Start = true;
				m_Data.m_StartTime = Time.time;
				SetNpcState(ENpcState.Gain);
			}
			else if (Time.time - m_Data.m_StartTime >= m_Data.clearTime)
			{
				if (!m_Data.m_Clear)
				{
					bool flag = CSUtils.TryRemove(m_CurWork, base.entity);
					m_Data.m_Clear = true;
					if (!flag)
					{
						SetNpcState(ENpcState.UnKnown);
						return BehaveResult.Failure;
					}
				}
				if (Time.time - m_Data.m_StartTime >= m_Data.clearEndTime)
				{
					m_Data.m_Clear = false;
					m_Data.m_Start = false;
					m_Data.m_StartTime = 0f;
					mCanWater = false;
					m_CurWork = null;
					m_Work = CSUtils.FindPlantRemove(base.entity);
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
