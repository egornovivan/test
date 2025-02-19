using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTakeFood), "TakeFood")]
public class BTTakeFood : BTNormal
{
	private class Data
	{
		[Behave]
		public string anim = string.Empty;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		public float m_Time;

		public float m_StartTime;
	}

	private Data m_Data;

	private bool m_Arrived;

	private bool m_Face;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Food == null)
		{
			return BehaveResult.Failure;
		}
		m_Arrived = false;
		m_Face = false;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Food == null)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.m_StartTime > float.Epsilon && m_Data.m_Time > float.Epsilon && Time.time - m_Data.m_StartTime > m_Data.m_Time)
		{
			return BehaveResult.Success;
		}
		if (!m_Arrived)
		{
			m_Arrived = PEUtil.SqrMagnitude(base.entity.tr, base.entity.bounds, base.entity.Food.tr, base.entity.Food.bounds, is3D: false) <= 1f;
		}
		if (m_Arrived)
		{
			MoveToPosition(Vector3.zero);
			if (!m_Face)
			{
				m_Face = PEUtil.IsScopeAngle(base.entity.Food.position - base.entity.position, base.transform.forward, Vector3.up, -15f, 15f);
			}
			if (m_Face)
			{
				FaceDirection(Vector3.zero);
				if (m_Data.m_StartTime < float.Epsilon)
				{
					SetBool(m_Data.anim, value: true);
					m_Data.m_StartTime = Time.time;
					m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
				}
				return BehaveResult.Running;
			}
			FaceDirection(base.entity.Food.position - base.entity.position);
			return BehaveResult.Running;
		}
		MoveToPosition(base.entity.Food.position, SpeedState.Run);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			base.entity.Food = null;
			SetBool(m_Data.anim, value: false);
			m_Data.m_Time = 0f;
			m_Data.m_StartTime = 0f;
		}
	}
}
