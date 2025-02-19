using UnityEngine;

public static class BSMath
{
	public struct DrawTarget
	{
		public RaycastHit rch;

		public Vector3 snapto;

		public Vector3 cursor;

		public IBSDataSource ds;

		public IntVector3 iSnapto => new IntVector3(Mathf.FloorToInt(snapto.x), Mathf.FloorToInt(snapto.y), Mathf.FloorToInt(snapto.z));

		public IntVector3 iCursor => new IntVector3(Mathf.FloorToInt(cursor.x), Mathf.FloorToInt(cursor.y), Mathf.FloorToInt(cursor.z));
	}

	public const int MC_ISO_VALUE = 128;

	public const float MC_ISO_VALUEF = 127.5f;

	public const float Epsilon = 0.0001f;

	public const float Upsilon = 10000f;

	private static Vector3 Extent = new Vector3(64f, 32f, 64f);

	public static Bounds GetSafeBound(IBSDataSource ds)
	{
		Bounds lod0Bound = ds.Lod0Bound;
		float num = Mathf.Min(Extent.x, lod0Bound.extents.x);
		float num2 = Mathf.Min(Extent.y, lod0Bound.extents.y);
		float num3 = Mathf.Min(Extent.z, lod0Bound.extents.z);
		Bounds result = new Bounds(VFVoxelTerrain.self.LodMan._Lod0ViewBounds.center, new Vector3(num * 2f, num2 * 2f, num3 * 2f));
		return result;
	}

	public static bool RayCastV(Ray ray, IBSDataSource ds, out RaycastHit rch, int minvol = 1)
	{
		ray.origin -= ds.Offset;
		rch = default(RaycastHit);
		IntVector3 intVector = new IntVector3(Mathf.FloorToInt(ray.origin.x * (float)ds.ScaleInverted), Mathf.FloorToInt(ray.origin.y * (float)ds.ScaleInverted), Mathf.FloorToInt(ray.origin.z * (float)ds.ScaleInverted));
		BSVoxel voxel = ds.SafeRead(intVector.x, intVector.y, intVector.z);
		if (!ds.VoxelIsZero(voxel, minvol))
		{
			return false;
		}
		float num = ((ray.direction.x > 0.0001f) ? ds.Scale : ((!(ray.direction.x < -0.0001f)) ? 0f : (0f - ds.Scale)));
		float num2 = ((ray.direction.y > 0.0001f) ? ds.Scale : ((!(ray.direction.y < -0.0001f)) ? 0f : (0f - ds.Scale)));
		float num3 = ((ray.direction.z > 0.0001f) ? ds.Scale : ((!(ray.direction.z < -0.0001f)) ? 0f : (0f - ds.Scale)));
		Bounds safeBound = GetSafeBound(ds);
		float x = safeBound.min.x;
		float y = safeBound.min.y;
		float z = safeBound.min.z;
		float x2 = safeBound.max.x;
		float y2 = safeBound.max.y;
		float z2 = safeBound.max.z;
		float value = (int)((float)(int)ray.origin.x + ds.Scale + num * 0.5f);
		float value2 = (int)((float)(int)ray.origin.y + ds.Scale + num2 * 0.5f);
		float value3 = (int)((float)(int)ray.origin.z + ds.Scale + num3 * 0.5f);
		value = Mathf.Clamp(value, x, x2);
		value2 = Mathf.Clamp(value2, y, y2);
		value3 = Mathf.Clamp(value3, z, z2);
		ray.direction = ray.direction.normalized;
		float num4 = 10001f;
		float num5 = 10001f;
		float num6 = 10001f;
		if (num != 0f)
		{
			for (float num7 = value; num7 > x - 0.1f && num7 <= x2 + 0.1f; num7 += num)
			{
				float num8 = (num7 - ray.origin.x) / ray.direction.x;
				Vector3 vector = ray.origin + ray.direction * num8;
				voxel = ds.Read(Mathf.FloorToInt((vector.x + num * 0.5f) * (float)ds.ScaleInverted), Mathf.FloorToInt(vector.y * (float)ds.ScaleInverted), Mathf.FloorToInt(vector.z * (float)ds.ScaleInverted));
				if (!ds.VoxelIsZero(voxel, minvol))
				{
					num4 = num8;
					break;
				}
			}
		}
		if (num2 != 0f)
		{
			for (float num9 = value2; num9 >= y - 0.1f && num9 <= y2 + 0.1f; num9 += num2)
			{
				float num10 = (num9 - ray.origin.y) / ray.direction.y;
				Vector3 vector2 = ray.origin + ray.direction * num10;
				voxel = ds.Read(Mathf.FloorToInt(vector2.x * (float)ds.ScaleInverted), Mathf.FloorToInt((vector2.y + num2 * 0.5f) * (float)ds.ScaleInverted), Mathf.FloorToInt(vector2.z * (float)ds.ScaleInverted));
				if (!ds.VoxelIsZero(voxel, minvol))
				{
					num5 = num10;
					break;
				}
			}
		}
		if (num3 != 0f)
		{
			for (float num11 = value3; num11 >= z - 0.1f && num11 <= z2 + 0.1f; num11 += num3)
			{
				float num12 = (num11 - ray.origin.z) / ray.direction.z;
				Vector3 vector3 = ray.origin + ray.direction * num12;
				voxel = ds.Read(Mathf.FloorToInt(vector3.x * (float)ds.ScaleInverted), Mathf.FloorToInt(vector3.y * (float)ds.ScaleInverted), Mathf.FloorToInt((vector3.z + num3 * 0.5f) * (float)ds.ScaleInverted));
				if (!ds.VoxelIsZero(voxel, minvol))
				{
					num6 = num12;
					break;
				}
			}
		}
		if (num4 < 0f)
		{
			num4 = 10001f;
		}
		if (num5 < 0f)
		{
			num5 = 10001f;
		}
		if (num6 < 0f)
		{
			num6 = 10001f;
		}
		if (num4 > 10000f && num5 > 10000f && num6 > 10000f)
		{
			return false;
		}
		if (num4 < num5 && num4 < num6)
		{
			rch.point = ray.GetPoint(num4);
			rch.normal = Vector3.left * num;
			rch.distance = (rch.point - ray.origin).magnitude;
			return true;
		}
		if (num5 < num6 && num5 < num4)
		{
			rch.point = ray.GetPoint(num5);
			rch.normal = Vector3.down * num2;
			rch.distance = (rch.point - ray.origin).magnitude;
			return true;
		}
		if (num6 < num4 && num6 < num5)
		{
			rch.point = ray.GetPoint(num6);
			rch.normal = Vector3.back * num3;
			rch.distance = (rch.point - ray.origin).magnitude;
			return true;
		}
		return false;
	}

