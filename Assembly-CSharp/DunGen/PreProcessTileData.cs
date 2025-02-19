using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DunGen;

public sealed class PreProcessTileData
{
	public readonly List<GameObject> ProxySockets = new List<GameObject>();

	public readonly List<DoorwaySocketType> DoorwaySockets = new List<DoorwaySocketType>();

	public readonly List<Doorway> Doorways = new List<Doorway>();

	public static Type ProBuilderObjectType { get; private set; }

	public GameObject Prefab { get; private set; }

	public GameObject Proxy { get; private set; }

	public PreProcessTileData(GameObject prefab, bool ignoreSpriteRendererBounds, Vector3 upVector)
	{
		Prefab = prefab;
		Proxy = new GameObject(prefab.name + "_PROXY");
		prefab.transform.position = Vector3.zero;
		prefab.transform.rotation = Quaternion.identity;
		GetAllDoorways();
		foreach (Doorway doorway in Doorways)
		{
			GameObject gameObject = new GameObject("ProxyDoor");
			gameObject.transform.position = doorway.transform.position;
			gameObject.transform.rotation = doorway.transform.rotation;
			gameObject.transform.parent = Proxy.transform;
			ProxySockets.Add(gameObject);
		}
		CalculateProxyBounds(ignoreSpriteRendererBounds, upVector);
	}

	public static void FindProBuilderObjectType()
	{
		if (ProBuilderObjectType != null)
		{
			return;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.Contains("ProBuilder"))
			{
				ProBuilderObjectType = assembly.GetType("pb_Object");
				if (ProBuilderObjectType != null)
				{
					break;
				}
			}
		}
	}

	public bool ChooseRandomDoorway(System.Random random, DoorwaySocketType? socketGroupFilter, Vector3? allowedDirection, out int doorwayIndex, out Doorway doorway)
	{
		doorwayIndex = -1;
		doorway = null;
		IEnumerable<Doorway> source = Doorways;
		if (socketGroupFilter.HasValue)
		{
			source = source.Where((Doorway x) => DoorwaySocket.IsMatchingSocket(x.SocketGroup, socketGroupFilter.Value));
		}
		if (allowedDirection.HasValue)
		{
			source = source.Where((Doorway x) => x.transform.forward == allowedDirection.GetValueOrDefault() && allowedDirection.HasValue);
		}
		if (source.Count() == 0)
		{
			return false;
		}
		doorway = source.ElementAt(random.Next(0, source.Count()));
		doorwayIndex = Doorways.IndexOf(doorway);
		return true;
	}

	private void CalculateProxyBounds(bool ignoreSpriteRendererBounds, Vector3 upVector)
	{
		Bounds bounds = UnityUtil.CalculateObjectBounds(Prefab, includeInactive: true, ignoreSpriteRendererBounds);
		if (ProBuilderObjectType != null)
		{
			Component[] componentsInChildren = Prefab.GetComponentsInChildren(ProBuilderObjectType);
			foreach (Component obj in componentsInChildren)
			{
				Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				Vector3[] array = (Vector3[])ProBuilderObjectType.GetProperty("vertices").GetValue(obj, null);
				Vector3[] array2 = array;
				foreach (Vector3 rhs in array2)
				{
					vector = Vector3.Min(vector, rhs);
					vector2 = Vector3.Max(vector2, rhs);
				}
				Vector3 vector3 = Prefab.transform.TransformDirection(vector2 - vector);
				Vector3 center = Prefab.transform.TransformPoint(vector) + vector3 / 2f;
				bounds.Encapsulate(new Bounds(center, vector3));
			}
		}
		bounds = UnityUtil.CondenseBounds(bounds, Prefab.GetComponentsInChildren<Doorway>(includeInactive: true));
		bounds.size *= 0.99f;
		BoxCollider boxCollider = Proxy.AddComponent<BoxCollider>();
		boxCollider.center = bounds.center;
		boxCollider.size = bounds.size;
	}

	private void GetAllDoorways()
	{
		DoorwaySockets.Clear();
		Doorway[] componentsInChildren = Prefab.GetComponentsInChildren<Doorway>(includeInactive: true);
		foreach (Doorway doorway in componentsInChildren)
		{
			Doorways.Add(doorway);
			if (!DoorwaySockets.Contains(doorway.SocketGroup))
			{
				DoorwaySockets.Add(doorway.SocketGroup);
			}
		}
	}
}
