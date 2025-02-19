using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSubTerrainMgr : MonoBehaviour
{
	public enum ColType
	{
		None,
		Collider,
		BoxCollider
	}

	public const int TreePlaceHolderPrototypeIndex = 63;

	public const int NearTreeLayer = 21;

	public const int NearTreeLayerMask = 2097152;

	private static LSubTerrainMgr s_Instance = null;

	public static Action<GameObject> OnTreeColliderCreated;

	public static Action<GameObject> OnTreeColliderDestroy;

	private LSubTerrIO m_IO;

	private static IntVector3 _iTmpCamPos = new IntVector3();

	private static IntVector3 _iCurCamPos = new IntVector3();

	private Dictionary<int, LSubTerrain> m_Nodes;

	public Dictionary<int, List<TreeInfo>> m_map32Trees;

	private Dictionary<int, List<GameObject>> m_mapExistTempTrees;

	private Dictionary<GameObject, GlobalTreeInfo> m_mapTempTreeInfos;

	public GameObject[] GlobalPrototypePrefabList;

	[HideInInspector]
	public float[] GlobalPrototypeBendFactorList;

	public Material[] GlobalPrototypeBillboardList;

	[HideInInspector]
	public Bounds[] GlobalPrototypeBounds;

	public ColType[] GlobalPrototypeCollidersType;

	public GameObject[] GlobalPrototypeColliders;

	public GameObject[] GlobalPrototypeLights;

	public BillboardTerrain BTerrainRes;

	private LTreePlaceHolderInfo[] GlobalPrototypeTPHInfo;

	private static List<GlobalTreeInfo> picklist = new List<GlobalTreeInfo>();

	private static Stack<GlobalTreeInfo> globalTreeInfos = new Stack<GlobalTreeInfo>();

	private static IntVector3 temp = new IntVector3();

	public Transform CameraTransform;

	public Transform PlayerTransform;

	public VoxelEditor VEditor;

	public int NumDataExpands = 1;

	public int ExtraCacheSize = 23;

	public List<LSubTerrLayerOption> Layers;

	public GameObject LayerGroup;

	public GameObject BTerrainGroup;

	public LSubTerrCreator[] LayerCreators;

	public GameObject PrototypeGroup;

	public GameObject TempTreesGroup;

	private float m_BeginTime;

	private float m_LifeTime;

	private float m_LifeFrame;

	private bool _bDataDirty;

	public LSubTerrEditor m_Editor;

	public static LSubTerrainMgr Instance => s_Instance;

	public static IntVector3 CameraPos => _iCurCamPos;

	public static LSubTerrIO IO => (!(s_Instance == null)) ? s_Instance.m_IO : null;

	public static GameObject GO => (!(s_Instance == null)) ? s_Instance.gameObject : null;

	public static LSubTerrain Node(int index)
	{
		return (s_Instance == null) ? null : ((!s_Instance.m_Nodes.ContainsKey(index)) ? null : s_Instance.m_Nodes[index]);
	}

	public static bool HasCollider(int prototype)
	{
		if (s_Instance != null && s_Instance.GlobalPrototypeCollidersType[prototype] != 0)
		{
			return true;
		}
		return false;
	}

	public static bool HasMultiCollider(int prototype)
	{
		if (s_Instance != null && s_Instance.GlobalPrototypeCollidersType[prototype] == ColType.BoxCollider)
		{
			return true;
		}
		return false;
	}

	public static bool HasLight(int prototype)
	{
		if (s_Instance != null && s_Instance.GlobalPrototypeLights[prototype] != null)
		{
			return true;
		}
		return false;
	}

	public static LTreePlaceHolderInfo GetTreePlaceHolderInfo(int prototype)
	{
		if (s_Instance == null)
		{
			return null;
		}
		return s_Instance.GlobalPrototypeTPHInfo[prototype];
	}

	private void Awake()
	{
		Debug.Log("Creating LSubTerrainMgr!");
		if (s_Instance != null)
		{
			Debug.LogError("Can not have a second instance of LSubTerrainMgr !");
		}
		s_Instance = this;
	}

	private void Init()
	{
		GlobalPrototypePrefabList = VEditor.m_treePrototypeList;
		GlobalPrototypeBendFactorList = VEditor.m_treePrototypeBendfactor;
		GlobalPrototypeBounds = new Bounds[GlobalPrototypePrefabList.Length];
		GlobalPrototypeCollidersType = new ColType[GlobalPrototypePrefabList.Length];
		GlobalPrototypeColliders = new GameObject[GlobalPrototypePrefabList.Length];
		GlobalPrototypeLights = new GameObject[GlobalPrototypePrefabList.Length];
		GlobalPrototypeTPHInfo = new LTreePlaceHolderInfo[GlobalPrototypePrefabList.Length];
		for (int i = 0; i < GlobalPrototypeBounds.Length; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(GlobalPrototypePrefabList[i]);
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
			UnityEngine.Object.Destroy(component);
			UnityEngine.Object.Destroy(component2);
			if (component3 != null)
			{
				UnityEngine.Object.Destroy(component3);
			}
			if (component4 != null)
			{
				UnityEngine.Object.Destroy(component4);
			}
			Collider component5 = gameObject.GetComponent<Collider>();
			if (component5 != null || gameObject.GetComponentsInChildren<Light>(includeInactive: true).Length >= 1)
			{
				BoxCollider component6 = gameObject.GetComponent<BoxCollider>();
				if (component6 != null)
				{
					GlobalPrototypeTPHInfo[i] = new LTreePlaceHolderInfo(component6.center, component6.extents.y, component6.extents.x + component6.extents.z);
				}
				else
				{
					GlobalPrototypeTPHInfo[i] = null;
				}
				gameObject.name = "Prototype [" + i + "]'s Collider";
				gameObject.SetActive(value: false);
				GlobalPrototypeColliders[i] = gameObject;
				GlobalPrototypeCollidersType[i] = ((component6 != null) ? ColType.BoxCollider : ((component5 != null) ? ColType.Collider : ColType.None));
			}
			else
			{
				UnityEngine.Object.Destroy(gameObject);
				GlobalPrototypeColliders[i] = null;
				GlobalPrototypeCollidersType[i] = ColType.None;
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(GlobalPrototypePrefabList[i]);
			gameObject2.transform.parent = PrototypeGroup.transform;
			gameObject2.transform.position = Vector3.zero;
			gameObject2.transform.rotation = Quaternion.identity;
			MeshRenderer component7 = gameObject2.GetComponent<MeshRenderer>();
			MeshFilter component8 = gameObject2.GetComponent<MeshFilter>();
			Collider[] componentsInChildren = gameObject2.GetComponentsInChildren<Collider>();
			Animator component9 = gameObject2.GetComponent<Animator>();
			Animation component10 = gameObject2.GetComponent<Animation>();
			ref Bounds reference2 = ref GlobalPrototypeBounds[i];
			reference2 = component8.mesh.bounds;
			if (i == 63)
			{
				GlobalPrototypeBounds[i].extents = new Vector3(1f, 2f, 1f);
			}
			UnityEngine.Object.Destroy(component7);
			UnityEngine.Object.Destroy(component8);
			Collider[] array = componentsInChildren;
			foreach (Collider obj in array)
			{
				UnityEngine.Object.Destroy(obj);
			}
			if (component9 != null)
			{
				UnityEngine.Object.Destroy(component9);
			}
			if (component10 != null)
			{
				UnityEngine.Object.Destroy(component10);
			}
			if (gameObject2.GetComponentsInChildren<Light>(includeInactive: true).Length >= 1)
			{
				gameObject2.name = "Prototype [" + i + "]'s Light";
				gameObject2.SetActive(value: false);
				GlobalPrototypeLights[i] = gameObject2;
			}
			else
			{
				UnityEngine.Object.Destroy(gameObject2);
				GlobalPrototypeLights[i] = null;
			}
		}
		m_IO = LSubTerrIO.CreateInst();
	}

	private void Start()
	{
		Init();
		m_Nodes = new Dictionary<int, LSubTerrain>();
		m_map32Trees = new Dictionary<int, List<TreeInfo>>();
		m_mapExistTempTrees = new Dictionary<int, List<GameObject>>();
		m_mapTempTreeInfos = new Dictionary<GameObject, GlobalTreeInfo>();
		TempTreesGroup.AddComponent<LSubTerrTempTrees>().m_TempMap = m_mapTempTreeInfos;
		m_BeginTime = Time.time;
		LayerCreators = new LSubTerrCreator[Layers.Count];
		for (int num = Layers.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = new GameObject("[" + num + "] " + Layers[num].Name + " Layer");
			gameObject.transform.parent = LayerGroup.transform;
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			LSubTerrCreator lSubTerrCreator = gameObject.AddComponent<LSubTerrCreator>();
			lSubTerrCreator.LayerIndex = num;
			LayerCreators[num] = lSubTerrCreator;
		}
	}

	private void Update()
	{
		LSubTerrUtils.WorldPosToPos(s_Instance.CameraTransform.position, _iTmpCamPos);
		if (m_IO.TryFill_T(_iTmpCamPos, m_Nodes))
		{
			_iCurCamPos.x = _iTmpCamPos.x;
			_iCurCamPos.y = _iTmpCamPos.y;
			_iCurCamPos.z = _iTmpCamPos.z;
			if (PlayerTransform == null)
			{
				PlayerTransform = CameraTransform;
			}
			else if (IsAllNodeFinishedProcess())
			{
				RefreshTreeGos();
			}
			DeleteNodesOutfield(_iCurCamPos);
			if (_bDataDirty)
			{
				_bDataDirty = false;
				RefreshAllLayerTerrains();
			}
		}
	}

	private void LateUpdate()
	{
		m_LifeTime = Time.time - m_BeginTime;
		m_LifeFrame += 1f;
	}

	private void OnDestroy()
	{
		if (m_Nodes != null)
		{
			foreach (KeyValuePair<int, LSubTerrain> node in m_Nodes)
			{
				node.Value.Release();
			}
			m_Nodes.Clear();
		}
		if (m_map32Trees != null)
		{
			m_map32Trees.Clear();
		}
		if (m_mapExistTempTrees != null)
		{
			m_mapExistTempTrees.Clear();
		}
		if (m_mapTempTreeInfos != null)
		{
			m_mapTempTreeInfos.Clear();
		}
		LSubTerrIO.DestroyInst(m_IO);
		s_Instance = null;
	}

	private void RefreshTreeGos()
	{
		int num = Mathf.FloorToInt(PlayerTransform.position.x / 32f);
		int num2 = Mathf.FloorToInt(PlayerTransform.position.z / 32f);
		for (int i = num - 2; i <= num + 2; i++)
		{
			for (int j = num2 - 2; j <= num2 + 2; j++)
			{
				int key = LSubTerrUtils.Tree32PosTo32Key(i, j);
				if (m_mapExistTempTrees.ContainsKey(key) || !m_map32Trees.TryGetValue(key, out var value))
				{
					continue;
				}
				List<GameObject> list = new List<GameObject>();
				int count = value.Count;
				for (int k = 0; k < count; k++)
				{
					TreeInfo treeInfo = value[k];
					if (!(GlobalPrototypeColliders[treeInfo.m_protoTypeIdx] == null))
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(GlobalPrototypeColliders[treeInfo.m_protoTypeIdx], LSubTerrUtils.TreeTerrainPosToWorldPos(i / 8, j / 8, treeInfo.m_pos), Quaternion.identity) as GameObject;
						gameObject.transform.parent = TempTreesGroup.transform;
						gameObject.transform.localScale = new Vector3(treeInfo.m_widthScale, treeInfo.m_heightScale, treeInfo.m_widthScale);
						gameObject.name = gameObject.transform.position.ToString() + " Type " + treeInfo.m_protoTypeIdx;
						gameObject.layer = 21;
						gameObject.SetActive(value: true);
						if (OnTreeColliderCreated != null)
						{
							OnTreeColliderCreated(gameObject);
						}
						list.Add(gameObject);
						m_mapTempTreeInfos.Add(gameObject, new GlobalTreeInfo(i / 8, j / 8, treeInfo));
					}
				}
				m_mapExistTempTrees.Add(key, list);
			}
		}
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<int, List<GameObject>> mapExistTempTree in m_mapExistTempTrees)
		{
			IntVector3 intVector = LSubTerrUtils.Tree32KeyTo32Pos(mapExistTempTree.Key);
			if (Mathf.Abs(intVector.x - num) <= 2 && Mathf.Abs(intVector.z - num2) <= 2)
			{
				continue;
			}
			list2.Add(mapExistTempTree.Key);
			foreach (GameObject item in mapExistTempTree.Value)
			{
				if (OnTreeColliderDestroy != null)
				{
					OnTreeColliderDestroy(item);
				}
				m_mapTempTreeInfos.Remove(item);
				UnityEngine.Object.Destroy(item);
			}
		}
		foreach (int item2 in list2)
		{
			m_mapExistTempTrees.Remove(item2);
		}
	}

	private void DeleteNode(int index)
	{
		_bDataDirty = true;
		if (m_Nodes.TryGetValue(index, out var value))
		{
			m_Nodes.Remove(index);
			value.Release();
		}
		else
		{
			Debug.LogError("Deleting an LSubTerrain node but it doesn't exist in the map !");
		}
	}

	private void DeleteNodesOutfield(IntVector3 iCamPos)
	{
		int num = ((m_LifeTime > 90f) ? ExtraCacheSize : 0);
		int num2 = m_Nodes.Count - ((NumDataExpands * 2 + 1) * (NumDataExpands * 2 + 1) + num);
		for (int i = 0; i < num2; i++)
		{
			float num3 = 0f;
			int index = -1;
			foreach (KeyValuePair<int, LSubTerrain> node in m_Nodes)
			{
				IntVector3 iPos = node.Value.iPos;
				int num4 = Mathf.Max(Mathf.Abs(iPos.x - iCamPos.x), Mathf.Abs(iPos.z - iCamPos.z));
				if ((float)num4 >= num3)
				{
					num3 = num4;
					index = node.Key;
				}
			}
			DeleteNode(index);
		}
	}

	private void DrawOutline()
	{
		foreach (KeyValuePair<int, List<GameObject>> mapExistTempTree in m_mapExistTempTrees)
		{
			IntVector3 intVector = LSubTerrUtils.Tree32KeyTo32Pos(mapExistTempTree.Key);
			int x = intVector.x;
			int z = intVector.z;
			Debug.DrawLine(new Vector3(x * 32, PlayerTransform.position.y, z * 32), new Vector3(x * 32 + 32, PlayerTransform.position.y, z * 32), Color.yellow);
			Debug.DrawLine(new Vector3(x * 32 + 32, PlayerTransform.position.y, z * 32), new Vector3(x * 32 + 32, PlayerTransform.position.y, z * 32 + 32), Color.yellow);
			Debug.DrawLine(new Vector3(x * 32 + 32, PlayerTransform.position.y, z * 32 + 32), new Vector3(x * 32, PlayerTransform.position.y, z * 32 + 32), Color.yellow);
			Debug.DrawLine(new Vector3(x * 32, PlayerTransform.position.y, z * 32 + 32), new Vector3(x * 32, PlayerTransform.position.y, z * 32), Color.yellow);
		}
	}

	public static GlobalTreeInfo GetTreeinfo(Collider col)
	{
		if (null == col)
		{
			return null;
		}
		if (s_Instance.m_mapTempTreeInfos.ContainsKey(col.gameObject))
		{
			return s_Instance.m_mapTempTreeInfos[col.gameObject];
		}
		return null;
	}

	public static GlobalTreeInfo RayCast(Ray ray, float distance, out RaycastHit hitinfo)
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

	public static GlobalTreeInfo RayCast(Ray ray, float distance)
	{
		RaycastHit hitinfo;
		return RayCast(ray, distance, out hitinfo);
	}

	public static List<GlobalTreeInfo> Picking(Vector3 position, Vector3 direction, bool includeTrees = false, float distance = 1.5f, float angle = 45f)
	{
		if (s_Instance == null)
		{
			return picklist;
		}
		ClearTreeinfo();
		distance = Mathf.Clamp(distance, 0.1f, 2f);
		int num = Mathf.FloorToInt(position.x);
		int num2 = Mathf.FloorToInt(position.y);
		int num3 = Mathf.FloorToInt(position.z);
		for (int i = num - 2; i <= num + 2; i++)
		{
			for (int j = num2 - 2; j <= num2 + 2; j++)
			{
				for (int k = num3 - 2; k <= num3 + 2; k++)
				{
					int num4 = LSubTerrUtils.WorldPosToIndex(new Vector3((float)i + 0.5f, 0f, (float)k + 0.5f));
					if (Node(num4) == null)
					{
						continue;
					}
					temp.x = i % 256;
					temp.y = j;
					temp.z = k % 256;
					TreeInfo treeInfo = Node(num4).GetTreeInfoListAtPos(temp);
					while (treeInfo != null)
					{
						TreeInfo treeInfo2 = treeInfo;
						treeInfo = treeInfo.Next;
						if (HasCollider(treeInfo2.m_protoTypeIdx) && !includeTrees)
						{
							continue;
						}
						Vector3 vector = LSubTerrUtils.TreeTerrainPosToWorldPos(i >> 8, k >> 8, treeInfo2.m_pos);
						Vector3 from = vector - position;
						from.y = 0f;
						if (!(from.magnitude > distance))
						{
							direction.y = 0f;
							if (Vector3.Angle(from, direction) < angle)
							{
								GlobalTreeInfo globalTreeInfo = GetGlobalTreeInfo();
								globalTreeInfo._terrainIndex = num4;
								globalTreeInfo._treeInfo = treeInfo2;
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
		int x = position.x;
		int y = position.y;
		int z = position.z;
		int num = LSubTerrUtils.WorldPosToIndex(new Vector3((float)x + 0.5f, 0f, (float)z + 0.5f));
		if (Node(num) != null)
		{
			int x_ = x % 256;
			int y_ = y;
			int z_ = z % 256;
			TreeInfo treeInfo = Node(num).GetTreeInfoListAtPos(new IntVector3(x_, y_, z_));
			while (treeInfo != null)
			{
				TreeInfo treeInfo2 = treeInfo;
				treeInfo = treeInfo.Next;
				if (!HasCollider(treeInfo2.m_protoTypeIdx) || includeTrees)
				{
					GlobalTreeInfo globalTreeInfo = GetGlobalTreeInfo();
					globalTreeInfo._terrainIndex = num;
					globalTreeInfo._treeInfo = treeInfo2;
					picklist.Add(globalTreeInfo);
				}
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

	public static void AddTree(Vector3 wpos, int prototype, float width_scale = 1f, float height_scale = 1f)
	{
		if (s_Instance == null)
		{
			return;
		}
		int num = LSubTerrUtils.WorldPosToIndex(wpos);
		IntVector3 intVector = LSubTerrUtils.IndexToPos(num);
		IntVector3 iCurCamPos = _iCurCamPos;
		if (Mathf.Abs(intVector.x - iCurCamPos.x) > 1 || Mathf.Abs(intVector.z - iCurCamPos.z) > 1 || !s_Instance.m_Nodes.ContainsKey(num))
		{
			return;
		}
		LSubTerrain lSubTerrain = Node(num);
		if (lSubTerrain == null)
		{
			return;
		}
		TreeInfo treeInfo = lSubTerrain.AddTreeInfo(wpos, prototype, width_scale, height_scale);
		if (treeInfo == null)
		{
			return;
		}
		float num2 = s_Instance.GlobalPrototypeBounds[treeInfo.m_protoTypeIdx].extents.y * 2f;
		for (int num3 = s_Instance.Layers.Count - 1; num3 >= 0; num3--)
		{
			if (s_Instance.Layers[num3].MinTreeHeight <= num2 && num2 < s_Instance.Layers[num3].MaxTreeHeight)
			{
				s_Instance.LayerCreators[num3].m_allTreesInLayer[num].Add(treeInfo);
				break;
			}
		}
	}

	public static void DeleteTree(GameObject nearTreeGo)
	{
		if (!(s_Instance == null))
		{
			GlobalTreeInfo value = null;
			if (s_Instance.m_mapTempTreeInfos.TryGetValue(nearTreeGo, out value))
			{
				DeleteTree(value);
			}
		}
	}

	public static void DeleteTree(GlobalTreeInfo treeinfo)
	{
		if (s_Instance == null || treeinfo == null)
		{
			return;
		}
		int key = LSubTerrUtils.TreeWorldPosTo32Key(treeinfo.WorldPos);
		if (s_Instance.m_map32Trees.TryGetValue(key, out var value))
		{
			value.Remove(treeinfo._treeInfo);
			if (value.Count == 0)
			{
				s_Instance.m_map32Trees.Remove(key);
			}
		}
		if (s_Instance.m_mapExistTempTrees.TryGetValue(key, out var value2))
		{
			GameObject gameObject = null;
			foreach (GameObject item in value2)
			{
				if (s_Instance.m_mapTempTreeInfos.TryGetValue(item, out var value3))
				{
					if (value3._treeInfo == treeinfo._treeInfo)
					{
						gameObject = item;
						if (OnTreeColliderDestroy != null)
						{
							OnTreeColliderDestroy(gameObject);
						}
						UnityEngine.Object.Destroy(item);
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
				value2.Remove(gameObject);
			}
		}
		TreeInfo treeInfo = null;
		if (Node(treeinfo._terrainIndex) != null)
		{
			treeInfo = Node(treeinfo._terrainIndex).DeleteTreeInfo(treeinfo._treeInfo);
		}
		else
		{
			Debug.LogError("Can not find the subterrain node when delete tree");
		}
		LSubTerrCreator[] layerCreators = s_Instance.LayerCreators;
		foreach (LSubTerrCreator lSubTerrCreator in layerCreators)
		{
			if (lSubTerrCreator.m_allTreesInLayer.TryGetValue(treeinfo._terrainIndex, out value))
			{
				value.Remove(treeinfo._treeInfo);
			}
			else
			{
				Debug.LogError("Can not find the key in layer's m_allTreesInLayer when delete tree");
			}
		}
		LSubTerrSL.AddDeletedTree(treeinfo._terrainIndex, treeinfo._treeInfo);
		if (treeInfo != null)
		{
			GlobalTreeInfo treeinfo2 = new GlobalTreeInfo(treeinfo._terrainIndex, treeInfo);
			DeleteTree(treeinfo2);
		}
	}

	public static bool IsAllNodeFinishedProcess()
	{
		if (s_Instance == null)
		{
			return true;
		}
		foreach (KeyValuePair<int, LSubTerrain> node in s_Instance.m_Nodes)
		{
			if (!node.Value.FinishedProcess && node.Value.HasData)
			{
				return false;
			}
		}
		return true;
	}

	public static void DeleteTreesAtPos(IntVector3 position, int filter_min = 0, int filter_max = 65536)
	{
		List<GlobalTreeInfo> list = Picking(position, includeTrees: true);
		foreach (GlobalTreeInfo item in list)
		{
			if (item._treeInfo.m_protoTypeIdx >= filter_min && item._treeInfo.m_protoTypeIdx <= filter_max)
			{
				DeleteTree(item);
			}
		}
	}

	public static GlobalTreeInfo TryGetTreeInfo(GameObject tree)
	{
		if (s_Instance == null)
		{
			return null;
		}
		GlobalTreeInfo value = null;
		if (s_Instance.m_mapTempTreeInfos.TryGetValue(tree, out value))
		{
			return value;
		}
		return null;
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
		LSubTerrCreator[] layerCreators = LayerCreators;
		foreach (LSubTerrCreator creator in layerCreators)
		{
			while (creator.bProcessing || creator.bBillboardProcessing)
			{
				yield return 0;
			}
			creator.StartCoroutine("RefreshRegion");
		}
	}

	public static void CacheAllNodes()
	{
		if (s_Instance == null)
		{
			return;
		}
		foreach (KeyValuePair<int, LSubTerrain> node in s_Instance.m_Nodes)
		{
			node.Value.SaveCache();
		}
	}
}
