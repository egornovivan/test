using System;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCheckPostion), "CheckPostion")]
public class BTCheckPostion : BTNormal
{
	private class Data
	{
		[Behave]
		public int Type;

		[Behave]
		public float Probability;

		[Behave]
		public int Speed;

		[Behave]
		public string AttackBreak;

		public SpeedState speedState;

		private int[] PosIds;

		public PEBuilding mBuild;

		public bool CanAttackBreak;

		public void InitPos(Camp camp)
		{
			PosIds = camp.GetPosByType((EPosType)Type);
			CanAttackBreak = Convert.ToInt32(AttackBreak) > 0;
			speedState = (SpeedState)Speed;
		}

		public bool GetBuidPos()
		{
			if (PosIds != null && PosIds.Length == 1 && PosIds[0] != 0)
			{
				PeEntity[] doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(PosIds[0]);
				if (doodadEntities == null || doodadEntities.Length < 1)
				{
					return false;
				}
				if (mBuild == null)
				{
					mBuild = doodadEntities[0].Doodabuid;
				}
				if (mBuild == null)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public bool IsReached(Vector3 pos, Vector3 targetPos, float radiu = 1f)
		{
			float num = PEUtil.SqrMagnitudeH(pos, targetPos);
			return num < radiu * radiu;
		}
	}

	private Data m_Data;

	private Transform mTrans;

	private Vector3 mDoorPos;

	private Vector3 dirPos;

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Work))
		{
			return BehaveResult.Failure;
		}
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.InitPos(base.Campsite);
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.CanAttackBreak && !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.CanAttackBreak && base.entity.NpcCmpt.NpcInAlert)
		{
			return BehaveResult.Failure;
		}
		if (UnityEngine.Random.value > m_Data.Probability)
		{
			return BehaveResult.Failure;
		}
		if (!m_Data.GetBuidPos())
		{
			return BehaveResult.Failure;
		}
		mTrans = m_Data.mBuild.Occupy(base.entity.Id);
		if (mTrans == null)
		{
			return BehaveResult.Failure;
		}
		dirPos = mTrans.position;
		if (m_Data.mBuild.mDoorPos != null && m_Data.mBuild.mDoorPos.Length > 0)
		{
			mDoorPos = m_Data.mBuild.mDoorPos[UnityEngine.Random.Range(0, m_Data.mBuild.mDoorPos.Length)].position;
		}
		if (mDoorPos != Vector3.zero)
		{
			dirPos = mDoorPos;
		}
		SetOccpyBuild(m_Data.mBuild);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Work))
		{
			return BehaveResult.Failure;
		}
		mTrans = m_Data.mBuild.Occupy(base.entity.Id);
		if (mTrans == null)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.CanAttackBreak && !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.CanAttackBreak && base.entity.NpcCmpt.NpcInAlert)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.IsReached(base.position, mTrans.position))
		{
			StopMove();
			SetPosition(mTrans.position);
			return BehaveResult.Success;
		}
		if (Stucking())
		{
			SetPosition(mTrans.position);
		}
		if (m_Data.IsReached(base.position, dirPos) && dirPos != mTrans.position)
		{
			SetPosition(mTrans.position);
		}
		MoveToPosition(dirPos, m_Data.speedState, avoid: false);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		dirPos = Vector3.zero;
		mDoorPos = Vector3.zero;
	}
}
