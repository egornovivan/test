using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTSwitchStateSky), "SwitchStateSky")]
public class BTSwitchStateSky : BTNormal
{
	private class Data
	{
		[Behave]
		public float height;
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
		bool flag = base.hasAttackEnemy || base.TDObj != null || base.TDpos != Vector3.zero;
		if (GetBool("Fly") == flag)
		{
			return BehaveResult.Failure;
		}
		if (flag)
		{
			m_FlyPosition = base.position + Vector3.up * m_Data.height;
		}
		else
		{
			m_FlyPosition = GetLandPos();
			if (PEUtil.CheckPositionUnderWater(m_FlyPosition))
			{
				return BehaveResult.Failure;
			}
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
		if (base.hasAttackEnemy || base.TDObj != null || base.TDpos != Vector3.zero)
		{
			if (GetBool("Fly") && GetBool("Rising"))
			{
				return BehaveResult.Running;
			}
			ActivateGravity(value: false);
			MoveDirection(m_FlyPosition - base.position, SpeedState.Run);
			Fly(value: true);
			if (PEUtil.Magnitude(base.position, m_FlyPosition) <= 1f || Time.time - m_StartTime > 10f)
			{
				return BehaveResult.Success;
			}
		}
		else
		{
			ActivateGravity(value: true);
			MoveDirection(m_FlyPosition - base.position, SpeedState.Run);
			if ((PEUtil.Magnitude(base.position, m_FlyPosition) <= 1f || base.grounded || Time.time - m_StartTime > 10f) && GetBool("Fly"))
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
