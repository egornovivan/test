using System;
using System.Collections.Generic;
using UnityEngine;

public static class VCEMath
{
	public enum EPickLayer
	{
		Part = 1,
		Effect = 2,
		Decal = 4,
		All = 65535
	}

	public struct DrawTarget
	{
		public RaycastHit rch;

		public IntVector3 snapto;

		public IntVector3 cursor;
	}

	public const int MC_ISO_VALUE = 128;

	public const float MC_ISO_VALUEF = 127.5f;

	public static Ray TransformRayToIsoCoord(Ray world_ray)
	{
		return new Ray(world_ray.origin / VCEditor.s_Scene.m_Setting.m_VoxelSize, world_ray.direction.normalized);
	}

	public static bool RayCastVoxel(Ray ray, out RaycastHit rch, int minvol = 1)
	{
		rch = default(RaycastHit);
		if (!VCEditor.DocumentOpen())
		{
			return false;
		}
		VCIsoData isoData = VCEditor.s_Scene.m_IsoData;
		float num = 0.0001f;
		float num2 = 10000f;
		ray = TransformRayToIsoCoord(ray);
		if (isoData.GetVoxel(VCIsoData.IPosToKey(Mathf.FloorToInt(ray.origin.x), Mathf.FloorToInt(ray.origin.y), Mathf.FloorToInt(ray.origin.z))).Volume >= minvol)
		{
			return false;
		}
		float num3 = ((ray.direction.x > num) ? 1f : ((!(ray.direction.x < 0f - num)) ? 0f : (-1f)));
		float num4 = ((ray.direction.y > num) ? 1f : ((!(ray.direction.y < 0f - num)) ? 0f : (-1f)));
		float num5 = ((ray.direction.z > num) ? 1f : ((!(ray.direction.z < 0f - num)) ? 0f : (-1f)));
		float value = (int)((float)((int)ray.origin.x + 1) + num3 * 0.5f);
		float value2 = (int)((float)((int)ray.origin.y + 1) + num4 * 0.5f);
		float value3 = (int)((float)((int)ray.origin.z + 1) + num5 * 0.5f);
		value = Mathf.Clamp(value, 0f, VCEditor.s_Scene.m_Setting.m_EditorSize.x);
		value2 = Mathf.Clamp(value2, 0f, VCEditor.s_Scene.m_Setting.m_EditorSize.y);
		value3 = Mathf.Clamp(value3, 0f, VCEditor.s_Scene.m_Setting.m_EditorSize.z);
		ray.direction = ray.direction.normalized;
		float num6 = num2 + 1f;
		float num7 = num2 + 1f;
		float num8 = num2 + 1f;
		if (num3 != 0f)
		{
			for (float num9 = value; num9 > -0.1f && num9 <= (float)VCEditor.s_Scene.m_Setting.m_EditorSize.x + 0.1f; num9 += num3)
			{
				float num10 = (num9 - ray.origin.x) / ray.direction.x;
				Vector3 vector = ray.origin + ray.direction * num10;
				if (isoData.GetVoxel(VCIsoData.IPosToKey(Mathf.FloorToInt(vector.x + num3 * 0.5f), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z))).Volume >= minvol)
				{
					num6 = num10;
					break;
				}
			}
		}
		if (num4 != 0f)
		{
			for (float num11 = value2; num11 >= -0.1f && num11 <= (float)VCEditor.s_Scene.m_Setting.m_EditorSize.y + 0.1f; num11 += num4)
			{
				float num12 = (num11 - ray.origin.y) / ray.direction.y;
				Vector3 vector2 = ray.origin + ray.direction * num12;
				if (isoData.GetVoxel(VCIsoData.IPosToKey(Mathf.FloorToInt(vector2.x), Mathf.FloorToInt(vector2.y + num4 * 0.5f), Mathf.FloorToInt(vector2.z))).Volume >= minvol)
				{
					num7 = num12;
					break;
				}
			}
		}
		if (num5 != 0f)
		{
			for (float num13 = value3; num13 >= -0.1f && num13 <= (float)VCEditor.s_Scene.m_Setting.m_EditorSize.z + 0.1f; num13 += num5)
			{
				float num14 = (num13 - ray.origin.z) / ray.direction.z;
				Vector3 vector3 = ray.origin + ray.direction * num14;
				if (isoData.GetVoxel(VCIsoData.IPosToKey(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.y), Mathf.FloorToInt(vector3.z + num5 * 0.5f))).Volume >= minvol)
				{
					num8 = num14;
					break;
				}
			}
		}
		if (num6 < 0f)
		{
			num6 = num2 + 1f;
		}
		if (num7 < 0f)
		{
			num7 = num2 + 1f;
		}
		if (num8 < 0f)
		{
			num8 = num2 + 1f;
		}
		if (num6 > num2 && num7 > num2 && num8 > num2)
		{
			return false;
		}
		if (num6 < num7 && num6 < num8)
		{
			rch.point = ray.GetPoint(num6);
			rch.normal = Vector3.left * num3;
			rch.distance = (rch.point - ray.origin).magnitude;
			return true;
		}
		if (num7 < num8 && num7 < num6)
		{
			rch.point = ray.GetPoint(num7);
			rch.normal = Vector3.down * num4;
			rch.distance = (rch.point - ray.origin).magnitude;
			return true;
		}
		if (num8 < num6 && num8 < num7)
		{
			rch.point = ray.GetPoint(num8);
			rch.normal = Vector3.back * num5;
			rch.distance = (rch.point - ray.origin).magnitude;
			return true;
		}
		return false;
	}

	public static bool RayCastGrid(Ray ray, out RaycastHit rch)
	{
		rch = default(RaycastHit);
		if (!VCEditor.DocumentOpen())
		{
			return false;
		}
		if (Physics.Raycast(ray, out rch, VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 5f, VCConfig.s_EditorLayerMask))
		{
			if (rch.collider.gameObject.GetComponent<GLGridPlane>() != null)
			{
				rch.point /= VCEditor.s_Scene.m_Setting.m_VoxelSize;
				rch.distance /= VCEditor.s_Scene.m_Setting.m_VoxelSize;
				return true;
			}
			rch = default(RaycastHit);
			return false;
		}
		return false;
	}

	public static bool RayCastCoordPlane(Ray ray, ECoordPlane coordplane, float position, out RaycastHit rch)
	{
		rch = default(RaycastHit);
		if (!VCEditor.DocumentOpen())
		{
			return false;
		}
		float num = 0.0001f;
		ray = TransformRayToIsoCoord(ray);
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

	public static bool RayCastDrawTarget(Ray ray, out DrawTarget target, int minvol, bool voxelbest = false)
	{
		target = default(DrawTarget);
		if (!VCEditor.DocumentOpen())
		{
			return false;
		}
		RaycastHit rch = default(RaycastHit);
		RaycastHit rch2 = default(RaycastHit);
		float num = 10000f;
		float num2 = 10000f;
		bool flag = true;
		if (RayCastVoxel(VCEInput.s_PickRay, out rch, minvol))
		{
			num = rch.distance;
			if (voxelbest)
			{
				flag = false;
			}
		}
		if (flag && RayCastGrid(VCEInput.s_PickRay, out rch2) && rch2.normal.y > 0f)
		{
			num2 = rch2.distance;
		}
		if (num < num2)
		{
			target.rch = rch;
			target.snapto = new IntVector3(Mathf.FloorToInt(rch.point.x - rch.normal.x * 0.5f), Mathf.FloorToInt(rch.point.y - rch.normal.y * 0.5f), Mathf.FloorToInt(rch.point.z - rch.normal.z * 0.5f));
			target.cursor = new IntVector3(Mathf.FloorToInt(rch.point.x + rch.normal.x * 0.5f), Mathf.FloorToInt(rch.point.y + rch.normal.y * 0.5f), Mathf.FloorToInt(rch.point.z + rch.normal.z * 0.5f));
			return true;
		}
		if (num2 < num)
		{
			target.rch = rch2;
			target.snapto = new IntVector3(Mathf.FloorToInt(rch2.point.x - rch2.normal.x * 0.5f), Mathf.FloorToInt(rch2.point.y - rch2.normal.y * 0.5f), Mathf.FloorToInt(rch2.point.z - rch2.normal.z * 0.5f));
			target.cursor = new IntVector3(Mathf.FloorToInt(rch2.point.x + rch2.normal.x * 0.5f), Mathf.FloorToInt(rch2.point.y + rch2.normal.y * 0.5f), Mathf.FloorToInt(rch2.point.z + rch2.normal.z * 0.5f));
			if (VCEditor.s_Scene.m_IsoData.GetVoxel(VCIsoData.IPosToKey(target.cursor)).Volume > 128)
			{
				target.cursor = null;
				return false;
			}
			return true;
		}
		target.rch = default(RaycastHit);
		target.snapto = null;
		target.cursor = null;
		return false;
	}

	public static bool RayAdjustHeight(Ray ray, Vector3 basepoint, out float height)
	{
		height = basepoint.y;
		if (!VCEditor.DocumentOpen())
		{
			return false;
		}
		ray = TransformRayToIsoCoord(ray);
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

	public static List<VCEComponentTool> RayPickComponents(Ray ray)
	{
		if (!VCEditor.DocumentOpen())
		{
			return null;
		}
		float num = 1000000f;
		if (RayCastVoxel(ray, out var rch, 128))
		{
			num = rch.distance * VCEditor.s_Scene.m_Setting.m_VoxelSize;
		}
		RaycastHit[] array = Physics.RaycastAll(ray, VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 10f, VCConfig.s_EditorLayerMask);
		for (int i = 0; i < array.Length - 1; i++)
		{
			for (int j = i + 1; j < array.Length; j++)
			{
				if (array[i].distance > array[j].distance)
				{
					RaycastHit raycastHit = array[i];
					ref RaycastHit reference = ref array[i];
					reference = array[j];
					array[j] = raycastHit;
				}
			}
		}
		List<VCEComponentTool> list = new List<VCEComponentTool>();
		RaycastHit[] array2 = array;
		for (int k = 0; k < array2.Length; k++)
		{
			RaycastHit raycastHit2 = array2[k];
			if (raycastHit2.distance <= num)
			{
				GLComponentBound component = raycastHit2.collider.gameObject.GetComponent<GLComponentBound>();
				if (component != null)
				{
					list.Add(component.m_ParentComponent);
				}
			}
		}
		return list;
	}

	public static VCEComponentTool RayPickComponent(Ray ray, int order = 0)
	{
		if (!VCEditor.DocumentOpen())
		{
			return null;
		}
		float num = 1000000f;
		if (RayCastVoxel(ray, out var rch, 128))
		{
			num = rch.distance * VCEditor.s_Scene.m_Setting.m_VoxelSize;
		}
		RaycastHit[] array = Physics.RaycastAll(ray, VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 10f, VCConfig.s_EditorLayerMask);
		for (int i = 0; i < array.Length - 1; i++)
		{
			for (int j = i + 1; j < array.Length; j++)
			{
				if (array[i].distance > array[j].distance)
				{
					RaycastHit raycastHit = array[i];
					ref RaycastHit reference = ref array[i];
					reference = array[j];
					array[j] = raycastHit;
				}
			}
		}
		List<VCEComponentTool> list = new List<VCEComponentTool>();
		RaycastHit[] array2 = array;
		for (int k = 0; k < array2.Length; k++)
		{
			RaycastHit raycastHit2 = array2[k];
			if (raycastHit2.distance <= num)
			{
				GLComponentBound component = raycastHit2.collider.gameObject.GetComponent<GLComponentBound>();
				if (component != null)
				{
					list.Add(component.m_ParentComponent);
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[order % list.Count];
	}

	public static bool RayCastMesh(Ray ray, out RaycastHit rch, float dist)
	{
		if (Physics.Raycast(ray, out rch, dist, VCConfig.s_EditorLayerMask) && VCEditor.Instance.m_MeshMgr.Exist(rch.collider.gameObject))
		{
			return true;
		}
		return false;
	}

	public static bool RayCastMesh(Ray ray, out RaycastHit rch)
	{
		return RayCastMesh(ray, out rch, VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 2f);
	}

	public static List<IntVector3> AffectedChunks(IntBox box)
	{
		int num = 5;
		box.xMax++;
		box.yMax++;
		box.zMax++;
		List<IntVector3> list = new List<IntVector3>();
		for (int i = box.xMin >> num; i <= box.xMax >> num; i++)
		{
			for (int j = box.yMin >> num; j <= box.yMax >> num; j++)
			{
				for (int k = box.zMin >> num; k <= box.zMax >> num; k++)
				{
					list.Add(new IntVector3(i, j, k));
				}
			}
		}
		return list;
	}

	public static float BoxFeather(IntVector3 pos, IntVector3 min, IntVector3 max, float feather_dist)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (pos.x < min.x)
		{
			num = ((float)(min.x - pos.x) - 0.5f) / feather_dist;
		}
		else if (pos.x > max.x)
		{
			num = ((float)(pos.x - max.x) - 0.5f) / feather_dist;
		}
		if (pos.y < min.y)
		{
			num2 = ((float)(min.y - pos.y) - 0.5f) / feather_dist;
		}
		else if (pos.y > max.y)
		{
			num2 = ((float)(pos.y - max.y) - 0.5f) / feather_dist;
		}
		if (pos.z < min.z)
		{
			num3 = ((float)(min.z - pos.z) - 0.5f) / feather_dist;
		}
		else if (pos.z > max.z)
		{
			num3 = ((float)(pos.z - max.z) - 0.5f) / feather_dist;
		}
		return 1f - Mathf.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public static float DetermineVolume(int x, int y, int z, Plane p)
	{
		Vector3 vector = new Vector3((float)x + 0.5f, (float)y + 0.5f, (float)z + 0.5f);
		float num = Mathf.Sign(p.normal.x);
		float num2 = Mathf.Sign(p.normal.y);
		float num3 = Mathf.Sign(p.normal.z);
		float distanceToPoint = p.GetDistanceToPoint(vector);
		if (distanceToPoint > 0f)
		{
			Ray ray = new Ray(vector, Vector3.left * num);
			Ray ray2 = new Ray(vector, Vector3.down * num2);
			Ray ray3 = new Ray(vector, Vector3.back * num3);
			float enter = 0f;
			float enter2 = 0f;
			float enter3 = 0f;
			if (!p.Raycast(ray, out enter))
			{
				enter = 1000f;
			}
			if (!p.Raycast(ray2, out enter2))
			{
				enter2 = 1000f;
			}
			if (!p.Raycast(ray3, out enter3))
			{
				enter3 = 1000f;
			}
			float num4 = Mathf.Min(Mathf.Min(enter, enter2), enter3);
			if (num4 >= 0.5f)
			{
				return 0f;
			}
			return (127.5f - 255f * num4) / (1f - num4);
		}
		if (distanceToPoint < 0f)
		{
			Ray ray4 = new Ray(vector, Vector3.right * num);
			Ray ray5 = new Ray(vector, Vector3.up * num2);
			Ray ray6 = new Ray(vector, Vector3.forward * num3);
			float enter4 = 0f;
			float enter5 = 0f;
			float enter6 = 0f;
			if (!p.Raycast(ray4, out enter4))
			{
				enter4 = 1000f;
			}
			if (!p.Raycast(ray5, out enter5))
			{
				enter5 = 1000f;
			}
			if (!p.Raycast(ray6, out enter6))
			{
				enter6 = 1000f;
			}
			float num5 = Mathf.Min(Mathf.Min(enter4, enter5), enter6);
			if (num5 >= 0.5f)
			{
				return 255f;
			}
			return 127.5f / (1f - num5);
		}
		return 127.5f;
	}

	public static float SmoothConstraint(float val, float threshold, float pressure = 1f)
	{
		if (val <= threshold)
		{
			return val;
		}
		if (pressure < 1f)
		{
			pressure = 1f;
		}
		float num = threshold / pressure;
		return Mathf.Log10(val) * num - Mathf.Log10(threshold) * num + threshold;
	}

	public static Vector3 NormalizeEulerAngle(Vector3 eulerAngle)
	{
		eulerAngle.x %= 360f;
		eulerAngle.y %= 360f;
		eulerAngle.z %= 360f;
		if (eulerAngle.x > 180f)
		{
			eulerAngle.x -= 360f;
		}
		if (eulerAngle.y > 180f)
		{
			eulerAngle.y -= 360f;
		}
		if (eulerAngle.z > 180f)
		{
			eulerAngle.z -= 360f;
		}
		if (eulerAngle.x < -180f)
		{
			eulerAngle.x += 360f;
		}
		if (eulerAngle.y < -180f)
		{
			eulerAngle.y += 360f;
		}
		if (eulerAngle.z < -180f)
		{
			eulerAngle.z += 360f;
		}
		return eulerAngle;
	}

	public static bool IsEqualVector(Vector3 vec1, Vector3 vec2)
	{
		return Vector3.Distance(vec1, vec2) < 0.0002f;
	}

	public static bool IsEqualRotation(Vector3 rot1, Vector3 rot2)
	{
		Quaternion quaternion = Quaternion.Euler(rot1);
		Quaternion quaternion2 = Quaternion.Euler(rot2);
		double num = quaternion.x;
		double num2 = quaternion.y;
		double num3 = quaternion.z;
		double num4 = quaternion.w;
		double num5 = quaternion2.x;
		double num6 = quaternion2.y;
		double num7 = quaternion2.z;
		double num8 = quaternion2.w;
		double num9 = Math.Sqrt(num * num + num2 * num2 + num3 * num3 + num4 * num4);
		double num10 = Math.Sqrt(num5 * num5 + num6 * num6 + num7 * num7 + num8 * num8);
		num /= num9;
		num2 /= num9;
		num3 /= num9;
		num4 /= num9;
		num5 /= num10;
		num6 /= num10;
		num7 /= num10;
		num8 /= num10;
		double value = num * num5 + num2 * num6 + num3 * num7 + num4 * num8;
		double num11 = Math.Acos(Math.Min(Math.Abs(value), 1.0)) * 2.0 * 57.295779513;
		return num11 < 0.008999999612569809;
	}

	public static float FadeInCurve(float t)
	{
		t = Mathf.Clamp01(t);
		return Mathf.Clamp01(t * t * t);
	}
}
