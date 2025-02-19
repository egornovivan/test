using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExtTrans;
using Pathea.Projectile;
using Pathfinding;
using SkillSystem;
using UnityEngine;
using WhiteCat;
using WhiteCat.UnityExtension;

namespace PETools;

public class PEUtil
{
	private static Transform _transMainCamera;

	internal static int TreeLayer = 2113536;

	internal static int WanderLayer = 6144;

	internal static int IgnoreWanderLayer = 73728;

	internal static int Standlayer = 2177024;

	internal static int GetOffRideMask = 11132161;

	public static Transform MainCamTransform
	{
		get
		{
			if (_transMainCamera == null && Camera.main != null)
			{
				_transMainCamera = Camera.main.transform;
			}
			return _transMainCamera;
		}
	}

	public static T GetComponent<T>(GameObject obj) where T : MonoBehaviour
	{
		T result = (T)null;
		Transform transform = obj.transform;
		while (transform != null && (result = transform.GetComponent<T>()) == null)
		{
			transform = transform.parent;
		}
		return result;
	}

	public static string ToPrefabName(string name)
	{
		if (name.Contains("(Clone)"))
		{
			return name.Substring(0, name.LastIndexOf("(Clone)"));
		}
		return name;
	}

	public static bool GetStandPosWithoutOverlap(Vector3 centerPos, float radius, ref Vector3 retPos, int overlapLayermask)
	{
		if (Physics.CheckSphere(centerPos, radius, overlapLayermask))
		{
			Collider[] array = Physics.OverlapSphere(centerPos, radius, overlapLayermask);
			if (array != null && array.Length > 0)
			{
				Bounds bounds = array[0].bounds;
				for (int i = 1; i < array.Length; i++)
				{
					bounds.Encapsulate(array[i].bounds);
				}
				float num = centerPos.x - bounds.center.x;
				float num2 = centerPos.z - bounds.center.z;
				float num3 = bounds.extents.x + radius - Mathf.Abs(num);
				float num4 = bounds.extents.z + radius - Mathf.Abs(num2);
				retPos = centerPos;
				if (num3 < num4)
				{
					if (num < 0f)
					{
						retPos.x = bounds.center.x - (bounds.extents.x + radius);
					}
					else
					{
						retPos.x = bounds.center.x + (bounds.extents.x + radius);
					}
				}
				else if (num2 < 0f)
				{
					retPos.z = bounds.center.z - (bounds.extents.z + radius);
				}
				else
				{
					retPos.z = bounds.center.z + (bounds.extents.z + radius);
				}
				return true;
			}
		}
		return false;
	}

	public static Transform GetChild(Transform root, string boneName, bool lowerCompare = false)
	{
		if (root == null || boneName == string.Empty || "0" == boneName)
		{
			return null;
		}
		if (lowerCompare)
		{
			if (root.name.ToLower().Equals(boneName.ToLower()))
			{
				return root;
			}
		}
		else if (root.name.Equals(boneName))
		{
			return root;
		}
		if (root.childCount > 0)
		{
			for (int i = 0; i < root.childCount; i++)
			{
				Transform child = GetChild(root.GetChild(i), boneName, lowerCompare);
				if (child != null)
				{
					return child;
				}
			}
		}
		return null;
	}

	public static Transform GetChild(Transform root, Transform child)
	{
		if (root == null || child == null)
		{
			return null;
		}
		List<Transform> list = new List<Transform>(root.GetComponentsInChildren<Transform>(includeInactive: true));
		return list.Find((Transform ret) => ret == child);
	}

	public static string ToSystemPath(string path)
	{
		return System.IO.Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
	}

	public static float Magnitude(Vector3 v, bool is3D = true)
	{
		if (!is3D)
		{
			return Mathf.Sqrt(v.x * v.x + v.z * v.z);
		}
		return Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
	}

	public static float Magnitude(Vector3 v1, Vector3 v2, bool is3D = true)
	{
		return Magnitude(v1 - v2, is3D);
	}

	public static float MagnitudeH(Vector3 v1, Vector3 v2)
	{
		Vector3 vector = v1 - v2;
		return Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
	}

	public static float SqrMagnitude(Vector3 v1, Vector3 v2, bool is3D = true)
	{
		Vector3 vector = v1 - v2;
		if (!is3D)
		{
			vector.y = 0f;
		}
		return vector.sqrMagnitude;
	}

	public static float SqrMagnitudeH(Vector3 v1, Vector3 v2)
	{
		Vector3 vector = v1 - v2;
		return vector.x * vector.x + vector.z * vector.z;
	}

	public static float Height(Vector3 v1, Vector3 v2)
	{
		return v2.y - v1.y;
	}

	public static float SqrMagnitude(Vector3 v)
	{
		return v.sqrMagnitude;
	}

	public static float SqrMagnitudeH(Vector3 v)
	{
		return Vector3.ProjectOnPlane(v, Vector3.up).sqrMagnitude;
	}

	public static float SqrMagnitude(Transform t1, Bounds b1, Transform t2, Bounds b2, bool is3D = true)
	{
		if (t1 == null || t2 == null)
		{
			return 0f;
		}
		Vector3 vector = t1.TransformPoint(b1.ClosestPoint(t1.InverseTransformPoint(t2.position)));
		Vector3 vector2 = t2.TransformPoint(b2.ClosestPoint(t2.InverseTransformPoint(t1.position)));
		Vector3 vector3 = vector - vector2;
		if (!is3D)
		{
			vector3.y = 0f;
		}
		return vector3.sqrMagnitude;
	}

	public static float Angle(Vector3 v1, Vector3 v2)
	{
		return Vector3.Angle(v1, v2);
	}

	public static float AngleH(Vector3 v1, Vector3 v2)
	{
		v1.y = 0f;
		v2.y = 0f;
		return Vector3.Angle(v1, v2);
	}

	public static float AngleZ(Vector3 v1, Vector3 v2)
	{
		v1.z = 0f;
		v2.z = 0f;
		return Vector3.Angle(v1, v2);
	}

	public static float AngleX(Vector3 v1, Vector3 v2)
	{
		v1.x = 0f;
		v2.x = 0f;
		return Vector3.Angle(v1, v2);
	}

	public static float SqrDistHToCam(Vector3 v1)
	{
		Vector3 position = Camera.main.transform.position;
		Vector3 vector = v1 - position;
		return vector.x * vector.x + vector.z * vector.z;
	}

	public static IntVector4 ToIntVector4(Vector3 position, int lod)
	{
		IntVector4 intVector = new IntVector4(new IntVector3(position), lod);
		int num = lod + 5;
		intVector.x = intVector.x >> num << num;
		intVector.y = intVector.y >> num << num;
		intVector.z = intVector.z >> num << num;
		return intVector;
	}

	public static IntVector3 ToIntVector3(Vector3 position, int lod)
	{
		IntVector4 intVector = new IntVector4(new IntVector3(position), lod);
		int num = lod + 5;
		intVector.x = intVector.x >> num << num;
		intVector.y = intVector.y >> num << num;
		intVector.z = intVector.z >> num << num;
		return new IntVector3(intVector.x, intVector.y, intVector.z);
	}

	public static IntVector2 ToIntVector2(Vector3 position)
	{
		return new IntVector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
	}

	public static IntVector2 ToIntVector2(Vector3 position, int lod)
	{
		IntVector4 intVector = new IntVector4(new IntVector3(position), lod);
		int num = lod + 5;
		intVector.x = intVector.x >> num << num;
		intVector.y = intVector.y >> num << num;
		intVector.z = intVector.z >> num << num;
		return new IntVector2(intVector.x, intVector.z);
	}

	public static IntVector2 ToIntVector2(IntVector4 intVector4)
	{
		return new IntVector2(intVector4.x, intVector4.z);
	}

