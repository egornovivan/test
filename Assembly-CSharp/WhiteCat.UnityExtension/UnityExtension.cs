using System;
using System.Collections;
using UnityEngine;

namespace WhiteCat.UnityExtension;

public static class UnityExtension
{
	public static T SafeGetComponent<T>(this GameObject gameObject) where T : Component
	{
		T val = gameObject.GetComponent<T>();
		if (!val)
		{
			val = gameObject.AddComponent<T>();
		}
		return val;
	}

	public static void DestroyAllChildren(this Transform parent)
	{
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(parent.GetChild(num).gameObject);
		}
	}

	public static Quaternion TransformRotation(this Transform parent, Quaternion localRotation)
	{
		return Quaternion.LookRotation(parent.TransformVector(localRotation * Vector3.forward), parent.TransformVector(localRotation * Vector3.up));
	}

	public static Quaternion InverseTransformRotation(this Transform parent, Quaternion rotation)
	{
		return Quaternion.LookRotation(parent.InverseTransformVector(rotation * Vector3.forward), parent.InverseTransformVector(rotation * Vector3.up));
	}

	public static void TraverseHierarchy(this Transform root, Action<Transform, int> operate, int depthLimit = -1)
	{
		if (operate == null)
		{
			return;
		}
		operate(root, depthLimit);
		if (depthLimit != 0)
		{
			for (int num = root.childCount - 1; num >= 0; num--)
			{
				root.GetChild(num).TraverseHierarchy(operate, depthLimit - 1);
			}
		}
	}

	public static object TraverseHierarchy(this Transform root, Func<Transform, int, object> operate, int depthLimit = -1)
	{
		if (operate != null)
		{
			object obj = operate(root, depthLimit);
			if (obj != null || depthLimit == 0)
			{
				return obj;
			}
			for (int num = root.childCount - 1; num >= 0; num--)
			{
				obj = root.GetChild(num).TraverseHierarchy(operate, depthLimit - 1);
				if (obj != null)
				{
					return obj;
				}
			}
		}
		return null;
	}

	public static void Invoke(this MonoBehaviour monoBehaviour, float seconds, Action method)
	{
		if (method != null)
		{
			monoBehaviour.StartCoroutine(DelayCall(seconds, method));
		}
	}

	private static IEnumerator DelayCall(float seconds, Action method)
	{
		yield return new WaitForSeconds(seconds);
		method();
	}

	public static float UnitsPerPixel(this Camera camera)
	{
		return camera.orthographicSize * 2f / (float)Screen.height;
	}

	public static float PixelsPerUnit(this Camera camera)
	{
		return (float)Screen.height * 0.5f / camera.orthographicSize;
	}
}
