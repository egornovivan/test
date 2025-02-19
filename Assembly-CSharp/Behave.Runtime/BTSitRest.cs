using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTSitRest), "SitRest")]
public class BTSitRest : BTNormal
{
	private class Data
	{
		[Behave]
		public string idle = string.Empty;

		[Behave]
		public float prob;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		[Behave]
		public float relaxProb;

		[Behave]
		public float relaxTime;

		[Behave]
		public string[] relax = new string[0];

		public float m_StartIdleTime;

		public float m_CurrentIdleTime;

		public float m_StartRestTime;

		public ChatTeamDb m_chatTeamDb;
	}

	private Data m_Data;

	private Enemy m_Escape;

	private Enemy m_Threat;

	private bool DoingRelax()
	{
		for (int i = 0; i < m_Data.relax.Length; i++)
		{
			if (base.entity.animCmpt != null && base.entity.animCmpt.animator != null && base.entity.animCmpt.ContainsParameter(m_Data.relax[i]) && base.entity.animCmpt.animator.GetBool(m_Data.relax[i]))
			{
				return true;
			}
		}
		return false;
	}

	private void EndRelax()
	{
		for (int i = 0; i < m_Data.relax.Length; i++)
		{
			if (GetBool(m_Data.relax[i]))
			{
				SetBool(m_Data.relax[i], value: false);
			}
		}
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.m_chatTeamDb == null)
		{
			m_Data.m_chatTeamDb = TeamDb.LoadchatTeamDb(base.entity);
		}
		if (m_Data.m_chatTeamDb == null)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || base.NpcJob != ENpcJob.Resident)
		{
			return BehaveResult.Failure;
		}
		float num = PEUtil.Magnitude(base.position, base.Creater.Assembly.Position);
		if (num > base.Creater.Assembly.Radius)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartRestTime = Time.time;
		m_Data.m_StartIdleTime = Time.time;
		m_Data.m_CurrentIdleTime = Random.Range(m_Data.minTime, m_Data.maxTime);
		StopMove();
		SetBool(m_Data.idle, value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || !base.IsNpcBase || !Enemy.IsNullOrInvalid(base.attackEnemy) || base.NpcJob != ENpcJob.Resident)
		{
			if (DoingRelax())
			{
				EndRelax();
				return BehaveResult.Running;
			}
			return BehaveResult.Failure;
		}
		if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
		{
			return BehaveResult.Running;
		}
		float num = PEUtil.Magnitude(base.position, base.Creater.Assembly.Position);
		if (num > base.Creater.Assembly.Radius)
		{
			if (DoingRelax())
			{
				EndRelax();
				return BehaveResult.Running;
			}
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.lineType != ELineType.TeamChat)
		{
			if (DoingRelax())
			{
				EndRelax();
				return BehaveResult.Running;
			}
			return BehaveResult.Success;
		}
		FaceDirection(m_Data.m_chatTeamDb.CenterPos - base.position);
		if (Time.time - m_Data.m_StartRestTime > m_Data.relaxTime)
		{
			m_Data.m_StartRestTime = Time.time;
			if (!GetBool("BehaveWaiting") && !GetBool("Leisureing") && Random.value < m_Data.relaxProb)
			{
				SetBool(m_Data.relax[Random.Range(0, m_Data.relax.Length)], value: true);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartIdleTime > float.Epsilon)
		{
			m_Data.m_StartRestTime = 0f;
			m_Data.m_StartIdleTime = 0f;
			m_Data.m_CurrentIdleTime = 0f;
			if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
			{
				SetBool("Interrupt", value: true);
			}
			EndRelax();
			SetBool(m_Data.idle, value: false);
			m_Data.m_chatTeamDb = null;
		}
	}
}
