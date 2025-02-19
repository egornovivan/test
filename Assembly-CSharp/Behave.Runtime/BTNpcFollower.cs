using System.Collections.Generic;
using Pathea;
using PETools;
using Railway;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcFollower), "NpcFollower")]
public class BTNpcFollower : BTNormal
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

		[Behave]
		public string GatherTime;

		[Behave]
		public string LootTime;

		[Behave]
		public float normalPatrolRadiu;

		[Behave]
		public float specialPatrolRadiu;

		private Vector3 m_Anchor;

		private Vector3 m_AnchorDir;

		public float PatrolRadiu = 2f;

		public float SearchGatherTime = 2f;

		public float StartSearchGatherTime;

		public float SearchLootTime = 2f;

		public float StartLootTime;

		public float StandRadiu = 2f;

		private Vector3 m_LastPatrol;

		private bool m_Init;

		private bool m_Reached;

		private bool m_Calculated;

		private bool m_StandPatorl;

		private bool m_CalculatedDir;

		private SpeedState m_SpeedState;

		private static float maxWalkRadius = 6f;

		private static float minRunRadius = 6f;

		private static float maxRunRadius = 24f;

		private static float minSprintRadius = 24f;

		private GameObject mObj;

		private float m_FirTime;

		private float m_CurFirTime;

		private float m_SndTime;

		private float m_CurSndTime;

		public float startPralTime;

		public float lastSetPosTime;

		public float waitPralTime;

		public float sWalkTime0 = 2f;

		public float sWalkTime1 = 2f;

		public float sWalkTime2 = 10f;

		public float LastWalkTime;

		public float LastStopTime;

		public Vector3 Anchor => m_Anchor;

		public bool hasCalculatedDir => m_CalculatedDir;

		public void Init()
		{
			if (!m_Init)
			{
				float[] array = PEUtil.ToArraySingle(GatherTime, ',');
				if (array.Length == 2)
				{
					SearchGatherTime = Random.Range(array[0], array[1]);
					array = PEUtil.ToArraySingle(LootTime, ',');
					SearchLootTime = Random.Range(array[0], array[1]);
				}
				m_Init = true;
			}
		}

		public void CalculateAnchor(Vector3 pos, Vector3 center, Vector3 dir)
		{
			if (!m_Calculated)
			{
				m_Reached = false;
				m_Calculated = true;
				float num = ((!PeGameMgr.IsAdventure || !(RandomDungenMgr.Instance != null) || RandomDungenMgrData.dungeonBaseData == null) ? 90f : 20f);
				m_Anchor = PEUtil.GetRandomPosition(Vector3.zero, dir, firRadius, sndRadius, 0f - num, num);
				m_Anchor = new Vector3(m_Anchor.x, 0f, m_Anchor.z);
			}
		}

		public void CalculateAvoidAnchor(Vector3 pos, Vector3 center, Vector3 dir)
		{
			if (!m_Calculated)
			{
				m_Reached = false;
				m_Calculated = true;
				m_Anchor = PEUtil.GetRandomPosition(Vector3.zero, dir, firRadius, sndRadius, -90f, 90f);
				m_Anchor = new Vector3(m_Anchor.x, 0f, m_Anchor.z);
			}
		}

		public bool IsOutside(Vector3 pos, Transform target)
		{
			float num = PEUtil.SqrMagnitudeH(pos, target.position + m_Anchor);
			return num > thdRadius * thdRadius;
		}

		public bool isReached(Vector3 pos, Transform target)
		{
			float num = PEUtil.SqrMagnitudeH(pos, target.position + m_Anchor);
			if (num < 1f)
			{
				m_Reached = true;
				m_Calculated = false;
			}
			return m_Reached;
		}

		public bool InRadius(Vector3 pos, Vector3 targetCentor, float r, float R, bool is3D)
		{
			float num = PEUtil.Magnitude(pos, targetCentor, is3D);
			return num > r && num <= R;
		}

		public void ResetCalculated()
		{
			m_Calculated = false;
		}

		public Vector3 GetFollowPosition(Transform target, Vector3 velocity)
		{
			if (CheckFirst())
			{
			}
			return target.position + m_Anchor;
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
		Vector3 randomPosition = PEUtil.GetRandomPosition(pos1 + m_Data.Anchor, direction1, m_Data.firRadius, m_Data.sndRadius - 0.5f, -90f, 90f);
		if (PEUtil.CheckPositionNearCliff(randomPosition))
		{
			origin = pos2;
			randomPosition = PEUtil.GetRandomPosition(pos2 + m_Data.Anchor, direction2, m_Data.firRadius, m_Data.sndRadius - 0.5f, -90f, 90f);
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

	private bool CanCather(float radius = 2f, float angle = 360f)
	{
		List<GlobalTreeInfo> list = new List<GlobalTreeInfo>();
		if (null != LSubTerrainMgr.Instance)
		{
			list = LSubTerrainMgr.Picking(base.position, base.transform.forward, includeTrees: false, radius, angle);
		}
		else if (null != RSubTerrainMgr.Instance)
		{
			list = RSubTerrainMgr.Picking(base.position, base.transform.forward, includeTrees: false, radius, angle);
		}
		return list.Count > 0;
	}

	private void EatSth()
	{
		if (NpcEatDb.IsContinueEat(base.entity, out var item) && base.entity.UseItem.GetCdByItemProtoId(item.protoId) < float.Epsilon)
		{
			UseItem(item);
		}
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpc || !base.IsNpcFollower || base.IsNpcFollowerCut)
		{
			return BehaveResult.Failure;
		}
		SetNpcState(ENpcState.Follow);
		base.entity.NpcCmpt.servantCallback = false;
		m_Data.StartSearchGatherTime = Time.time;
		m_Data.StartLootTime = Time.time;
		m_Data.waitPralTime = Random.Range(5f, 10f);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.IsDeath())
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpc || !base.IsNpcFollower || base.IsNpcFollowerWork || base.IsNpcFollowerCut)
		{
			if (base.IsOnVCCarrier)
			{
				GetOff();
			}
			if (base.IsOnRail)
			{
				GetOffRailRoute();
			}
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Failure;
		}
		SetNpcAiType(ENpcAiType.NpcFollower);
		PeTrans peTrans = base.NpcMaster.peEntity.peTrans;
		EatSth();
		if (base.IsSkillCast)
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Failure;
		}
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		if (!base.IsOnVCCarrier && !base.IsOnRail && Time.time - m_Data.StartSearchGatherTime > m_Data.SearchGatherTime)
		{
			if (CanCather())
			{
				return BehaveResult.Failure;
			}
			m_Data.StartSearchGatherTime = Time.time;
		}
		if (base.entity.isRagdoll || base.entity.IsDeath())
		{
			return BehaveResult.Running;
		}
		Vector3 zero = Vector3.zero;
		PassengerCmpt passengerCmpt = base.NpcMaster.Entity.passengerCmpt;
		if (passengerCmpt != null && passengerCmpt.IsOnVCCarrier)
		{
			int num = passengerCmpt.drivingController.FindEmptySeatIndex();
			if (num >= 0 && !base.IsOnVCCarrier && num - base.NpcMaster.RQFollowersIndex >= 0)
			{
				GetOn(passengerCmpt.drivingController, num);
				MoveToPosition(Vector3.zero);
			}
			else if (base.NpcMaster != null && base.NpcMaster.Entity.motionMove != null)
			{
				zero = base.NpcMaster.Entity.motionMove.velocity;
			}
		}
		else if (passengerCmpt != null && passengerCmpt.IsOnRail)
		{
			Route route = PeSingleton<Manager>.Instance.GetRoute(passengerCmpt.railRouteId);
			if (route != null && route.train != null && route.train.HasEmptySeat() && !base.IsOnRail)
			{
				GetOn(passengerCmpt.railRouteId, base.entity.Id);
			}
			else if (base.NpcMaster != null && base.NpcMaster.Entity.motionMove != null)
			{
				zero = base.NpcMaster.Entity.motionMove.velocity;
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
			if (base.NpcMaster != null && base.NpcMaster.Entity.motionMove != null)
			{
				zero = base.NpcMaster.Entity.motionMove.velocity;
			}
		}
		Vector3 avoidPos = Vector3.zero;
		Vector3 avoidPos2 = Vector3.zero;
		Vector3 avoidPos3 = Vector3.zero;
		Vector3 avoidPos4 = Vector3.zero;
		bool hasNearleague = base.entity.NpcCmpt.HasNearleague;
		bool flag = AiUtil.CheckBlockBrush(base.entity, out avoidPos);
		bool flag2 = AiUtil.CheckDig(base.entity, base.NpcMaster.Entity, out avoidPos2);
		bool flag3 = AiUtil.CheckDraging(base.entity, out avoidPos3);
		bool flag4 = AiUtil.CheckCreation(base.entity, out avoidPos4);
		bool flag5 = hasNearleague || flag2 || flag || flag3 || flag4;
		bool flag6 = base.IsNpcFollowerSentry && !flag5;
		bool flag7 = base.IsOnVCCarrier || base.IsOnRail;
		bool flag8 = m_Data.InRadius(base.position, peTrans.position, 0f, m_Data.firRadius, is3D: true);
		bool flag9 = m_Data.InRadius(base.position, peTrans.position, m_Data.firRadius, m_Data.sndRadius, is3D: true);
		Vector3 vector = base.position - peTrans.trans.position;
		Vector3 vector2 = ((!(avoidPos != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos));
		Vector3 vector3 = ((!(avoidPos2 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos2));
		Vector3 vector4 = ((!(avoidPos3 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos3));
		Vector3 vector5 = ((!(avoidPos4 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos4));
		Vector3 dir = vector + vector2 + vector3 + vector4 + vector5 + base.existent.forward;
		bool flag10 = RandomDunGenUtil.IsInDungeon(base.entity);
		bool flag11 = PEUtil.IsUnderBlock(base.entity);
		bool flag12 = !flag10 && flag11 && PEUtil.IsForwardBlock(base.entity, base.entity.peTrans.forward, 2f);
		if (!flag7)
		{
			if (!flag8 && !flag9)
			{
				if (GameConfig.IsMultiMode)
				{
					if (Stucking(1f) || base.IsNpcFollowerSentry)
					{
						Vector3 fixedPosition = GetFixedPosition(PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward, peTrans.position, -peTrans.forward, peTrans.bound.size.y);
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
					if (Stucking(1f) || flag12)
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
			if (flag8 && !m_Data.InRadius(base.position, peTrans.trans.position + m_Data.Anchor, m_Data.firRadius, m_Data.sndRadius, is3D: true))
			{
				m_Data.ResetCalculated();
				m_Data.CalculateAnchor(base.position, PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward);
				m_Data.startPralTime = Time.time;
			}
			if ((!flag10 || !flag9) && Time.time - m_Data.startPralTime >= m_Data.waitPralTime)
			{
				m_Data.startPralTime = Time.time;
				m_Data.CalculateAnchor(base.position, PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward);
				m_Data.ResetCalculatedDir();
			}
			bool flag13 = IsReached(base.position, peTrans.trans.position + m_Data.Anchor, Is3D: true, 2f);
			bool flag14 = Time.time - m_Data.startPralTime < m_Data.waitPralTime && flag13 && !flag5;
			if (!flag6)
			{
				if (!flag5)
				{
					if (flag14)
					{
						if (flag12 || Stucking(1f))
						{
							if (flag10)
							{
								m_Data.ResetCalculatedDir();
							}
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
					else if (flag12 || Stucking(1f))
					{
						if (flag10)
						{
							m_Data.ResetCalculatedDir();
						}
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
						Vector3 v = ((!flag11) ? followPosition : peTrans.trans.position);
						SpeedState state = m_Data.CalculateSpeedState(PEUtil.SqrMagnitudeH(base.position, v));
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
		if (base.attackEnemy != null && !base.IsOnVCCarrier && !base.IsOnRail)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}
}
