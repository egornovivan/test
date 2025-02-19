using ItemAsset;
using Pathea;
using UnityEngine;

namespace PETools;

public static class PE
{
	public static float PointInWater(Vector3 point)
	{
		if (null == VFVoxelWater.self)
		{
			return 0f;
		}
		return (!VFVoxelWater.self.IsInWater(point.x, point.y, point.z)) ? 0f : 1f;
	}

	public static float PointInTerrain(Vector3 point)
	{
		if (point.y < 0f)
		{
			return 1f;
		}
		return (float)(int)VFVoxelTerrain.self.Voxels.SafeRead(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z)).Volume / 255f;
	}

	public static bool RaycastVoxel(Ray ray, out Vector3 point, int maxdist, int step, int layer, bool voxel = true)
	{
		point = Vector3.zero;
		float num = maxdist + step;
		for (float num2 = 0f; num2 <= (float)maxdist; num2 += (float)step)
		{
			Vector3 point2 = ray.GetPoint(num2);
			if (((layer & 1) > 0 && PointInTerrain(point2) > 0.52f == voxel) || ((layer & 2) > 0 && PointInWater(point2) > 0.52f == voxel))
			{
				num = num2 - (float)step + 1f;
				break;
			}
		}
		if (num > (float)maxdist)
		{
			return false;
		}
		if (num < 0f)
		{
			point = ray.origin;
			return true;
		}
		float distance = num;
		for (float num3 = num; num3 < num + (float)step && num3 <= (float)maxdist; num3 += 1f)
		{
			Vector3 point3 = ray.GetPoint(num3);
			if (((layer & 1) > 0 && PointInTerrain(point3) > 0.52f == voxel) || ((layer & 2) > 0 && PointInWater(point3) > 0.52f == voxel))
			{
				distance = num3;
				break;
			}
		}
		point = ray.GetPoint(distance);
		return true;
	}

	public static bool CheckHumanSafePos(Vector3 pos)
	{
		if (PEUtil.CheckPositionUnderWater(pos))
		{
			return true;
		}
		if (null != VFVoxelTerrain.self)
		{
			return VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y + 0.5f), Mathf.RoundToInt(pos.z)).Volume < 128 && VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y + 1.5f), Mathf.RoundToInt(pos.z)).Volume < 128;
		}
		return false;
	}

	public static bool FindHumanSafePos(Vector3 originPos, out Vector3 safePos, int checkDis = 10)
	{
		for (int i = 0; i < checkDis; i++)
		{
			if (CheckHumanSafePos(originPos + i * Vector3.up))
			{
				safePos = originPos + i * Vector3.up;
				return true;
			}
		}
		safePos = originPos;
		return false;
	}

	public static RagdollHitInfo CapsuleHitToRagdollHit(PECapsuleHitResult hitResult)
	{
		RagdollHitInfo ragdollHitInfo = new RagdollHitInfo();
		ragdollHitInfo.hitTransform = hitResult.hitTrans;
		ragdollHitInfo.hitPoint = hitResult.hitPos;
		ragdollHitInfo.hitForce = hitResult.hitDir * 1000f;
		return ragdollHitInfo;
	}

	public static bool WeaponCanCombat(PeEntity entity, IWeapon weapon)
	{
		return SelectItem.EquipCanAttack(entity, weapon.ItemObj);
	}
}
