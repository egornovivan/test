using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPAutomatic : CommonInterface, ITowerDefenceData
{
	public static List<SPAutomatic> autos = new List<SPAutomatic>();

	private Transform mPointParent;

	private AISpawnWaveData mCurrentWave;

	private AISpawnAutomaticData mCurrentAutomatic;

	private int mID;

	private float mDelay;

	private int mKilledCount;

	private int mTotalCount;

	private float mStartTime;

	private float mDelayTime;

	private bool mGrounded;

	private bool mSpawning;

	private List<AiObject> mAiObjs = new List<AiObject>();

	public List<AiObject> aiObjs => mAiObjs;

	public int ID
	{
		get
		{
			return mID;
		}
		set
		{
			mID = value;
		}
	}

	public float Delay
	{
		get
		{
			return mDelay;
		}
		set
		{
			mDelay = value;
		}
	}

	public bool Grounded => mGrounded;

	public bool Spawning => mSpawning;

	public bool Begin => mCurrentWave != null;

	public float DelayTime => currentDelayTime;

	public int KilledCount => mKilledCount;

	public int TotalCount => mTotalCount;

	public float currentDelayTime => Mathf.Max(0f, mDelayTime + mStartTime - Time.time);

	public AISpawnAutomaticData automaticData => mCurrentAutomatic;

	public Transform pointParent
	{
		get
		{
			if (mPointParent == null)
			{
				mPointParent = new GameObject("Points").transform;
				mPointParent.parent = base.transform;
			}
			return mPointParent;
		}
	}

	public static bool IsSpawning()
	{
		foreach (SPAutomatic auto in autos)
		{
			if (auto != null && auto.Spawning)
			{
				return true;
			}
		}
		return false;
	}

	public static SPTowerDefence GetTowerDefence(int missionID)
	{
		foreach (SPAutomatic auto in autos)
		{
			SPTowerDefence sPTowerDefence = auto as SPTowerDefence;
			if (sPTowerDefence != null && sPTowerDefence.MissionID == missionID)
			{
				return sPTowerDefence;
			}
		}
		return null;
	}

	public void SyncTotalCount(int argCount)
	{
		mTotalCount = argCount;
	}

	public void SyncKilledCount(int argCount)
	{
		mKilledCount = argCount;
	}

	public void SyncWave(float time, float delay, int index)
	{
		mStartTime = time;
		mDelayTime = delay;
		if (mCurrentAutomatic != null)
		{
			mCurrentWave = mCurrentAutomatic.GetWaveData(index);
		}
	}

	public virtual void OnSpawned(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		obj.transform.parent = base.transform;
		AiObject component = obj.GetComponent<AiObject>();
		if (component != null)
		{
			component.DeathHandlerEvent += OnDeath;
			if (!mAiObjs.Contains(component))
			{
				mAiObjs.Add(component);
			}
		}
	}

	protected virtual void OnDeath(AiObject aiObj)
	{
		if (!(aiObj == null))
		{
			mKilledCount++;
		}
	}

	protected virtual void OnSpawnStart()
	{
	}

	protected virtual void OnSpawnComplete()
	{
	}

	protected virtual SPPoint Spawn(AISpawnData spData)
	{
		return null;
	}

	protected virtual void SpawnWave(AISpawnWaveData wave)
	{
	}

	public void SpawnAutomatic(int index = -1)
	{
		AISpawnAutomaticData aISpawnAutomaticData = AISpawnAutomaticData.CreateAutomaticData(mDelay, mID);
		if (aISpawnAutomaticData != null)
		{
			StartCoroutine(SpawnCoroutine(aISpawnAutomaticData, index));
		}
		else
		{
			Debug.LogError("Can't fin automatic data!");
		}
	}

	public void StopAutomatic()
	{
		mStartTime = 0f;
		mDelayTime = 0f;
		mSpawning = false;
		mCurrentWave = null;
		mCurrentAutomatic = null;
		HideAutomaticGUI();
		StopAllCoroutines();
	}

	public void ShowAutomaticGUI()
	{
	}

	public void HideAutomaticGUI()
	{
	}

	private IEnumerator SpawnCoroutine(AISpawnAutomaticData auto, int index)
	{
		mStartTime = Time.time;
		mSpawning = true;
		mCurrentWave = null;
		mCurrentAutomatic = auto;
		mTotalCount = 0;
		ShowAutomaticGUI();
		OnSpawnStart();
		if (index <= -1)
		{
			mDelayTime = auto.delayTime;
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_TD_Wave, mDelayTime, -1);
			}
			yield return new WaitForSeconds(auto.delayTime);
		}
		foreach (AISpawnWaveData wave in auto.data.data)
		{
			Debug.LogWarning("wave index = " + wave.index + " : index = " + index);
			if (wave.index < index)
			{
				continue;
			}
			mDelayTime = wave.delayTime;
			if (index > -1 && wave.index == index)
			{
				mDelayTime = 0f;
			}
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_TD_Wave, mDelayTime, wave.index);
			}
			mStartTime = Time.time;
			mCurrentWave = wave;
			yield return new WaitForSeconds(mDelayTime);
			SpawnWave(wave);
			foreach (AISpawnData sp in wave.data.data)
			{
				int number = Random.Range(sp.minCount, sp.maxCount + 1);
				mTotalCount += number;
				for (int i = 0; i < number; i++)
				{
					SPTerrainEvent.instance.RegisterSPPoint(Spawn(sp));
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
		mStartTime = 0f;
		mDelayTime = 0f;
		mSpawning = false;
		mCurrentWave = null;
		mCurrentAutomatic = null;
		HideAutomaticGUI();
		OnSpawnComplete();
		if (IsController)
		{
			RPCServer(EPacketType.PT_TD_End);
		}
	}

	public void Delete(float delayTime = 0f)
	{
		Object.Destroy(base.gameObject, delayTime);
	}

	private bool Match(IntVector4 node)
	{
		float num = base.transform.position.x - (float)node.x;
		float num2 = base.transform.position.y - (float)node.y;
		float num3 = base.transform.position.z - (float)node.z;
		return num >= float.Epsilon && num <= (float)(32 << node.w) && num2 >= float.Epsilon && num2 <= (float)(32 << node.w) && num3 >= float.Epsilon && num3 <= (float)(32 << node.w);
	}

	protected virtual void OnTerrainEnter(IntVector4 node)
	{
		mGrounded = true;
	}

	protected virtual void OnTerrainExit(IntVector4 node)
	{
		mGrounded = false;
		if (IsController)
		{
			OnSpawnComplete();
			StopAllCoroutines();
			RPCServer(EPacketType.PT_TD_End);
		}
	}

	private void OnTerrainColliderCreated(IntVector4 node)
	{
		if (Match(node))
		{
			OnTerrainEnter(node);
		}
	}

	private void OnTerrainColliderDestroy(IntVector4 node)
	{
		if (Match(node))
		{
			OnTerrainExit(node);
		}
	}

	public void Awake()
	{
		autos.Add(this);
	}

	public void Start()
	{
		LODOctreeMan.self.AttachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
	}

	public void OnDestroy()
	{
		foreach (AiObject mAiObj in mAiObjs)
		{
			if (mAiObj != null)
			{
				mAiObj.DeathHandlerEvent -= OnDeath;
			}
		}
		HideAutomaticGUI();
		if (LODOctreeMan.self != null)
		{
			LODOctreeMan.self.DetachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
		}
		autos.Remove(this);
	}
}
