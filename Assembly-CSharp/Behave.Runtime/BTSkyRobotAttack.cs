using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTSkyRobotAttack), "SkyRobotAttack")]
public class BTSkyRobotAttack : BTNormal
{
	private class Data
	{
		[Behave]
		public float cdTime = 5f;

		[Behave]
		public int skillID;

		[Behave]
		public float minR;

		[Behave]
		public float maxR;

		[Behave]
		public float minH;

		[Behave]
		public float maxH;

		[Behave]
		public int targetID;

		[Behave]
		public float angle = 30f;

		public float mFindTime = 10f;

		private PeEntity mfollowEntity;

		public bool changrPos;

		public PeEntity followEntity
		{
			get
			{
				if (mfollowEntity == null)
				{
					mfollowEntity = PeSingleton<EntityMgr>.Instance.Get(targetID);
				}
				return mfollowEntity;
			}
		}

		public bool IsInFollowRadiu(Vector3 self, Vector3 targetPos, float radiu = 1f)
		{
			float num = PEUtil.SqrMagnitudeH(self, targetPos);
			return num < radiu * radiu;
		}
	}

	private Data m_Data;

	private static float _angle = 45f;

	private Vector3 m_LocalPos;

	private Vector3 m_AttcakPos;

	private static float x1 = 5f;

	private static float x2 = 50f;

	private static float x3 = 2f;

	private float realRadius => (base.attackEnemy == null) ? base.radius : (base.attackEnemy.radius + base.radius);

	private Vector3 GetAroundPos(Vector3 targetPos)
	{
		if (base.field == MovementField.Sky)
		{
			return PEUtil.GetRandomFollowPosInSky(targetPos, base.transform.position - targetPos, m_Data.minR, m_Data.maxR, m_Data.minH, m_Data.maxH, -60f, 60f);
		}
		return base.position + m_LocalPos;
	}

	private Vector3 GetAttackPos()
	{
		if (base.field == MovementField.Sky)
		{
			return PEUtil.GetRandomPositionInSky(m_Data.followEntity.position, m_Data.followEntity.position - base.attackEnemy.centerPos, m_Data.minR, m_Data.maxR, m_Data.minH, m_Data.maxH, -60f, 60f);
		}
		return base.position;
	}

	private void PitchRotation()
	{
		Quaternion identity = Quaternion.identity;
		Transform transform = base.existent;
		Vector3 vector = base.attackEnemy.position - base.position;
		Vector3 planeNormal = transform.TransformDirection(Vector3.right);
		Vector3 fromDirection = transform.TransformDirection(Vector3.forward);
		Vector3 toDirection = Vector3.ProjectOnPlane(vector, planeNormal);
		identity = Quaternion.FromToRotation(fromDirection, toDirection);
		transform.rotation = Quaternion.Inverse(identity) * transform.rotation;
	}

	private bool IsInRange(Enemy e)
	{
		float num = base.position.y - e.position.y - e.height;
		float num2 = PEUtil.MagnitudeH(e.closetPoint, e.farthestPoint) - realRadius;
		if (num < m_Data.minH || num > m_Data.maxH)
		{
			return false;
		}
		if (num2 < m_Data.minR || num2 > m_Data.maxR)
		{
			return false;
		}
		return true;
	}

	private bool IsArrived(Enemy e)
	{
		return Vector3.Distance(base.position, base.attackEnemy.position + m_LocalPos) < 1f;
	}

	private bool IsAttacked(Enemy e)
	{
		return Vector3.Distance(base.position, base.attackEnemy.position + m_AttcakPos) < 1f;
	}

	private bool IsInAngle(Enemy e)
	{
		Vector3 forward = base.transform.forward;
		Vector3 to = base.attackEnemy.centerPos - base.transform.position;
		float num = Vector3.Angle(forward, to);
		return num < _angle;
	}

	private Vector3 GetLocalPosition(Enemy e)
	{
		Vector3 normalized = Vector3.ProjectOnPlane(base.position - base.attackEnemy.position, Vector3.up).normalized;
		Vector3 vector = normalized * m_Data.maxR * 1.5f;
		float num = e.height + (m_Data.minH + m_Data.maxH) * 0.5f;
		return vector + Vector3.up * num;
	}

	private Vector3 GetAttackPosition(Enemy e)
	{
		Vector3 normalized = Vector3.ProjectOnPlane(base.position - base.attackEnemy.position, Vector3.up).normalized;
		Vector3 vector = normalized * (m_Data.minR + m_Data.maxR) * 0.5f;
		float num = e.height + (m_Data.minH + m_Data.maxH) * 0.5f;
		return vector + Vector3.up * num;
	}

	private void coatSkill()
	{
		base.existent.LookAt(base.attackEnemy.centerPos);
		StartSkill(base.attackEnemy.entityTarget, m_Data.skillID);
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.robotCmpt == null)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.followEntity == null)
		{
			return BehaveResult.Failure;
		}
		PeEntityCreator.InitRobotInfo(base.entity, m_Data.followEntity);
		m_Data.changrPos = true;
		m_AttcakPos = GetAroundPos(base.attackEnemy.centerPos);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			base.existent.rotation = Quaternion.identity;
			return BehaveResult.Success;
		}
		if (!IsReached(base.position, base.attackEnemy.position, Is3D: true, 64f) || !IsReached(base.position, m_Data.followEntity.position, Is3D: true, 64f))
		{
			base.Enemies.Remove(base.attackEnemy);
			Vector3 aroundPos = GetAroundPos(m_Data.followEntity.position);
			SetPosition(aroundPos);
			return BehaveResult.Failure;
		}
		m_LocalPos = GetLocalPosition(base.attackEnemy);
		if (IsReached(m_AttcakPos, base.attackEnemy.centerPos, Is3D: false, x1))
		{
			m_AttcakPos = GetAroundPos(base.attackEnemy.centerPos);
		}
		if (!IsReached(base.position, m_AttcakPos, Is3D: false, x3))
		{
			MoveToPosition(m_AttcakPos, SpeedState.Run);
		}
		else if (IsReached(base.position, base.attackEnemy.centerPos, Is3D: false, x1))
		{
			m_AttcakPos = GetAroundPos(base.attackEnemy.centerPos);
		}
		else if (!IsInAngle(base.attackEnemy))
		{
			Vector3 b = base.attackEnemy.centerPos - base.position;
			Vector3 dir = Vector3.Slerp(base.transform.forward, b, x2 * Time.deltaTime);
			FaceDirection(dir);
		}
		else
		{
			StopMove();
			FaceDirection(base.attackEnemy.centerPos - base.position);
			coatSkill();
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		base.existent.rotation = Quaternion.identity;
	}
}
