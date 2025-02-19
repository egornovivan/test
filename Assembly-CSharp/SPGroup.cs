using System.Collections;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class SPGroup : CommonInterface
{
	public GameObject aiPrefab;

	private AiObject mHeader;

	private AiBehaveGroup mBehaveGroup;

	private List<AiObject> mAiObjects = new List<AiObject>();

	private List<AiObject> mActiveAiObjects = new List<AiObject>();

	private List<AssetReq> mReqs = new List<AssetReq>();

	private bool mSpawned;

	private int mMaxHp;

	private int mCount;

	private IntVector4 mIndex;

	private int mSpawnedCount;

	private Transform mTDInfo;

	public int curCount => ++mCount;

	public int spawnedCount
	{
		get
		{
			return mSpawnedCount;
		}
		set
		{
			mSpawnedCount = value;
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

	public List<AiObject> aiObjects => mAiObjects;

	public List<AiObject> activeAiObjects => mActiveAiObjects;

	public Transform tdInfo
	{
		get
		{
			return mTDInfo;
		}
		set
		{
			mTDInfo = value;
		}
	}

	public bool spawned
	{
		get
		{
			return mSpawned;
		}
		set
		{
			if (mSpawned != value)
			{
				if (value)
				{
					StartCoroutine(DestroyGroup(2f));
				}
				mSpawned = value;
			}
		}
	}

	public bool hasEnemy => false;

	public AiObject header
	{
		get
		{
			if (mHeader == null || mHeader.dead)
			{
				foreach (AiObject mActiveAiObject in mActiveAiObjects)
				{
					if (mActiveAiObject != null && !mActiveAiObject.dead)
					{
						mHeader = mActiveAiObject;
						break;
					}
				}
			}
			return mHeader;
		}
	}

	public float damage
	{
		get
		{
			float num = 0f;
			foreach (AiObject mActiveAiObject in mActiveAiObjects)
			{
				if (mActiveAiObject != null && !mActiveAiObject.dead)
				{
					num += mActiveAiObject.GetAttribute(AttribType.Atk);
				}
			}
			return num;
		}
	}

	public int Hp
	{
		get
		{
			int num = 0;
			foreach (AiObject mActiveAiObject in mActiveAiObjects)
			{
				num += mActiveAiObject.life;
			}
			return num;
		}
	}

	public int maxHp => mMaxHp;

	public float hpPercent => (float)Hp / (float)maxHp;

	public event AssetReq.ReqFinishDelegate OnSpawndEvent;

	public virtual IEnumerator SpawnGroup()
	{
		yield return null;
	}

	private void Start()
	{
		SpawnAiPrefab();
		if (!GameConfig.IsMultiMode)
		{
			Spawn();
		}
		else
		{
			InitMember();
		}
	}

	private void OnDestroy()
	{
	}

	public void SetOwenerView()
	{
		if (IsController)
		{
			if (mBehaveGroup != null)
			{
				mBehaveGroup.enabled = true;
			}
		}
		else if (mBehaveGroup != null)
		{
			mBehaveGroup.enabled = false;
		}
	}

	public void SetRunSpeed()
	{
		foreach (AiObject mAiObject in mAiObjects)
		{
			if (mAiObject != null)
			{
				mAiObject.speed = mAiObject.runSpeed;
			}
		}
	}

	public void SetWalkSpeed()
	{
		foreach (AiObject mAiObject in mAiObjects)
		{
			if (mAiObject != null)
			{
				mAiObject.speed = mAiObject.walkSpeed;
			}
		}
	}

	public void Spawn()
	{
		StartCoroutine(SpawnAIGroup());
	}

	public void Instantiate(int id, Vector3 pos, Quaternion rot)
	{
		Vector3 position = AIResource.FixedHeightOfAIResource(id, pos);
		if (!GameConfig.IsMultiMode)
		{
			mSpawnedCount++;
			AssetReq item = AIResource.Instantiate(id, position, rot, OnSpawned);
			mReqs.Add(item);
		}
	}

	private void OnSpawned(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		go.transform.parent = base.transform;
		AiObject component = go.GetComponent<AiObject>();
		if (component != null)
		{
			if (mTDInfo != null)
			{
				component.tdInfo = mTDInfo;
			}
			RegisterAiObjects(component);
		}
		if (this.OnSpawndEvent != null)
		{
			this.OnSpawndEvent(go);
		}
	}

	private void SpawnAiPrefab()
	{
		if (!(aiPrefab != null))
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(aiPrefab, base.transform.position, base.transform.rotation) as GameObject;
		if (gameObject != null)
		{
			gameObject.transform.parent = base.transform;
			mBehaveGroup = gameObject.GetComponent<AiBehaveGroup>();
			if (mBehaveGroup != null)
			{
				mBehaveGroup.RegisterSPGroup(this);
			}
		}
	}

	private void AddGroupMaxHP(AiObject aiObj)
	{
		mMaxHp += aiObj.maxLife;
	}

	private void Delete()
	{
		foreach (AssetReq mReq in mReqs)
		{
			if (mReq != null)
			{
				mReq.Deactivate();
				mReq.ReqFinishHandler -= OnSpawned;
			}
		}
		Object.Destroy(base.gameObject, 0.5f);
	}

	private void InitMember()
	{
		if (!GameConfig.IsMultiMode)
		{
		}
	}

	private IEnumerator SpawnAIGroup()
	{
		OnSpawnGroupStart();
		yield return StartCoroutine(SpawnGroup());
		OnSpawnGroupEnd();
	}

	private IEnumerator DestroyGroup(float interval)
	{
		yield return new WaitForSeconds(10f);
		while (GetComponentsInChildren<AiObject>().Length > 0)
		{
			yield return new WaitForSeconds(interval);
		}
		Delete();
	}

	public void RegisterAiObjects(AiObject aiObj)
	{
		if (!mAiObjects.Contains(aiObj))
		{
			aiObj.transform.parent = base.transform;
			aiObj.DeathHandlerEvent += OnAiObjectDeath;
			aiObj.DestroyHandlerEvent += OnAiObjectDestroy;
			aiObj.ActiveHandlerEvent += OnAiObjectActive;
			aiObj.InactiveHandlerEvent += OnAiObjectDeActive;
			mAiObjects.Add(aiObj);
			RegisterActiveAiObjects(aiObj);
			AddGroupMaxHP(aiObj);
		}
	}

	public void RemoveAiObjects(AiObject aiObj)
	{
		if (mAiObjects.Contains(aiObj))
		{
			mAiObjects.Remove(aiObj);
			RemoveActiveAiObjects(aiObj);
		}
	}

	private void RegisterActiveAiObjects(AiObject aiObj)
	{
		if (!mActiveAiObjects.Contains(aiObj))
		{
			mActiveAiObjects.Add(aiObj);
		}
	}

	private void RemoveActiveAiObjects(AiObject aiObj)
	{
		if (mActiveAiObjects.Contains(aiObj))
		{
			mActiveAiObjects.Remove(aiObj);
		}
	}

	protected virtual void OnSpawnGroupStart()
	{
	}

	protected virtual void OnSpawnGroupEnd()
	{
		Spawned();
		spawned = true;
	}

	protected virtual void OnAiObjectDeath(AiObject aiObj)
	{
		RemoveActiveAiObjects(aiObj);
	}

	protected virtual void OnAiObjectDestroy(AiObject aiObj)
	{
		RemoveAiObjects(aiObj);
		RemoveActiveAiObjects(aiObj);
	}

	protected virtual void OnAiObjectActive(AiObject aiObj)
	{
		RegisterActiveAiObjects(aiObj);
	}

	protected virtual void OnAiObjectDeActive(AiObject aiObj)
	{
		RemoveActiveAiObjects(aiObj);
	}

	private void Spawned()
	{
	}

	public void ClearHatredAll()
	{
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AG_ClearHatredAll);
		}
	}

	public void ClearMoveAndRotation()
	{
		foreach (AiObject mAiObject in mAiObjects)
		{
			if (mAiObject != null)
			{
				mAiObject.StopMoveAndRotation();
			}
		}
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AG_ClearMoveAndRotation);
		}
	}

	public void ActivateSingleBehave(bool value)
	{
		foreach (AiObject mAiObject in mAiObjects)
		{
			if (mAiObject != null && mAiObject.behave != null && mAiObject.behave.isMember)
			{
				mAiObject.behave.isActive = value;
			}
		}
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AG_ActivateSingleBehave, value);
		}
	}

	public void MoveToPositionArmy(Vector3 position)
	{
		foreach (AiObject activeAiObject in activeAiObjects)
		{
			Vector3 desiredMoveDestination = position + header.transform.TransformDirection(activeAiObject.offset - header.offset);
			activeAiObject.desiredMoveDestination = desiredMoveDestination;
		}
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AG_MoveToPositionArmy, position);
		}
	}

	public void MoveToPosition(Vector3 position)
	{
		foreach (AiObject activeAiObject in activeAiObjects)
		{
			if (!(activeAiObject == null) && !(activeAiObject.behave == null) && !activeAiObject.behave.running && (!GameConfig.IsMultiMode || activeAiObject.IsController))
			{
				Vector3 vector = position + header.transform.TransformDirection(activeAiObject.offset - header.offset);
				if (AiUtil.SqrMagnitudeH(vector - activeAiObject.position) > 1f)
				{
					activeAiObject.desiredMoveDestination = vector;
				}
				else
				{
					activeAiObject.desiredMoveDestination = Vector3.zero;
				}
				activeAiObject.speed = activeAiObject.walkSpeed;
			}
		}
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AG_MoveToPosition, position);
		}
	}

	public void MoveToPositionFree()
	{
	}

	public void RunawayForPosition(Vector3 position)
	{
		if (!GameConfig.IsMultiMode)
		{
			foreach (AiObject activeAiObject in activeAiObjects)
			{
				if (activeAiObject != null && !activeAiObject.dead && (!GameConfig.IsMultiMode || activeAiObject.IsController))
				{
					Vector3 normalized = (activeAiObject.position - position).normalized;
					normalized = AiUtil.RunawayDirectionCorrect(activeAiObject, normalized);
					activeAiObject.speed = activeAiObject.runSpeed;
					activeAiObject.desiredMoveDestination = activeAiObject.position + normalized.normalized * 2f;
				}
			}
		}
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AG_RunawayForPosition, position);
		}
	}
}
