using ItemAsset;
using Pathea;
using Pathfinding;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTRunawayFromSkyEnemy), "RunawayFromSkyEnemy")]
public class BTRunawayFromSkyEnemy : BTNormal
{
	private class Data
	{
		[Behave]
		public float RunRadius;

		[Behave]
		public float minHpPercent;

		public float minRadius = 32f;
	}

	private Data m_Data;

	private Vector3 runPos;

	private float startRunTime;

	private float CHECK_TIME = 10f;

	private float startHideTime;

	private float CHECK_Hide_TIME = 1f;

	private void OnPathComplete(Path path)
	{
		if (path != null && path.vectorPath.Count > 0)
		{
			Vector3 vector = path.vectorPath[path.vectorPath.Count - 1];
			runPos = vector;
		}
	}

	private Vector3 GetPatrolPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
	{
		if (AstarPath.active != null)
		{
			RandomPath randomPath = RandomPath.Construct(base.position, (int)Random.Range(minRadius, maxRadius) * 100, OnPathComplete);
			randomPath.spread = 40000;
			randomPath.aimStrength = 1f;
			randomPath.aim = PEUtil.GetRandomPosition(base.position, direction, minRadius, maxRadius, -75f, 75f);
			AstarPath.StartPath(randomPath);
			return Vector3.zero;
		}
		return Vector3.zero;
	}

	private Vector3 GetRunDir(PeEntity npc, float radius = 32f)
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < base.Enemies.Count; i++)
		{
			float num = PEUtil.Magnitude(npc.position, base.Enemies[i].position);
			if (!(num > radius))
			{
				zero += (npc.position - base.Enemies[i].position).normalized * (1f - num / radius);
			}
		}
		return zero;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		startRunTime = Time.time;
		startHideTime = Time.time;
		SetCambat(value: false);
		Vector3 runDir = GetRunDir(base.entity);
		if (runDir == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		GetPatrolPosition(base.position, runDir, 32f, 64f);
		if (base.selectattackEnemy.entityTarget.Field == MovementField.Sky)
		{
			m_Data.minRadius = 32f;
		}
		else if (base.selectattackEnemy.entityTarget.IsBoss)
		{
			m_Data.minRadius = 128f;
		}
		else
		{
			m_Data.minRadius = 16f;
		}
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		bool flag = false;
		if (base.entity.NpcCmpt.BattleMgr.CanChoiceEnemy(base.Enemies) && !Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			flag = SelectItem.MatchEnemyAttack(base.entity, base.selectattackEnemy.entityTarget);
		}
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			SetCambat(value: true);
			return BehaveResult.Success;
		}
		if (!IsReached(base.position, base.selectattackEnemy.position, Is3D: false, m_Data.minRadius) || flag)
		{
			SetCambat(value: true);
			return BehaveResult.Success;
		}
		if (IsReached(runPos, base.position) || Stucking())
		{
			Vector3 runDir = GetRunDir(base.entity);
			GetPatrolPosition(base.position, runDir, 32f, 64f);
		}
		MoveToPosition(runPos, SpeedState.Run);
		return BehaveResult.Running;
	}
}
