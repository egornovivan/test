using Pathea;
using PETools;
using Railway;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BRFollowTarget), "RFollowTarget")]
public class BRFollowTarget : BTNormal
{
	private class Data
	{
		[Behave]
		public float firRadius;

		[Behave]
		public float sndRadius;

		[Behave]
		public float thdRadius;

		[Behave]
		public string[] relax = new string[0];

		private Vector3 m_Anchor;

		private Vector3 m_LastPatrol;

		private bool m_Reached;

		private bool m_Calculated;

		private SpeedState m_SpeedState;

		private static float maxWalkRadius = 8f;

		private static float minRunRadius = 8f;

		private static float maxRunRadius = 16f;

		private static float minSprintRadius = 16f;

		public float startPralTime;

		public float waitPralTime = 8f;

		public float startFindTime;

		public float waitFindTime = 3f;

		public float StandRadiu = 2f;

		private Vector3 m_AnchorDir;

		private bool m_CalculatedDir;

		private float m_FirTime;

		private float m_CurFirTime;

		private float m_SndTime;

		private float m_CurSndTime;

		public Vector3 Anchor => m_Anchor;

		public bool hasCalculatedDir => m_CalculatedDir;

		public void CalculateAnchor(Vector3 pos, Vector3 center, Vector3 dir)
		{
			if (!m_Calculated)
			{
				m_Reached = false;
				m_Calculated = true;
				m_Anchor = PEUtil.GetRandomPosition(Vector3.zero, dir, firRadius, sndRadius, -90f, 90f);
				m_Anchor = new Vector3(m_Anchor.x, 0f, m_Anchor.z);
			}
		}

		public void ResetCalculated()
		{
			m_Calculated = false;
		}

		public bool IsOutside(Vector3 pos, Transform target)
		{
			float num = PEUtil.Magnitude(pos, target.position + m_Anchor);
			return num > thdRadius;
		}

		public bool InRadius(Vector3 pos, Vector3 targetCentor, float r, float R, bool is3D)
		{
			float num = PEUtil.Magnitude(pos, targetCentor, is3D);
			return num > r && num <= R;
		}

		public bool isReached(Vector3 pos, Transform target, bool is3D)
		{
			float num = PEUtil.Magnitude(pos, target.position + m_Anchor);
			if (num < 1f)
			{
				m_Reached = true;
				m_Calculated = false;
			}
			return m_Reached;
		}

		public Vector3 GetFollowPosition(Transform target, Vector3 velocity)
		{
			if (CheckFirst())
			{
			}
			return target.position + m_Anchor;
		}

		public Vector3 GetPatrolPosition(Transform target)
		{
			if (CheckSecond())
			{
				m_LastPatrol = PEUtil.GetRandomPosition(target.position + m_Anchor, firRadius, thdRadius);
			}
			return m_LastPatrol;
		}

		public Vector3 GetAnchorDir()
		{
			return m_AnchorDir;
		}

		public void ResetCalculatedDir()
		{
			m_CalculatedDir = false;
		}

		public bool GetCanMoveDirtion(PeEntity entity, float minAngle)
		{
			if (!m_CalculatedDir)
			{
				for (int i = 1; (float)i < 360f / minAngle; i++)
				{
					m_AnchorDir = Quaternion.AngleAxis(minAngle * (float)i, Vector3.up) * entity.peTrans.forward;
					if (!PEUtil.IsForwardBlock(entity, m_AnchorDir, 3f))
					{
						m_CalculatedDir = true;
						return true;
					}
					m_AnchorDir = Vector3.zero;
				}
			}
			m_AnchorDir = Vector3.zero;
			return false;
		}

		public SpeedState CalculateSpeedState(float sqrDistanceH)
		{
			switch (m_SpeedState)
			{
			case SpeedState.None:
				m_SpeedState = SpeedState.Walk;
				break;
			case SpeedState.Walk:
				if (sqrDistanceH > maxWalkRadius * maxWalkRadius)
				{
					m_SpeedState = SpeedState.Run;
				}
				break;
			case SpeedState.Run:
				if (sqrDistanceH < minRunRadius * minRunRadius)
				{
					m_SpeedState = SpeedState.Walk;
				}
				if (sqrDistanceH > maxRunRadius * maxRunRadius)
				{
					m_SpeedState = SpeedState.Sprint;
				}
				break;
			case SpeedState.Sprint:
				if (sqrDistanceH < minSprintRadius * minSprintRadius)
				{
					m_SpeedState = SpeedState.Run;
				}
				break;
			default:
				m_SpeedState = SpeedState.Walk;
				break;
			}
			return m_SpeedState;
		}