	public static bool RayCastDrawTarget(Ray ray, IBSDataSource ds, out DrawTarget target, int minvol, bool ignoreDiagonal = true, params IBSDataSource[] relative_ds)
	{
		target = default(DrawTarget);
		float num = 10000f;
		if (ds == null)
		{
			return false;
		}
		IBSDataSource iBSDataSource = null;
		if (RayCastV(ray, ds, out var rch, minvol))
		{
			num = rch.distance;
			iBSDataSource = ds;
		}
		foreach (IBSDataSource iBSDataSource2 in relative_ds)
		{
			if (iBSDataSource2 != ds && RayCastV(ray, iBSDataSource2, out var rch2, minvol) && num > rch2.distance)
			{
				rch = rch2;
				num = rch2.distance;
				iBSDataSource = iBSDataSource2;
			}
		}
		target.ds = iBSDataSource;
		if (iBSDataSource != null)
		{
			if (iBSDataSource == ds)
			{
				target.rch = rch;
				target.snapto = new Vector3((float)Mathf.FloorToInt((rch.point.x - rch.normal.x * 0.5f) * (float)ds.ScaleInverted) * ds.Scale, (float)Mathf.FloorToInt((rch.point.y - rch.normal.y * 0.5f) * (float)ds.ScaleInverted) * ds.Scale, (float)Mathf.FloorToInt((rch.point.z - rch.normal.z * 0.5f) * (float)ds.ScaleInverted) * ds.Scale);
				target.cursor = new Vector3((float)Mathf.FloorToInt((rch.point.x + rch.normal.x * 0.5f) * (float)ds.ScaleInverted) * ds.Scale, (float)Mathf.FloorToInt((rch.point.y + rch.normal.y * 0.5f) * (float)ds.ScaleInverted) * ds.Scale, (float)Mathf.FloorToInt((rch.point.z + rch.normal.z * 0.5f) * (float)ds.ScaleInverted) * ds.Scale);
				IntVector3 intVector = DiagonalOffset(rch.point * ds.ScaleInverted, rch.normal * ds.ScaleInverted, ds.DiagonalOffset);
				if (intVector.x != 0)
				{
					Vector3 vector = target.snapto * ds.ScaleInverted + new Vector3(intVector.x, 0f, 0f);
					BSVoxel voxel = ds.SafeRead((int)vector.x, (int)vector.y, (int)vector.z);
					if (!ds.VoxelIsZero(voxel, minvol))
					{
						intVector.x = 0;
					}
				}
				if (intVector.y != 0)
				{
					Vector3 vector2 = target.snapto * ds.ScaleInverted + new Vector3(0f, intVector.y, 0f);
					BSVoxel voxel2 = ds.SafeRead((int)vector2.x, (int)vector2.y, (int)vector2.z);
					if (!ds.VoxelIsZero(voxel2, minvol))
					{
						intVector.y = 0;
					}
				}
				if (intVector.z != 0)
				{
					Vector3 vector3 = target.snapto * ds.ScaleInverted + new Vector3(0f, 0f, intVector.z);
					BSVoxel voxel3 = ds.SafeRead((int)vector3.x, (int)vector3.y, (int)vector3.z);
					if (!ds.VoxelIsZero(voxel3, minvol))
					{
						intVector.z = 0;
					}
				}
				if (intVector.x != 0 && intVector.y != 0)
				{
					Vector3 vector4 = target.snapto * ds.ScaleInverted + new Vector3(intVector.x, intVector.y, 0f);
					BSVoxel voxel4 = ds.SafeRead((int)vector4.x, (int)vector4.y, (int)vector4.z);
					if (!ds.VoxelIsZero(voxel4, minvol))
					{
						intVector.x = 0;
						intVector.y = 0;
					}
				}
				if (intVector.x != 0 && intVector.z != 0)
				{
					Vector3 vector5 = target.snapto * ds.ScaleInverted + new Vector3(intVector.x, 0f, intVector.z);
					BSVoxel voxel5 = ds.SafeRead((int)vector5.x, (int)vector5.y, (int)vector5.z);
					if (!ds.VoxelIsZero(voxel5, minvol))
					{
						intVector.x = 0;
						intVector.z = 0;
					}
				}
				if (intVector.y != 0 && intVector.z != 0)
				{
					Vector3 vector6 = target.snapto * ds.ScaleInverted + new Vector3(0f, intVector.y, intVector.z);
					BSVoxel voxel6 = ds.SafeRead((int)vector6.x, (int)vector6.y, (int)vector6.z);
					if (!ds.VoxelIsZero(voxel6, minvol))
					{
						intVector.y = 0;
						intVector.z = 0;
					}
				}
				target.cursor += intVector.ToVector3() * ds.Scale;
				if (ds == BuildingMan.Blocks && !ignoreDiagonal)
				{
					Vector3 vector7 = target.snapto * ds.ScaleInverted;
					BSVoxel bSVoxel = ds.SafeRead((int)vector7.x, (int)vector7.y, (int)vector7.z);
					if (bSVoxel.blockType > 128 && bSVoxel.blockType >> 2 != 63)
					{
						return false;
					}
				}
			}
			else
			{
				rch.point += iBSDataSource.Offset;
				rch.point -= ds.Offset;
				target.rch = rch;
				float num2 = Mathf.Min(iBSDataSource.Scale, ds.Scale);
				target.snapto = new Vector3((float)Mathf.FloorToInt((rch.point.x - rch.normal.x * 0.5f * num2) * (float)ds.ScaleInverted) * ds.Scale, (float)Mathf.FloorToInt((rch.point.y - rch.normal.y * 0.5f * num2) * (float)ds.ScaleInverted) * ds.Scale, (float)Mathf.FloorToInt((rch.point.z - rch.normal.z * 0.5f * num2) * (float)ds.ScaleInverted) * ds.Scale);
				target.cursor = new Vector3((float)Mathf.FloorToInt((rch.point.x + rch.normal.x * 0.5f * num2) * (float)ds.ScaleInverted) * ds.Scale, (float)Mathf.FloorToInt((rch.point.y + rch.normal.y * 0.5f * num2) * (float)ds.ScaleInverted) * ds.Scale, (float)Mathf.FloorToInt((rch.point.z + rch.normal.z * 0.5f * num2) * (float)ds.ScaleInverted) * ds.Scale);
			}
			return true;
		}
		return false;
	}

