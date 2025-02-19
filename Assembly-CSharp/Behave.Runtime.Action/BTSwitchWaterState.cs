using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTSwitchWaterState), "SwitchWaterState")]
public class BTSwitchWaterState : BTAttackBase
{
	private class Data
	{
		[Behave]
		public float downHeight;

		[Behave]
		public float upHeight;

		[Behave]
		public string downAnim = string.Empty;

		[Behave]
		public string upAnim = string.Empty;
	}

	private Data m_Data;

	private float m_StartTime;

	private Vector3 m_TargetPosition;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.IsDeepWater && !base.entity.monster.WaterSurface)
		{
			return BehaveResult.Success;
		}
		if (base.attackEnemy.IsShallowWater && base.entity.monster.WaterSurface)
		{
			return BehaveResult.Success;
		}
		m_StartTime = Time.time;
		m_TargetPosition = Vector3.zero;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		if (GetBool("Jumping"))
		{
			return BehaveResult.Running;
		}
		if (base.attackEnemy.IsDeepWater && base.entity.monster.WaterSurface)
		{
			SetBool(m_Data.downAnim, value: true);
			base.entity.monster.WaterSurface = false;
			if (Time.time - m_StartTime > 6f)
			{
				return BehaveResult.Failure;
			}
			return BehaveResult.Running;
		}
		if (base.attackEnemy.IsShallowWater && !base.entity.monster.WaterSurface)
		{
			float waterHeight;
			if (m_TargetPosition == Vector3.zero)
			{
				m_TargetPosition = PEUtil.GetRandomPositionInWater(base.attackEnemy.position, base.position - base.attackEnemy.position, 10f, 15f, 0f, 0.5f, -90f, 90f);
				if (m_TargetPosition == Vector3.zero)
				{
					return BehaveResult.Failure;
				}
				if (PEUtil.GetWaterSurfaceHeight(m_TargetPosition, out waterHeight))
				{
					m_TargetPosition.y = waterHeight - m_Data.upHeight;
				}
			}
			if (PEUtil.GetWaterSurfaceHeight(base.position, out waterHeight))
			{
				MoveToPosition(m_TargetPosition, SpeedState.Run);
				if (Mathf.Abs(waterHeight - base.position.y - m_Data.upHeight) < 0.5f)
				{
					MoveToPosition(Vector3.zero);
					SetBool(m_Data.upAnim, value: true);
					base.entity.monster.WaterSurface = true;
				}
			}
			if (Time.time - m_StartTime > 6f)
			{
				return BehaveResult.Failure;
			}
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