		public bool CheckFirst()
		{
			if (Time.time - m_FirTime > m_CurFirTime)
			{
				m_FirTime = Time.time;
				m_CurFirTime = Random.Range(1f, 3f);
				return true;
			}
			return false;
		}

		public bool CheckSecond()
		{
			if (Time.time - m_SndTime > m_CurSndTime)
			{
				m_SndTime = Time.time;
				m_CurSndTime = Random.Range(10f, 15f);
				return true;
			}
			return false;
		}
	}

	private Data m_Data;

	private Vector3 GetFixedPosition(Vector3 pos1, Vector3 direction1, Vector3 pos2, Vector3 direction2, float height)
	{
		Vector3 origin = pos1;
		Vector3 randomPosition = PEUtil.GetRandomPosition(pos1 + m_Data.Anchor, direction1, 2f, m_Data.sndRadius - 0.5f, -90f, 90f);
		if (PEUtil.CheckPositionNearCliff(randomPosition))
		{
			origin = pos2;
			randomPosition = PEUtil.GetRandomPosition(pos2 + m_Data.Anchor, direction2, 2f, m_Data.sndRadius - 0.5f, -90f, 90f);
		}
		Ray ray = new Ray(origin, Vector3.up);
		if (Physics.Raycast(ray, out var hitInfo, 128f, 71680))
		{
			ray = new Ray(randomPosition, Vector3.up);
			if (!Physics.Raycast(ray, out hitInfo, 128f, 71680))
			{
				return Vector3.zero;
			}
			if (PEUtil.CheckPositionUnderWater(hitInfo.point - Vector3.up))
			{
				return randomPosition;
			}
			ray = new Ray(randomPosition, Vector3.down);
			if (Physics.Raycast(ray, out hitInfo, 128f, 71680))
			{
				return hitInfo.point + Vector3.up;
			}
		}
		else
		{
			Ray ray2 = new Ray(randomPosition + 128f * Vector3.up, -Vector3.up);
			if (Physics.Raycast(ray2, out hitInfo, 256f, 71680))
			{
				if (PEUtil.CheckPositionUnderWater(hitInfo.point))
				{
					return randomPosition;
				}
				return hitInfo.point + Vector3.up;
			}
		}
		return Vector3.zero;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!(GetRequest(EReqType.FollowTarget) is RQFollowTarget rQFollowTarget))
		{
			if (base.IsOnVCCarrier && !base.IsNpcFollower)
			{
				GetOff();
			}
			if (base.IsOnRail && !base.IsNpcFollower)
			{
				GetOffRailRoute();
			}
			if (!base.IsNpcFollower && (base.IsOnVCCarrier || base.IsOnRail))
			{
				return BehaveResult.Running;
			}
			return BehaveResult.Failure;
		}
		SetNpcAiType(ENpcAiType.RFollowTarget);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(rQFollowTarget.id);
		PeEntity peEntity2 = ((rQFollowTarget.dirTargetID == 0) ? null : PeSingleton<EntityMgr>.Instance.Get(rQFollowTarget.dirTargetID));
		if (peEntity == null)
		{
			if (base.IsOnVCCarrier)
			{
				GetOff();
			}
			if (base.IsOnRail)
			{
				GetOffRailRoute();
			}
			return BehaveResult.Failure;
		}
		PeTrans peTrans = peEntity.peTrans;
		Vector3 zero = Vector3.zero;
		if (base.entity.isRagdoll || base.entity.IsDeath())
		{
			return BehaveResult.Running;
		}
		PassengerCmpt passengerCmpt = peEntity.passengerCmpt;
		if (passengerCmpt != null && passengerCmpt.IsOnVCCarrier)
		{
			int num = passengerCmpt.drivingController.FindEmptySeatIndex();
			if (num >= 0 && !base.IsOnVCCarrier)
			{
				GetOn(passengerCmpt.drivingController, num);
				MoveToPosition(Vector3.zero);
			}
			else if (peEntity.motionMove != null)
			{
				zero = peEntity.motionMove.velocity;
			}
		}
		else if (passengerCmpt != null && passengerCmpt.IsOnRail)
		{
			Route route = PeSingleton<Manager>.Instance.GetRoute(passengerCmpt.railRouteId);
			if (route != null && route.train != null && route.train.HasEmptySeat() && !base.IsOnRail)
			{
				GetOn(passengerCmpt.railRouteId, base.entity.Id);
			}
			else if (peEntity.motionMove != null)
			{
				zero = peEntity.motionMove.velocity;
			}
		}
		else
		{
			if (passengerCmpt != null && !passengerCmpt.IsOnVCCarrier && base.IsOnVCCarrier)
			{
				GetOff();
			}
			if (passengerCmpt != null && !passengerCmpt.IsOnRail && base.IsOnRail)
			{
				GetOffRailRoute();
			}
			if (peEntity.motionMove != null)
			{
				zero = peEntity.motionMove.velocity;
			}
		}
		bool flag = PeGameMgr.IsStory && PeGameMgr.IsSingle && SingleGameStory.curType != SingleGameStory.StoryScene.MainLand;
		Vector3 avoidPos = Vector3.zero;
		Vector3 avoidPos2 = Vector3.zero;
		Vector3 avoidPos3 = Vector3.zero;
		Vector3 avoidPos4 = Vector3.zero;
		bool hasNearleague = base.entity.NpcCmpt.HasNearleague;
		bool flag2 = AiUtil.CheckBlockBrush(base.entity, out avoidPos);
		bool flag3 = AiUtil.CheckDig(base.entity, peEntity, out avoidPos2);
		bool flag4 = AiUtil.CheckDraging(base.entity, out avoidPos3);
		bool flag5 = AiUtil.CheckCreation(base.entity, out avoidPos4);
		bool flag6 = hasNearleague || flag3 || flag2 || flag4 || flag5;
		bool flag7 = base.IsNpcFollowerSentry && !flag6;
		bool flag8 = base.IsOnVCCarrier || base.IsOnRail;
		bool flag9 = m_Data.InRadius(base.position, peTrans.position, 0f, m_Data.firRadius, is3D: true);
		bool flag10 = m_Data.InRadius(base.position, peTrans.position, m_Data.firRadius, m_Data.sndRadius, is3D: true);
		Vector3 vector = base.position - peTrans.trans.position;
		Vector3 vector2 = ((!(avoidPos != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos));
		Vector3 vector3 = ((!(avoidPos2 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos2));
		Vector3 vector4 = ((!(avoidPos3 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos3));
		Vector3 vector5 = ((!(avoidPos4 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos4));
		Vector3 dir = vector + vector2 + vector3 + vector4 + vector5;
		bool flag11 = PEUtil.IsForwardBlock(base.entity, base.existent.forward, 2f) || !GetBool("OnGround");
		if (!flag8)
		{
			if (peEntity2 != null && (flag11 || Stucking()) && m_Data.InRadius(peEntity.position, peEntity2.position, 0f, rQFollowTarget.tgtRadiu, is3D: true))
			{
				Vector3 vector6 = ((!flag) ? PEUtil.GetRandomPositionOnGroundForWander(peEntity2.position, 1f, 3f) : peEntity2.position);
				if (vector6 != Vector3.zero)
				{
					SetPosition(vector6);
				}
			}
			if (rQFollowTarget.targetPos != Vector3.zero && (flag11 || Stucking()) && m_Data.InRadius(peEntity.position, rQFollowTarget.targetPos, 0f, rQFollowTarget.tgtRadiu, is3D: true))
			{
				Vector3 vector7 = ((!flag) ? PEUtil.GetRandomPositionOnGroundForWander(rQFollowTarget.targetPos, 1f, 3f) : rQFollowTarget.targetPos);
				if (vector7 != Vector3.zero)
				{
					SetPosition(vector7);
				}
			}
			if (!flag9 && !flag10)
			{
				if (GameConfig.IsMultiMode)
				{
					if (Stucking() || base.IsNpcFollowerSentry || flag11)
					{
						Vector3 fixedPosition = GetFixedPosition(PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward, peTrans.position, -peTrans.forward, peTrans.bound.size.y);
						if (flag11)
						{
							fixedPosition = PeSingleton<MainPlayer>.Instance.entity.position;
							Vector3 vector8 = new Vector3(0f - PEUtil.MainCamTransform.forward.x, 0f, 0f - PEUtil.MainCamTransform.forward.z);
							fixedPosition += (Quaternion.AngleAxis(Random.Range(-90f, 90f), Vector3.up) * vector8).normalized * Random.Range(4f, 6f);
						}
						if (PEUtil.CheckErrorPos(fixedPosition))
						{
							SetPosition(fixedPosition);
							MoveToPosition(Vector3.zero);
						}
					}
					else
					{
						m_Data.CalculateAnchor(base.position, PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward);
					}
				}
				else
				{
					if (Stucking() || flag11)
					{
						Vector3 fixedPosition2 = GetFixedPosition(PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward, peTrans.position, -peTrans.forward, peTrans.bound.size.y);
						float num2 = Mathf.Abs(peTrans.position.y - fixedPosition2.y);
						if (PEUtil.CheckErrorPos(fixedPosition2) && num2 <= 3f)
						{
							SetPosition(fixedPosition2);
							MoveToPosition(Vector3.zero);
						}
					}
					else
					{
						m_Data.CalculateAnchor(base.position, PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward);
					}
					m_Data.startPralTime = Time.time;
				}
			}
			if (flag9 && !m_Data.InRadius(base.position, peTrans.trans.position + m_Data.Anchor, m_Data.firRadius, m_Data.sndRadius, is3D: true))
			{
				m_Data.ResetCalculated();
				m_Data.CalculateAnchor(base.position, PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward);
				m_Data.startPralTime = Time.time;
			}
			if (Time.time - m_Data.startPralTime >= m_Data.waitPralTime)
			{
				m_Data.startPralTime = Time.time;
				m_Data.CalculateAnchor(base.position, PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward);
				m_Data.ResetCalculatedDir();
			}
			bool flag12 = IsReached(base.position, peTrans.trans.position + m_Data.Anchor, Is3D: true, 2f);
			bool flag13 = Time.time - m_Data.startPralTime < m_Data.waitPralTime && flag12 && !flag6;
			if (!flag7)
			{
				if (!flag6)
				{
					if (flag13)
					{
						if (flag11)
						{
							m_Data.GetCanMoveDirtion(base.entity, 30f);
							if (m_Data.GetAnchorDir() != Vector3.zero)
							{
								m_Data.ResetCalculated();
								m_Data.CalculateAnchor(base.position, PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward);
								FaceDirection(m_Data.GetAnchorDir());
							}
						}
						StopMove();
						m_Data.ResetCalculated();
					}
					else if (flag11 || Stucking())
					{
						m_Data.GetCanMoveDirtion(base.entity, 30f);
						if (m_Data.GetAnchorDir() != Vector3.zero)
						{
							m_Data.ResetCalculated();
							m_Data.CalculateAnchor(base.position, PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward);
							FaceDirection(m_Data.GetAnchorDir());
						}
						StopMove();
					}
					else
					{
						Vector3 followPosition = m_Data.GetFollowPosition(peTrans.trans, zero);
						SpeedState state = m_Data.CalculateSpeedState(PEUtil.SqrMagnitudeH(base.position, peTrans.trans.position));
						if (IsReached(base.position, peTrans.trans.position + m_Data.Anchor))
						{
							Vector3 pos = peTrans.trans.position + m_Data.Anchor;
							float num3 = pos.y - base.position.y;
							float num4 = Mathf.Abs(num3);
							if (num4 >= 1f)
							{
								pos = ((!(num3 > 0f)) ? PEUtil.CorrectionPostionToStand(pos, 6f, 8f) : PEUtil.CorrectionPostionToStand(pos));
								SetPosition(pos);
								m_Data.ResetCalculated();
								m_Data.CalculateAnchor(base.position, PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward);
							}
						}
						MoveToPosition(followPosition, state);
					}
				}
				else
				{
					m_Data.ResetCalculated();
					m_Data.CalculateAnchor(base.position, peTrans.trans.position, dir);
					MoveDirection(dir, SpeedState.Run);
				}
			}
			else
			{
				StopMove();
			}
		}
		if (!rQFollowTarget.CanRun())
		{
			if (!(GetRequest(EReqType.Attack) is RQAttack))
			{
				if (base.IsOnVCCarrier)
				{
					GetOff();
				}
				if (base.IsOnRail)
				{
					GetOffRailRoute();
				}
				return BehaveResult.Failure;
			}
			if ((Enemy.IsNullOrInvalid(base.attackEnemy) || (!base.IsOnVCCarrier && !base.IsOnRail)) && !Enemy.IsNullOrInvalid(base.attackEnemy))
			{
				return BehaveResult.Failure;
			}
		}
		return BehaveResult.Running;
	}
}
