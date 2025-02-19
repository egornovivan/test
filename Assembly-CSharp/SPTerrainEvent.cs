using System.Collections.Generic;
using UnityEngine;

public class SPTerrainEvent : NetworkInterface
{
	public const int VoxelTerrainMaxLod = 3;

	public static SPTerrainEvent instance;

	public int minCount;

	public int maxCount;

	public int minCountCave;

	public int maxCountCave;

	private Dictionary<IntVector2, SPTerrainRect> pointTable = new Dictionary<IntVector2, SPTerrainRect>();

	private List<IntVector2> requestList = new List<IntVector2>();

	private List<IntVector4> mMeshNodes = new List<IntVector4>();

	private Transform staticPoints;

	private SimplexNoise mNoise;

	public List<IntVector4> meshNodes => mMeshNodes;

	public bool isActive => base.gameObject.activeSelf;

	private void Awake()
	{
		instance = this;
		mNoise = new SimplexNoise((long)(RandomMapConfig.RandomMapID + RandomMapConfig.RandSeed));
		if (Application.loadedLevelName.Equals("GameStory"))
		{
			AISpawnPoint.Reset();
			LoadStaticSpawnPoints();
		}
	}

	private void Start()
	{
		LODOctreeMan.self.AttachEvents(OnTerrainMeshCreated, OnTerrainMeshDestroy, null, OnTerrainPositionChange);
	}

	private void OnDisable()
	{
		CleanupSPTerrainRect();
		LODOctreeMan.self.DetachEvents(OnTerrainMeshCreated, OnTerrainMeshDestroy, null, OnTerrainPositionChange);
	}

	private void OnDestroy()
	{
		CleanupSPTerrainRect();
		LODOctreeMan.self.DetachEvents(OnTerrainMeshCreated, OnTerrainMeshDestroy, null, OnTerrainPositionChange);
	}

	public void Activate(bool value)
	{
		base.gameObject.SetActive(value);
	}

	public SPTerrainRect GetSPTerrainRect(IntVector2 mark)
	{
		if (pointTable.ContainsKey(mark))
		{
			return pointTable[mark];
		}
		return null;
	}

	public void RegisterSPPoint(SPPoint point, bool updateIndex = false)
	{
		if (point == null)
		{
			return;
		}
		IntVector4 intVector = AiUtil.ConvertToIntVector4(point.position, 3);
		IntVector2 key = new IntVector2(intVector.x, intVector.z);
		if (pointTable.ContainsKey(key))
		{
			pointTable[key].RegisterSPPoint(point);
			if (updateIndex)
			{
				point.index = pointTable[key].nextIndex;
			}
		}
	}

	public SPTerrainRect GetSPTerrainRect(Vector3 position)
	{
		if (position == Vector3.zero)
		{
			return null;
		}
		IntVector4 intVector = AiUtil.ConvertToIntVector4(position, 3);
		IntVector2 key = new IntVector2(intVector.x, intVector.z);
		if (pointTable.ContainsKey(key))
		{
			return pointTable[key];
		}
		return null;
	}

	private void LoadStaticSpawnPoints()
	{
		if (!Application.loadedLevelName.Equals("GameStory"))
		{
			return;
		}
		GameObject gameObject = new GameObject("StaticPoints");
		gameObject.transform.parent = base.transform;
		foreach (KeyValuePair<int, AISpawnPoint> s_spawnPointDatum in AISpawnPoint.s_spawnPointData)
		{
			AISpawnPoint value = s_spawnPointDatum.Value;
			value.spPoint = SPPoint.InstantiateSPPoint<SPPoint>(value.Position, Quaternion.Euler(value.euler), IntVector4.Zero, gameObject.transform, 0, value.resId, value.isActive, revisePos: false, isBoss: false, erode: false, delete: false, null, value.OnSpawned);
			value.spPoint.name = "Static id = " + value.id + " , path id = " + value.resId + " : " + value.spPoint.name;
		}
	}

	private void RegisterMeshNode(IntVector4 node)
	{
		if (!mMeshNodes.Contains(node))
		{
			mMeshNodes.Add(node);
		}
	}

	private void RemoveMeshNode(IntVector4 node)
	{
		if (mMeshNodes.Contains(node))
		{
			mMeshNodes.Remove(node);
		}
	}

	private void RegisterSPTerrainRect(IntVector4 node)
	{
		node.w = 3;
		IntVector2 intVector = AiUtil.ConvertToIntVector2FormLodLevel(node, 3);
		IntVector4 node2 = new IntVector4(intVector.x, node.y, intVector.y, node.w);
		if (!pointTable.ContainsKey(intVector))
		{
			if (!GameConfig.IsMultiMode)
			{
				SPTerrainRect sPTerrainRect = InstantiateSPTerrainRect(node2, minCount, maxCount + 1);
				sPTerrainRect.RegisterMeshNode(node);
				pointTable.Add(intVector, sPTerrainRect);
			}
		}
		else
		{
			pointTable[intVector].RegisterMeshNode(node);
		}
	}

