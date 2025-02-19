using UnityEngine;

namespace PETools;

public static class PEMath
{
	public struct DrawTarget
	{
		public RaycastHit rch;

		public IntVector3 snapto;

		public IntVector3 cursor;
	}

	public const float Epsilon = float.Epsilon;

	public const int MC_ISO_VALUE = 128;

	public const float MC_ISO_VALUEF = 127.5f;

	public static bool RayCastVoxel(Ray ray, out RaycastHit rch, int minvol = 1)
	{
		rch = default(RaycastHit);
		if (VFVoxelTerrain.self == null)
		{
			return false;
		}
		IVxDataSource voxels = VFVoxelTerrain.self.Voxels;
		float num = 0.0001f;
		float num2 = 10000f;
		if (voxels.SafeRead(Mathf.FloorToInt(ray.origin.x), Mathf.FloorToInt(ray.origin.y), Mathf.FloorToInt(ray.origin.z)).Volume >= minvol)
		{
			return false;
		}
		float num3 = ((ray.direction.x > num) ? 1 : ((ray.direction.x < 0f - num) ? (-1) : 0));
		float num4 = ((ray.direction.x > num) ? 1 : ((ray.direction.x < 0f - num) ? (-1) : 0));
		float num5 = ((ray.direction.x > num) ? 1 : ((ray.direction.x < 0f - num) ? (-1) : 0));
		Bounds lod0ViewBounds = VFVoxelTerrain.self.LodMan._Lod0ViewBounds;
		float x = lod0ViewBounds.min.x;
		float y = lod0ViewBounds.min.y;
		float z = lod0ViewBounds.min.z;
		float x2 = lod0ViewBounds.max.x;
		float y2 = lod0ViewBounds.max.y;
		float z2 = lod0ViewBounds.max.z;
		float value = (int)((float)((int)ray.origin.x + 1) + num3 * 0.5f);
		float value2 = (int)((float)((int)ray.origin.y + 1) + num4 * 0.5f);
		float value3 = (int)((float)((int)ray.origin.z + 1) + num5 * 0.5f);
		value = Mathf.Clamp(value, x, x2);
		value2 = Mathf.Clamp(value2, y, y2);
		value3 = Mathf.Clamp(value3, z, z2);
		ray.direction = ray.direction.normalized;
		if (num3 != 0f)
		{
			for (float num6 = value; num6 > x - 0.1f && num6 <= x2 + 0.1f; num6 += num3)
			{
				float num7 = (num6 - ray.origin.x) / ray.direction.x;
				Vector3 vector = ray.origin + ray.direction * num7;
				if (voxels.Read(Mathf.FloorToInt(vector.x + num3 * 0.5f), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z)).Volume >= minvol)
				{
					break;
				}
			}
		}
		return false;
	}

	public static bool IsNumeral(string tmp)
	{
		try
		{
			int.Parse(tmp);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