	public static RaycastHit[] RaycastAll(Vector3 pos, Vector3 dir, float distance, int layerMask = 0, bool trigger = false)
	{
		List<RaycastHit> list = ((layerMask != 0) ? new List<RaycastHit>(Physics.RaycastAll(pos, dir, distance, layerMask)) : new List<RaycastHit>(Physics.RaycastAll(pos, dir, distance)));
		return list.FindAll((RaycastHit ret) => ret.collider.isTrigger).ToArray();
	}

	public static bool Raycast(Vector3 pos, Vector3 dir, float distance, out RaycastHit hitInfo, int layerMask = 0, bool trigger = false)
	{
		RaycastHit[] array = RaycastAll(pos, dir, distance, layerMask, trigger);
		if (array != null && array.Length > 0)
		{
			hitInfo = array[0];
			float sqrMagnitude = (pos - array[0].point).sqrMagnitude;
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				if ((pos - raycastHit.point).sqrMagnitude < sqrMagnitude)
				{
					hitInfo = raycastHit;
					sqrMagnitude = (pos - raycastHit.point).sqrMagnitude;
				}
			}
			return true;
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	public static bool Raycast(Vector3 pos, Vector3 dir, float distance, out RaycastHit hitInfo, Vector3 srcPos, int layerMask = 0, bool trigger = false)
	{
		RaycastHit[] array = RaycastAll(pos, dir, distance, layerMask, trigger);
		if (array != null && array.Length > 0)
		{
			hitInfo = array[0];
			float sqrMagnitude = (srcPos - array[0].point).sqrMagnitude;
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				if ((srcPos - raycastHit.point).sqrMagnitude < sqrMagnitude)
				{
					hitInfo = raycastHit;
					sqrMagnitude = (pos - raycastHit.point).sqrMagnitude;
				}
			}
			return true;
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	public static Vector3 GetTreeSurfacePos(GlobalTreeInfo tree)
	{
		Vector3 vector = ((!PeGameMgr.IsStory) ? tree._treeInfo.m_pos : tree.WorldPos);
		Physics.Raycast(vector + Vector3.up * 10f, Vector3.down, out var hitInfo, 30f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
		return (!(hitInfo.collider == null)) ? hitInfo.point : Vector3.zero;
	}

	public static Vector3 CalculateAimPos(Vector3 dirPos, Vector3 standPos)
	{
		Vector3 vector = dirPos;
		float num = ((!(dirPos.y - standPos.y > Mathf.Epsilon)) ? 0.15f : 0.05f);
		float num2 = ((!(Mathf.Abs(dirPos.y - standPos.y) > num)) ? (dirPos.y - standPos.y) : ((dirPos.y - standPos.y) / Mathf.Abs(dirPos.y - standPos.y) * num));
		vector.y = standPos.y + num2;
		return vector + Vector3.up * 1.5f;
	}

	public static bool InAimAngle(Vector3 targetPos, Vector3 standPos, Vector3 rootDir, float angle = 80f)
	{
		Vector3 v = targetPos - standPos;
		float num = AngleH(rootDir, v);
		return (num <= angle) ? true : false;
	}

	public static bool InAimDistance(Vector3 targetPos, Vector3 standPos, float minDistance, float maxdistance)
	{
		return MagnitudeH(targetPos, standPos) >= minDistance && MagnitudeH(targetPos, standPos) <= maxdistance;
	}

	public static bool InAimAngle(Transform trans, Transform mode, float angle = 80f)
	{
		Vector3 v = trans.position - mode.position;
		float num = AngleH(mode.forward, v);
		return (num <= angle) ? true : false;
	}

	public static bool GetFixedPosition(IntVector4[] points, Vector3 pos, out Vector3 fixedPos, int layerMask = 0)
	{
		int num = points[0].y;
		int num2 = points[0].y + 32 << points[0].w;
		foreach (IntVector4 intVector in points)
		{
			num = Mathf.Min(num, intVector.y);
			num2 = Mathf.Max(num2, intVector.y + 32 << intVector.w);
		}
		Vector3 pos2 = new Vector3(pos.x, num2, pos.z);
		float distance = Mathf.Abs(num2 - num);
		if (Raycast(pos2, Vector3.down, distance, out var hitInfo, layerMask))
		{
			fixedPos = hitInfo.point;
			return true;
		}
		fixedPos = Vector3.zero;
		return false;
	}

	public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, bool is3D = false)
	{
		Vector3 zero = Vector3.zero;
		if (is3D)
		{
			zero = UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
		}
		else
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
			zero = new Vector3(vector.x, 0f, vector.y);
		}
		return center + zero;
	}

	public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, int layer)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(minRadius, maxRadius);
		Vector3 vector2 = center + new Vector3(vector.x, 0f, vector.y);
		if (Physics.Raycast(vector2 + Vector3.up * 128f, Vector3.down, out var hitInfo, 256f, layer))
		{
			return hitInfo.point;
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, float minHeight, float maxHeight)
	{
		return GetRandomPosition(center, minRadius, maxRadius) + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight);
	}

	public static Vector3 GetRandomPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minDir, float maxDir)
	{
		Vector3 vector = new Vector3(direction.x, 0f, direction.z);
		return center + (Quaternion.AngleAxis(UnityEngine.Random.Range(minDir, maxDir), Vector3.up) * vector).normalized * UnityEngine.Random.Range(minRadius, maxRadius);
	}

	public static Vector3 GetRandomPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minDir, float maxDir, int layer, float upD = 128f, float downD = 256f)
	{
		Vector3 vector = new Vector3(direction.x, 0f, direction.z);
		Vector3 vector2 = center + (Quaternion.AngleAxis(UnityEngine.Random.Range(minDir, maxDir), Vector3.up) * vector).normalized * UnityEngine.Random.Range(minRadius, maxRadius);
		if (Physics.Raycast(vector2 + Vector3.up * upD, Vector3.down, out var hitInfo, downD, layer))
		{
			return hitInfo.point;
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minDir, float maxDir, float minHeight, float maxHeight)
	{
		Vector3 randomPosition = GetRandomPosition(center, direction, minRadius, maxRadius, minDir, maxDir);
		return randomPosition + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight);
	}

	public static Vector3 GetRandomPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minDir, float maxDir, float minHeight, float maxHeight, int layer)
	{
		Vector3 randomPosition = GetRandomPosition(center, direction, minRadius, maxRadius, minDir, maxDir);
		Vector3 vector = randomPosition + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight);
		if (Physics.Raycast(vector + Vector3.up * 128f, Vector3.down, out var hitInfo, 256f, layer))
		{
			return hitInfo.point;
		}
		return Vector3.zero;
	}

	public static bool GetNearbySafetyPos(PeTrans boundsTrans, int layermask, int standLayer, ref Vector3 position)
	{
		position = Vector3.zero;
		Transform realTrans = boundsTrans.realTrans;
		Vector3 max = boundsTrans.bound.max;
		Vector3 extents = boundsTrans.bound.extents;
		Vector3 center = boundsTrans.bound.center;
		Vector3 position2 = default(Vector3);
		position2.y = max.y;
		while (position2.y < max.y + 2.5f)
		{
			for (float num = 0.25f; num < extents.z + 2.5f; num += 0.5f)
			{
				for (float num2 = 0.25f; num2 < extents.x + 2.5f; num2 += 0.5f)
				{
					position2.z = center.z + num;
					position2.x = center.x - num2;
					if (CheckPosIsSafety(realTrans, layermask, standLayer, position = realTrans.TransformPoint(position2)))
					{
						return true;
					}
					position2.x = center.x + num2;
					if (CheckPosIsSafety(realTrans, layermask, standLayer, position = realTrans.TransformPoint(position2)))
					{
						return true;
					}
					position2.z = center.z - num;
					if (CheckPosIsSafety(realTrans, layermask, standLayer, position = realTrans.TransformPoint(position2)))
					{
						return true;
					}
					position2.x = center.x - num2;
					if (CheckPosIsSafety(realTrans, layermask, standLayer, position = realTrans.TransformPoint(position2)))
					{
						return true;
					}
				}
			}
			position2.y += 0.5f;
		}
		return false;
	}

	public static bool CheckPosIsSafety(Transform trans, int layermask, int standLayer, Vector3 pos)
	{
		Vector3 end = pos;
		end.y += 1.5f;
		if (!Physics.CheckCapsule(pos, end, 0.55f, layermask, QueryTriggerInteraction.Ignore))
		{
			Vector3 direction = pos - trans.position;
			RaycastHit[] array = Physics.RaycastAll(trans.position, direction, direction.magnitude, layermask, QueryTriggerInteraction.Ignore);
			bool flag = true;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].transform.IsChildOf(trans))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return !Physics.Raycast(pos, Vector3.down, 128f, standLayer, QueryTriggerInteraction.Ignore);
			}
		}
		return false;
	}

	public static Vector3 GetVoxelPosition(Vector3 pos)
	{
		Ray ray = new Ray(pos + Vector3.up * 128f, Vector3.down);
		if (PE.RaycastVoxel(ray, out var point, 256, 10, 1))
		{
			return point;
		}
		return Vector3.zero;
	}

	public static Vector3 GetVoxelPositionOnGround(Vector3 pos, float minHeight, float maxHeight)
	{
		Vector3 voxelPosition = GetVoxelPosition(pos);
		if (voxelPosition != Vector3.zero && !CheckPositionUnderWater(pos))
		{
			return voxelPosition + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight);
		}
		return Vector3.zero;
	}

	public static Vector3 GetVoxelPositionInWater(Vector3 pos, float minHeight, float maxHeight)
	{
		Vector3 voxelPosition = GetVoxelPosition(pos);
		if (voxelPosition != Vector3.zero && CheckPositionUnderWater(voxelPosition, out var height))
		{
			float num = Mathf.Max(0f, Mathf.Min(minHeight, height));
			float num2 = Mathf.Max(0f, Mathf.Min(maxHeight, height));
			if (num2 - num > 1f)
			{
				return voxelPosition + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight);
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetVoxelPositionOnGroundInSky(Vector3 pos, float minHeight, float maxHeight)
	{
		Vector3 voxelPosition = GetVoxelPosition(pos);
		if (voxelPosition != Vector3.zero)
		{
			if (!CheckPositionUnderWater(voxelPosition, out var height) || height < float.Epsilon)
			{
				return voxelPosition + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight);
			}
			return voxelPosition + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight) + Vector3.up * height;
		}
		return Vector3.zero;
	}

	public static Vector3 GetEqualPositionToStand(Vector3 _Camerapos, Vector3 _Cameradir, Vector3 _Playerpos, Vector3 _Palyerdir, float radiu)
	{
		Vector3 vector = _Camerapos;
		Vector3 randomPosition = GetRandomPosition(vector, _Cameradir, 2f, radiu, -90f, 90f);
		if (CheckPositionNearCliff(randomPosition))
		{
			vector = _Playerpos;
			randomPosition = GetRandomPosition(vector, _Palyerdir, 2f, radiu, -90f, 90f);
		}
		Ray ray = new Ray(vector, Vector3.up);
		if (Physics.Raycast(ray, out var hitInfo, 128f, 71680))
		{
			ray = new Ray(randomPosition, Vector3.up);
			if (!Physics.Raycast(ray, out hitInfo, 128f, 71680))
			{
				return _Playerpos;
			}
			if (CheckPositionUnderWater(hitInfo.point - Vector3.up))
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
				if (CheckPositionUnderWater(hitInfo.point))
				{
					return randomPosition;
				}
				return hitInfo.point + Vector3.up;
			}
		}
		return _Playerpos;
	}

	private static bool GetCorrectHeight(Vector3 pos, float minHeight, float maxHeight, ref Vector3 newPos)
	{
		if (Physics.Raycast(pos, Vector3.up, out var hitInfo, 128f, WanderLayer))
		{
			if (Physics.Raycast(hitInfo.point - Vector3.up * 0.1f, Vector3.down, out var hitInfo2, 256f, IgnoreWanderLayer))
			{
				return false;
			}
			if (!Physics.Raycast(hitInfo.point - Vector3.up * 0.1f, Vector3.down, out hitInfo2, 256f, WanderLayer) || CheckPositionUnderWater(hitInfo2.point))
			{
				return false;
			}
			newPos = hitInfo2.point + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight);
		}
		else
		{
			if (Physics.Raycast(pos + Vector3.up * 128f, Vector3.down, out var hitInfo3, 256f, IgnoreWanderLayer))
			{
				return false;
			}
			if (!Physics.Raycast(pos + Vector3.up * 128f, Vector3.down, out hitInfo3, 256f, WanderLayer) || CheckPositionUnderWater(hitInfo3.point))
			{
				return false;
			}
			newPos = hitInfo3.point + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight);
		}
		return true;
	}

	public static Vector3 GetRandomPositionOnGround(Vector3 center, float minRadius, float maxRadius, bool isResult = true)
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 newPos = GetRandomPosition(center, minRadius, maxRadius);
			if (GetCorrectHeight(newPos, 0f, 0f, ref newPos))
			{
				return newPos;
			}
		}
		return (!isResult) ? Vector3.zero : GetVoxelPositionOnGround(GetRandomPosition(center, minRadius, maxRadius), 0f, 0f);
	}

	public static Vector3 GetRandomPositionOnGroundForWander(Vector3 center, float minRadius, float maxRadius)
	{
		Vector3 forward = Vector3.forward;
		for (int i = 0; i < 5; i++)
		{
			Vector3 randomPosition = GetRandomPosition(center, minRadius, maxRadius);
			if (!Physics.SphereCast(randomPosition + Vector3.up * 16f, 0.5f, Vector3.down, out var hitInfo, 20f, IgnoreWanderLayer) && Physics.Raycast(randomPosition + Vector3.up * 16f, Vector3.down, out hitInfo, 20f, WanderLayer) && !CheckPositionUnderWater(hitInfo.point) && !CheckPositionNearCliff(hitInfo.point))
			{
				return hitInfo.point;
			}
		}
		return center;
	}

	public static Vector3 GetRandomPositionInCircle(Vector3 center, float minRadius, float maxRadius, Vector3 direction, float minDir, float maxDir)
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 randomPosition = GetRandomPosition(center, direction, minRadius, maxRadius, minDir, maxDir);
			if (!Physics.SphereCast(randomPosition + Vector3.up * 16f, 0.5f, Vector3.down, out var hitInfo, 20f, IgnoreWanderLayer) && Physics.Raycast(randomPosition + Vector3.up * 16f, Vector3.down, out hitInfo, 20f, WanderLayer) && !CheckPositionUnderWater(hitInfo.point) && !CheckPositionNearCliff(hitInfo.point))
			{
				return hitInfo.point;
			}
		}
		return center;
	}

	public static Vector3 GetRandomPositionOnGround(Vector3 center, float minRadius, float maxRadius, float minHeight, float maxHeight, bool isResult = true)
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 newPos = GetRandomPosition(center, minRadius, maxRadius);
			if (GetCorrectHeight(newPos, minHeight, maxHeight, ref newPos))
			{
				return newPos;
			}
		}
		return (!isResult) ? Vector3.zero : GetVoxelPositionOnGround(GetRandomPosition(center, minRadius, maxRadius), minHeight, maxHeight);
	}

	public static Vector3 GetEmptyPositionOnGround(Vector3 center, float minRadius, float maxRadius)
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 randomPosition = GetRandomPosition(center, minRadius, maxRadius);
			if (!Physics.SphereCast(randomPosition + Vector3.up * 16f, 0.5f, Vector3.down, out var _, 20f, IgnoreWanderLayer))
			{
				return randomPosition;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPositionOnGround(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minAngle, float maxAngle, bool isResult = true)
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 newPos = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);
			if (GetCorrectHeight(newPos, 0f, 0f, ref newPos) && !CheckPositionUnderWater(newPos + Vector3.up * 0.5f))
			{
				return newPos;
			}
		}
		return (!isResult) ? Vector3.zero : GetVoxelPositionOnGround(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), 0f, 0f);
	}

	public static Vector3 GetRandomPositionOnGround(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minHeight, float maxHeight, float minAngle, float maxAngle, bool isResult = true)
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 newPos = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);
			if (GetCorrectHeight(newPos, minHeight, maxHeight, ref newPos) && !CheckPositionUnderWater(newPos + Vector3.up * 0.5f))
			{
				return newPos;
			}
		}
		return (!isResult) ? Vector3.zero : GetVoxelPositionOnGround(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), minHeight, maxHeight);
	}

	public static Vector3 GetRandomPositionInWater(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minHeight, float maxHeight, float minAngle, float maxAngle, bool isResult = true)
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 randomPosition = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);
			if (!Physics.Raycast(randomPosition + Vector3.up * 128f, Vector3.down, out var hitInfo, 256f, IgnoreWanderLayer) && Physics.Raycast(randomPosition + Vector3.up * 128f, Vector3.down, out hitInfo, 256f, WanderLayer) && GetWaterSurfaceHeight(hitInfo.point, out var waterHeight))
			{
				float a = waterHeight - maxHeight;
				float a2 = waterHeight - minHeight;
				a = Mathf.Max(a, hitInfo.point.y);
				a2 = Mathf.Max(a2, hitInfo.point.y);
				if (!(a2 <= a))
				{
					return new Vector3(hitInfo.point.x, UnityEngine.Random.Range(a, a2), hitInfo.point.z);
				}
			}
		}
		return (!isResult) ? Vector3.zero : GetVoxelPositionInWater(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), minHeight, maxHeight);
	}

	public static Vector3 GetRandomPositionInSky(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minHeight, float maxHeight, float minAngle, float maxAngle, bool isReslut = true)
	{
		for (int i = 0; i < 10; i++)
		{
			Vector3 randomPosition = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);
			if (!Physics.Raycast(randomPosition + Vector3.up * 128f, Vector3.down, out var hitInfo, 256f, IgnoreWanderLayer) && Physics.Raycast(randomPosition + Vector3.up * 128f, Vector3.down, out hitInfo, 256f, WanderLayer))
			{
				Vector3 zero = Vector3.zero;
				zero = ((CheckPositionUnderWater(hitInfo.point, out var height) && !(height < 0f)) ? (hitInfo.point + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight) + Vector3.up * height) : (hitInfo.point + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight)));
				Vector3 vector = zero - center;
				float maxDistance = Vector3.Distance(zero, center);
				if (!Physics.Raycast(center, zero - center, maxDistance, 2189312))
				{
					return zero;
				}
			}
		}
		return (!isReslut) ? Vector3.zero : GetVoxelPositionOnGroundInSky(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), minHeight, maxHeight);
	}

	public static Vector3 GetRandomFollowPosInSky(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minHeight, float maxHeight, float minAngle, float maxAngle, bool isReslut = true)
	{
		for (int i = 0; i < 10; i++)
		{
			Vector3 randomPosition = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);
			if (!Physics.Raycast(randomPosition + Vector3.up * maxHeight, Vector3.down, out var hitInfo, 2f * maxHeight, IgnoreWanderLayer) && Physics.Raycast(randomPosition + Vector3.up * maxHeight, Vector3.down, out hitInfo, 2f * maxHeight, WanderLayer))
			{
				if (!CheckPositionUnderWater(hitInfo.point, out var height) || height < 0f)
				{
					return hitInfo.point + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight);
				}
				return hitInfo.point + Vector3.up * UnityEngine.Random.Range(minHeight, maxHeight) + Vector3.up * height;
			}
		}
		return (!isReslut) ? Vector3.zero : GetVoxelPositionOnGroundInSky(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), minHeight, maxHeight);
	}

	public static Vector3 CorrectionPostionToStand(Vector3 pos, float upHeigtht = 1f, float downHeight = 16f)
	{
		if (Physics.Raycast(pos + Vector3.up * upHeigtht, Vector3.down, out var hitInfo, downHeight, Standlayer))
		{
			return hitInfo.point;
		}
		return pos;
	}

	public static Vector3 GetDirtionPostion(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minAngle, float maxAngle, float dis3D)
	{
		for (int i = 0; i < 10; i++)
		{
			Vector3 randomPosition = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);
			if (Physics.Raycast(randomPosition + Vector3.up * 32f, Vector3.down, out var hitInfo, 64f, Standlayer))
			{
				float num = Magnitude(hitInfo.point, center);
				if (num <= dis3D)
				{
					return hitInfo.point;
				}
			}
		}
		return Vector3.zero;
	}

	public static Vector3 CheckPosForNpcStand(Vector3 pos)
	{
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(pos, Vector3.up, out hitInfo, 2f, Standlayer);
		Vector3 point = hitInfo.point;
		if (flag && Physics.Raycast(point + Vector3.up * 8f, Vector3.down, out hitInfo, 8f, Standlayer))
		{
			return hitInfo.point;
		}
		return pos;
	}

	public static Vector3 GetCenter(Collider c)
	{
		if (c is BoxCollider)
		{
			return (c as BoxCollider).center;
		}
		if (c is SphereCollider)
		{
			return (c as SphereCollider).center;
		}
		if (c is CapsuleCollider)
		{
			return (c as CapsuleCollider).center;
		}
		Debug.LogError("type is error : " + c.GetType().ToString());
		return Vector3.zero;
	}

	public static Vector3 GetCenterOfWorld(Collider c)
	{
		return c.transform.TransformPoint(GetCenter(c));
	}

	public static bool IsScopeAngle(Vector3 v, Vector3 n, Vector3 axis, float minAngle, float maxAngle)
	{
		Vector3 vector = Vector3.ProjectOnPlane(v, axis);
		Vector3 vector2 = Vector3.ProjectOnPlane(n, axis);
		Vector3 lhs = Quaternion.AngleAxis(minAngle, axis) * vector2;
		Vector3 vector3 = Quaternion.AngleAxis(maxAngle, axis) * vector2;
		float y = Vector3.Cross(lhs, vector.normalized).y;
		float y2 = Vector3.Cross(vector3, vector.normalized).y;
		if (Vector3.Cross(lhs, vector3).y > 0f)
		{
			if (Mathf.Abs(y - y2) < 0.0001f)
			{
				return true;
			}
			return y >= 0f && y2 <= 0f;
		}
		if (Mathf.Abs(y - y2) < 0.0001f)
		{
			return true;
		}
		return y >= 0f || y2 <= 0f;
	}

	public static float GetAngle(Vector3 v, Vector3 n, Vector3 axis)
	{
		Vector3 lhs = Vector3.ProjectOnPlane(v, axis);
		Vector3 rhs = Vector3.ProjectOnPlane(n, axis);
		float num = Vector3.Angle(lhs.normalized, rhs.normalized);
		float y = Vector3.Cross(lhs, rhs).y;
		return (!(y > 0f)) ? num : (0f - num);
	}

	public static float Angle(Vector3 v, Vector3 n, Vector3 axis)
	{
		Vector3 vector = Vector3.ProjectOnPlane(v, axis);
		Vector3 vector2 = Vector3.ProjectOnPlane(n, axis);
		return Vector3.Angle(vector.normalized, vector2.normalized);
	}

	public static void ResetTransform(Transform trans)
	{
		trans.localPosition = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = Vector3.one;
	}

	public static string[] ToArrayString(string str, char c)
	{
		return str.Split(c);
	}

	public static int[] ToArrayInt32(string str, char c)
	{
		string[] array = ToArrayString(str, c);
		List<int> list = new List<int>();
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToInt32(value));
		}
		return list.ToArray();
	}

	public static byte[] ToArrayByte(string str, char c)
	{
		string[] array = ToArrayString(str, c);
		List<byte> list = new List<byte>();
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToByte(value));
		}
		return list.ToArray();
	}

	public static float[] ToArraySingle(string str, char c)
	{
		string[] array = ToArrayString(str, c);
		List<float> list = new List<float>();
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToSingle(value));
		}
		return list.ToArray();
	}

	public static Vector3 ToVector3(string str, char c)
	{
		string[] array = ToArrayString(str, c);
		if (array.Length != 3)
		{
			return Vector3.zero;
		}
		List<float> list = new List<float>();
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToSingle(value));
		}
		return new Vector3(list[0], list[1], list[2]);
	}

	public static Color32 ToColor32(string data, char c)
	{
		byte[] array = ToArrayByte(data, c);
		if (array.Length != 4)
		{
			return new Color32(0, 0, 0, 0);
		}
		return new Color32(array[0], array[1], array[2], array[3]);
	}

	public static void DrawBounds(Bounds bound, Color color)
	{
		if (!Application.isEditor)
		{
			return;
		}
		Vector3[] array = new Vector3[8];
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = bound.center;
			if ((i & 1) == 0)
			{
				array[i] -= bound.extents.x * new Vector3(1f, 0f, 0f);
			}
			else
			{
				array[i] += bound.extents.x * new Vector3(1f, 0f, 0f);
			}
			if ((i & 2) == 0)
			{
				array[i] -= bound.extents.y * new Vector3(0f, 1f, 0f);
			}
			else
			{
				array[i] += bound.extents.y * new Vector3(0f, 1f, 0f);
			}
			if ((i & 4) == 0)
			{
				array[i] -= bound.extents.z * new Vector3(0f, 0f, 1f);
			}
			else
			{
				array[i] += bound.extents.z * new Vector3(0f, 0f, 1f);
			}
		}
		Debug.DrawLine(array[0], array[1], color);
		Debug.DrawLine(array[2], array[3], color);
		Debug.DrawLine(array[4], array[5], color);
		Debug.DrawLine(array[6], array[7], color);
		Debug.DrawLine(array[0], array[4], color);
		Debug.DrawLine(array[1], array[5], color);
		Debug.DrawLine(array[2], array[6], color);
		Debug.DrawLine(array[3], array[7], color);
		Debug.DrawLine(array[0], array[2], color);
		Debug.DrawLine(array[1], array[3], color);
		Debug.DrawLine(array[4], array[6], color);
		Debug.DrawLine(array[5], array[7], color);
	}

	public static void DrawBounds(Transform tr, Bounds bound, Color color)
	{
		if (!Application.isEditor || !(tr != null))
		{
			return;
		}
		Vector3[] array = new Vector3[8];
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = bound.center;
			if ((i & 1) == 0)
			{
				array[i] -= bound.extents.x * new Vector3(1f, 0f, 0f);
			}
			else
			{
				array[i] += bound.extents.x * new Vector3(1f, 0f, 0f);
			}
			if ((i & 2) == 0)
			{
				array[i] -= bound.extents.y * new Vector3(0f, 1f, 0f);
			}
			else
			{
				array[i] += bound.extents.y * new Vector3(0f, 1f, 0f);
			}
			if ((i & 4) == 0)
			{
				array[i] -= bound.extents.z * new Vector3(0f, 0f, 1f);
			}
			else
			{
				array[i] += bound.extents.z * new Vector3(0f, 0f, 1f);
			}
			ref Vector3 reference2 = ref array[i];
			reference2 = tr.TransformPoint(array[i]);
		}
		Debug.DrawLine(array[0], array[1], color);
		Debug.DrawLine(array[2], array[3], color);
		Debug.DrawLine(array[4], array[5], color);
		Debug.DrawLine(array[6], array[7], color);
		Debug.DrawLine(array[0], array[4], color);
		Debug.DrawLine(array[1], array[5], color);
		Debug.DrawLine(array[2], array[6], color);
		Debug.DrawLine(array[3], array[7], color);
		Debug.DrawLine(array[0], array[2], color);
		Debug.DrawLine(array[1], array[3], color);
		Debug.DrawLine(array[4], array[6], color);
		Debug.DrawLine(array[5], array[7], color);
	}

	public static void DrawGLBounds(Bounds bound, Color color, Material mat)
	{
		if (mat == null)
		{
			return;
		}
		Vector3[] array = new Vector3[8];
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = bound.center;
			if ((i & 1) == 0)
			{
				array[i] -= bound.extents.x * Vector3.right;
			}
			else
			{
				array[i] += bound.extents.x * Vector3.right;
			}
			if ((i & 2) == 0)
			{
				array[i] -= bound.extents.y * Vector3.up;
			}
			else
			{
				array[i] += bound.extents.y * Vector3.up;
			}
			if ((i & 4) == 0)
			{
				array[i] -= bound.extents.z * Vector3.forward;
			}
			else
			{
				array[i] += bound.extents.z * Vector3.forward;
			}
		}
		GL.PushMatrix();
		mat.SetPass(0);
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.End();
		GL.Begin(7);
		GL.Color(color * 0.15f);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.End();
		GL.PopMatrix();
	}

	public static Bounds GetLocalBounds(GameObject obj, Bounds bounds)
	{
		Bounds result = bounds;
		result.center = obj.transform.InverseTransformPoint(result.center);
		return result;
	}

	public static Bounds GetWorldBounds(GameObject obj, Bounds bounds)
	{
		Bounds result = bounds;
		result.center = obj.transform.TransformPoint(result.center);
		return result;
	}

	public static Bounds GetWordColliderBoundsInChildren(GameObject obj)
	{
		Bounds result = default(Bounds);
		if (obj != null)
		{
			Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>();
			if (componentsInChildren != null)
			{
				Collider[] array = componentsInChildren;
				foreach (Collider collider in array)
				{
					if (collider != null && !collider.isTrigger)
					{
						if (result.size == Vector3.zero)
						{
							result = collider.bounds;
						}
						else
						{
							result.Encapsulate(collider.bounds);
						}
					}
				}
			}
		}
		return result;
	}

	public static Bounds GetLocalColliderBoundsInChildren(GameObject obj)
	{
		Bounds result = default(Bounds);
		if (obj != null)
		{
			Quaternion rotation = obj.transform.rotation;
			obj.transform.rotation = Quaternion.identity;
			Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>();
			Collider[] array = componentsInChildren;
			foreach (Collider collider in array)
			{
				if (!collider.isTrigger)
				{
					Bounds bounds = collider.bounds;
					bounds.center = obj.transform.InverseTransformPoint(bounds.center);
					if (result.size == Vector3.zero)
					{
						result = bounds;
					}
					else
					{
						result.Encapsulate(bounds);
					}
				}
			}
			obj.transform.rotation = rotation;
		}
		return result;
	}

	public static Bounds GetLocalViewBoundsInChildren(GameObject obj)
	{
		Bounds result = default(Bounds);
		if (obj != null)
		{
			Quaternion rotation = obj.transform.rotation;
			obj.transform.rotation = Quaternion.identity;
			Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				Bounds bounds = renderer.bounds;
				bounds.center = obj.transform.InverseTransformPoint(bounds.center);
				if (result.size == Vector3.zero)
				{
					result = bounds;
				}
				else
				{
					result.Encapsulate(bounds);
				}
			}
			obj.transform.rotation = rotation;
		}
		return result;
	}

	public static bool CheckPositionOnGround(Vector3 position, out Vector3 groundPosition, float lowHeight, float upHeight, LayerMask groundLayer)
	{
		if (Physics.Raycast(position + Vector3.up * upHeight, -Vector3.up, out var hitInfo, lowHeight + upHeight, groundLayer))
		{
			groundPosition = hitInfo.point;
			return true;
		}
		groundPosition = Vector3.zero;
		return false;
	}

	public static bool CheckPositionUnderWater(Vector3 v)
	{
		if (VFVoxelWater.self != null)
		{
			return VFVoxelWater.self.IsInWater(v.x, v.y, v.z);
		}
		return false;
	}

	public static bool CheckPositionUnderWater(Vector3 v, out float height)
	{
		if (CheckPositionUnderWater(v))
		{
			height = VFVoxelWater.self.UpToWaterSurface(v.x, v.y, v.z);
			return true;
		}
		height = 0f;
		return false;
	}

	public static bool CheckPositionInSky(Vector3 position, float upHeight = 5f)
	{
		if (!Physics.Raycast(position + Vector3.up * upHeight, Vector3.down, out var _, upHeight + 2f, GameConfig.GroundLayer))
		{
			return true;
		}
		return false;
	}

	public static bool CheckPositionNearCliff(Vector3 position, float radiu = 16f)
	{
		if (!Physics.Raycast(position + Vector3.up * radiu, Vector3.down, out var hitInfo, 128f, GameConfig.GroundLayer))
		{
			return hitInfo.distance >= radiu * 2f;
		}
		return false;
	}

	public static bool CheckErrorPos(Vector3 fixedPos)
	{
		if (fixedPos.x > -10f && fixedPos.x < 10f && fixedPos.y > -10f && fixedPos.y < 10f && fixedPos.z > -10f && fixedPos.z < 10f)
		{
			return false;
		}
		if (fixedPos.x < -9999999f || fixedPos.x > 9999999f || fixedPos.y < -9999999f || fixedPos.y > 9999999f || fixedPos.z < -9999999f || fixedPos.z > 9999999f)
		{
			return false;
		}
		return true;
	}

	public static bool GetWaterSurfaceHeight(Vector3 v, out float waterHeight)
	{
		if (CheckPositionUnderWater(v))
		{
			float num = VFVoxelWater.self.UpToWaterSurface(v.x, v.y, v.z);
			waterHeight = num + v.y;
			return true;
		}
		waterHeight = 0f;
		return false;
	}

	public static Vector3 GetTerrainNormal(Vector3 center, float radius, int radiusAccuracy, int angleAccuracy, int groundlayer, Vector3 checkRayUpDir, float checkDis = 5f)
	{
		if (checkRayUpDir == Vector3.zero)
		{
			checkRayUpDir = Vector3.up;
		}
		checkRayUpDir.Normalize();
		if (radiusAccuracy < 1)
		{
			radiusAccuracy = 1;
		}
		if (angleAccuracy < 3)
		{
			angleAccuracy = 3;
		}
		Vector3 normal = checkRayUpDir;
		Vector3 tangent = Vector3.right;
		if (checkRayUpDir != Vector3.up)
		{
			Vector3.OrthoNormalize(ref normal, ref tangent);
		}
		Vector3 result = Vector3.zero;
		if (Physics.Raycast(center + checkRayUpDir, -checkRayUpDir, out var hitInfo, checkDis + 1f, groundlayer))
		{
			result = hitInfo.normal;
		}
		for (int i = 0; i < radiusAccuracy; i++)
		{
			for (int j = 0; j < angleAccuracy; j++)
			{
				Vector3 origin = center + checkRayUpDir + Quaternion.AngleAxis(360 * j / angleAccuracy, checkRayUpDir) * (tangent * radius * (i + 1) / radiusAccuracy);
				if (Physics.Raycast(origin, checkRayUpDir * -2f, out hitInfo, checkDis + 1f, groundlayer))
				{
					result += hitInfo.normal * (1f - ((float)i + 1f) / (float)radiusAccuracy);
				}
			}
		}
		return result;
	}

	public static bool IsDamageCollider(Collider collider)
	{
		return collider.gameObject.tag.Equals("Damage");
	}

	public static void IgnoreCollision(GameObject obj1, GameObject obj2, bool isIgnore = true)
	{
		if (!(obj1 != null) || !(obj2 != null))
		{
			return;
		}
		Collider[] componentsInChildren = obj1.GetComponentsInChildren<Collider>();
		Collider[] componentsInChildren2 = obj2.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!componentsInChildren[i].gameObject.activeSelf)
			{
				continue;
			}
			bool enabled = componentsInChildren[i].enabled;
			componentsInChildren[i].enabled = true;
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				if (componentsInChildren2[j].gameObject.activeSelf)
				{
					bool enabled2 = componentsInChildren2[j].enabled;
					componentsInChildren2[j].enabled = true;
					if (componentsInChildren[i] != componentsInChildren2[j])
					{
						Physics.IgnoreCollision(componentsInChildren[i], componentsInChildren2[j], isIgnore);
					}
					componentsInChildren2[j].enabled = enabled2;
				}
			}
			componentsInChildren[i].enabled = enabled;
		}
	}

	public static void IgnoreCollision(Collider col1, Collider col2, bool isIgnore = true)
	{
		if (!(null == col1) && !(null == col2))
		{
			bool enabled = col1.enabled;
			bool enabled2 = col2.enabled;
			col1.enabled = true;
			col2.enabled = true;
			if (col1 != col2 && col1.enabled && col2.enabled)
			{
				Physics.IgnoreCollision(col1, col2, isIgnore);
			}
			col1.enabled = enabled;
			col2.enabled = enabled2;
		}
	}

	public static void IgnoreCollision(Collider[] cols1, Collider col2, bool isIgnore = true)
	{
		if (cols1 != null && !(null == col2))
		{
			foreach (Collider col3 in cols1)
			{
				IgnoreCollision(col3, col2);
			}
		}
	}

	public static void IgnoreCollision(Collider[] cols1, Collider[] cols2, bool isIgnore = true)
	{
		if (cols1 == null || cols2 == null)
		{
			return;
		}
		for (int i = 0; i < cols1.Length; i++)
		{
			for (int j = 0; j < cols2.Length; j++)
			{
				Collider col = cols1[i];
				Collider col2 = cols2[j];
				IgnoreCollision(col, col2);
			}
		}
	}

	public static RaycastHit[] SortHitInfo(RaycastHit[] hits, bool ignoreTrigger = true)
	{
		List<RaycastHit> list = new List<RaycastHit>(hits);
		if (ignoreTrigger)
		{
			list = list.FindAll((RaycastHit ret) => !ret.collider.isTrigger);
		}
		list.Sort((RaycastHit hit1, RaycastHit hit2) => hit1.distance.CompareTo(hit2.distance));
		return list.ToArray();
	}

	public static bool GetPositionLayer(Vector3 position, out Vector3 point, int layer, int obstructLayer)
	{
		point = Vector3.zero;
		if (Physics.SphereCast(position + Vector3.up * 128f, 1f, Vector3.down, out var hitInfo, 256f, IgnoreWanderLayer))
		{
			return false;
		}
		if (Physics.Raycast(position + Vector3.up * 128f, Vector3.down, out hitInfo, 256f, layer))
		{
			point = hitInfo.point;
			return true;
		}
		return false;
	}

	public static Vector3 ConstantSlerp(Vector3 from, Vector3 to, float angle)
	{
		float t = Mathf.Min(1f, angle / Vector3.Angle(from, to));
		return Vector3.Slerp(from, to, t);
	}

	public static Vector3 GetTerrainNormal(Vector3 center, Vector3 direction, int groundLayer, Vector3 checkRayUpDir, int accuracy = 3, float checkDis = 5f)
	{
		if (direction == Vector3.zero)
		{
			return Vector3.up;
		}
		direction.Normalize();
		float num = checkDis / (float)accuracy;
		Ray ray = new Ray(center + checkRayUpDir, direction);
		float num2 = checkDis;
		if (Physics.Raycast(ray, out var hitInfo, checkDis, groundLayer))
		{
			num2 = hitInfo.distance;
		}
		int num3 = Mathf.FloorToInt(num2 / num);
		if (num3 == 0)
		{
			num3 = 1;
		}
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < num3; i++)
		{
			ray.origin = center + checkRayUpDir + (float)i * num * direction;
			ray.direction = Vector3.down;
			if (Physics.Raycast(ray, out hitInfo, checkDis, groundLayer))
			{
				zero += hitInfo.normal * (num3 - i) / num3;
				continue;
			}
			break;
		}
		return zero;
	}

	public static bool ContainsParameter(Animator animator, string parameter)
	{
		for (int i = 0; i < animator.parameters.Length; i++)
		{
			if (animator.parameters[i].name.Equals(parameter))
			{
				return true;
			}
		}
		return false;
	}

	public static Vector3 CardinalSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension = 0.5f)
	{
		return p1 + (p2 - p0) * tension * t + ((p2 - p1) * 3f - (p3 - p1) * tension - (p2 - p0) * tension * 2f) * t * t + ((p3 - p1) * tension - (p2 - p1) * 2f + (p2 - p0) * tension) * t * t * t;
	}

	public static bool CheckForWard(PeEntity entity, Vector3 pos, Vector3 dir, float wide, out Vector3 v, out float ditance, float radiu)
	{
		v = Vector3.zero;
		ditance = 0f;
		if (entity == null)
		{
			return false;
		}
		if (entity.IsNpcInDinnerTime || entity.IsNpcInSleepTime || entity.NpcHasAnyRequest || entity.attackEnemy != null)
		{
			return false;
		}
		Vector3 vector = pos + Vector3.up;
		int layerMask = 1024;
		Debug.DrawLine(vector, vector + dir * radiu, Color.red);
		if (Physics.SphereCast(vector, wide, dir, out var hitInfo, radiu, layerMask))
		{
			v = Vector3.ProjectOnPlane(pos - hitInfo.transform.position, Vector3.up);
			ditance = hitInfo.distance;
			return true;
		}
		return false;
	}

	public static bool CanAttackReputation(PeEntity e1, PeEntity e2)
	{
		if (e1 == null || e2 == null)
		{
			return false;
		}
		int pid = (int)e1.GetAttribute(AttribType.DefaultPlayerID);
		int pid2 = (int)e2.GetAttribute(AttribType.DefaultPlayerID);
		return CanAttackReputation(pid, pid2);
	}

	public static bool CanAttackReputation(int pid1, int pid2)
	{
		if (PeSingleton<ReputationSystem>.Instance.HasReputation(pid1, pid2) && PeSingleton<ReputationSystem>.Instance.GetReputationLevel(pid1, pid2) >= ReputationSystem.ReputationLevel.Neutral)
		{
			return false;
		}
		if (PeSingleton<ReputationSystem>.Instance.HasReputation(pid2, pid1) && PeSingleton<ReputationSystem>.Instance.GetReputationLevel(pid2, pid1) >= ReputationSystem.ReputationLevel.Neutral)
		{
			return false;
		}
		return true;
	}

	public static bool CanCordialReputation(int pid1, int pid2)
	{
		if (PeSingleton<ReputationSystem>.Instance.HasReputation(pid1, pid2) && PeSingleton<ReputationSystem>.Instance.GetReputationLevel(pid1, pid2) >= ReputationSystem.ReputationLevel.Cordial)
		{
			return true;
		}
		if (PeSingleton<ReputationSystem>.Instance.HasReputation(pid2, pid1) && PeSingleton<ReputationSystem>.Instance.GetReputationLevel(pid2, pid1) >= ReputationSystem.ReputationLevel.Cordial)
		{
			return true;
		}
		return false;
	}

	public static bool CanDamage(SkEntity e1, SkEntity e2)
	{
		if (null == e1 || null == e2)
		{
			return true;
		}
		SkProjectile skProjectile = e1 as SkProjectile;
		if (skProjectile != null)
		{
			SkEntity skEntityCaster = skProjectile.GetSkEntityCaster();
			if (skEntityCaster == null)
			{
				return false;
			}
			e1 = skProjectile.GetSkEntityCaster();
		}
		int num = Convert.ToInt32(e1.GetAttribute(91));
		int num2 = Convert.ToInt32(e2.GetAttribute(91));
		int src = Convert.ToInt32(e1.GetAttribute(95));
		int dst = Convert.ToInt32(e2.GetAttribute(95));
		return CanDamageReputation(num, num2) && Singleton<ForceSetting>.Instance.Conflict(num, num2) && DamageData.GetValue(src, dst) != 0;
	}

	public static bool CanDamageReputation(PeEntity e1, PeEntity e2)
	{
		if (e1 == null || e2 == null)
		{
			return false;
		}
		int pid = (int)e1.GetAttribute(AttribType.DefaultPlayerID);
		int pid2 = (int)e2.GetAttribute(AttribType.DefaultPlayerID);
		return CanDamageReputation(pid, pid2);
	}

	public static bool CanDamageReputation(int pid1, int pid2)
	{
		if (PeSingleton<ReputationSystem>.Instance.HasReputation(pid1, pid2) && !PeGameMgr.IsAdventure && PeSingleton<ReputationSystem>.Instance.GetReputationLevel(pid1, pid2) > ReputationSystem.ReputationLevel.Neutral)
		{
			return false;
		}
		if (PeSingleton<ReputationSystem>.Instance.HasReputation(pid2, pid1) && !PeGameMgr.IsAdventure && PeSingleton<ReputationSystem>.Instance.GetReputationLevel(pid2, pid1) > ReputationSystem.ReputationLevel.Neutral)
		{
			return false;
		}
		return true;
	}

	public static bool CanAttack(PeEntity e1, PeEntity e2)
	{
		if (e1 == null || e2 == null)
		{
			return false;
		}
		if (!CanAttackReputation(e1, e2))
		{
			return false;
		}
		int srcID = (int)e1.GetAttribute(AttribType.DefaultPlayerID);
		int dstID = (int)e2.GetAttribute(AttribType.DefaultPlayerID);
		int src = (int)e1.GetAttribute(AttribType.CampID);
		int dst = (int)e2.GetAttribute(AttribType.CampID);
		return Singleton<ForceSetting>.Instance.Conflict(srcID, dstID) && (float)Mathf.Abs(ThreatData.GetInitData(src, dst)) > float.Epsilon;
	}

	public static bool IsBlocked(PeEntity e1, PeEntity e2)
	{
		Vector3 centerTop = e1.centerTop;
		Vector3 centerPos = e2.centerPos;
		int num = ((!(e1.biologyViewCmpt != null) || !(e1.biologyViewCmpt.monoModelCtrlr != null)) ? (-1) : e1.biologyViewCmpt.monoModelCtrlr.gameObject.layer);
		RaycastHit[] hitInfos = Physics.RaycastAll(centerTop, centerPos - centerTop, Vector3.Distance(centerTop, centerPos));
		for (int i = 0; i < hitInfos.Length; i++)
		{
			if (hitInfos[i].collider.isTrigger || (num == 10 && num == hitInfos[i].transform.gameObject.layer) || hitInfos[i].collider.gameObject.layer == 4 || hitInfos[i].transform.IsChildOf(e1.transform) || hitInfos[i].transform.IsChildOf(e2.transform))
			{
				continue;
			}
			if (e2.carrier != null)
			{
				bool isPassenger = false;
				e2.carrier.ForeachPassenger(delegate(PESkEntity passenger, bool isDriver)
				{
					if (hitInfos[i].transform.IsChildOf(passenger.transform))
					{
						isPassenger = true;
					}
				});
				if (isPassenger)
				{
					continue;
				}
			}
			return true;
		}
		return false;
	}

	public static bool IsBlocked(PeEntity e, Vector3 position)
	{
		int num = ((!(e.biologyViewCmpt != null) || !(e.biologyViewCmpt.monoModelCtrlr != null)) ? (-1) : e.biologyViewCmpt.monoModelCtrlr.gameObject.layer);
		RaycastHit[] array = Physics.RaycastAll(e.centerTop, position - e.centerTop, Vector3.Distance(e.centerTop, position));
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].collider.isTrigger && (num != 10 || num != array[i].transform.gameObject.layer) && array[i].collider.gameObject.layer != 4 && !array[i].transform.IsChildOf(e.transform))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsNpcsuperposition(PeEntity npc, Enemy enemy)
	{
		if (npc.NpcCmpt == null || enemy.entityTarget.target == null)
		{
			return false;
		}
		Bounds bounds = new Bounds(npc.position, npc.peTrans.bound.size);
		List<PeEntity> melees = enemy.entityTarget.target.GetMelees();
		for (int i = 0; i < melees.Count; i++)
		{
			if (!melees[i].Equals(npc))
			{
				Bounds bounds2 = new Bounds(melees[i].position, melees[i].peTrans.bound.size);
				if (bounds.Intersects(bounds2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsNpcsuperposition(Vector3 dirPos, Enemy enemy)
	{
		if (enemy.entityTarget.target == null)
		{
			return false;
		}
		List<PeEntity> melees = enemy.entityTarget.target.GetMelees();
		for (int i = 0; i < melees.Count; i++)
		{
			Bounds bounds = new Bounds(melees[i].position, melees[i].peTrans.bound.size);
			if (bounds.Contains(dirPos))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsInAstarGrid(Vector3 position)
	{
		if (AstarPath.active == null)
		{
			return false;
		}
		int num = AstarPath.active.graphs.Length;
		for (int i = 0; i < num; i++)
		{
			if (AstarPath.active.graphs[i] is GridGraph gridGraph)
			{
				float num2 = Mathf.Abs(position.x - gridGraph.center.x);
				float num3 = Mathf.Abs(position.z - gridGraph.center.z);
				if (num2 > gridGraph.nodeSize * (float)gridGraph.width || num3 > gridGraph.nodeSize * (float)gridGraph.depth)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool IsDamageBlock(PeEntity peEntity)
	{
		if (peEntity.maxRadius > 2f || peEntity.Stucking(1f))
		{
			Vector3 position = peEntity.position;
			Vector3 point = peEntity.position + Vector3.up * peEntity.maxHeight;
			float x = peEntity.bounds.extents.x;
			float maxDistance = Mathf.Max(0f, peEntity.bounds.extents.z - peEntity.bounds.extents.x) + 0.5f;
			RaycastHit[] array = Physics.CapsuleCastAll(position, point, x, peEntity.tr.forward, maxDistance, 4096);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].collider.name.StartsWith("b45Chnk"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsUnderBlock(PeEntity peEntity)
	{
		if (Physics.Raycast(peEntity.position, Vector3.up, out var _, 128f, Standlayer))
		{
			return true;
		}
		return false;
	}

	public static bool IsForwardBlock(PeEntity peEntity, Vector3 dir, float distance, float minAngle = 15f)
	{
		for (int i = -2; i < 3; i++)
		{
			Vector3 vector = Vector3.Cross(dir, Vector3.up);
			Vector3 vector2 = Quaternion.AngleAxis(minAngle * (float)i, Vector3.up) * dir;
			Debug.DrawRay(peEntity.position + Vector3.up, vector2 * distance, Color.white);
			if (Physics.Raycast(peEntity.position + Vector3.up, vector2, out var _, distance, Standlayer))
			{
				return true;
			}
		}
		return false;
	}

	public static SkEntity GetCaster(SkEntity caster)
	{
		SkEntity skEntity = caster;
		SkProjectile skProjectile = skEntity as SkProjectile;
		if (skProjectile != null)
		{
			skEntity = skProjectile.GetSkEntityCaster();
		}
		CreationSkEntity creationSkEntity = skEntity as CreationSkEntity;
		if (creationSkEntity != null && creationSkEntity.driver != null)
		{
			skEntity = creationSkEntity.driver.skEntity;
		}
		return skEntity;
	}

	public static T GetCmpt<T>(Transform root) where T : Component
	{
		return (T)root.TraverseHierarchySerial((Transform t, int ddddd) => t.GetComponent<T>());
	}

	public static T[] GetCmpts<T>(Transform root) where T : Component
	{
		List<T> list = new List<T>(4);
		root.TraverseHierarchySerial(delegate(Transform t, int ddddd)
		{
			T component = t.GetComponent<T>();
			if ((bool)component)
			{
				list.Add(component);
			}
		});
		return list.ToArray();
	}

	public static T[] GetAllCmpts<T>(Transform root) where T : Component
	{
		List<T> list = new List<T>(4);
		root.TraverseHierarchy(delegate(Transform t, int ddddd)
		{
			T[] components = t.GetComponents<T>();
			if (components != null && components.Length > 0)
			{
				list.AddRange(components);
			}
		});
		return list.ToArray();
	}

	public static bool IsChildItemType(int seldTypeID, int parentTypeID)
	{
		if (seldTypeID == 0 || parentTypeID == 0 || seldTypeID == parentTypeID)
		{
			return true;
		}
		ItemProto.Mgr.ItemEditorType editorType = PeSingleton<ItemProto.Mgr>.Instance.GetEditorType(seldTypeID);
		if (editorType == null || editorType.parentID == 0)
		{
			return false;
		}
		if (parentTypeID == editorType.parentID)
		{
			return true;
		}
		return IsChildItemType(editorType.parentID, parentTypeID);
	}

	public static bool InRange(Vector3 centor, Vector3 targetPos, float radius, bool Is3D = true)
	{
		float num = Magnitude(centor, targetPos, Is3D);
		return num < radius;
	}

	public static GlobalTreeInfo RayCastTree(Ray ray, float distance)
	{
		GlobalTreeInfo result = null;
		if (null != LSubTerrainMgr.Instance)
		{
			result = LSubTerrainMgr.RayCast(ray, distance);
		}
		else if (null != RSubTerrainMgr.Instance)
		{
			TreeInfo treeInfo = RSubTerrainMgr.RayCast(ray, distance);
			if (treeInfo != null)
			{
				result = new GlobalTreeInfo(-1, treeInfo);
			}
		}
		return result;
	}

	public static GlobalTreeInfo RayCastTree(Vector3 origin, Vector3 dir, float distance)
	{
		return RayCastTree(new Ray(origin, dir), distance);
	}

	public static GlobalTreeInfo GetTreeinfo(Collider col)
	{
		if (null == col)
		{
			return null;
		}
		GlobalTreeInfo result = null;
		if (null != LSubTerrainMgr.Instance)
		{
			result = LSubTerrainMgr.GetTreeinfo(col);
		}
		else if (null != RSubTerrainMgr.Instance)
		{
			TreeInfo treeinfo = RSubTerrainMgr.GetTreeinfo(col);
			if (treeinfo != null)
			{
				result = new GlobalTreeInfo(-1, treeinfo);
			}
		}
		return result;
	}

	public static bool IsVoxelOrBlock45(SkEntity entity)
	{
		return entity is VFVoxelTerrain || entity is Block45Man;
	}

	public static bool RagdollTranlate(PeEntity entity, Vector3 pos)
	{
		if (entity != null && entity.lodCmpt != null && pos != Vector3.zero)
		{
			entity.lodCmpt.DestroyView();
			entity.ExtSetPos(pos);
			SceneMan.SetDirty(entity.lodCmpt);
			return true;
		}
		return false;
	}
}
