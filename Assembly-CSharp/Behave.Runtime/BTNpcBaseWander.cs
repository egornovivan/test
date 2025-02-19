using Pathea;
using Pathfinding;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBaseWander), "NpcBaseWander")]
public class BTNpcBaseWander : BTNormal
{
	private class Data
	{
		[Behave]
		public float wanderTime;

		public float sWalkTime0 = 2f;

		public float sWalkTime1 = 20f;

		public float sWalkTime2 = 30f;

		public float LastWalkTime;

		public float LastStopTime;

		private int TIMES = 4;

		private bool m_CalculatedDir;

		private Vector3 m_AnchorDir;

		private int underBlockTimes;

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

	private Vector3 m_WanderCenter;

	private Vector3 m_CurWanderPos;

	private float m_WanderRadius;

	private float mWanderStartTime;

	private EThinkingType mStroll = EThinkingType.Stroll;

	private Vector3 GetRandomPositionForWander()
	{
		return PEUtil.GetRandomPositionOnGroundForWander(m_WanderCenter, m_WanderRadius - 5f, m_WanderRadius);
	}

	private void OnPathComplete(Path path)
	{
		if (base.Creater != null && base.Creater.Assembly != null && PEUtil.IsInAstarGrid(base.position) && path != null && path.vectorPath.Count > 15)
		{
			Vector3 vector = path.vectorPath[path.vectorPath.Count - 1];
			float num = PEUtil.Magnitude(base.Creater.Assembly.Position, vector);
			if (num < base.Creater.Assembly.Radius)
			{
				m_CurWanderPos = vector;
			}
			else if (base.Creater != null && base.Creater.Assembly != null)
			{
				NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_CurWanderPos);
			}
		}
		else if (base.Creater != null && base.Creater.Assembly != null)
		{
			NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_CurWanderPos);
		}
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (!NpcThinkDb.CanDo(base.entity, mStroll))
		{
			return BehaveResult.Failure;
		}
		if (!base.CanNpcWander)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.lineType != 0)
		{
			return BehaveResult.Failure;
		}
		if (base.Creater.Assembly != null)
		{
			float num = PEUtil.Magnitude(base.Creater.Assembly.Position, base.position);
			if (num > base.Creater.Assembly.Radius)
			{
				return BehaveResult.Failure;
			}
		}
		if (base.Creater.Assembly != null)
		{
			m_WanderCenter = base.Creater.Assembly.Position;
			m_WanderRadius = base.Creater.Assembly.Radius;
		}
		if (m_WanderCenter == Vector3.zero || m_WanderRadius <= 0f)
		{
			return BehaveResult.Failure;
		}
		if (m_CurWanderPos == Vector3.zero)
		{
			NpcMgr.GetRandomPathForCsWander(base.entity, base.Creater.Assembly.Position, base.transform.forward, 15f, base.Creater.Assembly.Radius, OnPathComplete);
		}
		SetNpcState(ENpcState.Patrol);
		mWanderStartTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!base.IsNpcBase || base.hasAnyRequest)
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (!NpcThinkDb.CanDoing(base.entity, mStroll) || base.entity.NpcCmpt.lineType != 0)
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (base.Creater.Assembly != null)
		{
			float num = PEUtil.Magnitude(base.Creater.Assembly.Position, base.position);
			if (num > base.Creater.Assembly.Radius)
			{
				return BehaveResult.Failure;
			}
		}
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		if (PEUtil.SqrMagnitudeH(base.position, m_CurWanderPos) < 0.25f)
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Success;
		}
		if (m_CurWanderPos == Vector3.zero && Time.time - mWanderStartTime >= m_Data.wanderTime)
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Success;
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
				if (NpcCanWalkPos(m_WanderCenter, m_WanderRadius, out m_CurWanderPos))
				{
					SetPosition(m_CurWanderPos);
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
			if (Stucking(1f))
			{
				if (base.entity.viewCmpt != null && base.entity.viewCmpt.hasView)
				{
					if (NpcCanWalkPos(m_WanderCenter, m_WanderRadius, out m_CurWanderPos))
					{
						if (Stucking(10f))
						{
							SetPosition(m_CurWanderPos);
						}
					}
					else
					{
						StopMove();
					}
				}
				else if (base.Creater.Assembly != null)
				{
					float num2 = PEUtil.Magnitude(base.Creater.Assembly.Position, base.position);
					if (num2 > base.Creater.Assembly.Radius && NpcCanWalkPos(m_WanderCenter, m_WanderRadius, out m_CurWanderPos))
					{
						Vector3 curWanderPos = m_CurWanderPos;
						if (curWanderPos != Vector3.zero)
						{
							SetPosition(curWanderPos);
							m_CurWanderPos = curWanderPos;
							return BehaveResult.Failure;
						}
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
			MoveToPosition(m_CurWanderPos);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		m_CurWanderPos = Vector3.zero;
	}
}
