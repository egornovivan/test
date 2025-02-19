using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBaseGuard), "NpcBaseGuard")]
public class BTNpcBaseGuard : BTNormal
{
	private Vector3 m_CurWanderPos;

	private Vector3 mGuardPos;

	private bool GetWanderPos(out Vector3 guardPos)
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

	private bool GetCanWalkPos(out Vector3 walkPos)
	{
		if (GetWanderPos(out var guardPos) && AiUtil.GetNearNodePosWalkable(guardPos, out walkPos))
		{
			return true;
		}
		walkPos = Vector3.zero;
		return false;
	}

	private Vector3 GetGuardPos(Vector3 nowPos, Vector3 centorPos, float Maxradiu, float minRadiu)
	{
		float num = PEUtil.Magnitude(nowPos, centorPos);
		if (num < Maxradiu && num > minRadiu)
		{
			return nowPos;
		}
		if (NpcCanWalkPos(centorPos, Maxradiu, out var walkPos))
		{
			return walkPos;
		}
		return centorPos;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Soldier || base.NpcSoldier != ENpcSoldier.Guard)
		{
			return BehaveResult.Failure;
		}
		if (NpcMgr.IsOutRadiu(base.position, base.Creater.Assembly.Position, base.Creater.Assembly.Radius))
		{
			return BehaveResult.Failure;
		}
		mGuardPos = GetGuardPos(base.position, base.Creater.Assembly.Position, base.Creater.Assembly.Radius, 5f);
		SetNpcState(ENpcState.Patrol);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcBaseSoldier_Guard);
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Soldier || base.NpcSoldier != ENpcSoldier.Guard)
		{
			SetNpcState(ENpcState.UnKnown);
			return BehaveResult.Failure;
		}
		if (NpcMgr.IsOutRadiu(base.position, base.Creater.Assembly.Position, base.Creater.Assembly.Radius))
		{
			return BehaveResult.Failure;
		}
		if (Stucking() || PEUtil.Magnitude(base.position, mGuardPos) < 1f)
		{
			SetNpcState(ENpcState.Patrol);
			StopMove();
			return BehaveResult.Running;
		}
		if (Stucking())
		{
			mGuardPos = GetGuardPos(base.position, base.Creater.Assembly.Position, base.Creater.Assembly.Radius, 5f);
		}
		MoveToPosition(mGuardPos);
		return BehaveResult.Running;
	}
}
