using System.Collections.Generic;
using UnityEngine;

public class RSubTerrainChunk : MonoBehaviour
{
	public int m_Index = -1;

	private List<TreeInfo> m_listTrees;

	public Dictionary<int, TreeInfo> m_mapTrees;

	public int XIndex => RSubTerrUtils.IndexToChunkX(m_Index);

	public int ZIndex => RSubTerrUtils.IndexToChunkZ(m_Index);

	public Vector3 wPos => new Vector3((float)XIndex * RSubTerrConstant.ChunkSizeF, 0f, (float)ZIndex * RSubTerrConstant.ChunkSizeF);

	public List<TreeInfo> TreeList => m_listTrees;

	public int TreeCount => m_listTrees.Count;

	public void AddTree(TreeInfo ti)
	{
		m_listTrees.Add(ti);
		int key = RSubTerrUtils.TreeWorldPosToChunkIndex(ti.m_pos, m_Index);
		if (m_mapTrees.TryGetValue(key, out var value))
		{
			value.AttachTi(ti);
		}
		else
		{
			m_mapTrees.Add(key, ti);
		}
	}

	public void DelTree(TreeInfo ti)
	{
		int key = RSubTerrUtils.TreeWorldPosToChunkIndex(ti.m_pos, m_Index);
		if (!TreeInfo.RemoveTiFromDict(m_mapTrees, key, ti))
		{
			Debug.LogError("The tree to del dosen't exist");
		}
		else if (!m_listTrees.Remove(ti))
		{
			Debug.LogError("The tree to del dosen't exist");
		}
	}

	public void Clear()
	{
		m_listTrees.Clear();
		m_mapTrees.Clear();
	}

	private void Awake()
	{
		m_listTrees = new List<TreeInfo>();
		m_mapTrees = new Dictionary<int, TreeInfo>();
	}
}
