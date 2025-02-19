using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTDoubt), "Doubt")]
public class BTDoubt : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public string[] doubts = new string[0];

		public float m_LastCDTime;
	}

	private Data m_Data;

	private bool m_Face;

	private bool m_DoubtAnim;

	private PeEntity m_Doubt;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
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
		if (m_Data.doubts == null || m_Data.doubts.Length <= 0)
		{
			return BehaveResult.Failure;
		}
		if (m_Doubt == null)
		{
			m_Doubt = GetDoubtTarget();
		}
		if (m_Doubt == null)
		{
			return BehaveResult.Failure;
		}
		m_Face = false;
		m_DoubtAnim = false;
		m_Data.m_LastCDTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		Vector3 vector = m_Doubt.position - base.transform.position;
		if (!m_Face)
		{
			m_Face = PEUtil.IsScopeAngle(vector, base.transform.forward, Vector3.up, -75f, 75f);
		}
		if (!m_Face)
		{
			FaceDirection(vector);
		}
		else
		{
			FaceDirection(Vector3.zero);
			if (!m_DoubtAnim)
			{
				m_DoubtAnim = true;
				SetBool(m_Data.doubts[Random.Range(0, m_Data.doubts.Length)], value: true);
			}
			else if (!GetBool("Afraiding"))
			{
				return BehaveResult.Success;
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		m_Doubt = null;
	}
}
