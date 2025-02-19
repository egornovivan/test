using Pathea;
using Pathea.Operate;
using Pathfinding;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBaseWait), "NpcBaseJobWait")]
public class BTNpcBaseWait : BTNormal
{
	private class Data
	{
		[Behave]
		public int Jobtype;

		[Behave]
		public float WaitTime;

		[Behave]
		public float RadiuMax;

		[Behave]
		public float RadiuMin;

		public Vector3 mWaitPos;

		public float mStartWaitTime;

		public float sWalkTime0 = 2f;

		public float sWalkTime1 = 20f;

		public float sWalkTime2 = 30f;

		public float LastWalkTime;

		public float LastStopTime;

		private int TIMES = 4;

		private bool m_CalculatedDir;

		private Vector3 m_AnchorDir;

		private int underBlockTimes;

		public ENpcJob mNpcJobType => (ENpcJob)Jobtype;

		public bool hasCalculatedDir => m_CalculatedDir;

		public bool EndUnderBlock()
		{
			if (underBlockTimes > TIMES)
			{
				underBlockTimes = 0;
				return true;
			}
			return false;
		}

		public Vector3 GetAnchorDir()
		{
			return m_AnchorDir;
		}

		public void ResetCalculatedDir()
		{
			m_CalculatedDir = false;
			underBlockTimes++;
		}

		public bool GetCanMoveDirtion(PeEntity entity, float minAngle)
		{
			if (!m_CalculatedDir)
			{
				for (int i = 1; (float)i < 360f / minAngle; i++)
				{
					m_AnchorDir = Quaternion.AngleAxis(minAngle * (float)i, Vector3.up) * entity.peTrans.forward;
					Debug.DrawRay(entity.position + Vector3.up, m_AnchorDir * 3f, Color.cyan);
					if (!PEUtil.IsForwardBlock(entity, m_AnchorDir, 3f))
					{
						m_CalculatedDir = true;
						return true;
					}
					m_AnchorDir = Vector3.zero;
				}
				return false;
			}
			return false;
		}
	}

	private Data m_Data;

	private void OnPathComplete(Path path)
	{
		if (base.IsNpcBase && PEUtil.IsInAstarGrid(base.position) && path != null && path.vectorPath.Count > 15)
		{
			Vector3 vector = path.vectorPath[path.vectorPath.Count - 1];
			if (NpcMgr.IsIncenterAraound(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, vector))
			{
				m_Data.mWaitPos = vector;
			}
			else
			{
				NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_Data.mWaitPos);
			}
		}
		else if (base.IsNpcBase)
		{
			NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_Data.mWaitPos);
		}
	}

	private bool StopWait()
	{
		return m_Data.mNpcJobType switch
		{
			ENpcJob.Trainer => base.WorkEntity != null && base.WorkEntity.workTrans != null && base.NpcTrainerType != 0 && base.IsNpcTrainning, 
			ENpcJob.Worker => base.WorkEntity != null && base.WorkEntity.workTrans != null, 
			ENpcJob.Processor => base.IsNpcProcessing, 
			ENpcJob.Follower => base.entity != null && base.entity.NpcCmpt != null && base.entity.NpcCmpt.IsServant, 
			ENpcJob.Doctor => base.Cured != null && !base.Cured.Equals(null) && base.WorkEntity != null && base.WorkEntity.workTrans != null, 
			ENpcJob.Farmer => IsStopFormerWait(), 
			_ => false, 
		};
	}

	private bool IsStopFormerWait()
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Farmer)
		{
			return true;
		}
		if (ContainsTitle(ENpcTitle.Plant))
		{
			return !CSUtils.FarmPlantReady(base.entity) && CSUtils.FindPlantPosNewChunk(base.entity) != null;
		}
		if (ContainsTitle(ENpcTitle.Manage))
		{
			return (!CSUtils.FarmWaterReady(base.entity) && CSUtils.FindPlantToWater(base.entity) != null) || (!CSUtils.FarmCleanReady(base.entity) && CSUtils.FindPlantToClean(base.entity) != null);
		}
		if (ContainsTitle(ENpcTitle.Harvest))
		{
			return CSUtils.FindPlantGet(base.entity) != null || CSUtils.FindPlantRemove(base.entity) != null;
		}
		return true;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase || base.NpcJob != m_Data.mNpcJobType)
		{
			return BehaveResult.Failure;
		}
		if (base.Creater == null || base.Creater.Assembly == null || base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.mWaitPos == Vector3.zero)
		{
			NpcMgr.GetRandomPathForCsWander(base.entity, base.Creater.Assembly.Position, base.transform.forward, 15f, base.Creater.Assembly.Radius, OnPathComplete);
		}
		m_Data.mStartWaitTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!Enemy.IsNullOrInvalid(base.attackEnemy) || base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase || m_Data.mNpcJobType != base.NpcJob || base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (StopWait())
		{
			return BehaveResult.Failure;
		}
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		if (base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator) && base.Operator.IsActionRunning(PEActionType.Sleep))
		{
			base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Sleep);
		}
		if (base.Operator.IsActionRunning(PEActionType.Sleep))
		{
			base.Operator.EndAction(PEActionType.Sleep);
		}
		if (Time.time - m_Data.LastWalkTime > m_Data.sWalkTime1)
		{
			m_Data.LastWalkTime = Time.time;
			m_Data.ResetCalculatedDir();
		}
		bool flag = PEUtil.IsUnderBlock(base.entity);
		bool flag2 = PEUtil.IsForwardBlock(base.entity, base.entity.peTrans.forward, 2f);
		if (!m_Data.hasCalculatedDir || flag2 || flag)
		{
			if (m_Data.EndUnderBlock())
			{
				m_Data.LastWalkTime = Time.time;
				m_Data.ResetCalculatedDir();
				if (NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_Data.mWaitPos))
				{
					SetPosition(m_Data.mWaitPos);
				}
				return BehaveResult.Success;
			}
			if (m_Data.GetCanMoveDirtion(base.entity, 30f) || Time.time - m_Data.LastWalkTime < m_Data.sWalkTime0)
			{
				if (m_Data.GetAnchorDir() != Vector3.zero)
				{
					MoveDirection(m_Data.GetAnchorDir());
				}
				else
				{
					StopMove();
				}
			}
			else
			{
				StopMove();
			}
		}
		else
		{
			if (IsReached(base.position, m_Data.mWaitPos))
			{
				StopMove();
				return BehaveResult.Success;
			}
			if (Stucking())
			{
				if (base.entity.viewCmpt != null && base.entity.viewCmpt.hasView)
				{
					if (NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_Data.mWaitPos))
					{
						if (Stucking(10f))
						{
							SetPosition(m_Data.mWaitPos);
						}
					}
					else
					{
						StopMove();
					}
				}
				else if (NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_Data.mWaitPos))
				{
					SetPosition(m_Data.mWaitPos);
				}
				else
				{
					StopMove();
				}
				return BehaveResult.Success;
			}
			MoveToPosition(m_Data.mWaitPos);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		m_Data.mWaitPos = Vector3.zero;
	}
}