	private void RemoveSPTerrainRect(IntVector4 node)
	{
		IntVector2 key = AiUtil.ConvertToIntVector2FormLodLevel(node, 3);
		if (pointTable.ContainsKey(key))
		{
			pointTable[key].RemoveMeshNode(node);
			if (pointTable[key].meshNodes.Count <= 0)
			{
				pointTable[key].Destroy();
				pointTable.Remove(key);
			}
		}
	}

	private void CleanupSPTerrainRect()
	{
		foreach (KeyValuePair<IntVector2, SPTerrainRect> item in pointTable)
		{
			item.Value.Cleanup();
		}
		pointTable.Clear();
	}

	private void OnTerrainColliderCreated(IntVector4 node)
	{
	}

	private void OnTerrainMeshCreated(IntVector4 node)
	{
		if (node.w != 0)
		{
			return;
		}
		IntVector2 key = AiUtil.ConvertToIntVector2FormLodLevel(node, 3);
		if (pointTable.ContainsKey(key))
		{
			SPTerrainRect sPTerrainRect = pointTable[key];
			if (sPTerrainRect != null)
			{
				List<SPPoint> points = sPTerrainRect.points;
				points = points.FindAll((SPPoint ret) => Match(ret, node.x, node.z));
				foreach (SPPoint item in points)
				{
					item.AttachEvent(node);
				}
			}
		}
		RegisterMeshNode(node);
		LODOctreeMan.self.AttachNodeEvents(null, null, null, null, OnTerrainColliderCreated, node.ToVector3());
	}

	private void OnTerrainMeshDestroy(IntVector4 node)
	{
		if (node.w == 0)
		{
			RemoveMeshNode(node);
		}
	}

	private void OnTerrainPositionChange(IntVector4 preNode, IntVector4 postNode)
	{
		if (preNode.x != postNode.x || preNode.z != postNode.z)
		{
			if (preNode.x != -999 && preNode.y != -999 && preNode.z != -999)
			{
				OnTerrainPositionDetory(preNode);
			}
			OnTerrainPositionCreated(postNode);
		}
	}

	private void OnTerrainPositionCreated(IntVector4 node)
	{
		if (isActive && node.w == LODOctreeMan._maxLod)
		{
			RegisterSPTerrainRect(node);
		}
	}

	private void OnTerrainPositionDetory(IntVector4 node)
	{
		if (node.w == LODOctreeMan._maxLod)
		{
			RemoveSPTerrainRect(node);
		}
	}

	private bool Match(SPPoint point, int x, int z)
	{
		if (point == null)
		{
			return false;
		}
		float num = point.position.x - (float)x;
		float num2 = point.position.z - (float)z;
		return num >= float.Epsilon && num <= 32f && num2 >= float.Epsilon && num2 <= 32f;
	}

	private SPTerrainRect InstantiateSPTerrainRect(IntVector4 node, int min, int max)
	{
		return SPTerrainRect.InstantiateSPTerrainRect(node, min, max, base.transform, mNoise);
	}

	public void RegisterAIToServer(IntVector4 index, Vector3 position, int pathID)
	{
		if (GameConfig.IsMultiMode && !(null == PlayerNetwork.mainPlayer))
		{
			if (AIResource.IsGroup(pathID))
			{
				NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateGroupAi(pathID, position));
			}
			else
			{
				NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAi(pathID, position, -1, -1, -1));
			}
		}
	}

	public void RegisterGift(Vector3 position, int pathID)
	{
		NetworkManager.SyncServer(EPacketType.PT_AI_Gift, position, pathID);
	}

	public void CreateMultiNativeStatic(Vector3 pos, int id, int townID)
	{
		NetworkManager.SyncServer(EPacketType.PT_AI_NativeStatic, pos, id, townID);
	}

	public void AddSPTerrainRect(SPTerrainRect spTerrain)
	{
		if (!(spTerrain != null))
		{
			return;
		}
		IntVector2 key = new IntVector2(spTerrain.position.x, spTerrain.position.z);
		if (pointTable.ContainsKey(key))
		{
			pointTable.Remove(key);
		}
		pointTable.Add(key, spTerrain);
		foreach (SPPoint point in spTerrain.points)
		{
			point.Activate(value: true);
		}
	}

	public void CreateAdNpcByIndex(Vector3 pos, int key, int type = 0)
	{
		NetworkManager.SyncServer(EPacketType.PT_NPC_CreateTown, pos, key, type);
	}
}
