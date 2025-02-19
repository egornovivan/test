using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RSubTerrainMgr : MonoBehaviour
{
	public const int TreePlaceHolderPrototypeIndex = 63;

	public const int NearTreeLayer = 21;

	public const int NearTreeLayerMask = 2097152;

	private static RSubTerrainMgr s_Instance = null;

	private static List<GlobalTreeInfo> picklist = new List<GlobalTreeInfo>();

	private static Stack<GlobalTreeInfo> globalTreeInfos = new Stack<GlobalTreeInfo>();

	private static IntVector3 temp = new IntVector3();

	private static IntVector3 ipos = new IntVector3();

	private static List<TreeInfo> treelist = new List<TreeInfo>();

	[SerializeField]
	private Bounds m_TerrainRegion;

	private Dictionary<int, RSubTerrainChunk> m_Chunks;

	[SerializeField]
	private bool m_IsDirty;

	private List<int> _lstChnkIdxActive = new List<int>();

	private List<int> _lstChnkIdxToDel = new List<int>();

	public Dictionary<int, List<TreeInfo>> m_map32Trees;

	private Dictionary<int, List<GameObject>> m_mapExistTempTrees;

	private Dictionary<GameObject, TreeInfo> m_mapTempTreeInfos;

	public Transform CameraTransform;

	public Transform PlayerTransform;

	public VoxelEditor VEditor;

	public List<LSubTerrLayerOption> Layers;

	public RSubTerrCreator[] LayerCreators;

	public GameObject ChunkGroup;

	public GameObject TerrainGroup;

	public GameObject PrototypeGroup;

	public GameObject TempTreesGroup;

	public GameObject[] GlobalPrototypePrefabList;

	[HideInInspector]
	public float[] GlobalPrototypeBendFactorList;

	[HideInInspector]
	public Bounds[] GlobalPrototypeBounds;

	public GameObject[] GlobalPrototypeColliders;

	public GameObject[] GlobalPrototypeLights;

	private RTreePlaceHolderInfo[] GlobalPrototypeTPHInfo;

	public static RSubTerrainMgr Instance => s_Instance;

	public Vector3 TerrainPos => m_TerrainRegion.min;

	public bool IsDirty
	{
		get
		{
			return m_IsDirty;
		}
		set
		{
			m_IsDirty = value;
		}
	}

	public static GameObject GO => (!(s_Instance == null)) ? s_Instance.gameObject : null;

	public static IntVec3 CameraIntPos => (!(s_Instance == null) && !(s_Instance.CameraTransform == null)) ? RSubTerrUtils.IndexToChunkPos(RSubTerrUtils.GetChunkIdContainsTree(s_Instance.CameraTransform.position)) : IntVec3.zero;

	public List<int> ChunkListToRender()
	{
		List<int> list = new List<int>();
		int num = Mathf.FloorToInt(m_TerrainRegion.min.x / RSubTerrConstant.ChunkSizeF);
		int num2 = Mathf.FloorToInt(m_TerrainRegion.min.z / RSubTerrConstant.ChunkSizeF);
		for (int i = num; i < num + RSubTerrConstant.ChunkCountPerAxis.x; i++)
		{
			for (int j = num2; j < num2 + RSubTerrConstant.ChunkCountPerAxis.z; j++)
			{
				int num3 = RSubTerrUtils.ChunkPosToIndex(i, j);
				if (m_Chunks.ContainsKey(num3) && m_Chunks[num3].TreeCount > 0)
				{
					list.Add(num3);
				}
			}
		}
		return list;
	}

	public bool IsChunkRendering(int index)
	{
		int num = RSubTerrUtils.IndexToChunkX(index);
		int num2 = RSubTerrUtils.IndexToChunkZ(index);
		return m_TerrainRegion.Contains(new Vector3(((float)num + 0.5f) * RSubTerrConstant.ChunkSizeF, 1f, ((float)num2 + 0.5f) * RSubTerrConstant.ChunkSizeF));
	}

	public static RSubTerrainChunk ReadChunk(int index)
	{
		if (s_Instance == null)
		{
			return null;
		}
		if (s_Instance.m_Chunks.ContainsKey(index))
		{
			return s_Instance.m_Chunks[index];
		}
		return null;
	}

	public void SyncChunksData()
	{
		if (!Monitor.TryEnter(VFDataRTGen.s_dicTreeInfoList))
		{
			return;
		}
		_lstChnkIdxActive.Clear();
		foreach (KeyValuePair<IntVector2, List<TreeInfo>> s_dicTreeInfo in VFDataRTGen.s_dicTreeInfoList)
		{
			int num = RSubTerrUtils.TilePosToIndex(s_dicTreeInfo.Key);
			_lstChnkIdxActive.Add(num);
			if (m_Chunks.ContainsKey(num))
			{
				continue;
			}
			int num2 = RSubTerrUtils.IndexToChunkX(num);
			int num3 = RSubTerrUtils.IndexToChunkZ(num);
			GameObject gameObject = new GameObject("Tile [" + num2 + "," + num3 + "]");
			gameObject.transform.parent = s_Instance.ChunkGroup.transform;
			gameObject.transform.position = new Vector3(((float)num2 + 0.5f) * RSubTerrConstant.ChunkSizeF, 0f, ((float)num3 + 0.5f) * RSubTerrConstant.ChunkSizeF);
			RSubTerrainChunk rSubTerrainChunk = gameObject.AddComponent<RSubTerrainChunk>();
			rSubTerrainChunk.m_Index = num;
			s_Instance.m_Chunks.Add(num, rSubTerrainChunk);
			foreach (TreeInfo item in s_dicTreeInfo.Value)
			{
				bool flag = false;
				if (RSubTerrSL.m_mapDelPos != null)
				{
					for (int i = num2 - 1; i <= num2 + 1; i++)
					{
						for (int j = num3 - 1; j <= num3 + 1; j++)
						{
							int key = RSubTerrUtils.ChunkPosToIndex(i, j);
							if (!RSubTerrSL.m_mapDelPos.ContainsKey(key))
							{
								continue;
							}
							foreach (Vector3 item2 in RSubTerrSL.m_mapDelPos[key])
							{
								float num4 = Mathf.Abs(item2.x - item.m_pos.x) + Mathf.Abs(item2.y - item.m_pos.y) + Mathf.Abs(item2.z - item.m_pos.z);
								if (num4 < 0.05f)
								{
									flag = true;
									break;
								}
							}
						}
					}
				}
				if (!flag)
				{
					rSubTerrainChunk.AddTree(item);
					int key2 = RSubTerrUtils.Tree32PosTo32Index(Mathf.FloorToInt(item.m_pos.x / 32f), Mathf.FloorToInt(item.m_pos.z / 32f));
					if (!m_map32Trees.ContainsKey(key2))
					{
						m_map32Trees.Add(key2, new List<TreeInfo>());
					}
					if (HasCollider(item.m_protoTypeIdx) || HasLight(item.m_protoTypeIdx))
					{
						m_map32Trees[key2].Add(item);
					}
				}
			}
			if (IsChunkRendering(num))
			{
				m_IsDirty = true;
			}
		}
		_lstChnkIdxToDel.Clear();
		foreach (KeyValuePair<int, RSubTerrainChunk> chunk in m_Chunks)
		{
			if (!_lstChnkIdxActive.Contains(chunk.Key))
			{
				_lstChnkIdxToDel.Add(chunk.Key);
			}
		}
		foreach (int item3 in _lstChnkIdxToDel)
		{
			RemoveChunk(item3);
			if (IsChunkRendering(item3))
			{
				m_IsDirty = true;
			}
		}
		Monitor.Exit(VFDataRTGen.s_dicTreeInfoList);
	}

	public void RemoveChunk(int index)
	{
		if (m_Chunks.ContainsKey(index))
		{
			Object.Destroy(m_Chunks[index].gameObject);
			m_Chunks.Remove(index);
			if (IsChunkRendering(index))
			{
				m_IsDirty = true;
			}
			if (m_map32Trees.ContainsKey(index))
			{
				m_map32Trees[index].Clear();
				m_map32Trees.Remove(index);
			}
		}
	}

	public static bool HasCollider(int prototype)
	{
		if (s_Instance == null)
		{
			return false;
		}
		if (s_Instance.GlobalPrototypeColliders[prototype] == null)
		{
			return false;
		}
		if (s_Instance.GlobalPrototypeColliders[prototype].GetComponent<Collider>() != null)
		{
			return true;
		}
		return false;
	}

	public static bool HasLight(int prototype)
	{
		if (s_Instance == null)
		{
			return false;
		}
		if (s_Instance.GlobalPrototypeColliders[prototype] == null)
		{
			return false;
		}
		if (s_Instance.GlobalPrototypeColliders[prototype].GetComponentsInChildren<Light>(includeInactive: true).Length > 0)
		{
			return true;
		}
		return false;
	}

	public static bool HasMultiCollider(int prototype)
	{
		if (s_Instance == null)
		{
			return false;
		}
		if (s_Instance.GlobalPrototypeColliders[prototype] == null)
		{
			return false;
		}
		if (s_Instance.GlobalPrototypeColliders[prototype].GetComponent<BoxCollider>() != null)
		{
			return true;
		}
		return false;
	}

	public static RTreePlaceHolderInfo GetTreePlaceHolderInfo(int prototype)
	{
		if (s_Instance == null)
		{
			return null;
		}
		return s_Instance.GlobalPrototypeTPHInfo[prototype];
	}

	private void Awake()
	{
		Debug.Log("Creating RSubTerrainMgr!");
		if (s_Instance != null)
		{
			Debug.LogError("Can not have a second instance of RSubTerrainMgr !");
		}
		s_Instance = this;
	}

	private void Init()
	{
		GlobalPrototypePrefabList = VEditor.m_treePrototypeList;
		GlobalPrototypeBendFactorList = VEditor.m_treePrototypeBendfactor;
		GlobalPrototypeBounds = new Bounds[GlobalPrototypePrefabList.Length];
		GlobalPrototypeColliders = new GameObject[GlobalPrototypePrefabList.Length];
		GlobalPrototypeLights = new GameObject[GlobalPrototypePrefabList.Length];
		GlobalPrototypeTPHInfo = new RTreePlaceHolderInfo[GlobalPrototypePrefabList.Length];
		for (int i = 0; i < GlobalPrototypeBounds.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(GlobalPrototypePrefabList[i]);
			gameObject.transform.parent = PrototypeGroup.transform;
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
			MeshFilter component2 = gameObject.GetComponent<MeshFilter>();
			Animator component3 = gameObject.GetComponent<Animator>();
			Animation component4 = gameObject.GetComponent<Animation>();
			ref Bounds reference = ref GlobalPrototypeBounds[i];
			reference = component2.mesh.bounds;
			if (i == 63)
			{
				GlobalPrototypeBounds[i].extents = new Vector3(1f, 2f, 1f);
			}
			Object.Destroy(component);
			Object.Destroy(component2);
			if (component3 != null)
			{
				Object.Destroy(component3);
			}
			if (component4 != null)
			{
				Object.Destroy(component4);
			}
			if (gameObject.GetComponent<Collider>() != null || gameObject.GetComponentsInChildren<Light>(includeInactive: true).Length >= 1)
			{
				BoxCollider component5 = gameObject.GetComponent<BoxCollider>();
				if (component5 != null)
				{
					GlobalPrototypeTPHInfo[i] = new RTreePlaceHolderInfo(component5.center, component5.extents.y, component5.extents.x + component5.extents.z);
				}
				else
				{
					GlobalPrototypeTPHInfo[i] = null;
				}
				Collider[] components = gameObject.GetComponents<Collider>();
				gameObject.name = "Prototype [" + i + "]'s Collider";
				gameObject.SetActive(value: false);
				GlobalPrototypeColliders[i] = gameObject;
			}
			else
			{
				Object.Destroy(gameObject);
				GlobalPrototypeColliders[i] = null;
			}
		}
	}

	private void Start()
	{
		Init();
		m_Chunks = new Dictionary<int, RSubTerrainChunk>();
		m_map32Trees = new Dictionary<int, List<TreeInfo>>();
		m_mapExistTempTrees = new Dictionary<int, List<GameObject>>();
		m_mapTempTreeInfos = new Dictionary<GameObject, TreeInfo>();
		m_TerrainRegion = new Bounds(Vector3.zero, Vector3.zero);
		LayerCreators = new RSubTerrCreator[Layers.Count];
		for (int num = Layers.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = new GameObject("[" + num + "] " + Layers[num].Name + " Layer");
			gameObject.transform.parent = TerrainGroup.transform;
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			RSubTerrCreator rSubTerrCreator = gameObject.AddComponent<RSubTerrCreator>();
			rSubTerrCreator.LayerIndex = num;
			LayerCreators[num] = rSubTerrCreator;
		}
	}

	private void Update()
	{
		IntVec3 cameraIntPos = CameraIntPos;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (RSubTerrConstant.ChunkCountPerAxis.x % 2 == 0)
		{
			zero.x = (float)(cameraIntPos.x - (RSubTerrConstant.ChunkCountPerAxis.x / 2 - 1)) * RSubTerrConstant.ChunkSizeF;
		}
		else
		{
			zero.x = (float)(cameraIntPos.x - RSubTerrConstant.ChunkCountPerAxis.x / 2) * RSubTerrConstant.ChunkSizeF;
		}
		if (RSubTerrConstant.ChunkCountPerAxis.z % 2 == 0)
		{
			zero.z = (float)(cameraIntPos.z - (RSubTerrConstant.ChunkCountPerAxis.z / 2 - 1)) * RSubTerrConstant.ChunkSizeF;
		}
		else
		{
			zero.z = (float)(cameraIntPos.z - RSubTerrConstant.ChunkCountPerAxis.z / 2) * RSubTerrConstant.ChunkSizeF;
		}
		zero.y = 0f;
		zero2.x = (float)(cameraIntPos.x + RSubTerrConstant.ChunkCountPerAxis.x / 2 + 1) * RSubTerrConstant.ChunkSizeF;
		zero2.y = RSubTerrConstant.ChunkHeightF;
		zero2.z = (float)(cameraIntPos.z + RSubTerrConstant.ChunkCountPerAxis.z / 2 + 1) * RSubTerrConstant.ChunkSizeF;
		m_TerrainRegion.SetMinMax(zero, zero2);
		if (Application.isEditor)
		{
			Bounds terrainRegion = m_TerrainRegion;
			terrainRegion.center = new Vector3(terrainRegion.center.x, 0f, terrainRegion.center.z);
			terrainRegion.extents = new Vector3(terrainRegion.extents.x, 0f, terrainRegion.extents.z);
			AiUtil.DrawBounds(base.transform, terrainRegion, Color.blue);
		}
		if (PlayerTransform == null)
		{
			PlayerTransform = CameraTransform;
			return;
		}
		if (Time.frameCount % 32 == 0)
		{
			SyncChunksData();
		}
		if (Time.frameCount % 128 == 0 && IsDirty)
		{
			RSubTerrCreator[] layerCreators = LayerCreators;
			foreach (RSubTerrCreator rSubTerrCreator in layerCreators)
			{
				if (!rSubTerrCreator.bProcessing)
				{
					rSubTerrCreator.StartCoroutine("RefreshRegion");
					m_IsDirty = false;
				}
			}
		}
		RefreshTempGOsIn32Meter();
	}

	private void OnDestroy()
	{
		if (m_Chunks != null)
		{
			m_Chunks.Clear();
			m_Chunks = null;
		}
		s_Instance = null;
	}

	public static TreeInfo GetTreeinfo(Collider col)
	{
		if (null == col || null == s_Instance)
		{
			return null;
		}
		if (s_Instance.m_mapTempTreeInfos.ContainsKey(col.gameObject))
		{
			return s_Instance.m_mapTempTreeInfos[col.gameObject];
		}
		return null;
	}

	public static TreeInfo RayCast(Ray ray, float distance, out RaycastHit hitinfo)
	{
		hitinfo = default(RaycastHit);
		if (s_Instance == null)
		{
			return null;
		}
		if (Physics.Raycast(ray, out hitinfo, distance, 2097152))
		{
			if (s_Instance.m_mapTempTreeInfos.ContainsKey(hitinfo.collider.gameObject))
			{
				return s_Instance.m_mapTempTreeInfos[hitinfo.collider.gameObject];
			}
			return null;
		}
		return null;
	}

	public static TreeInfo RayCast(Ray ray, float distance)
	{
		RaycastHit hitinfo;
		return RayCast(ray, distance, out hitinfo);
	}

	public List<TreeInfo> TreesAtPos(IntVector3 pos)
	{
		treelist.Clear();
		int num = Mathf.FloorToInt((float)pos.x / RSubTerrConstant.ChunkSizeF);
		int num2 = Mathf.FloorToInt((float)pos.z / RSubTerrConstant.ChunkSizeF);
		for (int i = num - 1; i <= num + 1; i++)
		{
			for (int j = num2 - 1; j <= num2 + 1; j++)
			{
				int num3 = RSubTerrUtils.ChunkPosToIndex(i, j);
				int key = RSubTerrUtils.TreeWorldPosToChunkIndex(pos.ToVector3(), num3);
				if (m_Chunks.ContainsKey(num3) && m_Chunks[num3].m_mapTrees.TryGetValue(key, out var value))
				{
					TreeInfo.AddTiToList(treelist, value);
				}
			}
		}
		return treelist;
	}

	public static TreeInfo TreesAtPosF(Vector3 pos)
	{
		IntVector3 pos2 = new IntVector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		List<TreeInfo> list = s_Instance.TreesAtPos(pos2);
		foreach (TreeInfo item in list)
		{
			if (Vector3.Distance(item.m_pos, pos) < 0.005f)
			{
				return item;
			}
		}
		return null;
	}

	public static List<GlobalTreeInfo> Picking(Vector3 position, Vector3 direction, bool includeTrees = false, float distance = 1.5f, float angle = 45f)
	{
		if (s_Instance == null)
		{
			return picklist;
		}
		ClearTreeinfo();
		distance = Mathf.Clamp(distance, 0.1f, 2f);
		ipos.x = Mathf.FloorToInt(position.x);
		ipos.y = Mathf.FloorToInt(position.y);
		ipos.z = Mathf.FloorToInt(position.z);
		for (int i = ipos.x - 2; i <= ipos.x + 2; i++)
		{
			for (int j = ipos.y - 2; j <= ipos.y + 2; j++)
			{
				for (int k = ipos.z - 2; k <= ipos.z + 2; k++)
				{
					temp.x = i;
					temp.y = j;
					temp.z = k;
					List<TreeInfo> list = s_Instance.TreesAtPos(temp);
					for (int l = 0; l < list.Count; l++)
					{
						TreeInfo treeInfo = list[l];
						if (HasCollider(treeInfo.m_protoTypeIdx) && !includeTrees)
						{
							continue;
						}
						Vector3 from = treeInfo.m_pos - position;
						from.y = 0f;
						if (!(from.magnitude > distance))
						{
							direction.y = 0f;
							if (Vector3.Angle(from, direction) < angle)
							{
								GlobalTreeInfo globalTreeInfo = GetGlobalTreeInfo();
								globalTreeInfo._terrainIndex = -1;
								globalTreeInfo._treeInfo = treeInfo;
								picklist.Add(globalTreeInfo);
							}
						}
					}
				}
			}
		}
		return picklist;
	}

	public static List<GlobalTreeInfo> Picking(IntVector3 position, bool includeTrees)
	{
		if (s_Instance == null)
		{
			return picklist;
		}
		ClearTreeinfo();
		ipos.x = position.x;
		ipos.y = position.y;
		ipos.z = position.z;
		List<TreeInfo> list = s_Instance.TreesAtPos(ipos);
		for (int i = 0; i < list.Count; i++)
		{
			TreeInfo treeInfo = list[i];
			if (!HasCollider(treeInfo.m_protoTypeIdx) || includeTrees)
			{
				GlobalTreeInfo globalTreeInfo = GetGlobalTreeInfo();
				globalTreeInfo._terrainIndex = -1;
				globalTreeInfo._treeInfo = treeInfo;
				picklist.Add(globalTreeInfo);
			}
		}
		return picklist;
	}

	private static void ClearTreeinfo()
	{
		for (int i = 0; i < picklist.Count; i++)
		{
			globalTreeInfos.Push(picklist[i]);
		}
		picklist.Clear();
	}

	private static GlobalTreeInfo GetGlobalTreeInfo()
	{
		if (globalTreeInfos.Count > 0)
		{
			return globalTreeInfos.Pop();
		}
		return new GlobalTreeInfo(-1, null);
	}

	public static void DeleteTree(GameObject nearTreeGo)
	{
		if (!(s_Instance == null))
		{
			TreeInfo value = null;
			if (s_Instance.m_mapTempTreeInfos.TryGetValue(nearTreeGo, out value))
			{
				DeleteTree(value);
			}
		}
	}

	public static void DeleteTree(TreeInfo treeinfo)
	{
		if (s_Instance == null || treeinfo == null)
		{
			return;
		}
		TreeInfo treeInfo = null;
		int key = RSubTerrUtils.Tree32PosTo32Index(Mathf.FloorToInt(treeinfo.m_pos.x / 32f), Mathf.FloorToInt(treeinfo.m_pos.z / 32f));
		if (s_Instance.m_map32Trees.ContainsKey(key))
		{
			s_Instance.m_map32Trees[key].Remove(treeinfo);
			if (s_Instance.m_map32Trees[key].Count == 0)
			{
				s_Instance.m_map32Trees.Remove(key);
			}
		}
		if (s_Instance.m_mapExistTempTrees.ContainsKey(key))
		{
			GameObject gameObject = null;
			foreach (GameObject item in s_Instance.m_mapExistTempTrees[key])
			{
				if (s_Instance.m_mapTempTreeInfos.ContainsKey(item))
				{
					if (s_Instance.m_mapTempTreeInfos[item] == treeinfo)
					{
						gameObject = item;
						Object.Destroy(item);
						s_Instance.m_mapTempTreeInfos.Remove(item);
					}
				}
				else
				{
					Debug.LogError("Can not find the GameObject key in m_mapTempTreeInfos when delete tree");
				}
			}
			if (gameObject != null)
			{
				s_Instance.m_mapExistTempTrees[key].Remove(gameObject);
			}
		}
		int num = Mathf.FloorToInt(treeinfo.m_pos.x / RSubTerrConstant.ChunkSizeF);
		int num2 = Mathf.FloorToInt(treeinfo.m_pos.z / RSubTerrConstant.ChunkSizeF);
		int num3 = 0;
		for (int i = num - 1; i <= num + 1; i++)
		{
			for (int j = num2 - 1; j <= num2 + 1; j++)
			{
				int num4 = RSubTerrUtils.ChunkPosToIndex(i, j);
				int key2 = RSubTerrUtils.TreeWorldPosToChunkIndex(treeinfo.m_pos, num4);
				if (s_Instance.m_Chunks.ContainsKey(num4))
				{
					RSubTerrainChunk rSubTerrainChunk = s_Instance.m_Chunks[num4];
					if (rSubTerrainChunk.TreeList.Remove(treeinfo))
					{
						num3++;
					}
					if (TreeInfo.RemoveTiFromDict(rSubTerrainChunk.m_mapTrees, key2, treeinfo))
					{
						num3++;
					}
				}
			}
		}
		if (num3 != 2)
		{
			Debug.LogError("RSubTerrain Remove: count doesn't match");
		}
		RSubTerrCreator[] layerCreators = s_Instance.LayerCreators;
		foreach (RSubTerrCreator rSubTerrCreator in layerCreators)
		{
			rSubTerrCreator.m_allTreesInLayer.Remove(treeinfo);
		}
		RSubTerrSL.AddDeletedTree(treeinfo);
		if (treeInfo != null)
		{
			DeleteTree(treeInfo);
		}
	}

	public static void DeleteTreesAtPos(IntVector3 position)
	{
		List<TreeInfo> list = s_Instance.TreesAtPos(position);
		foreach (TreeInfo item in list)
		{
			DeleteTree(item);
		}
	}

	public static TreeInfo TryGetTreeInfo(GameObject tree)
	{
		if (s_Instance == null)
		{
			return null;
		}
		TreeInfo value = null;
		if (s_Instance.m_mapTempTreeInfos.TryGetValue(tree, out value))
		{
			return value;
		}
		return null;
	}

	public static void DeleteTreesAtPosF(Vector3 pos)
	{
		IntVector3 pos2 = new IntVector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		List<TreeInfo> list = s_Instance.TreesAtPos(pos2);
		foreach (TreeInfo item in list)
		{
			if (Vector3.Distance(item.m_pos, pos) < 0.005f)
			{
				DeleteTree(item);
			}
		}
	}

	public static void DeleteTreesAtPosForMultiMode(IntVector3 position)
	{
		List<TreeInfo> list = s_Instance.TreesAtPos(position);
		List<Vector3> list2 = new List<Vector3>();
		foreach (TreeInfo item in list)
		{
			list2.Add(item.m_pos);
		}
	}

	public static void DeleteTreesAtPosListForMultiMode(List<IntVector3> posList)
	{
		List<IntVector3> list = new List<IntVector3>();
		List<Vector3> list2 = new List<Vector3>();
		if (posList.Count == 0)
		{
			return;
		}
		int x = posList[0].x;
		int x2 = posList[0].x;
		int y = posList[0].y;
		int y2 = posList[0].y;
		int z = posList[0].z;
		int z2 = posList[0].z;
		foreach (IntVector3 pos in posList)
		{
			if (pos.x < x)
			{
				x = pos.x;
			}
			if (pos.y < x)
			{
				y = pos.y;
			}
			if (pos.z < x)
			{
				z = pos.z;
			}
			if (pos.x > x2)
			{
				x2 = pos.x;
			}
			if (pos.y > y2)
			{
				y2 = pos.y;
			}
			if (pos.z > z2)
			{
				z2 = pos.z;
			}
			List<TreeInfo> list3 = s_Instance.TreesAtPos(pos);
			foreach (TreeInfo item in list3)
			{
				list2.Add(item.m_pos);
			}
			if (list3.Count > 0)
			{
				list.Add(pos);
			}
		}
		for (int i = x; i < x2 + 1; i++)
		{
			for (int j = z; j < z2 + 1; j++)
			{
				for (int k = y - 2; k < y; k++)
				{
					IntVector3 intVector = new IntVector3(i, k, j);
					List<TreeInfo> list4 = s_Instance.TreesAtPos(intVector);
					foreach (TreeInfo item2 in list4)
					{
						list2.Add(item2.m_pos);
					}
					if (list4.Count > 0)
					{
						list.Add(intVector);
					}
				}
			}
		}
	}

	public void RefreshTempGOsIn32Meter()
	{
		int num = Mathf.FloorToInt(PlayerTransform.position.x / 32f);
		int num2 = Mathf.FloorToInt(PlayerTransform.position.z / 32f);
		for (int i = num - 2; i <= num + 2; i++)
		{
			for (int j = num2 - 2; j <= num2 + 2; j++)
			{
				int key = RSubTerrUtils.Tree32PosTo32Index(i, j);
				int num3 = 0;
				int num4 = 0;
				if (m_map32Trees.ContainsKey(key))
				{
					num4 = m_map32Trees[key].Count;
				}
				if (m_mapExistTempTrees.ContainsKey(key))
				{
					num3 = m_mapExistTempTrees[key].Count;
				}
				if (num4 == num3)
				{
					continue;
				}
				if (num3 != 0)
				{
					foreach (GameObject item in m_mapExistTempTrees[key])
					{
						m_mapTempTreeInfos.Remove(item);
						Object.Destroy(item);
					}
					m_mapExistTempTrees[key].Clear();
					m_mapExistTempTrees.Remove(key);
				}
				if (num4 == 0)
				{
					continue;
				}
				if (!m_mapExistTempTrees.ContainsKey(key))
				{
					m_mapExistTempTrees.Add(key, new List<GameObject>());
				}
				List<GameObject> list = m_mapExistTempTrees[key];
				foreach (TreeInfo item2 in m_map32Trees[key])
				{
					if (!(GlobalPrototypeColliders[item2.m_protoTypeIdx] == null))
					{
						GameObject gameObject = Object.Instantiate(GlobalPrototypeColliders[item2.m_protoTypeIdx], item2.m_pos, Quaternion.identity) as GameObject;
						gameObject.transform.parent = TempTreesGroup.transform;
						gameObject.transform.localScale = new Vector3(item2.m_widthScale, item2.m_heightScale, item2.m_widthScale);
						gameObject.name = gameObject.transform.position.ToString() + " Type " + item2.m_protoTypeIdx;
						gameObject.layer = 21;
						gameObject.SetActive(value: true);
						list.Add(gameObject);
						m_mapTempTreeInfos.Add(gameObject, item2);
					}
				}
			}
		}
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<int, List<GameObject>> mapExistTempTree in m_mapExistTempTrees)
		{
			IntVec3 intVec = RSubTerrUtils.Tree32KeyTo32Pos(mapExistTempTree.Key);
			if (Mathf.Abs(intVec.x - num) <= 2 && Mathf.Abs(intVec.z - num2) <= 2)
			{
				continue;
			}
			list2.Add(mapExistTempTree.Key);
			foreach (GameObject item3 in mapExistTempTree.Value)
			{
				m_mapTempTreeInfos.Remove(item3);
				Object.Destroy(item3);
			}
		}
		foreach (int item4 in list2)
		{
			m_mapExistTempTrees.Remove(item4);
		}
	}

	public static void RefreshAllLayerTerrains()
	{
		if (!(s_Instance == null))
		{
			s_Instance.StartCoroutine("RefreshAllLayerTerrains_Coroutine");
		}
	}

	private IEnumerator RefreshAllLayerTerrains_Coroutine()
	{
		RSubTerrCreator[] layerCreators = LayerCreators;
		foreach (RSubTerrCreator creator in layerCreators)
		{
			while (creator.bProcessing)
			{
				yield return 0;
			}
			creator.StartCoroutine("RefreshRegion");
		}
	}
}