	public static bool RayCastCoordPlane(Ray ray, ECoordPlane coordplane, float position, out RaycastHit rch)
	{
		rch = default(RaycastHit);
		float num = 0.0001f;
		ray.direction.Normalize();
		switch (coordplane)
		{
		case ECoordPlane.XY:
		{
			if (Mathf.Abs(ray.direction.z) < num)
			{
				return false;
			}
			float num4 = (position - ray.origin.z) / ray.direction.z;
			if (num4 < 0.01f)
			{
				return false;
			}
			rch.point = ray.GetPoint(num4);
			rch.distance = num4;
			rch.normal = ((!(ray.direction.z > 0f)) ? Vector3.forward : Vector3.back);
			return true;
		}
		case ECoordPlane.XZ:
		{
			if (Mathf.Abs(ray.direction.y) < num)
			{
				return false;
			}
			float num3 = (position - ray.origin.y) / ray.direction.y;
			if (num3 < 0.01f)
			{
				return false;
			}
			rch.point = ray.GetPoint(num3);
			rch.distance = num3;
			rch.normal = ((!(ray.direction.y > 0f)) ? Vector3.up : Vector3.down);
			return true;
		}
		case ECoordPlane.ZY:
		{
			if (Mathf.Abs(ray.direction.x) < num)
			{
				return false;
			}
			float num2 = (position - ray.origin.x) / ray.direction.x;
			if (num2 < 0.01f)
			{
				return false;
			}
			rch.point = ray.GetPoint(num2);
			rch.distance = num2;
			rch.normal = ((!(ray.direction.x > 0f)) ? Vector3.right : Vector3.left);
			return true;
		}
		default:
			return false;
		}
	}

