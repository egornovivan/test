using System.Collections.Generic;
using uLink;
using UnityEngine;

public class SPPoint : UnityEngine.MonoBehaviour
{
	private static int s_Count = 0;

	private static Dictionary<IntVector4, SPPoint> PointTable = new Dictionary<IntVector4, SPPoint>();

	public PointType mType;

	private int mSpID;

	private int mPathID;

	private bool mActive;

	private bool mIsBoss;

	private bool mErode;

	private bool mDelete;

	private bool mRevisePosition;

	private bool mWaitForSpawned;

	private GameObject mClone;

	private Quaternion mRotation;

	private IntVector4 mIndex;

	private List<IntVector4> mNodes;

	private SimplexNoise mNoise;

	private CommonInterface mCommon;

	private bool mDeath;

	private Transform mTDInfo;

	private AiObject mAiObject;

	private SPGroup mAiGroup;

	private AssetReq.ReqFinishDelegate mReqFinish;

	private List<AssetReq> mReqList = new List<AssetReq>();

	public AiObject aiObject => mAiObject;

	public SPGroup aiGroup => mAiGroup;

	public bool active => mActive;

	public virtual bool isActive
	{
		get
		{
			if (!GameConfig.IsMultiMode)
			{
				bool flag = false;
				if (clone != null)
				{
					AiObject component = clone.GetComponent<AiObject>();
					if (component == null || !component.dead)
					{
						flag = true;
					}
				}
				return mActive && !flag && !mWaitForSpawned;
			}
			return false;
		}
	}

	public bool spawning => mWaitForSpawned;

	public bool death => mDeath;

	public int pathID
	{
		get
		{
			if (mPathID <= 0)
			{
				CalculatePointType();
				if (mSpID <= 0)
				{
					return GetPathID();
				}
				return GetPathIDFromSPID(mSpID);
			}
			return mPathID;
		}
	}

	public IntVector4 index
	{
		get
		{
			return mIndex;
		}
		set
		{
			mIndex = value;
		}
	}

	public Vector3 position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public Transform tdInfo
	{
		set
		{
			mTDInfo = value;
		}
	}

	public bool revisePosition => mRevisePosition;

	public GameObject clone => mClone;

	public PointType type => mType;

	public int typeID => (int)type;

	public static T InstantiateSPPoint<T>(Vector3 position, Quaternion rotation, IntVector4 idx, Transform parent = null, int spid = 0, int pathid = 0, bool isActive = true, bool revisePos = true, bool isBoss = false, bool erode = true, bool delete = true, SimplexNoise noise = null, AssetReq.ReqFinishDelegate onSpawned = null, CommonInterface common = null) where T : SPPoint
	{
		GameObject gameObject = new GameObject("[" + position.x + " , " + position.z + "]");
		gameObject.transform.parent = parent;
		gameObject.transform.position = position;
		gameObject.transform.rotation = rotation;
		T result = gameObject.AddComponent<T>();
		result.Init(idx, parent, spid, pathid, isActive, revisePos, isBoss, erode, delete, noise, onSpawned, common);
		return result;
	}

	public virtual void Init(IntVector4 idx, Transform parent = null, int spid = 0, int pathid = 0, bool isActive = true, bool revisePos = true, bool isBoss = false, bool isErode = true, bool isDelete = true, SimplexNoise noise = null, AssetReq.ReqFinishDelegate onSpawned = null, CommonInterface common = null)
	{
		mIndex = idx;
		mType = PointType.PT_NULL;
		mSpID = spid;
		mPathID = pathid;
		mActive = isActive;
		mIsBoss = isBoss;
		mErode = isErode;
		mDelete = isDelete;
		mNoise = noise;
		mNodes = new List<IntVector4>();
		mRevisePosition = revisePos;
		mWaitForSpawned = false;
		mReqFinish = onSpawned;
		mCommon = common;
		AttachEventFromMesh();
		AttachCollider();
		RegisterPoint(this);
	}

	public static void RegisterPoint(SPPoint point)
	{
		IntVector4 key = point.index;
		if (!PointTable.ContainsKey(key))
		{
			PointTable.Add(key, point);
		}
	}

	public static void RemovePoint(SPPoint point)
	{
		IntVector4 key = point.index;
		if (PointTable.ContainsKey(key))
		{
			PointTable.Remove(key);
		}
	}

	public void SetActive(bool isActive)
	{
		mActive = isActive;
	}

	public virtual void Activate(bool value)
	{
		if (value)
		{
			AttachAllEvent();
			return;
		}
		DetachAllEvent();
		RemovePoint(this);
	}

