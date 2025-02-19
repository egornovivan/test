using System.Collections.Generic;
using uLink;
using UnityEngine;

[AddComponentMenu("uLink Utilities/Instantiate Pool")]
public class uLinkInstantiatePool : uLink.MonoBehaviour
{
	public uLink.NetworkView prefab;

	public int minSize = 50;

	private readonly Stack<uLink.NetworkView> pool = new Stack<uLink.NetworkView>();

	private Transform parent;

	private void Awake()
	{
		if (base.enabled)
		{
			CreatePool();
		}
	}

	private void Start()
	{
	}

	private void OnDisable()
	{
		DestroyPool();
	}

	public void CreatePool()
	{
		if (prefab._manualViewID != 0)
		{
			Debug.LogError("Prefab viewID must be set to Allocated or Unassigned", prefab);
			return;
		}
		parent = new GameObject(base.name + "-Pool").transform;
		for (int i = 0; i < minSize; i++)
		{
			uLink.NetworkView networkView = Object.Instantiate(prefab);
			SetActive(networkView, value: false);
			networkView.transform.parent = parent;
			pool.Push(networkView);
		}
		NetworkInstantiator.Add(prefab.name, Creator, Destroyer);
	}

	public void DestroyPool()
	{
		NetworkInstantiator.Remove(prefab.name);
		pool.Clear();
		if (parent != null)
		{
			Object.Destroy(parent.gameObject);
			parent = null;
		}
	}

	private uLink.NetworkView Creator(string prefabName, NetworkInstantiateArgs args, uLink.NetworkMessageInfo info)
	{
		uLink.NetworkView networkView;
		if (pool.Count > 0)
		{
			networkView = pool.Pop();
			args.SetupNetworkView(networkView);
			SetActive(networkView, value: true);
		}
		else
		{
			networkView = NetworkInstantiatorUtility.Instantiate(prefab, args);
		}
		NetworkInstantiatorUtility.BroadcastOnNetworkInstantiate(networkView, info);
		return networkView;
	}

	private void Destroyer(uLink.NetworkView instance)
	{
		SetActive(instance, value: false);
		pool.Push(instance);
	}

	private static void SetActive(uLink.NetworkView instance, bool value)
	{
		instance.gameObject.SetActive(value);
	}
}
