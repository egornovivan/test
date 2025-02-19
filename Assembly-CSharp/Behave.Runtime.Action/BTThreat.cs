using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTThreat), "Threat")]
public class BTThreat : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public string[] threatStr = new string[0];

		public float m_StartTime;

		public float m_StartThreatTime;

		public float m_LastCDTime;
	}

	private Data m_Data;

	private PeEntity m_Threat;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.followEnemy))
		{
			m_Threat = base.followEnemy.entityTarget;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			m_Threat = base.attackEnemy.entityTarget;
		}
		if (m_Threat == null)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_LastCDTime < m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.threatStr == null || m_Data.threatStr.Length <= 0)
		{
			return BehaveResult.Failure;
		}
		StopMove();
		m_Data.m_StartTime = Time.time;
		m_Data.m_StartThreatTime = 0f;
		m_Data.m_LastCDTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_Threat == null)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.m_StartThreatTime <= float.Epsilon && Time.time - m_Data.m_StartTime > 5f)
		{
			return BehaveResult.Failure;
		}
		if (base.escapeEnemy != null)
		{
			return BehaveResult.Success;
		}
		if (m_Data.m_StartThreatTime > float.Epsilon)
		{
			if (Time.time - m_Data.m_StartThreatTime > 0.5f && !GetBool("Threating") && !GetBool("BehaveWaiting"))
			{
				return BehaveResult.Success;
			}
		}
		else
		{
			Vector3 vector = m_Threat.position - base.transform.position;
			if (!PEUtil.IsScopeAngle(vector, base.transform.forward, Vector3.up, -15f, 15f))
			{
				FaceDirection(vector);
			}
			else
			{
				FaceDirection(Vector3.zero);
				m_Data.m_StartThreatTime = Time.time;
				SetBool(m_Data.threatStr[Random.Range(0, m_Data.threatStr.Length)], value: true);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartThreatTime > float.Epsilon)
		{
			if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
			{
				SetBool("Interrupt", value: true);
			}
			m_Data.m_StartThreatTime = 0f;
		}
	}
}
