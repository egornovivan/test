using System;
using UnityEngine;

public class AIUtility
{
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

	public static Vector3 GetNextPosition(Vector3 v, Vector3 velocity, float height)
	{
		LayerMask layerMask = (1 << LayerMask.NameToLayer("VFVoxelTerrain")) | (1 << LayerMask.NameToLayer("SceneStatic")) | (1 << LayerMask.NameToLayer("Unwalkable"));
		Ray ray = new Ray(v + velocity + Vector3.up * height, -Vector3.up);
		if (Physics.Raycast(ray, out var hitInfo, height * 2f, layerMask))
		{
			return hitInfo.point;
		}
		return Vector3.zero;
	}

	public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, float height, LayerMask layer, int attempt)
	{
		int num = Mathf.Clamp(attempt, 1, 100);
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 vector2 = center + new Vector3(vector.x, 0f, vector.y);
			if (Physics.Raycast(vector2 + Vector3.up * height, -Vector3.up, out var hitInfo, height * 2f, layer))
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
			if (Physics.Raycast(vector2 + Vector3.up * height, -Vector3.up, out var hitInfo, height * 2f, layer))
			{
				return hitInfo.point;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 ToVector3(string vecString)
	{
		if (vecString != string.Empty)
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

	public static bool CheckPositionInRiver(Vector3 position)
	{
		if (Physics.Raycast(position + Vector3.up * 100f, -Vector3.up, out var hitInfo, 105f, 1 << LayerMask.NameToLayer("River")) && position.y < hitInfo.point.y)
		{
			return true;
		}
		return false;
	}

	public static bool CheckPositionInRiver(Vector3 position, out float riverHeight)
	{
		riverHeight = 0f;
		if (Physics.Raycast(position + Vector3.up * 100f, -Vector3.up, out var hitInfo, 105f, 1 << LayerMask.NameToLayer("River")) && position.y < hitInfo.point.y)
		{
			riverHeight = hitInfo.point.y;
			return true;
		}
		return false;
	}

	public static bool CheckPositionInUnderSea(Vector3 position, float seaHeight)
	{
		if (position.y < seaHeight)
		{
			return true;
		}
		return false;
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
}