	public void AttachEvent(IntVector4 node)
	{
		if (!mNodes.Contains(node))
		{
			mNodes.Add(node);
			LODOctreeMan.self.AttachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, node.ToVector3());
		}
	}

	private void AttachAllEvent()
	{
		foreach (IntVector4 mNode in mNodes)
		{
			LODOctreeMan.self.AttachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, mNode.ToVector3());
		}
	}

	public void DetachEvent(IntVector4 node)
	{
		if (mNodes.Contains(node))
		{
			mNodes.Remove(node);
			LODOctreeMan.self.DetachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, node.ToVector3());
		}
	}

	private void DetachAllEvent()
	{
		foreach (IntVector4 mNode in mNodes)
		{
			LODOctreeMan.self.DetachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, mNode.ToVector3());
		}
		mNodes.Clear();
	}

	private bool Match(IntVector4 node)
	{
		float num = position.x - (float)node.x;
		float num2 = position.z - (float)node.z;
		return num >= float.Epsilon && num <= (float)(32 << node.w) && num2 >= float.Epsilon && num2 <= (float)(32 << node.w);
	}

	public void AttachCollider()
	{
	}

	public void AttachEventFromMesh()
	{
		if (!(SPTerrainEvent.instance != null))
		{
			return;
		}
		for (int num = mNodes.Count - 1; num >= 0; num--)
		{
			if (!Match(mNodes[num]))
			{
				DetachEvent(mNodes[num]);
			}
		}
		List<IntVector4> list = SPTerrainEvent.instance.meshNodes.FindAll((IntVector4 ret) => Match(ret));
		foreach (IntVector4 item in list)
		{
			AttachEvent(item);
		}
	}

	public void InstantiateImmediately()
	{
		if (RevisePosition())
		{
			Instantiate();
		}
	}

	public virtual void ClearDeathEvent()
	{
		if (clone != null)
		{
			AiObject component = clone.GetComponent<AiObject>();
			if (component != null)
			{
				component.DeathHandlerEvent -= OnDeath;
			}
		}
	}

	public void OnDestroy()
	{
		ClearDeathEvent();
		DetachAllEvent();
		foreach (AssetReq mReq in mReqList)
		{
			if (mReq != null)
			{
				mReq.ReqFinishHandler -= OnSpawned;
			}
		}
	}

	public void Delete()
	{
		Object.Destroy(base.gameObject);
	}

	protected virtual void OnSpawnedChild(GameObject obj)
	{
	}

	protected virtual void OnSpawned(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		if (mReqFinish != null)
		{
			mReqFinish(obj);
		}
		mClone = obj;
		mWaitForSpawned = false;
		mAiObject = clone.GetComponent<AiObject>();
		if (mAiObject != null)
		{
			if (mAiObject.motor != null && mAiObject.motor.gravity > float.Epsilon && !AiUtil.CheckPositionOnTerrainCollider(mAiObject.position))
			{
				mAiObject.Activate(value: false);
			}
			mAiObject.DeathHandlerEvent += OnDeath;
		}
		mAiGroup = clone.GetComponent<SPGroup>();
		if (mAiGroup != null)
		{
			mAiGroup.OnSpawndEvent += OnSpawnedChild;
		}
	}

	public virtual void OnDeath(AiObject aiObj)
	{
		mDeath = true;
		if (mDelete)
		{
			Delete();
		}
	}

	private void OnTerrainColliderDestroy(IntVector4 node)
	{
		if (mNodes.Contains(node))
		{
			LODOctreeMan.self.DetachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, node.ToVector3());
			mNodes.Remove(node);
		}
	}

	private void OnTerrainColliderCreated(IntVector4 node)
	{
		if (RevisePosition(node))
		{
			Instantiate();
		}
	}

	private void Instantiate()
	{
		if (!GameConfig.IsMultiMode)
		{
			InstantiateSingleMode();
		}
		else
		{
			InstantiateMultiMode();
		}
	}

	private bool RevisePosition()
	{
		if (!mRevisePosition)
		{
			return true;
		}
		Vector3 vector = base.transform.position;
		if (AiUtil.CheckPositionOnGround(vector, out var hitInfo, 256f, 256f, AiUtil.groundedLayer))
		{
			base.transform.position = hitInfo.point;
			if (!mErode || AIErodeMap.IsInErodeArea(base.transform.position) == null)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool RevisePosition(IntVector4 node)
	{
		if (!mRevisePosition)
		{
			return true;
		}
		Vector3 vector = new Vector3(base.transform.position.x, node.y, base.transform.position.z);
		if (AiUtil.CheckPositionOnGround(vector, out var hitInfo, 0f, 32 << node.w, AiUtil.groundedLayer))
		{
			base.transform.position = hitInfo.point;
			if (!mErode || AIErodeMap.IsInErodeArea(base.transform.position) == null)
			{
				return true;
			}
		}
		return false;
	}

	private void CalculatePointType()
	{
		mType = AiUtil.GetPointType(position);
	}

	private void InstantiateSingleMode()
	{
		if (isActive)
		{
			mWaitForSpawned = true;
			int id = pathID;
			AssetReq assetReq = ((!mRevisePosition) ? AIResource.Instantiate(id, base.transform.position, base.transform.rotation, OnSpawned) : AIResource.Instantiate(id, AIResource.FixedHeightOfAIResource(id, position), base.transform.rotation, OnSpawned));
			if (assetReq != null)
			{
				mReqList.Add(assetReq);
			}
		}
	}

	private void InstantiateMultiMode(int fixId = -1)
	{
		if (isActive)
		{
			int id = pathID;
			Vector3 vector = AIResource.FixedHeightOfAIResource(id, position);
			SPTerrainEvent.instance.RegisterAIToServer(index, vector, id);
		}
	}

	private int GetPathIDFromSPID(int spid)
	{
		int pointType = typeID;
		if (mTDInfo != null)
		{
			if (mType == PointType.PT_Water && !AiUtil.CheckPositionUnderWater(mTDInfo.position))
			{
				pointType = 4;
			}
			if (Mathf.Abs(position.y - mTDInfo.position.y) > 64f)
			{
				pointType = 4;
			}
		}
		return AISpawnPath.GetPathID(spid, pointType);
	}

	private int GetPathID()
	{
		if (GameConfig.IsMultiMode)
		{
			return GetPathIDNetwork();
		}
		if (Application.loadedLevelName.Equals("GameStory"))
		{
			return GetPathIDStory();
		}
		if (Application.loadedLevelName.Equals("GameAdventure"))
		{
			return GetPathIDAdventure();
		}
		return 0;
	}

	private int GetAreaID()
	{
		return AiUtil.GetAreaID(base.transform.position);
	}

	private int GetMapID()
	{
		return AiUtil.GetMapID(base.transform.position);
	}

	private float GetNoiseValue()
	{
		return (float)mNoise.Noise(base.transform.position.x, base.transform.position.z * 100f) * 0.5f + 0.5f;
	}

	private int GetPathIDStory()
	{
		return AISpawnDataStory.GetRandomPathIDFromType(typeID, position);
	}

	private int GetPathIDNetwork()
	{
		if (mNoise == null)
		{
			return AISpawnDataAdvMulti.GetPathID(GetMapID(), GetAreaID(), typeID);
		}
		if (!mIsBoss)
		{
			return AISpawnDataAdvMulti.GetPathID(GetMapID(), GetAreaID(), typeID, GetNoiseValue());
		}
		return AISpawnDataAdvMulti.GetBossPathID(GetMapID(), GetAreaID(), typeID, GetNoiseValue());
	}

	private int GetPathIDAdventure()
	{
		if (mNoise == null)
		{
			return AISpawnDataAdvSingle.GetPathID(GetMapID(), GetAreaID(), typeID);
		}
		if (!mIsBoss)
		{
			return AISpawnDataAdvSingle.GetPathID(GetMapID(), GetAreaID(), typeID, GetNoiseValue());
		}
		return AISpawnDataAdvSingle.GetBossPathID(GetMapID(), GetAreaID(), typeID, GetNoiseValue());
	}

	private void OnDrawGizmosSelected()
	{
		if (mIsBoss)
		{
			Gizmos.color = Color.red;
		}
		else
		{
			Gizmos.color = Color.green;
		}
		Gizmos.DrawSphere(base.transform.position, 2f);
	}

	public static void WriteSPPoint(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		SPPoint sPPoint = obj as SPPoint;
		stream.Write(sPPoint.transform.position);
		stream.Write(sPPoint.mIndex);
		stream.Write(sPPoint.mType);
		stream.Write(sPPoint.mNodes.ToArray());
	}

	public static object ReadSPPoint(uLink.BitStream stream, params object[] codecOptions)
	{
		SPPoint sPPoint = new SPPoint();
		sPPoint.transform.position = stream.Read<Vector3>(new object[0]);
		sPPoint.mIndex = stream.Read<IntVector4>(new object[0]);
		sPPoint.mType = stream.Read<PointType>(new object[0]);
		sPPoint.mNodes = new List<IntVector4>();
		IntVector4[] collection = stream.Read<IntVector4[]>(new object[0]);
		sPPoint.mNodes.AddRange(collection);
		return sPPoint;
	}
}
