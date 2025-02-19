using UnityEngine;

namespace Pathea.Maths;

public static class Phy
{
	public static bool Raycast(Vector3 origin, Vector3 direction, float distance, int maskLayer)
	{
		RaycastHit[] array = Physics.RaycastAll(origin, direction, distance, maskLayer);
		float num = distance;
		bool result = false;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (!raycastHit.collider.isTrigger)
			{
				float magnitude = (raycastHit.point - origin).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					result = true;
				}
			}
		}
		return result;
	}

	public static bool Raycast(Ray ray, ref RaycastHit hit, float distance, int maskLayer)
	{
		RaycastHit[] array = Physics.RaycastAll(ray, distance, maskLayer);
		float num = distance;
		bool result = false;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (!raycastHit.collider.isTrigger)
			{
				float magnitude = (raycastHit.point - ray.origin).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					hit = raycastHit;
					result = true;
				}
			}
		}
		return result;
	}

	public static bool Raycast(Ray ray, out RaycastHit hitInfo, float distance, int maskLayer, Transform ignoreTrans)
	{
		hitInfo = default(RaycastHit);
		RaycastHit[] array = Physics.RaycastAll(ray, distance, maskLayer);
		float num = distance;
		bool result = false;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (!raycastHit.collider.isTrigger && !raycastHit.collider.transform.IsChildOf(ignoreTrans))
			{
				float magnitude = (raycastHit.point - ray.origin).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					hitInfo = raycastHit;
					result = true;
				}
			}
		}
		return result;
	}
}
