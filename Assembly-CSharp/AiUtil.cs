using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class AiUtil
{
	public static LayerMask obstructLayer = LayerMask.NameToLayer(string.Empty);

	public static LayerMask groundedLayer = SceneMan.DependenceLayer;

	public static LayerMask voxelLayer = 4096;

	private static int _nFrmOfDcAgents = -1;

	private static List<ISceneObjAgent> _dcAgents = null;

	public static bool GetNearNodePosWalkable(Vector3 pos, out Vector3 outPos)
	{
		outPos = pos;
		return true;
	}

	public static IntVector2 ConvertToIntVector2FormLodLevel(IntVector4 node, int lodLevel)
	{
		int num = lodLevel + 5;
		int x_ = node.x >> num << num;
		int y_ = node.z >> num << num;
		return new IntVector2(x_, y_);
	}

	public static IntVector4 ConvertToIntVector4(Vector3 position, int lod)
	{
		IntVector4 intVector = new IntVector4(new IntVector3(position), lod);
		int num = lod + 5;
		intVector.x = intVector.x >> num << num;
		intVector.y = intVector.y >> num << num;
		intVector.z = intVector.z >> num << num;
		return intVector;
	}

	public static int GetHarm(GameObject go)
	{
		return -1;
	}

	public static int GetCamp(GameObject go)
	{
		return -1;
	}

	public static Bounds GetLocalBounds(Collider collider)
	{
		if (collider == null)
		{
			return default(Bounds);
		}
		Bounds bounds = collider.bounds;
		bounds.center = collider.transform.InverseTransformPoint(collider.bounds.center);
		return bounds;
	}

	public static Vector3 GetColliderCenter(Collider collider)
	{
		if (collider == null)
		{
			return Vector3.zero;
		}
		Vector3 position = Vector3.zero;
		if (collider is CapsuleCollider)
		{
			position = (collider as CapsuleCollider).center;
		}
		else if (collider is SphereCollider)
		{
			position = (collider as SphereCollider).center;
		}
		else if (collider is BoxCollider)
		{
			position = (collider as BoxCollider).center;
		}
		else if (collider is CharacterController)
		{
			position = (collider as CharacterController).center;
		}
		else
		{
			if (collider is MeshCollider)
			{
				return (collider as MeshCollider).bounds.center;
			}
			Debug.LogError("collider is error!!");
		}
		return collider.transform.TransformPoint(position);
	}

	public static float GetColliderRadius(Collider collider)
	{
		if (collider == null)
		{
			return 0f;
		}
		float result = 0f;
		if (collider is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			result = ((capsuleCollider.direction == 0) ? (capsuleCollider.height * 0.5f) : ((capsuleCollider.direction != 1) ? (capsuleCollider.height * 0.5f) : capsuleCollider.radius));
		}
		else if (collider is SphereCollider)
		{
			result = (collider as SphereCollider).radius;
		}
		else if (collider is BoxCollider)
		{
			result = (collider as BoxCollider).size.z * 0.5f;
		}
		else if (collider is CharacterController)
		{
			result = (collider as CharacterController).radius;
		}
		else if (collider is MeshCollider)
		{
			result = (collider as MeshCollider).bounds.extents.z;
		}
		else
		{
			Debug.LogError("collider is error!!");
		}
		return result;
	}

	public static float GetColliderHeight(Collider collider)
	{
		if (collider == null)
		{
			return 0f;
		}
		float result = 0f;
		if (collider is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			result = ((capsuleCollider.direction == 0) ? (capsuleCollider.radius * 2f) : ((capsuleCollider.direction != 1) ? (capsuleCollider.radius * 2f) : capsuleCollider.height));
		}
		else if (collider is SphereCollider)
		{
			result = (collider as SphereCollider).radius;
		}
		else if (collider is BoxCollider)
		{
			result = (collider as BoxCollider).size.y;
		}
		else if (collider is CharacterController)
		{
			result = (collider as CharacterController).height;
		}
		else if (collider is MeshCollider)
		{
			result = (collider as MeshCollider).bounds.extents.y;
		}
		else
		{
			Debug.LogError("collider is error!!");
		}
		return result;
	}

	public static float GetColliderSide(Collider collider)
	{
		if (collider == null)
		{
			return 0f;
		}
		float result = 0f;
		if (collider is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			result = ((capsuleCollider.direction == 0) ? (capsuleCollider.height * 0.5f) : ((capsuleCollider.direction != 1) ? (capsuleCollider.height * 0.5f) : capsuleCollider.radius));
		}
		else if (collider is SphereCollider)
		{
			result = (collider as SphereCollider).radius;
		}
		else if (collider is BoxCollider)
		{
			result = (collider as BoxCollider).size.x * 0.5f;
		}
		else if (collider is CharacterController)
		{
			result = (collider as CharacterController).radius;
		}
		else
		{
			Debug.LogError("collider is error!!");
		}
		return result;
	}

	public static float Angle3D(Vector3 from, Vector3 to)
	{
		return Vector3.Angle(from, to);
	}

	public static float Angle2D(Vector3 from, Vector3 to)
	{
		Vector3 from2 = new Vector3(from.x, 0f, from.z);
		Vector3 to2 = new Vector3(to.x, 0f, to.z);
		return Vector3.Angle(from2, to2);
	}

	public static float MagnitudeH(Vector3 distance)
	{
		distance.y = 0f;
		return distance.magnitude;
	}

	public static float SqrMagnitudeH(Vector3 distance)
	{
		distance.y = 0f;
		return distance.sqrMagnitude;
	}

	public static float SqrMagnitude(Vector3 distance)
	{
		return distance.sqrMagnitude;
	}

	public static float DistanceXZ(Vector3 v1, Vector3 v2)
	{
		v1.y = v2.y;
		return Vector3.Distance(v1, v2);
	}

	public static float DistanceYABS(Vector3 v1, Vector3 v2)
	{
		return Mathf.Abs(v1.y - v2.y);
	}

	public static float DotH(Vector3 vector1, Vector3 vector2)
	{
		Vector3 vector3 = new Vector3(vector1.x, 0f, vector1.z);
		Vector3 vector4 = new Vector3(vector2.x, 0f, vector2.z);
		return Vector3.Dot(vector3.normalized, vector4.normalized);
	}

	public static float DotV(Vector3 vector)
	{
		return Vector3.Dot(new Vector3(vector.x, 0f, vector.z).normalized, vector.normalized);
	}

	public static float Angle(Vector3 v1, Vector3 v2)
	{
		if (v1 == Vector3.zero || v2 == Vector3.zero)
		{
			return 0f;
		}
		return Vector3.Angle(v1, v2);
	}

	public static float AngleXZ(Vector3 v1, Vector3 v2)
	{
		v1.y = 0f;
		v2.y = 0f;
		if (v1 == Vector3.zero || v2 == Vector3.zero)
		{
			return 0f;
		}
		return Vector3.Angle(v1, v2);
	}

	public static Vector3 GetNextPosition(Vector3 v, Vector3 velocity, float height)
	{
		LayerMask layerMask = 79872;
		Ray ray = new Ray(v + velocity + Vector3.up * height, -Vector3.up);
		if (Physics.Raycast(ray, out var hitInfo, height * 2f, layerMask))
		{
			return hitInfo.point;
		}
		return Vector3.zero;
	}

	public static Vector3 GetNextPosition(Vector3 v, Vector3 velocity, float upHeight, float lowHeight)
	{
		LayerMask layerMask = 71680;
		Ray ray = new Ray(v + velocity + Vector3.up * upHeight, -Vector3.up);
		if (Physics.Raycast(ray, out var hitInfo, upHeight + lowHeight, layerMask))
		{
			return hitInfo.point;
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPositionInCave(Vector3 center, float minRadius, float maxRadius, LayerMask layer, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 position = center + new Vector3(vector.x, 0f, vector.y);
			if (CheckPositionInCave(ref position, 128f, voxelLayer))
			{
				return position;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPositionInCave(IntVector4 node, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		for (int i = 0; i < num; i++)
		{
			Vector3 position = node.ToVector3();
			position += new Vector3(UnityEngine.Random.Range(0f, 32 << node.w), 0f, UnityEngine.Random.Range(0f, 32 << node.w));
			if (CheckPositionOnGround(position, out var hitInfo, 0f, 32 << node.w, 4096) && CheckSlopeValid(hitInfo.normal, 45f) && CheckPositionInCave(hitInfo.point, 128f, voxelLayer))
			{
				return hitInfo.point;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPosition(IntVector4 node)
	{
		Vector3 vector = node.ToVector3();
		return vector + new Vector3(UnityEngine.Random.Range(0f, 32 << node.w), 0f, UnityEngine.Random.Range(0f, 32 << node.w));
	}

	public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
		return center + new Vector3(vector.x, 0f, vector.y);
	}

	public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, Vector3 direction, float minAngle, float maxAngle)
	{
		direction.y = 0f;
		Vector3 vector = (Quaternion.AngleAxis(UnityEngine.Random.Range(minAngle, maxAngle), Vector3.up) * direction).normalized * UnityEngine.Random.Range(minRadius, maxRadius);
		return center + vector;
	}

	public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, float height, LayerMask layer, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 vector2 = center + new Vector3(vector.x, 0f, vector.y);
			if (Physics.Raycast(vector2 + Vector3.up * height, -Vector3.up, out var hitInfo, height * 2f, layer) && CheckSlopeValid(hitInfo.normal, 45f) && GetVoxelType(hitInfo) >= 0)
			{
				return hitInfo.point;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, Vector3 direction, float minAngle, float maxAngle, float height, LayerMask layer, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		for (int i = 0; i < num; i++)
		{
			direction.y = 0f;
			Vector3 vector = (Quaternion.AngleAxis(UnityEngine.Random.Range(minAngle, maxAngle), Vector3.up) * direction).normalized * UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 vector2 = center + vector;
			if (Physics.Raycast(vector2 + Vector3.up * height, -Vector3.up, out var hitInfo, height * 2f, layer) && CheckSlopeValid(hitInfo.normal, 45f))
			{
				return hitInfo.point;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPositionInLand(Vector3 center, float minRadius, float maxRadius, float height, LayerMask layer, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 vector2 = center + new Vector3(vector.x, 0f, vector.y);
			if (Physics.Raycast(vector2 + Vector3.up * height, -Vector3.up, out var hitInfo, height * 2f, layer) && !CheckPositionUnderWater(hitInfo.point))
			{
				return hitInfo.point;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPositionInLand(Vector3 center, float minRadius, float maxRadius, Vector3 direction, float minAngle, float maxAngle, float height, LayerMask layer, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		for (int i = 0; i < num; i++)
		{
			direction.y = 0f;
			Vector3 vector = (Quaternion.AngleAxis(UnityEngine.Random.Range(minAngle, maxAngle), Vector3.up) * direction).normalized * UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 vector2 = center + vector;
			if (Physics.Raycast(vector2 + Vector3.up * height, -Vector3.up, out var hitInfo, height * 2f, layer) && !CheckPositionUnderWater(hitInfo.point))
			{
				return hitInfo.point;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPositionInWater(Vector3 center, float minHeight, float maxHeight, float minRadius, float maxRadius, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		if (CheckPositionUnderWater(center, out var waterHeight))
		{
			for (int i = 0; i < num; i++)
			{
				float a = waterHeight;
				Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
				Vector3 vector2 = center + new Vector3(vector.x, 0f, vector.y);
				if (Physics.Raycast(vector2 + Vector3.up * (waterHeight - vector2.y), Vector3.down, out var hitInfo, 512f, voxelLayer))
				{
					a = hitInfo.point.y;
				}
				float num2 = waterHeight - minHeight;
				float num3 = Mathf.Max(a, waterHeight - maxHeight);
				if (!(num2 <= float.Epsilon) && !(num2 <= num3))
				{
					float y = UnityEngine.Random.Range(num3, num2);
					Vector3 vector3 = new Vector3(vector2.x, y, vector2.z);
					if (CheckPositionUnderWater(vector3))
					{
						return vector3;
					}
				}
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPositionInWater(Vector3 center, float minHeight, float maxHeight, Vector3 direction, float minAngle, float maxAngle, float minRadius, float maxRadius, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		if (CheckPositionUnderWater(center, out var waterHeight))
		{
			for (int i = 0; i < num; i++)
			{
				float a = waterHeight;
				direction.y = 0f;
				Vector2 vector = (Quaternion.AngleAxis(UnityEngine.Random.Range(minAngle, maxAngle), Vector3.up) * direction).normalized * UnityEngine.Random.Range(minRadius, maxRadius);
				Vector3 vector2 = center + new Vector3(vector.x, 0f, vector.y);
				if (Physics.Raycast(vector2 + Vector3.up * (waterHeight - vector2.y), Vector3.down, out var hitInfo, 512f, voxelLayer))
				{
					a = hitInfo.point.y;
				}
				float num2 = waterHeight - minHeight;
				float num3 = Mathf.Max(a, waterHeight - maxHeight);
				if (!(num2 <= float.Epsilon) && !(num2 <= num3))
				{
					float y = UnityEngine.Random.Range(num3, num2);
					Vector3 vector3 = new Vector3(vector2.x, y, vector2.z);
					if (CheckPositionUnderWater(vector3))
					{
						return vector3;
					}
				}
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPositionInSky(Vector3 center, float minHeight, float maxHeight, float minRadius, float maxRadius, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 vector2 = center + new Vector3(vector.x, UnityEngine.Random.Range(minHeight, maxHeight), vector.y);
			if (!CheckPositionUnderWater(vector2) && !CheckPositionInCave(vector2, 128f, voxelLayer))
			{
				return vector2;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPositionInSky(Vector3 center, float minHeight, float maxHeight, Vector3 direction, float minAngle, float maxAngle, float minRadius, float maxRadius, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		for (int i = 0; i < num; i++)
		{
			direction.y = 0f;
			Vector3 vector = Quaternion.AngleAxis(UnityEngine.Random.Range(minAngle, maxAngle), Vector3.up) * direction;
			Vector2 vector2 = vector * UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 vector3 = center + new Vector3(vector2.x, UnityEngine.Random.Range(minHeight, maxHeight), vector2.y);
			if (!CheckPositionUnderWater(vector3) && !CheckPositionInCave(vector3, 128f, voxelLayer))
			{
				return vector3;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 ToVector3(string vecString)
	{
		if (!string.IsNullOrEmpty(vecString))
		{
			string[] array = vecString.Split(',');
			if (array.Length == 3)
			{
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				return new Vector3(x, y, z);
			}
		}
		return Vector3.zero;
	}

	public static string[] Split(string value, char parameter)
	{
		return Split(value, new char[1] { parameter });
	}

	public static string[] Split(string value, char[] parameter)
	{
		return value.Split(parameter);
	}

	public static bool CheckPositionOnGround(Vector3 position, out RaycastHit hitInfo, float height, LayerMask groundLayer)
	{
		if (Physics.Raycast(position + Vector3.up * height, -Vector3.up, out hitInfo, height * 2f, groundLayer))
		{
			position = hitInfo.point;
			return true;
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	public static bool CheckPositionOnGround(Vector3 position, out RaycastHit hitInfo, float lowHeight, float upHeight, LayerMask groundLayer)
	{
		if (Physics.Raycast(position + Vector3.up * upHeight, -Vector3.up, out hitInfo, lowHeight + upHeight, groundLayer))
		{
			position = hitInfo.point;
			return true;
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	public static bool CheckPositionOnGround(Vector3 position, float height, LayerMask groundLayer)
	{
		return Physics.Raycast(position + Vector3.up * height, -Vector3.up, height * 2f, groundLayer);
	}

	public static bool CheckPositionOnGround(ref Vector3 position, float height, LayerMask groundLayer)
	{
		if (Physics.Raycast(position + Vector3.up * height, -Vector3.up, out var hitInfo, height * 2f, groundLayer))
		{
			position = hitInfo.point;
			return true;
		}
		return false;
	}

	public static bool CheckPositionOnGround(ref Vector3 position, float lowHeight, float upHeight, LayerMask groundLayer)
	{
		if (Physics.Raycast(position + Vector3.up * upHeight, -Vector3.up, out var hitInfo, lowHeight + upHeight, groundLayer))
		{
			position = hitInfo.point;
			return true;
		}
		return false;
	}

	public static bool CheckSlopeValid(Vector3 normal, float slopeAngle)
	{
		float num = Vector3.Dot(normal.normalized, Vector3.up);
		float num2 = Mathf.Cos(slopeAngle * ((float)Math.PI / 180f));
		return num > num2;
	}

	public static bool CheckPositionOnGround(ref Vector3 position, out Vector3 normal, float lowHeight, float upHeight, LayerMask groundLayer)
	{
		normal = Vector3.zero;
		if (Physics.Raycast(position + Vector3.up * upHeight, -Vector3.up, out var hitInfo, lowHeight + upHeight, groundLayer))
		{
			position = hitInfo.point;
			normal = hitInfo.normal;
			return true;
		}
		return false;
	}

	public static bool CheckTransformInCave(Transform tr)
	{
		if (tr == null)
		{
			return false;
		}
		return CheckPositionInCave(GetColliderCenter(tr.GetComponent<Collider>()), 256f, voxelLayer);
	}

	public static bool CheckPositionInCave(Vector3 position, float distance, LayerMask layer)
	{
		int num = 0;
		int num2 = 4;
		int num3 = 8;
		float num4 = 0.7f;
		float num5 = 60f;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				Vector3 b = Quaternion.AngleAxis((float)(360 * j) / (float)num3, Vector3.up) * Vector3.forward;
				b = Vector3.Slerp(Vector3.up, b, (float)i * num5 / (float)num2 / 90f);
				if (Physics.Raycast(position, b, out var _, distance, layer))
				{
					num++;
				}
			}
		}
		if ((float)num >= (float)(num2 * num3) * num4)
		{
			return true;
		}
		return false;
	}

	public static bool CheckPositionInCave(ref Vector3 position, float distance, LayerMask layer)
	{
		if (CheckPositionInCave(position, distance, layer))
		{
			if (Physics.Raycast(position, Vector3.up, out var hitInfo, distance, layer))
			{
				if (Physics.Raycast(hitInfo.point, -Vector3.up, out hitInfo, distance, layer))
				{
					position = hitInfo.point;
				}
			}
			else if (Physics.Raycast(position + Vector3.up * distance, -Vector3.up, out hitInfo, 2f * distance, layer))
			{
				position = hitInfo.point;
			}
			return true;
		}
		return false;
	}

	public static bool CheckPositionInCave(Vector3 position, out Vector3 point)
	{
		point = Vector3.zero;
		if (Physics.Raycast(position, Vector3.up, out var hitInfo, 256f, 4096))
		{
			point = hitInfo.point;
			return true;
		}
		return false;
	}

	public static bool CheckPositionUnderWater(Vector3 v)
	{
		if (VFVoxelWater.self != null)
		{
			return VFVoxelWater.self.IsInWater(v.x, v.y + 0.5f, v.z);
		}
		return false;
	}

	public static bool CheckPositionUnderWater(Vector3 v, out float waterHeight)
	{
		if (VFVoxelWater.self != null)
		{
			float num = VFVoxelWater.self.UpToWaterSurface(v.x, v.y + 0.5f, v.z);
			if (num > float.Epsilon)
			{
				waterHeight = num + v.y + 0.5f;
				return true;
			}
		}
		waterHeight = 0f;
		return false;
	}

	public static LightUnit CheckPositionInLightRange(Vector3 position)
	{
		if (LightMgr.Instance != null)
		{
			foreach (LightUnit light in LightMgr.Instance.lights)
			{
				if (light == null || light.lamp == null)
				{
					continue;
				}
				if (light.lamp.type == LightType.Point)
				{
					float num = SqrMagnitude(light.transform.position - position);
					if (num <= light.lamp.range * light.lamp.range)
					{
						return light;
					}
				}
				else
				{
					if (light.lamp.type != 0)
					{
						continue;
					}
					float num2 = SqrMagnitude(light.transform.position - position);
					if (num2 <= light.lamp.range * light.lamp.range)
					{
						float num3 = Vector3.Angle(light.lamp.transform.forward, position - light.lamp.transform.position);
						if (num3 <= light.lamp.spotAngle)
						{
							return light;
						}
					}
				}
			}
		}
		return null;
	}

	public static bool CheckHitObstacle(Collider collider, Vector3 pos, LayerMask layer)
	{
		if (pos == Vector3.zero)
		{
			return false;
		}
		if (collider == null)
		{
			return true;
		}
		if (collider is CharacterController)
		{
			CharacterController characterController = collider as CharacterController;
			return !Physics.CheckCapsule(pos, pos + Vector3.up * characterController.height, characterController.radius + 0.5f, layer);
		}
		if (collider is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			return capsuleCollider.direction switch
			{
				0 => !Physics.CheckCapsule(pos, pos + Vector3.up * capsuleCollider.height, capsuleCollider.radius + 0.5f, layer), 
				1 => !Physics.CheckCapsule(pos, pos + Vector3.up * capsuleCollider.height, capsuleCollider.radius + 0.5f, layer), 
				2 => !Physics.CheckCapsule(pos, pos + Vector3.up * capsuleCollider.height, capsuleCollider.radius + 0.5f, layer), 
				_ => true, 
			};
		}
		return false;
	}

	public static bool CheckObstacle(Collider collider, LayerMask layer)
	{
		if (collider == null)
		{
			return true;
		}
		Transform transform = collider.transform;
		if (collider is CharacterController)
		{
			CharacterController characterController = collider as CharacterController;
			return !Physics.CheckCapsule(transform.position, transform.position + Vector3.up * characterController.height, characterController.radius + 0.5f, layer);
		}
		if (collider is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			Vector3 vector = transform.position + capsuleCollider.center;
			return capsuleCollider.direction switch
			{
				0 => !Physics.CheckCapsule(vector - transform.right * capsuleCollider.height * 0.5f, vector + transform.right * capsuleCollider.height * 0.5f, capsuleCollider.radius, layer), 
				1 => !Physics.CheckCapsule(vector - transform.up * capsuleCollider.height * 0.5f, vector + transform.up * capsuleCollider.height * 0.5f, capsuleCollider.radius, layer), 
				2 => !Physics.CheckCapsule(vector - transform.forward * capsuleCollider.height * 0.5f, vector + transform.forward * capsuleCollider.height * 0.5f, capsuleCollider.radius, layer), 
				_ => true, 
			};
		}
		return false;
	}

	public static VFVoxelChunkGo GetChunk(Vector3 position)
	{
		if (VFVoxelTerrain.self == null)
		{
			return null;
		}
		int num = Mathf.FloorToInt(position.x);
		int num2 = Mathf.FloorToInt(position.y);
		int num3 = Mathf.FloorToInt(position.z);
		return VFVoxelTerrain.self.Voxels.readChunk(num >> 5, num2 >> 5, num3 >> 5)?.ChunkGo;
	}

	public static VFVoxelChunkGo GetChunk(IntVector4 intPos)
	{
		if (VFVoxelTerrain.self == null)
		{
			return null;
		}
		return VFVoxelTerrain.self.Voxels.readChunk(intPos.x >> 5, intPos.y >> 5, intPos.z >> 5, intPos.w)?.ChunkGo;
	}

	public static Vector3 GetChunkPosMin(Vector3 position)
	{
		int num = Mathf.FloorToInt(position.x / 32f);
		int num2 = Mathf.FloorToInt(position.y / 32f);
		int num3 = Mathf.FloorToInt(position.z / 32f);
		return new Vector3(num, num2, num3);
	}

	public static IntVector3 GetChunkPosMinInt(Vector3 position)
	{
		int x_ = Mathf.FloorToInt(position.x / 32f);
		int y_ = Mathf.FloorToInt(position.y / 32f);
		int z_ = Mathf.FloorToInt(position.z / 32f);
		return new IntVector3(x_, y_, z_);
	}

	public static Bounds GetBounds(Vector3 position)
	{
		Vector3 chunkPosMin = GetChunkPosMin(position);
		Vector3 center = chunkPosMin + Vector3.one * 32f * 0.5f;
		return new Bounds(center, Vector3.one * 32f);
	}

	public static Vector3 GetBoundsCenter(Vector3 position)
	{
		Vector3 chunkPosMin = GetChunkPosMin(position);
		return chunkPosMin + Vector3.one * 32f * 0.5f;
	}

	public static Bounds GetCustomBounds(Vector3 position, uint size)
	{
		return new Bounds(GetBoundsCenter(position), Vector3.one * 32f * size);
	}

	public static Bounds GetCustomBoundsIdx(Vector3 position, uint size)
	{
		return new Bounds(GetChunkPosMin(position), Vector3.one * size);
	}

	public static int GetDependType(Vector3 pos)
	{
		Ray ray = new Ray(pos + 0.5f * Vector3.up, Vector3.down);
		if (Physics.Raycast(ray, out var hitInfo, 1f, 595968) && hitInfo.distance < 1f)
		{
			return GetDependType(hitInfo);
		}
		return 0;
	}

	private static int GetDependType(RaycastHit hitInfo)
	{
		if (hitInfo.transform == null)
		{
			return 0;
		}
		if (hitInfo.transform.gameObject.layer == 12)
		{
			if (hitInfo.transform.GetComponent<B45ChunkGo>() != null)
			{
				return 2;
			}
			return 1;
		}
		if (hitInfo.transform.gameObject.layer == 11)
		{
			return 3;
		}
		if (hitInfo.transform.gameObject.layer == 16)
		{
			return 4;
		}
		if (hitInfo.transform.gameObject.layer == 19)
		{
			return 5;
		}
		return 0;
	}

	public static Transform GetChild(Transform parent, string childName)
	{
		if (childName == string.Empty)
		{
			return null;
		}
		foreach (Transform item in parent)
		{
			if (item.name.Equals(childName))
			{
				return item;
			}
			Transform child = GetChild(item, childName);
			if (child != null)
			{
				return child;
			}
		}
		return null;
	}

	public static bool CheckCorrectPosition(Vector3 position, LayerMask mask)
	{
		return CheckPositionOnGround(position, 5f, mask);
	}

	public static void DrawBounds(Transform tr, Bounds bound, Color color)
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

	public static Bounds TransfromOBB2AABB(Transform tr, Bounds bound)
	{
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
		Bounds result = default(Bounds);
		result.center = tr.TransformPoint(bound.center);
		Vector3[] array2 = array;
		foreach (Vector3 point in array2)
		{
			result.Encapsulate(point);
		}
		return result;
	}

	public static Vector3 GetRunawayPosition(AiObject aiObj, Vector3 direction, float minRadius, float maxRadius, float angle)
	{
		float minRadius2 = aiObj.radius + minRadius;
		float maxRadius2 = aiObj.radius + maxRadius;
		if (aiObj.motor.habit == LifeArea.LA_Land)
		{
			return GetRandomPositionInLand(aiObj.position, minRadius2, maxRadius2, direction, 0f - angle, angle, 10f, groundedLayer, 10);
		}
		if (aiObj.motor.habit == LifeArea.LA_Water)
		{
			return GetRandomPositionInWater(aiObj.position, aiObj.height, 128f, direction, 0f - angle, angle, minRadius2, maxRadius2, 10);
		}
		if (aiObj.motor.habit == LifeArea.LA_Sky)
		{
			return GetRandomPositionInSky(aiObj.position, 64f, 128f, direction, 0f - angle, angle, minRadius2, maxRadius2, 10);
		}
		return Vector3.zero;
	}

	public static Vector3 RunawayDirectionCorrect(AiObject ai, Vector3 direction)
	{
		if (ai == null)
		{
			return direction;
		}
		if (!ai.motor.CheckMovementValid(direction))
		{
			return Vector3.zero;
		}
		return direction;
	}

	public static bool CalculateCollsion(Collider collider, Vector3 moveDir, float inspectRange, out RaycastHit hitInfo, LayerMask mask)
	{
		if (collider == null)
		{
			hitInfo = default(RaycastHit);
			return false;
		}
		Vector3 point = Vector3.zero;
		Vector3 point2 = Vector3.zero;
		float radius = 0f;
		if (collider is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			if (capsuleCollider.direction == 2)
			{
				Transform transform = collider.transform;
				radius = capsuleCollider.height * 0.5f;
				point = transform.position + transform.up * capsuleCollider.radius - transform.forward * capsuleCollider.height * 0.5f;
				point2 = transform.position + transform.up * capsuleCollider.radius + transform.forward * capsuleCollider.height * 0.5f;
			}
			else if (capsuleCollider.direction == 1)
			{
				Transform transform2 = collider.transform;
				radius = capsuleCollider.radius;
				point = transform2.position;
				point2 = transform2.position + transform2.up * capsuleCollider.height;
			}
		}
		else if (collider is CharacterController)
		{
			CharacterController characterController = collider as CharacterController;
			Transform transform3 = collider.transform;
			radius = characterController.radius;
			point = transform3.position;
			point2 = transform3.position + transform3.up * characterController.height;
		}
		return Physics.CapsuleCast(point, point2, radius, moveDir, out hitInfo, inspectRange, mask);
	}

	public static bool CheckPositionOnTerrainCollider(Vector3 position, float height = 96f)
	{
		return true;
	}

	public static Color GetPixelFromWorldPosition(Texture2D tex, Vector3 position)
	{
		if (tex == null)
		{
			return default(Color);
		}
		int x = (int)(position.x / 18432f * (float)tex.width);
		int y = (int)(position.z / 18432f * (float)tex.height);
		return tex.GetPixel(x, y);
	}

	public static Bounds IntVector4ToBounds(IntVector4 intVec4)
	{
		Bounds result = default(Bounds);
		Vector3 min = intVec4.ToVector3();
		Vector3 max = intVec4.ToVector3() + Vector3.one * (32 << intVec4.w);
		result.SetMinMax(min, max);
		return result;
	}

	private static int SortHitInfo(RaycastHit hit1, RaycastHit hit2)
	{
		return hit1.distance.CompareTo(hit2.distance);
	}

	public static RaycastHit[] SortHitInfoFromDistance(RaycastHit[] hits, bool ignoreTrigger = true)
	{
		List<RaycastHit> list = new List<RaycastHit>(hits);
		if (ignoreTrigger)
		{
			list = list.FindAll((RaycastHit ret) => !ret.collider.isTrigger);
		}
		list.Sort(SortHitInfo);
		return list.ToArray();
	}

	public static bool GetCloestRaycastHit(out RaycastHit hitInfo, RaycastHit[] hits, bool ignoreTrigger = true)
	{
		RaycastHit[] array = SortHitInfoFromDistance(hits, ignoreTrigger);
		if (array.Length > 0)
		{
			hitInfo = array[0];
			return true;
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	public static PointType GetPointType(Vector3 position)
	{
		PointType result = PointType.PT_NULL;
		RaycastHit hitInfo;
		if (CheckPositionUnderWater(position))
		{
			result = PointType.PT_Water;
		}
		else if (CheckPositionInCave(position, 128f, voxelLayer))
		{
			result = PointType.PT_Cave;
		}
		else if (CheckPositionOnGround(position, out hitInfo, 5f, groundedLayer))
		{
			result = (CheckSlopeValid(hitInfo.normal, 45f) ? PointType.PT_Ground : PointType.PT_Slope);
		}
		return result;
	}

	public static int GetMapID(Vector3 position)
	{
		return (int)VFDataRTGen.GetXZMapType((int)position.x, (int)position.z);
	}

	public static int GetAreaID(Vector3 position)
	{
		return VATownGenerator.Instance.GetLevelByRealPos(new IntVector2((int)position.x, (int)position.z)) switch
		{
			0 => 1, 
			1 => 2, 
			2 => 3, 
			3 => 4, 
			4 => 4, 
			_ => -1, 
		};
	}

	public static int GetVoxelType(RaycastHit hitInfo)
	{
		int layer = hitInfo.transform.gameObject.layer;
		if (layer == 12)
		{
			if (hitInfo.transform.GetComponent<B45ChunkGo>() != null)
			{
				return -1;
			}
			if (hitInfo.transform.GetComponent<VFVoxelChunkGo>() != null)
			{
				return 1;
			}
		}
		return 0;
	}

	public static bool CheckBlockBrush(PeEntity entity)
	{
		if (PEBuildingMan.Self != null && !PEBuildingMan.Self.brushBound.Equals(null))
		{
			Bounds brushBound = PEBuildingMan.Self.brushBound;
			Bounds bounds = new Bounds(entity.position, entity.peTrans.bound.size);
			return bounds.Intersects(brushBound);
		}
		return false;
	}

	public static bool InBlocking()
	{
		return PEBuildingMan.Self != null && !PEBuildingMan.Self.brushBound.Equals(null);
	}

	public static bool CheckBlockBrush(PeEntity entity, out Vector3 avoidPos)
	{
		avoidPos = Vector3.zero;
		if (InBlocking())
		{
			Bounds brushBound = PEBuildingMan.Self.brushBound;
			Bounds bounds = new Bounds(entity.position, entity.peTrans.bound.size);
			avoidPos = brushBound.center;
			return bounds.Intersects(brushBound);
		}
		return false;
	}

	public static bool InDigging(PeEntity target)
	{
		return target.motionEquipment.digTool != null && target.motionEquipment.digTool.m_Indicator != null && target.motionEquipment.digTool.m_Indicator.show;
	}

	public static bool CheckDig(PeEntity entity, PeEntity target)
	{
		if (entity == null || target == null)
		{
			return false;
		}
		if (InDigging(target))
		{
			Bounds bounds = new Bounds(entity.position, entity.peTrans.bound.size);
			Bounds bounds2 = target.motionEquipment.digTool.m_Indicator.bounds;
			return bounds.Intersects(bounds2);
		}
		return false;
	}

	public static bool CheckDig(PeEntity entity, PeEntity target, out Vector3 avoidPos)
	{
		avoidPos = Vector3.zero;
		if (entity == null || target == null)
		{
			return false;
		}
		if (target.motionEquipment.digTool != null && target.motionEquipment.digTool.m_Indicator != null && target.motionEquipment.digTool.m_Indicator.show)
		{
			Bounds bounds = new Bounds(entity.position, entity.peTrans.bound.size);
			Bounds bounds2 = target.motionEquipment.digTool.m_Indicator.bounds;
			avoidPos = bounds2.center;
			return bounds.Intersects(bounds2);
		}
		return false;
	}

	public static bool InDragging()
	{
		return PeSingleton<DraggingMgr>.Instance != null && PeSingleton<DraggingMgr>.Instance.IsDragging();
	}

	public static bool CheckDraging(PeEntity entity)
	{
		if (PeSingleton<DraggingMgr>.Instance == null || entity == null)
		{
			return false;
		}
		if (PeSingleton<DraggingMgr>.Instance.IsDragging())
		{
			if (!(PeSingleton<DraggingMgr>.Instance.Dragable is ItemObjDragging itemObjDragging))
			{
				return false;
			}
			if (itemObjDragging == null || itemObjDragging.DragBase == null || itemObjDragging.DragBase.rootGameObject == null || null == itemObjDragging.DragBase.itemBounds)
			{
				return false;
			}
			Bounds bounds = new Bounds(entity.position, entity.peTrans.bound.size);
			Bounds bounds2 = new Bounds(itemObjDragging.DragBase.rootGameObject.transform.position, itemObjDragging.DragBase.itemBounds.worldBounds.size);
			return bounds.Intersects(bounds2);
		}
		return false;
	}

	public static bool CheckDraging(PeEntity entity, out Vector3 avoidPos)
	{
		avoidPos = Vector3.zero;
		if (PeSingleton<DraggingMgr>.Instance == null || entity == null)
		{
			return false;
		}
		if (PeSingleton<DraggingMgr>.Instance.IsDragging())
		{
			if (!(PeSingleton<DraggingMgr>.Instance.Dragable is ItemObjDragging itemObjDragging))
			{
				return false;
			}
			if (itemObjDragging == null || itemObjDragging.DragBase == null || itemObjDragging.DragBase.rootGameObject == null || null == itemObjDragging.DragBase.itemBounds)
			{
				return false;
			}
			Bounds bounds = new Bounds(entity.position, entity.peTrans.bound.size);
			Bounds bounds2 = new Bounds(itemObjDragging.DragBase.rootGameObject.transform.position, itemObjDragging.DragBase.itemBounds.worldBounds.size);
			avoidPos = itemObjDragging.DragBase.rootGameObject.transform.position;
			return bounds.Intersects(bounds2);
		}
		return false;
	}

	public static bool CheckCreation(PeEntity entity)
	{
		if (Time.frameCount != _nFrmOfDcAgents)
		{
			_dcAgents = SceneMan.GetSceneObjs<DragCreationAgent>();
			_nFrmOfDcAgents = Time.frameCount;
		}
		if (_dcAgents != null && _dcAgents.Count > 0)
		{
			Bounds bounds = new Bounds(entity.position, entity.peTrans.bound.size);
			for (int i = 0; i < _dcAgents.Count; i++)
			{
				if (_dcAgents[i] is DragCreationAgent dragCreationAgent && dragCreationAgent.peTrans != null)
				{
					Bounds bounds2 = new Bounds(dragCreationAgent.position, dragCreationAgent.peTrans.bound.size);
					bounds2.Expand(5f);
					if (bounds2.size != Vector3.zero && bounds2.Intersects(bounds))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool CheckCreation(PeEntity entity, out Vector3 avoidPos)
	{
		avoidPos = Vector3.zero;
		if (Time.frameCount != _nFrmOfDcAgents)
		{
			_dcAgents = SceneMan.GetSceneObjs<DragCreationAgent>();
			_nFrmOfDcAgents = Time.frameCount;
		}
		if (_dcAgents != null && _dcAgents.Count > 0)
		{
			Bounds bounds = new Bounds(entity.position, entity.peTrans.bound.size);
			for (int i = 0; i < _dcAgents.Count; i++)
			{
				if (_dcAgents[i] is DragCreationAgent dragCreationAgent && dragCreationAgent.peTrans != null)
				{
					Bounds bounds2 = new Bounds(dragCreationAgent.position, dragCreationAgent.peTrans.bound.size);
					bounds2.Expand(5f);
					avoidPos = dragCreationAgent.position;
					if (bounds2.size != Vector3.zero && bounds2.Intersects(bounds))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
