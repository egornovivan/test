using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTFollowAround), "FollowAround")]
public class BTFollowAround : BTNormal
{
	private class Data
	{
		[Behave]
		public float cdTime = 10f;

		[Behave]
		public float prob = 1f;

		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float minHeight;

		[Behave]
		public float maxHeight;

		[Behave]
		public float minTime = 10f;

		[Behave]
		public float maxTime = 10f;

		[Behave]
		public int targetID;

		private PeEntity mfollowEntity;

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
	}

	private Data m_Data;

	private float m_Time;

	private float m_StartTime;

	private Vector3 m_HoverPosition;

	private Vector3 GetAroundPos()
	{
		if (base.field == MovementField.Sky)
		{
			return PEUtil.GetRandomFollowPosInSky(m_Data.followEntity.position, base.transform.position - m_Data.followEntity.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -90f, 90f);
		}
		if (base.field == MovementField.water)
		{
			return PEUtil.GetRandomPositionInWater(m_Data.followEntity.position, base.transform.position - m_Data.followEntity.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -90f, 90f);
		}
		return PEUtil.GetRandomPositionOnGround(m_Data.followEntity.position, base.transform.position - m_Data.followEntity.position, m_Data.minRange, m_Data.maxRange, -90f, 90f);
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.followEntity == null)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		PeEntityCreator.InitRobotInfo(base.entity, m_Data.followEntity);
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_HoverPosition = GetAroundPos();
		m_StartTime = Time.time;
		m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.followEntity == null)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!IsReached(base.position, m_Data.followEntity.position, Is3D: false, 40f))
		{
			m_HoverPosition = GetAroundPos();
			SetPosition(m_HoverPosition);
		}
		if (m_HoverPosition == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		float num = PEUtil.SqrMagnitude(base.position, m_HoverPosition);
		if (num < 1f || Stucking() || Time.time - m_StartTime > m_Time)
		{
			StopMove();
			return BehaveResult.Success;
		}
		MoveToPosition(m_HoverPosition, SpeedState.Run);
		return BehaveResult.Running;
	}
}
