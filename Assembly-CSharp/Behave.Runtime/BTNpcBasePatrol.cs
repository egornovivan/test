using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBasePatrol), "NpcBasePatrol")]
public class BTNpcBasePatrol : BTNormal
{
	private Vector3 m_CurPatrolPos;

	private float mStartTime;

	private float mPatrolTime = 35f;

	private bool GetPatrolPos(out Vector3 guardPos)
	{
		if (base.BaseEntities != null)
		{
			List<CSEntity> list = base.BaseEntities.FindAll((CSEntity ret) => ret.gameObject != null);
			if (list.Count > 0)
			{
				CSEntity cSEntity = list[Random.Range(0, list.Count)];
				float num = Random.Range(cSEntity.Bound.extents.x + 1f, cSEntity.Bound.extents.x + 3f);
				float num2 = Random.Range(cSEntity.Bound.extents.z + 1f, cSEntity.Bound.extents.z + 3f);
				num *= (float)((Random.value < 0.5f) ? 1 : (-1));
				num2 *= (float)((Random.value < 0.5f) ? 1 : (-1));
				Vector3 vector = cSEntity.gameObject.transform.TransformPoint(new Vector3(num, 0f, num2));
				if (PEUtil.GetPositionLayer(vector, out var point, BTNormal.WanderLayer, BTNormal.IgnoreWanderLayer))
				{
					guardPos = point;
					return true;
				}
			}
		}
		guardPos = Vector3.zero;
		return false;
	}

	private bool GetCanWalkPos(out Vector3 guardPos)
	{
		Vector3 guardPos2 = Vector3.zero;
		if (GetPatrolPos(out guardPos2) && AiUtil.GetNearNodePosWalkable(guardPos2, out guardPos))
		{
			return true;
		}
		guardPos = Vector3.zero;
		return false;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Soldier || base.NpcSoldier != ENpcSoldier.Patrol)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAttackEnemy)
		{
			return BehaveResult.Failure;
		}
		if (!GetCanWalkPos(out m_CurPatrolPos))
		{
			return BehaveResult.Failure;
		}
		SetNpcState(ENpcState.Patrol);
		mStartTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Soldier || base.NpcSoldier != ENpcSoldier.Patrol)
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Failure;
		}
		if (base.hasAttackEnemy)
		{
			return BehaveResult.Failure;
		}
		SetNpcAiType(ENpcAiType.NpcBaseSoldier_Patrol);
		if (Stucking(5f))
		{
			SetNpcState(ENpcState.UnKnown);
			SetPosition(m_CurPatrolPos);
			return BehaveResult.Success;
		}
		if (PEUtil.SqrMagnitudeH(base.position, m_CurPatrolPos) < 0.25f)
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Success;
		}
		if (Time.time - mStartTime > mPatrolTime)
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Success;
		}
		MoveToPosition(m_CurPatrolPos);
		return BehaveResult.Running;
	}
}
