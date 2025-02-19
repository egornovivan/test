using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class TreeInfo
{
	private static Stack<TreeInfo> _tiPool;

	public Color m_clr;

	public float m_heightScale;

	public Vector3 m_pos;

	public int m_protoTypeIdx;

	public float m_widthScale;

	public Color m_lightMapClr;

	private TreeInfo _next;

	public TreeInfo Next => _next;

	private TreeInfo()
	{
	}

	static TreeInfo()
	{
		_tiPool = new Stack<TreeInfo>();
		for (int i = 0; i < 65536; i++)
		{
			_tiPool.Push(new TreeInfo());
		}
	}

	public static TreeInfo GetTI()
	{
		if (_tiPool.Count > 0)
		{
			TreeInfo treeInfo;
			lock (_tiPool)
			{
				treeInfo = _tiPool.Pop();
			}
			treeInfo._next = null;
			return treeInfo;
		}
		return new TreeInfo();
	}

	public static void FreeTI(TreeInfo ti)
	{
		lock (_tiPool)
		{
			_tiPool.Push(ti);
		}
	}

	public static void FreeTIs(List<TreeInfo> tis)
	{
		int count = tis.Count;
		lock (_tiPool)
		{
			for (int i = 0; i < count; i++)
			{
				_tiPool.Push(tis[i]);
			}
		}
	}

	public void AttachTi(TreeInfo ti)
	{
		TreeInfo treeInfo = this;
		while (treeInfo._next != null)
		{
			treeInfo = treeInfo._next;
		}
		treeInfo._next = ti;
	}

	public TreeInfo RemoveTi(TreeInfo ti)
	{
		if (ti == this)
		{
			return _next;
		}
		TreeInfo treeInfo = this;
		while (treeInfo._next != null)
		{
			if (treeInfo._next == ti)
			{
				treeInfo._next = ti._next;
				break;
			}
			treeInfo = treeInfo._next;
		}
		return this;
	}

	public TreeInfo FindTi(Vector3 posInTile)
	{
		for (TreeInfo treeInfo = this; treeInfo != null; treeInfo = treeInfo._next)
		{
			Vector3 pos = treeInfo.m_pos;
			pos.x *= 256f;
			pos.y *= 3000f;
			pos.z *= 256f;
			float magnitude = (posInTile - pos).magnitude;
			if (magnitude < 0.1f)
			{
				return treeInfo;
			}
		}
		return null;
	}

	public static void AddTiToList(List<TreeInfo> tis, TreeInfo ti)
	{
		for (TreeInfo treeInfo = ti; treeInfo != null; treeInfo = treeInfo._next)
		{
			tis.Add(treeInfo);
		}
	}

	public static bool RemoveTiFromDict(Dictionary<int, TreeInfo> dict, int key, TreeInfo ti)
	{
		if (dict.TryGetValue(key, out var value))
		{
			TreeInfo treeInfo = value.RemoveTi(ti);
			if (treeInfo == null)
			{
				dict.Remove(key);
			}
			else if (treeInfo != value)
			{
				dict[key] = treeInfo;
			}
			return true;
		}
		return false;
	}

	public bool IsTree()
	{
		GameObject gameObject = null;
		if (PeGameMgr.IsStory)
		{
			gameObject = LSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_protoTypeIdx];
		}
		else if (PeGameMgr.IsAdventure)
		{
			gameObject = RSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_protoTypeIdx];
		}
		if (gameObject == null)
		{
			return false;
		}
		CapsuleCollider component = gameObject.GetComponent<CapsuleCollider>();
		if (component != null)
		{
			return true;
		}
		return false;
	}

	public bool IsDoubleFoot(out Vector3[] footsPos, Vector3 worldPos, Vector3 localScale)
	{
		footsPos = new Vector3[2];
		GameObject gameObject = null;
		if (PeGameMgr.IsStory)
		{
			gameObject = LSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_protoTypeIdx];
		}
		else if (PeGameMgr.IsAdventure)
		{
			gameObject = RSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_protoTypeIdx];
		}
		if (gameObject == null)
		{
			return false;
		}
		CapsuleCollider component = gameObject.GetComponent<CapsuleCollider>();
		BoxCollider component2 = gameObject.GetComponent<BoxCollider>();
		ref Vector3 reference = ref footsPos[0];
		reference = worldPos + new Vector3(component.center.x * localScale.x, component.center.y * localScale.y, component.center.z * localScale.z);
		if (component2 != null)
		{
			ref Vector3 reference2 = ref footsPos[1];
			reference2 = worldPos + new Vector3(component2.center.x * localScale.x, component2.center.y * localScale.y, component2.center.z * localScale.z);
			return true;
		}
		return false;
	}
}
