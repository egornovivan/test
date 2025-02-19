using UnityEngine;

namespace Pathea.PeEntityExtMotion_Move;

public static class PeEntityExtMotion_Move
{
	public static float GetWalkSpeed(this PeEntity entity)
	{
		return 1f;
	}

	public static void SetWalkSpeed(this PeEntity entity, float value)
	{
	}

	public static float GetRunSpeed(this PeEntity entity)
	{
		return 1f;
	}

	public static void SetRunSpeed(this PeEntity entity, float value)
	{
	}

	public static void MoveTo(this PeEntity entity, Vector3 dest, float stopDistance, float speed)
	{
		Motion_Move cmpt = entity.GetCmpt<Motion_Move>();
		if (!(cmpt == null))
		{
			cmpt.SetSpeed(speed);
			cmpt.MoveTo(dest);
		}
	}

	public static void MoveTo(this PeEntity entity, Vector3 dst)
	{
		Motion_Move cmpt = entity.GetCmpt<Motion_Move>();
		if (!(cmpt == null))
		{
			cmpt.MoveTo(dst);
		}
	}

	public static void PatrolMoveTo(this PeEntity entity, Vector3 dst)
	{
	}

	public static bool DisableMoveCheck(this PeEntity entity)
	{
		return false;
	}

	public static bool EnableMoveCheck(this PeEntity entity)
	{
		return false;
	}

	public static void CmdFaceToPoint(this PeEntity entity, Vector3 point)
	{
	}

	public static void CmdFaceToDirect(this PeEntity entity, Vector3 direct)
	{
	}

	public static void CmdFollow(this PeEntity entity, Transform dst)
	{
	}

	public static Transform GetFollowing(this PeEntity entity)
	{
		return null;
	}

	public static void StopMove(this PeEntity entity)
	{
	}
}
