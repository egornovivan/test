using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SPPlayerBase : SPAutomatic
{
	[Serializable]
	public class BaseTimer
	{
		public delegate void OnHourReadyDelegate(BaseTimer timer);

		public int minHour;

		public int maxHour;

		public int id;

		private int mStartHour;

		private int mCurrectWave;

		private bool mDirty;

		private float mDelayTime;

		private int mSpawnID;

		public float delayTime => mDelayTime;

		public int spawnId => mSpawnID;

		public int currentWave
		{
			get
			{
				return mCurrectWave;
			}
			set
			{
				mCurrectWave = value;
			}
		}

		public static event OnHourReadyDelegate OnHourReadyEvent;

		public void Export(BinaryWriter _out, int version)
		{
			_out.Write(minHour);
			_out.Write(maxHour);
			_out.Write(id);
			_out.Write(mStartHour);
			_out.Write(mCurrectWave);
			_out.Write(mDirty);
			if (version == 2)
			{
				_out.Write(mSpawnID);
				_out.Write(mDelayTime);
			}
		}

		public void Import(BinaryReader _in, int version)
		{
			minHour = _in.ReadInt32();
			maxHour = _in.ReadInt32();
			id = _in.ReadInt32();
			mStartHour = _in.ReadInt32();
			mCurrectWave = _in.ReadInt32();
			mDirty = _in.ReadBoolean();
			if (version == 2)
			{
				mSpawnID = _in.ReadInt32();
				mDelayTime = _in.ReadSingle();
			}
		}

		public override bool Equals(object obj)
		{
			BaseTimer baseTimer = (BaseTimer)obj;
			if (baseTimer == null)
			{
				return false;
			}
			return id == baseTimer.id && minHour == baseTimer.minHour && maxHour == baseTimer.maxHour;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public void SetStartHour()
		{
			mDirty = true;
			mCurrectWave = -1;
			mStartHour = UnityEngine.Random.Range(minHour, maxHour - 5);
			AISpawnPlayerBase randomPlayerBase = AISpawnPlayerBase.GetRandomPlayerBase(id, 1, 1);
			if (randomPlayerBase != null)
			{
				mDelayTime = randomPlayerBase.delayTime;
				mSpawnID = randomPlayerBase.spawnID;
			}
		}

		public void SyncTimer(BaseTimer timer)
		{
			mDirty = true;
			mCurrectWave = timer.mCurrectWave;
			mStartHour = timer.mStartHour;
			mSpawnID = timer.spawnId;
			mDelayTime = timer.delayTime;
		}

		public void ClearStartHour()
		{
			mDirty = false;
			mDelayTime = 0f;
			mSpawnID = 0;
			mCurrectWave = -1;
			mStartHour = -1;
		}

		public void OnHourTick(int hour)
		{
			if (mDirty && hour >= mStartHour && !SPAutomatic.IsSpawning())
			{
				if (BaseTimer.OnHourReadyEvent != null)
				{
					BaseTimer.OnHourReadyEvent(this);
				}
				mDirty = false;
			}
		}
	}

	private const int RecordVersion = 2;

	[HideInInspector]
	public float damage;

	public float minRadius;

	public float maxRadius;

	public float minInterval;

	public float maxInterval;

	[HideInInspector]
	public float damageRadius;

	public int cycleHours;

	public BaseTimer[] timers;

	private BaseTimer mCurrentTimer;

	private BaseTimer mLastTimer;

	private BaseTimer mRecordTimer;

	private List<SPPointSimulate> simulates = new List<SPPointSimulate>();

	private static SPPlayerBase mSinglePlayerBase;

	public static SPPlayerBase Single => mSinglePlayerBase;

	public static event AssetReq.ReqFinishDelegate OnSpawnedEvent;

	public void ApplySimulateDamage(float damage)
	{
		List<SPPointSimulate> list = simulates.FindAll((SPPointSimulate ret) => ret != null && ret.isDamage);
		if (list != null && list.Count != 0)
		{
			list[UnityEngine.Random.Range(0, list.Count)].ApplyDamage(damage);
		}
	}

	public void ApplySimulateDamage(float damage, Vector3 position, float radius)
	{
		List<SPPointSimulate> list = simulates.FindAll((SPPointSimulate ret) => ret != null && ret.isDamage && (ret.position - position).sqrMagnitude <= radius * radius);
		if (list != null && list.Count != 0)
		{
			list[UnityEngine.Random.Range(0, list.Count)].ApplyDamage(damage);
		}
	}

	public void Export(BinaryWriter bw)
	{
		bw.Write(2);
		if (mCurrentTimer == null)
		{
			bw.Write(0);
			return;
		}
		FixWaveIndex();
		bw.Write(1);
		mCurrentTimer.Export(bw, 2);
	}

	public void Import(byte[] buffer)
	{
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int version = binaryReader.ReadInt32();
		int num = binaryReader.ReadInt32();
		if (num == 1)
		{
			mRecordTimer = new BaseTimer();
			mRecordTimer.Import(binaryReader, version);
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	private void FixWaveIndex()
	{
		if (mCurrentTimer != null)
		{
			AISpawnAutomaticData aISpawnAutomaticData = AISpawnAutomaticData.CreateAutomaticData(0f, mCurrentTimer.spawnId);
			if (aISpawnAutomaticData != null && mCurrentTimer.currentWave == aISpawnAutomaticData.data.data.Count - 1 && GetComponentsInChildren<SPPointSimulate>().Length == 0)
			{
				mCurrentTimer.currentWave = aISpawnAutomaticData.data.data.Count;
			}
		}
	}

	protected override void SpawnWave(AISpawnWaveData wave)
	{
		base.SpawnWave(wave);
		if (mCurrentTimer != null)
		{
			mCurrentTimer.currentWave = wave.index;
		}
		Debug.Log("Current wave index = " + wave.index);
	}

	protected override SPPoint Spawn(AISpawnData spData)
	{
		base.Spawn(spData);
		if (GetPositionAndRotation(spData, out var pos, out var rot, out var target))
		{
			SPPointSimulate sPPointSimulate = SPPoint.InstantiateSPPoint<SPPointSimulate>(pos, rot, IntVector4.Zero, base.pointParent, (!spData.isPath) ? spData.spID : 0, spData.isPath ? spData.spID : 0, isActive: true, revisePos: true, isBoss: false, erode: false, delete: true, null, OnSpawned, this);
			sPPointSimulate.SetData(damage, minInterval, maxInterval, damageRadius);
			sPPointSimulate.targetPos = target;
			if (!simulates.Contains(sPPointSimulate))
			{
				simulates.Add(sPPointSimulate);
			}
			return sPPointSimulate;
		}
		return null;
	}

	private Transform GetAttackTransform()
	{
		List<Transform> list = new List<Transform>();
		if (CSMain.s_MgCreator.Assembly != null && CSMain.s_MgCreator.Assembly.gameObject != null)
		{
			list.Add(CSMain.s_MgCreator.Assembly.gameObject.transform);
		}
		foreach (KeyValuePair<int, CSCommon> commonEntity in CSMain.s_MgCreator.GetCommonEntities())
		{
			if (commonEntity.Value.gameObject != null)
			{
				list.Add(commonEntity.Value.gameObject.transform);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return null;
	}

	protected override void OnSpawnStart()
	{
		base.OnSpawnStart();
		GameTime.Lock(base.gameObject);
		simulates.Clear();
	}

	protected override void OnSpawnComplete()
	{
		base.OnSpawnComplete();
		GameTime.UnLock(base.gameObject);
	}

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		AiObject component = obj.GetComponent<AiObject>();
		if (component != null)
		{
			component.tdInfo = GetAttackTransform();
		}
		SPGroup component2 = obj.GetComponent<SPGroup>();
		if (component2 != null)
		{
			component2.tdInfo = GetAttackTransform();
		}
		if (SPPlayerBase.OnSpawnedEvent != null)
		{
			SPPlayerBase.OnSpawnedEvent(component.gameObject);
		}
	}

	private IEnumerator CheckAttacktransform()
	{
		while (true)
		{
			foreach (AiObject aiObj in base.aiObjs)
			{
				if (aiObj != null && aiObj.tdInfo == null && CSMain.s_MgCreator.Assembly != null && CSMain.s_MgCreator.Assembly.Data.m_Durability > float.Epsilon)
				{
					aiObj.tdInfo = GetAttackTransform();
				}
			}
			yield return new WaitForSeconds(2f);
		}
	}

	private Vector3 GetRandom()
	{
		Dictionary<int, CSCommon> commonEntities = CSMain.s_MgCreator.GetCommonEntities();
		List<int> list = new List<int>(commonEntities.Keys);
		if (commonEntities != null && commonEntities.Count > 0)
		{
			return commonEntities[list[UnityEngine.Random.Range(0, list.Count)]].Position;
		}
		if (CSMain.s_MgCreator.Assembly != null)
		{
			return CSMain.s_MgCreator.Assembly.Position;
		}
		return Vector3.zero;
	}

	private bool IsInCSCommon(Vector3 position)
	{
		foreach (KeyValuePair<int, CSCommon> commonEntity in CSMain.s_MgCreator.GetCommonEntities())
		{
			Vector3 pos = new Vector3(position.x, commonEntity.Value.Position.y + 0.5f, position.z);
			if (commonEntity.Value.ContainPoint(pos))
			{
				return true;
			}
		}
		return false;
	}

	private IntVector4 GetNode(IntVector4 node)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < 5; i++)
		{
			switch (UnityEngine.Random.Range(0, 4))
			{
			case 0:
				if (num >> 5 > 0)
				{
					return new IntVector4(node.x + num, node.y, node.z, node.w);
				}
				break;
			case 1:
				if (num2 >> 5 < 0)
				{
					return new IntVector4(node.x + num2, node.y, node.z, node.w);
				}
				break;
			case 2:
				if (num3 >> 5 > 0)
				{
					return new IntVector4(node.x, node.y, node.z + num3, node.w);
				}
				break;
			case 3:
				if (num4 >> 5 < 0)
				{
					return new IntVector4(node.x, node.y, node.z + num4, node.w);
				}
				break;
			}
		}
		return node;
	}

	private Vector3 GetPosition(IntVector4 node)
	{
		for (int i = 0; i < 10; i++)
		{
			Vector3 randomPosition = AiUtil.GetRandomPosition(node);
			if (!IsInCSCommon(randomPosition))
			{
				return randomPosition;
			}
		}
		return Vector3.zero;
	}

	private Vector3 GetPositionFromCollider(IntVector4 node)
	{
		return GetPosition(node);
	}

	private Vector3 GetRandomPosition(AISpawnData data, Vector3 center)
	{
		for (int i = 0; i < 10; i++)
		{
			Vector3 randomPosition = AiUtil.GetRandomPosition(center, minRadius, maxRadius, Vector3.forward, data.minAngle, data.maxAngle);
			if (!IsInCSCommon(randomPosition))
			{
				return randomPosition;
			}
		}
		return Vector3.zero;
	}

	private bool GetPositionAndRotation(AISpawnData data, out Vector3 pos, out Quaternion rot, out Vector3 target)
	{
		pos = Vector3.zero;
		rot = Quaternion.identity;
		target = Vector3.zero;
		Vector3 random = GetRandom();
		if (random != Vector3.zero)
		{
			IntVector4 node = AiUtil.ConvertToIntVector4(random, 0);
			pos = GetPosition(node);
			target = Vector3.zero;
			if (pos != Vector3.zero)
			{
				rot = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up);
				return true;
			}
		}
		return false;
	}

	private int GetCurrentHour()
	{
		return (int)CSMain.s_MgCreator.Timer.Hour % cycleHours;
	}

	private void RegisterEvent()
	{
		BaseTimer.OnHourReadyEvent += OnBaseTimerReady;
		LODOctreeMan.self.AttachEvents(OnTerrainMeshCreated, null, null, null);
	}

	private void RemoveEvent()
	{
		BaseTimer.OnHourReadyEvent -= OnBaseTimerReady;
		LODOctreeMan.self.DetachEvents(OnTerrainMeshCreated, null, null, null);
	}

	private BaseTimer GetCurrentTimer()
	{
		BaseTimer[] array = timers;
		foreach (BaseTimer baseTimer in array)
		{
			if (GetCurrentHour() >= baseTimer.minHour && GetCurrentHour() <= baseTimer.maxHour)
			{
				return baseTimer;
			}
		}
		return null;
	}

	private void UpdateBaseTimer()
	{
		BaseTimer currentTimer = GetCurrentTimer();
		if (mCurrentTimer != currentTimer)
		{
			if (mCurrentTimer != null)
			{
				OnBaseTimerExit(mCurrentTimer);
			}
			mCurrentTimer = currentTimer;
			if (mCurrentTimer != null)
			{
				OnBaseTimerEnter(mCurrentTimer);
			}
		}
		if (mCurrentTimer != null)
		{
			OnBaseTimerTick(mCurrentTimer);
		}
	}

	private void UpdateAssemblyHP()
	{
		if (base.automaticData != null && (CSMain.s_MgCreator.Assembly == null || CSMain.s_MgCreator.Assembly.Data.m_Durability <= float.Epsilon))
		{
			StopAutomatic();
		}
	}

	private void OnBaseTimerEnter(BaseTimer timer)
	{
		if (mRecordTimer != null && mRecordTimer.Equals(timer))
		{
			timer.SyncTimer(mRecordTimer);
		}
		else
		{
			timer.SetStartHour();
		}
	}

	private void OnBaseTimerExit(BaseTimer timer)
	{
		timer.ClearStartHour();
	}

	private void OnBaseTimerTick(BaseTimer timer)
	{
		timer.OnHourTick(GetCurrentHour());
	}

	private void OnBaseTimerReady(BaseTimer timer)
	{
		if (CSMain.s_MgCreator.Assembly != null && CSMain.s_MgCreator.Assembly.Data.m_Durability > float.Epsilon && timer.spawnId > 0)
		{
			base.ID = timer.spawnId;
			base.Delay = timer.delayTime;
			SpawnAutomatic(timer.currentWave);
			GameTime.ClearTimerPass();
		}
	}

	private void OnTerrainMeshCreated(IntVector4 node)
	{
		if (CSMain.s_MgCreator.Assembly == null || !VFVoxelTerrain.TerrainColliderComplete || node.w != 0 || base.automaticData == null)
		{
			return;
		}
		float num = CSMain.s_MgCreator.Assembly.Position.x - (float)node.x;
		float num2 = CSMain.s_MgCreator.Assembly.Position.y - (float)node.y;
		float num3 = CSMain.s_MgCreator.Assembly.Position.z - (float)node.z;
		float num4 = 32 << node.w;
		if (!(num >= float.Epsilon) || !(num <= num4) || !(num2 >= float.Epsilon) || !(num2 <= num4) || !(num3 >= float.Epsilon) || !(num3 <= num4))
		{
			return;
		}
		foreach (KeyValuePair<int, CSCommon> commonEntity in CSMain.s_MgCreator.GetCommonEntities())
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				DigTerrainManager.DestroyTerrainInRange(1, commonEntity.Value.Position, 255f, 10f);
			}
		}
	}

	public new void Awake()
	{
		base.Awake();
		if (mSinglePlayerBase == null)
		{
			mSinglePlayerBase = this;
		}
		else
		{
			Debug.LogError("Have too many SPPlayerBase!!");
		}
	}

	public new void Start()
	{
		base.Start();
		RegisterEvent();
		StartCoroutine(CheckAttacktransform());
	}

	private void Update()
	{
		UpdateBaseTimer();
		UpdateAssemblyHP();
	}

	public new void OnDestroy()
	{
		base.OnDestroy();
		RemoveEvent();
		GameTime.UnLock(base.gameObject);
	}
}