	public static bool RayAdjustHeight(Ray ray, Vector3 basepoint, out float height)
	{
		height = basepoint.y;
		if (Mathf.Abs(ray.direction.x) < 0.001f && Mathf.Abs(ray.direction.z) < 0.001f)
		{
			return false;
		}
		Vector3 normalized = Vector3.Cross(Vector3.up, ray.direction).normalized;
		Plane plane = new Plane(ray.origin, ray.GetPoint(10f), ray.origin + normalized * 10f);
		Ray ray2 = new Ray(basepoint + Vector3.up * 2000f, Vector3.down);
		float enter = 0f;
		if (plane.Raycast(ray2, out enter))
		{
			height = ray2.GetPoint(enter).y;
			return true;
		}
		return false;
	}

	public static bool RayAdjustHeight(Ray ray, ECoordAxis height_axis, Vector3 basepoint, out float height)
	{
		height = 0f;
		switch (height_axis)
		{
		case ECoordAxis.X:
		{
			height = basepoint.x;
			if (Mathf.Abs(ray.direction.y) < 0.001f && Mathf.Abs(ray.direction.z) < 0.001f)
			{
				return false;
			}
			Vector3 normalized3 = Vector3.Cross(Vector3.right, ray.direction).normalized;
			Plane plane3 = new Plane(ray.origin, ray.GetPoint(10f), ray.origin + normalized3 * 10f);
			Ray ray4 = new Ray(basepoint + Vector3.right * 2000f, Vector3.left);
			float enter3 = 0f;
			if (plane3.Raycast(ray4, out enter3))
			{
				height = ray4.GetPoint(enter3).x;
				return true;
			}
			return false;
		}
		case ECoordAxis.Y:
		{
			height = basepoint.y;
			if (Mathf.Abs(ray.direction.x) < 0.001f && Mathf.Abs(ray.direction.z) < 0.001f)
			{
				return false;
			}
			Vector3 normalized2 = Vector3.Cross(Vector3.up, ray.direction).normalized;
			Plane plane2 = new Plane(ray.origin, ray.GetPoint(10f), ray.origin + normalized2 * 10f);
			Ray ray3 = new Ray(basepoint + Vector3.up * 2000f, Vector3.down);
			float enter2 = 0f;
			if (plane2.Raycast(ray3, out enter2))
			{
				height = ray3.GetPoint(enter2).y;
				return true;
			}
			return false;
		}
		case ECoordAxis.Z:
		{
			height = basepoint.z;
			if (Mathf.Abs(ray.direction.x) < 0.001f && Mathf.Abs(ray.direction.y) < 0.001f)
			{
				return false;
			}
			Vector3 normalized = Vector3.Cross(Vector3.forward, ray.direction).normalized;
			Plane plane = new Plane(ray.origin, ray.GetPoint(10f), ray.origin + normalized * 10f);
			Ray ray2 = new Ray(basepoint + Vector3.forward * 2000f, Vector3.back);
			float enter = 0f;
			if (plane.Raycast(ray2, out enter))
			{
				height = ray2.GetPoint(enter).z;
				return true;
			}
			return false;
		}
		default:
			return false;
		}
	}

	private static IntVector3 DiagonalOffset(Vector3 v, Vector3 n, float eps = 0.15f)
	{
		int num = Mathf.FloorToInt(v.x);
		int num2 = Mathf.FloorToInt(v.y);
		int num3 = Mathf.FloorToInt(v.z);
		float num4 = v.x - (float)num;
		float num5 = v.y - (float)num2;
		float num6 = v.z - (float)num3;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		float num10 = 1f - eps;
		if (num4 < eps)
		{
			num7--;
		}
		else if (num4 > num10)
		{
			num7++;
		}
		if (num5 < eps)
		{
			num8--;
		}
		else if (num5 > num10)
		{
			num8++;
		}
		if (num6 < eps)
		{
			num9--;
		}
		else if (num6 > num10)
		{
			num9++;
		}
		if (n.x != 0f)
		{
			num7 = 0;
		}
		if (n.y != 0f)
		{
			num8 = 0;
		}
		if (n.z != 0f)
		{
			num9 = 0;
		}
		return new Vector3(num7, num8, num9);
	}
}
