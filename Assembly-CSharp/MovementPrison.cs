using Pathea;
using PETools;
using UnityEngine;

public class MovementPrison
{
	private PeEntity m_Entity;

	private PEMotorPhysics m_Motor;

	private Rigidbody m_Rigidbody;

	public MovementPrison(PeEntity entity, PEMotorPhysics motor, Rigidbody rigid)
	{
		m_Entity = entity;
		m_Motor = motor;
		m_Rigidbody = rigid;
	}

	public bool CalculateVelocity(ref Vector3 velocity)
	{
		if (m_Entity.Field == MovementField.Land)
		{
			return CalculateVelocityLand(ref velocity);
		}
		if (m_Entity.Field == MovementField.water)
		{
			return CalculateVelocityWater(ref velocity);
		}
		if (m_Entity.Field == MovementField.Sky)
		{
			return CalculateVelocitySky(ref velocity);
		}
		if (m_Entity.Field == MovementField.Amphibian)
		{
			return CalculateVelocityAmphibian(ref velocity);
		}
		if (m_Entity.Field == MovementField.All)
		{
			return CalculateVelocityAll(ref velocity);
		}
		return true;
	}

	private bool CalculateVelocityLand(ref Vector3 velocity)
	{
		Vector3 vector = new Vector3(velocity.x, 0f, velocity.z);
		if (m_Entity != null && m_Entity.peTrans != null && vector.sqrMagnitude > 0.0001f)
		{
			Vector3 vector2 = vector.normalized * Time.deltaTime * m_Motor.maxForwardSpeed * 5f;
			Vector3 forwardBottom = m_Entity.peTrans.forwardBottom;
			Vector3 forwardCenter = m_Entity.peTrans.forwardCenter;
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
			vector = vector.normalized * Time.deltaTime * m_Motor.maxForwardSpeed * 10f;
			Vector3 v = m_Entity.peTrans.position + vector;
			if (PEUtil.GetWaterSurfaceHeight(v, out var _))
			{
				if (velocity.y < -1E-45f)
				{
					velocity.y = 0f;
				}
				if (m_Rigidbody.velocity.y < -1E-45f)
				{
					velocity.y = 0f - m_Rigidbody.velocity.y;
				}
			}
		}
		return true;
	}

	private bool CalculateVelocityWater(ref Vector3 velocity)
	{
		if (m_Entity != null && m_Entity.peTrans != null && velocity.sqrMagnitude > 0.0001f)
		{
			float maxForwardSpeed = m_Motor.maxForwardSpeed;
			Vector3 vector = m_Entity.peTrans.trans.TransformPoint(m_Entity.peTrans.bound.center) - Vector3.up * m_Entity.peTrans.bound.extents.y;
			Vector3 vector2 = vector + Vector3.up * (m_Entity.peTrans.bound.size.y + 0.5f);
			Vector3 vector3 = Vector3.up * Time.deltaTime * maxForwardSpeed * 5f;
			if (!PEUtil.CheckPositionUnderWater(vector2 + vector3))
			{
				if (velocity.y > float.Epsilon)
				{
					velocity.y = 0f;
				}
				if (m_Rigidbody.velocity.y > float.Epsilon)
				{
					velocity.y = 0f - m_Rigidbody.velocity.y;
				}
				float num = VFVoxelWater.self.DownToWaterSurface(vector2.x, vector2.y, vector2.z);
				if (num > float.Epsilon)
				{
					velocity -= Vector3.up * Mathf.Clamp01(num);
				}
				Vector3 vector4 = new Vector3(velocity.x, 0f, velocity.z);
				if (vector4.sqrMagnitude > 0.0001f)
				{
					Vector3 direction = vector4.normalized * maxForwardSpeed * Time.deltaTime * 5f;
					float maxDistance = Mathf.Max(m_Entity.peTrans.bound.extents.x, m_Entity.peTrans.bound.extents.z) + direction.magnitude + 1f;
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
