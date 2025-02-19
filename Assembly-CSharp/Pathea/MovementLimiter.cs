using PETools;
using UnityEngine;

namespace Pathea;

public class MovementLimiter
{
	public delegate bool LifeFieldDelegate(ref Vector3 velocity);

	private PeTrans m_Trans;

	private Motion_Move_Motor m_MotionMove;

	private MovementField m_MovementField;

	public MovementLimiter(Motion_Move_Motor motor, MovementField movementField)
	{
		m_MotionMove = motor;
		m_Trans = m_MotionMove.GetComponent<PeTrans>();
		m_MovementField = movementField;
	}

	public bool CalculateVelocity(ref Vector3 velocity)
	{
		if (m_MovementField == MovementField.Land)
		{
			return CalculateVelocityLand(ref velocity);
		}
		if (m_MovementField == MovementField.water)
		{
			return CalculateVelocityWater(ref velocity);
		}
		if (m_MovementField == MovementField.Sky)
		{
			return CalculateVelocitySky(ref velocity);
		}
		if (m_MovementField == MovementField.Amphibian)
		{
			return CalculateVelocityAmphibian(ref velocity);
		}
		if (m_MovementField == MovementField.All)
		{
			return CalculateVelocityAll(ref velocity);
		}
		return true;
	}

	private bool CalculateVelocityLand(ref Vector3 velocity)
	{
		Vector3 vector = new Vector3(velocity.x, 0f, velocity.z);
		if (vector.sqrMagnitude > 0.0001f)
		{
			Vector3 vector2 = vector.normalized * Time.deltaTime * m_MotionMove.motor.maxForwardSpeed * 5f;
			Vector3 forwardBottom = m_Trans.forwardBottom;
			Vector3 forwardCenter = m_Trans.forwardCenter;
			Vector3 position = forwardBottom + vector2;
			Vector3 vector3 = forwardCenter + vector2;
			if (!PEUtil.CheckPositionOnGround(position, out var groundPosition, 5f, 5f, GameConfig.GroundLayer))
			{
				return false;
			}
			if (PEUtil.CheckPositionUnderWater(vector3))
			{
				if (RandomDungenMgrData.InDungeon)
				{
					return false;
				}
				if (PEUtil.CheckPositionOnGround(vector3 + vector.normalized * 32f, out groundPosition, 128f, 128f, GameConfig.GroundLayer) && PEUtil.CheckPositionUnderWater(groundPosition))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool CalculateVelocitySky(ref Vector3 velocity)
	{
		Vector3 vector = velocity;
		if (vector.sqrMagnitude > 0.0001f)
		{
			vector = vector.normalized * Time.deltaTime * m_MotionMove.motor.maxForwardSpeed * 10f;
			Vector3 v = m_Trans.position + vector;
			if (PEUtil.GetWaterSurfaceHeight(v, out var _) && velocity.y < -1E-45f)
			{
				velocity.y = 0f;
			}
		}
		return true;
	}

	private bool CalculateVelocityWater(ref Vector3 velocity)
	{
		if (velocity.sqrMagnitude > 0.0001f)
		{
			float maxForwardSpeed = m_MotionMove.motor.maxForwardSpeed;
			Vector3 vector = m_Trans.trans.TransformPoint(m_Trans.bound.center) - Vector3.up * m_Trans.bound.extents.y;
			Vector3 v = vector + Vector3.up * (m_Trans.bound.size.y + 0.5f) + Vector3.up * Time.deltaTime * maxForwardSpeed * 5f;
			if (!PEUtil.CheckPositionUnderWater(v))
			{
				if (velocity.y > float.Epsilon)
				{
					velocity.y = 0f;
				}
				Vector3 vector2 = new Vector3(velocity.x, 0f, velocity.z);
				if (vector2.sqrMagnitude > 0.0001f)
				{
					Vector3 direction = vector2.normalized * maxForwardSpeed * Time.deltaTime * 5f;
					float maxDistance = Mathf.Max(m_Trans.bound.extents.x, m_Trans.bound.extents.z) + direction.magnitude + 1f;
					if (Physics.Raycast(vector, direction, maxDistance, GameConfig.GroundLayer))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool CalculateVelocityAmphibian(ref Vector3 velocity)
	{
		return true;
	}

	private bool CalculateVelocityAll(ref Vector3 velocity)
	{
		return true;
	}
}
