using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSubTerrCreator : MonoBehaviour
{
	public delegate void VoidNotify();

	public int LayerIndex;

	public int xIndex = -1;

	public int zIndex = -1;

	public bool bProcessing;

	public bool bBillboardProcessing;

	public Dictionary<int, List<TreeInfo>> m_allTreesInLayer;

	public TerrainData m_TerrData;

	public Dictionary<int, int> m_mapPrototype;

	[HideInInspector]
	public List<int> m_listPrototype;

	public int _TreePrototypeCount;

	public int _TreeInstanceCount;

	private Dictionary<int, BillboardTerrain> m_BillboardTerrains = new Dictionary<int, BillboardTerrain>();

	private int Last_x;

	private int Last_z;

	public static event VoidNotify OnRefreshRegion;

	public void AddTreeBatch(int index, List<TreeInfo> tree_list)
	{
		if (m_allTreesInLayer != null)
		{
			if (m_allTreesInLayer.ContainsKey(index))
			{
				Debug.LogError("Adding a batch of tree in this layer, but the index already exist in map, it will be replaced!");
				m_allTreesInLayer[index].Clear();
				m_allTreesInLayer[index] = tree_list;
			}
			else
			{
				m_allTreesInLayer.Add(index, tree_list);
			}
		}
	}

	public void DelTreeBatch(int index)
	{
		if (m_allTreesInLayer != null && m_allTreesInLayer.ContainsKey(index))
		{
			m_allTreesInLayer[index].Clear();
			m_allTreesInLayer.Remove(index);
		}
	}

	private void Awake()
	{
		m_allTreesInLayer = new Dictionary<int, List<TreeInfo>>();
		m_mapPrototype = new Dictionary<int, int>();
		m_listPrototype = new List<int>();
	}

	private void Start()
	{
		m_TerrData = new TerrainData();
		m_TerrData.size = new Vector3(768f, 3000f, 768f);
		m_TerrData.heightmapResolution = 33;
		m_TerrData.baseMapResolution = 16;
		m_TerrData.alphamapResolution = 16;
		m_TerrData.SetDetailResolution(2, 8);
		Terrain terrain = base.gameObject.AddComponent<Terrain>();
		terrain.terrainData = m_TerrData;
		terrain.editorRenderFlags = (TerrainRenderFlags)(-2);
		terrain.treeDistance = 1024f;
		terrain.treeMaximumFullLODCount = 8192;
		terrain.treeBillboardDistance = LSubTerrainMgr.Instance.Layers[LayerIndex].BillboardDist.Level(SystemSettingData.Instance.treeLevel);
		terrain.treeCrossFadeLength = LSubTerrainMgr.Instance.Layers[LayerIndex].BillboardFadeLen.Level(SystemSettingData.Instance.treeLevel);
		terrain.gameObject.layer = 14;
		TerrainCollider terrainCollider = base.gameObject.AddComponent<TerrainCollider>();
		terrainCollider.terrainData = m_TerrData;
	}

	private void Update()
	{
		if (!(LSubTerrainMgr.Instance == null))
		{
			IntVector3 cameraPos = LSubTerrainMgr.CameraPos;
			Terrain component = base.gameObject.GetComponent<Terrain>();
			if (component != null && LSubTerrainMgr.Instance != null)
			{
				component.treeBillboardDistance = LSubTerrainMgr.Instance.Layers[LayerIndex].BillboardDist.Level(SystemSettingData.Instance.treeLevel);
				component.treeCrossFadeLength = LSubTerrainMgr.Instance.Layers[LayerIndex].BillboardFadeLen.Level(SystemSettingData.Instance.treeLevel);
			}
			if (!bProcessing && !bBillboardProcessing && (cameraPos.x != xIndex || cameraPos.z != zIndex))
			{
				Last_x = xIndex;
				Last_z = zIndex;
				StartCoroutine("RefreshRegion");
				xIndex = cameraPos.x;
				zIndex = cameraPos.z;
			}
		}
	}

	private void OnDestroy()
	{
		if (m_TerrData != null)
		{
			UnityEngine.Object.Destroy(m_TerrData);
			m_TerrData = null;
		}
		if (m_allTreesInLayer == null)
		{
			return;
		}
		foreach (KeyValuePair<int, List<TreeInfo>> item in m_allTreesInLayer)
		{
			item.Value.Clear();
		}
		m_allTreesInLayer.Clear();
		m_allTreesInLayer = null;
	}

	private IEnumerator RefreshBillboards()
	{
		if (LSubTerrainMgr.Instance.Layers[LayerIndex].MaxTreeHeight < 50f)
		{
			bBillboardProcessing = false;
			yield break;
		}
		bBillboardProcessing = true;
		List<int> del_list = new List<int>();
		foreach (KeyValuePair<int, BillboardTerrain> kvp in m_BillboardTerrains)
		{
			IntVector3 pos = LSubTerrUtils.IndexToPos(kvp.Key);
			long tempIndexX = pos.x - xIndex;
			long tempIndexZ = pos.z - zIndex;
			if (Math.Abs(tempIndexX) > 3 || Math.Abs(tempIndexZ) > 3 || (Math.Abs(tempIndexX) <= 1 && Math.Abs(tempIndexZ) <= 1))
			{
				kvp.Value.Reset();
				UnityEngine.Object.Destroy(kvp.Value.gameObject);
				del_list.Add(kvp.Key);
			}
		}
		foreach (int del in del_list)
		{
			m_BillboardTerrains.Remove(del);
		}
		for (int x = Last_x - 1; x <= Last_x + 1; x++)
		{
			for (int z = Last_z - 1; z <= Last_z + 1; z++)
			{
				if ((x < xIndex - 1 || x > xIndex + 1 || z < zIndex - 1 || z > zIndex + 1) && x <= xIndex + 3 && x >= xIndex - 3 && z <= zIndex + 3 && z >= zIndex - 3)
				{
					int idx = LSubTerrUtils.PosToIndex(x, z);
					while (!m_allTreesInLayer.ContainsKey(idx) && LSubTerrainMgr.Node(idx) != null && LSubTerrainMgr.Node(idx).HasData)
					{
						yield return 0;
					}
					if (m_allTreesInLayer.ContainsKey(idx))
					{
						CreateOneBTerrain(x, z);
					}
				}
			}
		}
		yield return 0;
		for (int i = xIndex - 3; i <= xIndex + 3; i++)
		{
			for (int j = zIndex - 3; j <= zIndex + 3; j++)
			{
				if ((i < xIndex - 1 || i > xIndex + 1 || j < zIndex - 1 || j > zIndex + 1) && (i < Last_x - 1 || i > Last_x + 1 || j < Last_z - 1 || j > Last_z + 1))
				{
					int idx2 = LSubTerrUtils.PosToIndex(i, j);
					while (!m_allTreesInLayer.ContainsKey(idx2) && LSubTerrainMgr.Node(idx2) != null && LSubTerrainMgr.Node(idx2).HasData)
					{
						yield return 0;
					}
					if (m_allTreesInLayer.ContainsKey(idx2))
					{
						CreateOneBTerrain(i, j);
					}
				}
			}
			yield return 0;
		}
		Last_x = xIndex;
		Last_z = zIndex;
		bBillboardProcessing = false;
	}

	private void CreateOneBTerrain(int x, int z)
	{
		int key = LSubTerrUtils.PosToIndex(x, z);
		List<TreeInfo> trees = m_allTreesInLayer[key];
		BillboardTerrain billboardTerrain = UnityEngine.Object.Instantiate(LSubTerrainMgr.Instance.BTerrainRes);
		billboardTerrain.transform.parent = LSubTerrainMgr.Instance.BTerrainGroup.transform;
		billboardTerrain.gameObject.name = "BTerrain [" + x + " " + z + "]";
		billboardTerrain.transform.position = new Vector3((float)x * 256f, 0f, (float)z * 256f);
		billboardTerrain.transform.localScale = Vector3.one;
		billboardTerrain.xPos = x;
		billboardTerrain.zPos = z;
		billboardTerrain.m_Center = new Vector3(((float)xIndex + 0.5f) * 256f, 0f, ((float)zIndex + 0.5f) * 256f);
		if (m_BillboardTerrains.ContainsKey(key))
		{
			m_BillboardTerrains[key].Reset();
			UnityEngine.Object.Destroy(m_BillboardTerrains[key].gameObject);
			m_BillboardTerrains.Remove(key);
		}
		billboardTerrain.SetTrees(trees);
		m_BillboardTerrains.Add(key, billboardTerrain);
	}

	private IEnumerator RefreshRegion()
	{
		bProcessing = true;
		yield return 0;
		m_mapPrototype.Clear();
		m_listPrototype.Clear();
		for (int x = xIndex - 1; x <= xIndex + 1; x++)
		{
			for (int z = zIndex - 1; z <= zIndex + 1; z++)
			{
				int idx = LSubTerrUtils.PosToIndex(x, z);
				while (!m_allTreesInLayer.ContainsKey(idx) && LSubTerrainMgr.Node(idx) != null && LSubTerrainMgr.Node(idx).HasData)
				{
					yield return 0;
				}
				if (!m_allTreesInLayer.ContainsKey(idx))
				{
					continue;
				}
				List<TreeInfo> trees_in_zone = m_allTreesInLayer[idx];
				foreach (TreeInfo ti in trees_in_zone)
				{
					if (!m_mapPrototype.ContainsKey(ti.m_protoTypeIdx))
					{
						int next_index = m_listPrototype.Count;
						m_mapPrototype.Add(ti.m_protoTypeIdx, next_index);
						m_listPrototype.Add(ti.m_protoTypeIdx);
					}
				}
			}
			yield return 0;
		}
		TreePrototype[] FinalPrototypeArray = new TreePrototype[m_listPrototype.Count];
		for (int i = 0; i < m_listPrototype.Count; i++)
		{
			FinalPrototypeArray[i] = new TreePrototype();
			FinalPrototypeArray[i].bendFactor = LSubTerrainMgr.Instance.GlobalPrototypeBendFactorList[m_listPrototype[i]];
			FinalPrototypeArray[i].prefab = LSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_listPrototype[i]];
		}
		int tree_count = 0;
		for (int j = xIndex - 1; j <= xIndex + 1; j++)
		{
			for (int k = zIndex - 1; k <= zIndex + 1; k++)
			{
				int idx2 = LSubTerrUtils.PosToIndex(j, k);
				if (m_allTreesInLayer.ContainsKey(idx2))
				{
					tree_count += m_allTreesInLayer[idx2].Count;
				}
			}
		}
		TreeInstance[] FinalTreeInstanceArray = new TreeInstance[tree_count];
		int t = 0;
		for (int l = xIndex - 1; l <= xIndex + 1; l++)
		{
			for (int m = zIndex - 1; m <= zIndex + 1; m++)
			{
				int idx3 = LSubTerrUtils.PosToIndex(l, m);
				if (!m_allTreesInLayer.ContainsKey(idx3))
				{
					continue;
				}
				List<TreeInfo> trees_in_zone2 = m_allTreesInLayer[idx3];
				foreach (TreeInfo ti2 in trees_in_zone2)
				{
					if (t < FinalTreeInstanceArray.Length)
					{
						Vector3 new_pos = ti2.m_pos;
						new_pos += new Vector3(l - xIndex + 1, 0f, m - zIndex + 1);
						new_pos.x /= 3f;
						new_pos.z /= 3f;
						FinalTreeInstanceArray[t].color = ti2.m_clr;
						FinalTreeInstanceArray[t].heightScale = ti2.m_heightScale;
						FinalTreeInstanceArray[t].widthScale = ti2.m_widthScale;
						FinalTreeInstanceArray[t].lightmapColor = ti2.m_lightMapClr;
						FinalTreeInstanceArray[t].position = new_pos;
						if (!m_mapPrototype.ContainsKey(ti2.m_protoTypeIdx))
						{
							FinalTreeInstanceArray[t].heightScale = 0f;
							FinalTreeInstanceArray[t].widthScale = 0f;
							FinalTreeInstanceArray[t].position = Vector3.zero;
							FinalTreeInstanceArray[t].prototypeIndex = 0;
							continue;
						}
						FinalTreeInstanceArray[t].prototypeIndex = m_mapPrototype[ti2.m_protoTypeIdx];
					}
					t++;
				}
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
		base.transform.position = LSubTerrUtils.PosToWorldPos(new IntVector3(xIndex - 1, 0, zIndex - 1));
		base.gameObject.SetActive(value: true);
		bProcessing = false;
		if (LSubTerrCreator.OnRefreshRegion != null)
		{
			LSubTerrCreator.OnRefreshRegion();
		}
		StartCoroutine("RefreshBillboards");
	}
}
