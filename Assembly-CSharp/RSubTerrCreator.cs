using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RSubTerrCreator : MonoBehaviour
{
	public int LayerIndex;

	public List<TreeInfo> m_allTreesInLayer;

	public TerrainData m_TerrData;

	public Dictionary<int, int> m_mapPrototype;

	[HideInInspector]
	public List<int> m_listPrototype;

	public int _TreePrototypeCount;

	public int _TreeInstanceCount;

	public bool bProcessing;

	private Vector3 _TargetPos = Vector3.zero;

	private void Awake()
	{
		m_allTreesInLayer = new List<TreeInfo>();
		m_mapPrototype = new Dictionary<int, int>();
		m_listPrototype = new List<int>();
	}

	private void Start()
	{
		m_TerrData = new TerrainData();
		m_TerrData.size = RSubTerrConstant.TerrainSize;
		m_TerrData.heightmapResolution = 33;
		m_TerrData.baseMapResolution = 16;
		m_TerrData.alphamapResolution = 16;
		m_TerrData.SetDetailResolution(2, 8);
		Terrain terrain = base.gameObject.AddComponent<Terrain>();
		terrain.terrainData = m_TerrData;
		terrain.editorRenderFlags = (TerrainRenderFlags)(-2);
		terrain.treeDistance = 1024f;
		terrain.treeMaximumFullLODCount = 8192;
		terrain.treeBillboardDistance = RSubTerrainMgr.Instance.Layers[LayerIndex].BillboardDist.Level(SystemSettingData.Instance.treeLevel);
		terrain.treeCrossFadeLength = RSubTerrainMgr.Instance.Layers[LayerIndex].BillboardFadeLen.Level(SystemSettingData.Instance.treeLevel);
		TerrainCollider terrainCollider = base.gameObject.AddComponent<TerrainCollider>();
		terrainCollider.terrainData = m_TerrData;
	}

	private void Update()
	{
		if (!(RSubTerrainMgr.Instance == null))
		{
			Terrain component = base.gameObject.GetComponent<Terrain>();
			if (component != null)
			{
				component.treeBillboardDistance = RSubTerrainMgr.Instance.Layers[LayerIndex].BillboardDist.Level(SystemSettingData.Instance.treeLevel);
				component.treeCrossFadeLength = RSubTerrainMgr.Instance.Layers[LayerIndex].BillboardFadeLen.Level(SystemSettingData.Instance.treeLevel);
			}
			bool flag = false;
			if ((base.transform.position - RSubTerrainMgr.Instance.TerrainPos).magnitude > 1f)
			{
				flag = true;
			}
			if (!bProcessing && flag)
			{
				_TargetPos = RSubTerrainMgr.Instance.TerrainPos;
				StartCoroutine("RefreshRegion");
			}
		}
	}

	private void OnDestroy()
	{
		if (m_TerrData != null)
		{
			Object.Destroy(m_TerrData);
			m_TerrData = null;
		}
		if (m_allTreesInLayer != null)
		{
			m_allTreesInLayer.Clear();
			m_allTreesInLayer = null;
		}
	}

	private IEnumerator RefreshRegion()
	{
		bProcessing = true;
		yield return 0;
		m_allTreesInLayer.Clear();
		m_mapPrototype.Clear();
		m_listPrototype.Clear();
		List<int> cnk_list_to_render = RSubTerrainMgr.Instance.ChunkListToRender();
		int cnk_count = cnk_list_to_render.Count;
		for (int i = 0; i < cnk_count; i++)
		{
			int idx = cnk_list_to_render[i];
			List<TreeInfo> trees_in_zone = RSubTerrainMgr.ReadChunk(idx).TreeList;
			foreach (TreeInfo ti in trees_in_zone)
			{
				float height = RSubTerrainMgr.Instance.GlobalPrototypeBounds[ti.m_protoTypeIdx].extents.y * 2f;
				if (RSubTerrainMgr.Instance.Layers[LayerIndex].MinTreeHeight <= height && height < RSubTerrainMgr.Instance.Layers[LayerIndex].MaxTreeHeight)
				{
					m_allTreesInLayer.Add(ti);
					if (!m_mapPrototype.ContainsKey(ti.m_protoTypeIdx))
					{
						int next_index = m_listPrototype.Count;
						m_mapPrototype.Add(ti.m_protoTypeIdx, next_index);
						m_listPrototype.Add(ti.m_protoTypeIdx);
					}
				}
			}
		}
		TreePrototype[] FinalPrototypeArray = new TreePrototype[m_listPrototype.Count];
		for (int j = 0; j < m_listPrototype.Count; j++)
		{
			FinalPrototypeArray[j] = new TreePrototype();
			FinalPrototypeArray[j].bendFactor = RSubTerrainMgr.Instance.GlobalPrototypeBendFactorList[m_listPrototype[j]];
			FinalPrototypeArray[j].prefab = RSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_listPrototype[j]];
		}
		yield return 0;
		int tree_cnt = m_allTreesInLayer.Count;
		TreeInstance[] FinalTreeInstanceArray = new TreeInstance[tree_cnt];
		for (int t = 0; t < tree_cnt; t++)
		{
			TreeInfo ti2 = m_allTreesInLayer[t];
			Vector3 new_pos = ti2.m_pos - _TargetPos;
			new_pos.x /= RSubTerrConstant.TerrainSize.x;
			new_pos.y /= RSubTerrConstant.TerrainSize.y;
			new_pos.z /= RSubTerrConstant.TerrainSize.z;
			if (m_mapPrototype.ContainsKey(ti2.m_protoTypeIdx))
			{
				FinalTreeInstanceArray[t].color = ti2.m_clr;
				FinalTreeInstanceArray[t].heightScale = ti2.m_heightScale;
				FinalTreeInstanceArray[t].widthScale = ti2.m_widthScale;
				FinalTreeInstanceArray[t].lightmapColor = ti2.m_lightMapClr;
				FinalTreeInstanceArray[t].position = new_pos;
				FinalTreeInstanceArray[t].prototypeIndex = m_mapPrototype[ti2.m_protoTypeIdx];
			}
		}
		yield return 0;
		base.gameObject.SetActive(value: false);
		m_TerrData.treeInstances = new TreeInstance[0];
		m_TerrData.treePrototypes = FinalPrototypeArray;
		m_TerrData.treeInstances = FinalTreeInstanceArray;
		if (Application.isEditor)
		{
			_TreePrototypeCount = m_TerrData.treePrototypes.Length;
			_TreeInstanceCount = m_TerrData.treeInstances.Length;
		}
		base.transform.position = _TargetPos;
		base.gameObject.SetActive(value: true);
		bProcessing = false;
	}
}
