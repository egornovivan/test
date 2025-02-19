using System;
using UnityEngine;

namespace AiAsset;

public class AiMath
{
	public static bool Raycast(Vector3 start, Vector3 end, LayerMask layer)
	{
		float maxDistance = Vector3.Distance(start, end);
		Vector3 direction = start - end;
		return Physics.Raycast(start, direction, maxDistance, layer);
	}

	public static bool Raycast(Vector3 start, Vector3 end, out RaycastHit hitInfo, LayerMask layer)
	{
		float maxDistance = Vector3.Distance(start, end);
		Vector3 direction = start - end;
		return Physics.Raycast(start, direction, out hitInfo, maxDistance, layer);
	}

	public static Vector3 ProjectOntoPlane(Vector3 v, Vector3 normal)
	{
		return v - Vector3.Project(v, normal);
	}

	public static float ProjectDistance(Vector3 v, Vector3 normal)
	{
		return (v - Vector3.Project(v, normal)).magnitude;
	}

	public static float ProjectDistance(Vector3 position1, Vector3 position2, Vector3 normal)
	{
		Vector3 vector = position2 - position1;
		return (vector - Vector3.Project(vector, normal)).magnitude;
	}

	public static float InverseAngle(Transform local, Vector3 dir)
	{
		if (local == null || dir == Vector3.zero)
		{
			Debug.LogWarning("local || dir is error");
			return 0f;
		}
		Vector3 to = ProjectOntoPlane(dir, local.transform.up);
		return Vector3.Angle(local.forward, to);
	}

	public static float Dot(Transform self, Transform target)
	{
		Vector3 vector = target.position - self.position;
		Vector3 vector2 = Vector3.Project(vector, self.forward);
		Vector3 vector3 = Vector3.Project(vector, self.right);
		return Vector3.Dot((vector2 + vector3).normalized, self.forward);
	}

	public static bool IsNumberic(string message, out int result)
	{
		result = -1;
		try
		{
			result = Convert.ToInt32(message);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
