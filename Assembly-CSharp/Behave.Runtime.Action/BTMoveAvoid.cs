using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveAvoid), "MoveAvoid")]
public class BTMoveAvoid : BTNormal
{
	private class Data
	{
		[Behave]
		public float Radius;

		[Behave]
		public float firAvoid;

		[Behave]
		public float sndAvoid;

		[Behave]
		public float trdAvoid;
	}

	private Vector3 m_AvoidPos;

	private Data m_Data;

	private SpeedState m_avoidSpeed;

	private Vector3 GetAvoidPos(Vector3 dirtion)
	{
		float upD = ((!base.IsNpcCampsite) ? 128f : 15f);
		float downD = ((!base.IsNpcCampsite) ? 256f : 18f);
		return PEUtil.GetRandomPosition(base.position, dirtion, m_Data.firAvoid, m_Data.sndAvoid, -30f, 30f, PEUtil.Standlayer, upD, downD);
	}

	private bool IsReach(Vector3 self, Vector3 target)
	{
		float f = PEUtil.Magnitude(target, self);
		if ((double)Mathf.Abs(f) < 0.5)
		{
			return true;
		}
		return false;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (PeGameMgr.IsSingleAdventure && base.entity.NpcCmpt.IsStoreNpc)
		{
			return BehaveResult.Failure;
		}
		if (PeGameMgr.IsTutorial)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!PEUtil.CheckErrorPos(base.position))
		{
			return BehaveResult.Failure;
		}
		m_avoidSpeed = SpeedState.Walk;
		Vector3 avoidPos = Vector3.zero;
		Vector3 avoidPos2 = Vector3.zero;
		Vector3 avoidPos3 = Vector3.zero;
		Vector3 avoidPos4 = Vector3.zero;
		bool hasNearleague = base.entity.NpcCmpt.HasNearleague;
		bool flag = AiUtil.CheckBlockBrush(base.entity, out avoidPos);
		bool flag2 = PeSingleton<PeCreature>.Instance != null && AiUtil.CheckDig(base.entity, PeSingleton<PeCreature>.Instance.mainPlayer, out avoidPos2);
		bool flag3 = AiUtil.CheckDraging(base.entity, out avoidPos3);
		bool flag4 = AiUtil.CheckCreation(base.entity, out avoidPos4);
		bool flag5 = hasNearleague || flag2 || flag || flag3 || flag4;
		Vector3 vector = ((!(avoidPos != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos));
		Vector3 vector2 = ((!(avoidPos2 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos2));
		Vector3 vector3 = ((!(avoidPos3 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos3));
		Vector3 vector4 = ((!(avoidPos4 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos4));
		Vector3 dirtion = base.entity.peTrans.forward + vector + vector2 + vector3 + vector4;
		if (!flag5)
		{
			return BehaveResult.Failure;
		}
		if (flag || flag2 || flag3 || flag4)
		{
			m_avoidSpeed = SpeedState.Run;
		}
		m_AvoidPos = GetAvoidPos(dirtion);
		if (!PEUtil.CheckErrorPos(m_AvoidPos))
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		Vector3 avoidPos = Vector3.zero;
		Vector3 avoidPos2 = Vector3.zero;
		Vector3 avoidPos3 = Vector3.zero;
		Vector3 avoidPos4 = Vector3.zero;
		bool hasNearleague = base.entity.NpcCmpt.HasNearleague;
		bool flag = AiUtil.CheckBlockBrush(base.entity, out avoidPos);
		bool flag2 = PeSingleton<PeCreature>.Instance != null && AiUtil.CheckDig(base.entity, PeSingleton<PeCreature>.Instance.mainPlayer, out avoidPos2);
		bool flag3 = AiUtil.CheckDraging(base.entity, out avoidPos3);
		bool flag4 = AiUtil.CheckCreation(base.entity, out avoidPos4);
		bool flag5 = hasNearleague || flag2 || flag || flag3 || flag4;
		Vector3 vector = ((!(avoidPos != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos));
		Vector3 vector2 = ((!(avoidPos2 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos2));
		Vector3 vector3 = ((!(avoidPos3 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos3));
		Vector3 vector4 = ((!(avoidPos4 != Vector3.zero)) ? Vector3.zero : (base.position - avoidPos4));
		Vector3 vector5 = base.entity.peTrans.forward + vector + vector2 + vector3 + vector4;
		if (!flag5)
		{
			return BehaveResult.Failure;
		}
		if (!IsReach(base.position, m_AvoidPos))
		{
			bool flag6 = PEUtil.IsUnderBlock(base.entity);
			bool flag7 = PEUtil.IsForwardBlock(base.entity, base.existent.forward, 2f);
			if (flag6)
			{
				if (flag7 || Stucking())
				{
					SetPosition(m_AvoidPos);
					return BehaveResult.Failure;
				}
				MoveDirection(vector5);
			}
			else
			{
				if (Stucking())
				{
					SetPosition(m_AvoidPos);
					return BehaveResult.Failure;
				}
				MoveToPosition(m_AvoidPos, m_avoidSpeed);
			}
		}
		else
		{
			if (!flag5)
			{
				return BehaveResult.Failure;
			}
			m_AvoidPos = GetAvoidPos(vector5);
			if (!PEUtil.CheckErrorPos(m_AvoidPos))
			{
				return BehaveResult.Failure;
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		m_AvoidPos = Vector3.zero;
	}
}
