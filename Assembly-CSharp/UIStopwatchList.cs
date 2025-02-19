using System.Collections.Generic;
using PeCustom;
using UnityEngine;

public class UIStopwatchList : MonoBehaviour
{
	[SerializeField]
	private UIStopwatchNode NodePrefab;

	[SerializeField]
	private Transform NodeGroup;

	private Dictionary<int, UIStopwatchNode> nodes;

	private bool mgrInited
	{
		get
		{
			if (PeCustomScene.Self == null)
			{
				return false;
			}
			if (PeCustomScene.Self.scenario == null)
			{
				return false;
			}
			if (PeCustomScene.Self.scenario.stopwatchMgr == null)
			{
				return false;
			}
			return true;
		}
	}

	private StopwatchMgr mgr => PeCustomScene.Self.scenario.stopwatchMgr;

	private void Start()
	{
		nodes = new Dictionary<int, UIStopwatchNode>();
	}

	private void Update()
	{
		if (!mgrInited)
		{
			return;
		}
		if (mgr.stopwatches.Count > 0)
		{
			foreach (KeyValuePair<int, Stopwatch> stopwatch in mgr.stopwatches)
			{
				if (!nodes.ContainsKey(stopwatch.Key))
				{
					UIStopwatchNode uIStopwatchNode = Object.Instantiate(NodePrefab);
					uIStopwatchNode.transform.parent = NodeGroup;
					uIStopwatchNode.transform.localScale = Vector3.one;
					uIStopwatchNode.List = this;
					uIStopwatchNode.StopwatchId = stopwatch.Key;
					uIStopwatchNode.Index = nodes.Count;
					uIStopwatchNode.gameObject.SetActive(value: true);
					nodes.Add(stopwatch.Key, uIStopwatchNode);
				}
			}
		}
		if (nodes.Count <= 0)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<int, UIStopwatchNode> node in nodes)
		{
			node.Value.Index = num++;
		}
	}

	public void DeleteNode(int id)
	{
		if (nodes.ContainsKey(id))
		{
			Object.Destroy(nodes[id].gameObject);
			nodes.Remove(id);
		}
	}
}
