using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTFlyAndLand), "FlyAndLand")]
public class BTFlyAndLand : BTNormal
{
	private class Data
	{
		[Behave]
		public bool fly;
	}

	private Data m_Data;

	private Vector3 m_FlyPosition;

	private float m_StartTime;

	private float m_StartLandTime;

	private Vector3 GetLandPos()
	{
		if (Physics.Raycast(base.position + Vector3.up, Vector3.down, out var hitInfo, 256f, 71680))
		{
			return hitInfo.point;
		}
		return Vector3.zero;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (IsFly() == m_Data.fly)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.fly)
		{
			m_FlyPosition = base.position;
		}
		else
		{
			m_FlyPosition = GetLandPos();
		}
		m_StartTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.fly)
		{
			ActivateGravity(value: false);
			MoveDirection(m_FlyPosition - base.position);
			Fly(value: true);
			if (GetBool("Fly") && GetBool("Rising"))
			{
				return BehaveResult.Running;
			}
			if (PEUtil.Magnitude(base.position, m_FlyPosition) < 1f)
			{
				return BehaveResult.Success;
			}
		}
		else
		{
			ActivateGravity(value: true);
			MoveDirection(m_FlyPosition - base.position);
			if ((PEUtil.Magnitude(base.position, m_FlyPosition) < 1f || base.grounded) && GetBool("Fly"))
			{
				Fly(value: false);
				m_StartLandTime = Time.time;
				return BehaveResult.Running;
			}
			if (!GetBool("Fly"))
			{
				if (GetBool("Landing") || Time.time - m_StartLandTime < 0.25f)
				{
					return BehaveResult.Running;
				}
				return BehaveResult.Success;
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_StartTime > float.Epsilon)
		{
			MoveDirection(Vector3.zero);
			m_StartTime = 0f;
		}
	}
}
