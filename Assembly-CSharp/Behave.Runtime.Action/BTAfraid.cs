using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAfraid), "Afraid")]
public class BTAfraid : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public string[] afraids = new string[0];

		public float m_LastCDTime;
	}

	private Data m_Data;

	private bool m_Face;

	private bool m_AfraidAnim;

	private PeEntity m_Afraid;

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
		if (m_Data.afraids == null || m_Data.afraids.Length <= 0)
		{
			return BehaveResult.Failure;
		}
		if (m_Afraid == null)
		{
			m_Afraid = GetAfraidTarget();
		}
		if (m_Afraid == null)
		{
			return BehaveResult.Failure;
		}
		m_Face = false;
		m_AfraidAnim = false;
		m_Data.m_LastCDTime = Time.time;
		MoveToPosition(Vector3.zero);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (GetBool("BehaveWaiting"))
		{
			return BehaveResult.Running;
		}
		if (m_Afraid == null)
		{
			return BehaveResult.Failure;
		}
		Vector3 vector = m_Afraid.position - base.transform.position;
		if (!m_Face)
		{
			m_Face = PEUtil.IsScopeAngle(vector, base.transform.forward, Vector3.up, -15f, 15f);
		}
		if (!m_Face)
		{
			FaceDirection(vector);
		}
		else
		{
			FaceDirection(Vector3.zero);
			if (!m_AfraidAnim)
			{
				m_AfraidAnim = true;
				SetBool(m_Data.afraids[Random.Range(0, m_Data.afraids.Length)], value: true);
			}
			else if (!GetBool("BehaveWaiting"))
			{
				return BehaveResult.Success;
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		m_Afraid = null;
	}
}
