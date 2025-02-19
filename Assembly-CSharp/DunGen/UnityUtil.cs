using System;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen;

public static class UnityUtil
{
	public static void Destroy(UnityEngine.Object obj)
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(obj);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	public static string GetUniqueName(string name, IEnumerable<string> usedNames)
	{
		if (string.IsNullOrEmpty(name))
		{
			return GetUniqueName("New", usedNames);
		}
		string text = name;
		int result = 0;
		bool flag = false;
		int num = name.LastIndexOf(' ');
		if (num > -1)
		{
			text = name.Substring(0, num);
			flag = int.TryParse(name.Substring(num + 1), out result);
			result++;
		}
		foreach (string usedName in usedNames)
		{
			if (usedName == name)
			{
				if (flag)
				{
					return GetUniqueName(text + " " + result, usedNames);
				}
				return GetUniqueName(name + " 2", usedNames);
			}
		}
		return name;
	}

	public static Bounds CalculateObjectBounds(GameObject obj, bool includeInactive, bool ignoreSpriteRenderers)
	{
		Bounds result = default(Bounds);
		bool flag = false;
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>(includeInactive);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer is MeshRenderer || (renderer is SpriteRenderer && !ignoreSpriteRenderers))
			{
				if (flag)
				{
					result.Encapsulate(renderer.bounds);
				}
				else
				{
					result = renderer.bounds;
				}
				flag = true;
			}
		}
		Collider[] componentsInChildren2 = obj.GetComponentsInChildren<Collider>(includeInactive);
		foreach (Collider collider in componentsInChildren2)
		{
			if (flag)
			{
				result.Encapsulate(collider.bounds);
			}
			else
			{
				result = collider.bounds;
			}
			flag = true;
		}
		return result;
	}

	public static void PositionObjectBySocket(GameObject objectA, GameObject socketA, GameObject socketB)
	{
		PositionObjectBySocket(objectA.transform, socketA.transform, socketB.transform);
	}

	public static void PositionObjectBySocket(Transform objectA, Transform socketA, Transform socketB)
	{
		Quaternion quaternion = Quaternion.LookRotation(-socketB.forward, socketB.up);
		objectA.rotation = quaternion * Quaternion.Inverse(Quaternion.Inverse(objectA.rotation) * socketA.rotation);
		Vector3 position = socketB.position;
		objectA.position = position - (socketA.position - objectA.position);
	}

	public static Vector3 GetCardinalDirection(Vector3 direction, out float magnitude)
	{
		float num = Math.Abs(direction.x);
		float num2 = Math.Abs(direction.y);
		float num3 = Math.Abs(direction.z);
		float num4 = direction.x / num;
		float num5 = direction.y / num2;
		float num6 = direction.z / num3;
		if (num > num2 && num > num3)
		{
			magnitude = num4;
			return new Vector3(num4, 0f, 0f);
		}
		if (num2 > num && num2 > num3)
		{
			magnitude = num5;
			return new Vector3(0f, num5, 0f);
		}
		if (num3 > num && num3 > num2)
		{
			magnitude = num6;
			return new Vector3(0f, 0f, num6);
		}
		magnitude = num4;
		return new Vector3(num4, 0f, 0f);
	}

	public static Vector3 VectorAbs(Vector3 vector)
	{
		return new Vector3(Math.Abs(vector.x), Math.Abs(vector.y), Math.Abs(vector.z));
	}

	public static void SetVector3Masked(ref Vector3 input, Vector3 value, Vector3 mask)
	{
		if (mask.x != 0f)
		{
			input.x = value.x;
		}
		if (mask.y != 0f)
		{
			input.y = value.y;
		}
		if (mask.z != 0f)
		{
			input.z = value.z;
		}
	}

	public static Bounds CondenseBounds(Bounds bounds, IEnumerable<Doorway> doorways)
	{
		Vector3 input = bounds.center - bounds.extents;
		Vector3 input2 = bounds.center + bounds.extents;
		foreach (Doorway doorway in doorways)
		{
			float magnitude;
			Vector3 cardinalDirection = GetCardinalDirection(doorway.transform.forward, out magnitude);
			if (magnitude < 0f)
			{
				SetVector3Masked(ref input, doorway.transform.position, cardinalDirection);
			}
			else
			{
				SetVector3Masked(ref input2, doorway.transform.position, cardinalDirection);
			}
		}
		Vector3 vector = input2 - input;
		Vector3 center = input + vector / 2f;
		return new Bounds(center, vector);
	}

	public static IEnumerable<T> GetComponentsInParents<T>(GameObject obj, bool includeInactive = false) where T : Component
	{
		if (obj.activeSelf || includeInactive)
		{
			T[] components = obj.GetComponents<T>();
			for (int i = 0; i < components.Length; i++)
			{
				yield return components[i];
			}
		}
		if (!(obj.transform.parent != null))
		{
			yield break;
		}
		foreach (T componentsInParent in GetComponentsInParents<T>(obj.transform.parent.gameObject, includeInactive))
		{
			yield return componentsInParent;
		}
	}

	public static T GetComponentInParents<T>(GameObject obj, bool includeInactive = false) where T : Component
	{
		if (obj.activeSelf || includeInactive)
		{
			T[] components = obj.GetComponents<T>();
			int num = 0;
			if (num < components.Length)
			{
				return components[num];
			}
		}
		if (obj.transform.parent != null)
		{
			return GetComponentInParents<T>(obj.transform.parent.gameObject, includeInactive);
		}
		return (T)null;
	}
}
