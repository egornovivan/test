using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveToTarget), "MoveToTarget")]
public class BTMoveToTarget : BTNormal
{
	private class Data
	{
		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		public Vector3 m_Local;
	}

	private Data m_Data;

	private Vector3 m_Local;

	private Vector3 GetLocalCenterPos()
	{
		if (PEUtil.CheckPositionOnGround(base.attackEnemy.position + m_Local, out var groundPosition, 32f, 32f, GameConfig.GroundLayer))
		{
			return groundPosition + base.entity.tr.up * base.entity.maxHeight * 0.5f;
		}
		return base.attackEnemy.position + m_Local + Vector3.up * base.entity.maxHeight * 0.5f;
	}

	private Vector3 GetRandomPosistion(Enemy e, IAttackPositive attack)
	{
		if (base.field == MovementField.water)
		{
			Vector3 centerUp = base.attackEnemy.entityTarget.peTrans.centerUp;
			Vector3 vector = base.attackEnemy.entityTarget.peTrans.position;
			float minHeight = VFVoxelWater.self.UpToWaterSurface(centerUp.x, centerUp.y, centerUp.z);
			float maxHeight = VFVoxelWater.self.UpToWaterSurface(vector.x, vector.y, vector.z);
			if (base.attackEnemy.IsDeepWater)
			{
				return PEUtil.GetRandomPositionInWater(e.position, base.position - e.position, attack.MinRange, attack.MaxRange, minHeight, maxHeight, -90f, 90f);
			}
			return PEUtil.GetRandomPositionInWater(e.position, base.position - e.position, attack.MinRange, attack.MaxRange, 0f, base.entity.maxHeight * 0.5f, -90f, 90f);
		}
		return PEUtil.GetRandomPositionOnGround(e.position, attack.MinRange, attack.MaxRange);
	}

	private Vector3 GetLocalPos(Enemy e, IAttackPositive attack)
	{
		if (!PEUtil.IsBlocked(e.entity, e.entityTarget) && !Stucking())
		{
			m_Local = Vector3.zero;
		}
		else if (m_Local == Vector3.zero || PEUtil.IsBlocked(e.entityTarget, GetLocalCenterPos()))
		{
			Vector3 local = Vector3.zero;
			for (int i = 0; i < 5; i++)
			{
				Vector3 randomPosistion = GetRandomPosistion(e, attack);
				Vector3 vector = randomPosistion + base.entity.tr.up * base.entity.maxHeight * 0.5f;
				if (!PEUtil.IsBlocked(e.entityTarget, vector))
				{
					local = randomPosistion - e.position;
					break;
				}
			}
			m_Local = local;
		}
		if (m_Local != Vector3.zero)
		{
			return m_Local;
		}
		return Vector3.zero;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy == null || base.attackEnemy.Attack == null)
		{
			return BehaveResult.Success;
		}
		if (!(base.attackEnemy.Attack is IAttackPositive attackPositive) || attackPositive.ReadyAttack(base.attackEnemy))
		{
			return BehaveResult.Success;
		}
		Vector3 localPos = GetLocalPos(base.attackEnemy, attackPositive);
		Vector3 pos = base.attackEnemy.position + localPos + base.attackEnemy.velocity.normalized;
		if (attackPositive is IAttackTop)
		{
			pos += Vector3.up * (attackPositive.MinHeight + attackPositive.MaxHeight) * 0.5f;
		}
		float sqrDistanceLogic = base.attackEnemy.SqrDistanceLogic;
		if (sqrDistanceLogic > attackPositive.MaxRange * attackPositive.MaxRange)
		{
			FaceDirection(Vector3.zero);
			MoveDirection(Vector3.zero);
			MoveToPosition(pos, SpeedState.Run);
		}
		else if (sqrDistanceLogic < attackPositive.MinRange * attackPositive.MinRange)
		{
			MoveToPosition(Vector3.zero);
			MoveDirection(-base.attackEnemy.Direction, SpeedState.Retreat);
			FaceDirection(base.attackEnemy.Direction);
		}
		else if (attackPositive.IsBlocked(base.attackEnemy))
		{
			MoveToPosition(pos, SpeedState.Run);
		}
		else
		{
			MoveToPosition(Vector3.zero);
			MoveDirection(Vector3.zero);
			if (!attackPositive.IsInAngle(base.attackEnemy))
			{
				FaceDirection(base.attackEnemy.Direction);
			}
		}
		return BehaveResult.Success;
	}
}
