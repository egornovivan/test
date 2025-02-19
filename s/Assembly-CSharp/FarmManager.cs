using System.Collections;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Mono.Data.SqliteClient;
using UnityEngine;

public class FarmManager : MonoBehaviour
{
	public delegate void PlantEvent(FarmPlantLogic plant);

	public const int Default_WorldId = 200;

	private const int Version = 6;

	private const int Version001 = 2016110100;

	private static FarmManager mInstance;

	private int frameCount;

	public static int curVersion = 2016110100;

	public static Dictionary<int, FarmPlantLogic> mPlantMap = new Dictionary<int, FarmPlantLogic>();

	public static Dictionary<IntVec3, int> mPlantHelpMap = new Dictionary<IntVec3, int>();

	public static Dictionary<int, List<FarmPlantLogic>> mPlantWorldIdMap = new Dictionary<int, List<FarmPlantLogic>>();

	public static FarmManager Instance => mInstance;

	public event PlantEvent CreatePlantEvent;

	public event PlantEvent RemovePlantEvent;

	private void Awake()
	{
		mInstance = this;
		StartCoroutine(AsyncSave());
	}

	public static void AddPlantToWorldIdMap(int worldId, FarmPlantLogic fpl)
	{
		if (mPlantWorldIdMap.ContainsKey(worldId))
		{
			mPlantWorldIdMap[worldId].Add(fpl);
			return;
		}
		mPlantWorldIdMap.Add(worldId, new List<FarmPlantLogic>());
		mPlantWorldIdMap[worldId].Add(fpl);
	}

	public static ItemObject[] ExportItemObj(int worldId)
	{
		List<ItemObject> list = new List<ItemObject>();
		if (!mPlantWorldIdMap.ContainsKey(worldId))
		{
			return null;
		}
		List<FarmPlantLogic> list2 = mPlantWorldIdMap[worldId];
		foreach (FarmPlantLogic item in list2)
		{
			list.Add(ItemManager.GetItemByID(item.mPlantInstanceId));
		}
		return list.ToArray();
	}

	public static byte[] ExportToByte(int worldId)
	{
		if (!mPlantWorldIdMap.ContainsKey(worldId))
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(curVersion);
		List<FarmPlantLogic> list = mPlantWorldIdMap[worldId];
		binaryWriter.Write(list.Count);
		foreach (FarmPlantLogic item in list)
		{
			binaryWriter.Write(item.mPlantInstanceId);
			binaryWriter.Write(item._PlantType);
			binaryWriter.Write(item.mPos.x);
			binaryWriter.Write(item.mPos.y);
			binaryWriter.Write(item.mPos.z);
			binaryWriter.Write(item.mRot.x);
			binaryWriter.Write(item.mRot.y);
			binaryWriter.Write(item.mRot.z);
			binaryWriter.Write(item.mRot.w);
			binaryWriter.Write(item.mPutOutGameTime);
			binaryWriter.Write(item.mLife);
			binaryWriter.Write(item.mWater);
			binaryWriter.Write(item.mClean);
			binaryWriter.Write(item.mDead);
			binaryWriter.Write(item.mGrowTimeIndex);
			binaryWriter.Write(item.mCurGrowTime);
			binaryWriter.Write(item.mTerrianType);
			binaryWriter.Write(item.mGrowRate);
			binaryWriter.Write(item.mExtraGrowRate);
			binaryWriter.Write(item.mNpcGrowRate);
			binaryWriter.Write(item.mLastUpdateTime);
		}
		binaryWriter.Close();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	public static byte[] Export()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(curVersion);
		binaryWriter.Write(mPlantMap.Count);
		foreach (int key in mPlantMap.Keys)
		{
			FarmPlantLogic farmPlantLogic = mPlantMap[key];
			binaryWriter.Write(farmPlantLogic.worldId);
			binaryWriter.Write(farmPlantLogic.mPlantInstanceId);
			binaryWriter.Write(farmPlantLogic._PlantType);
			binaryWriter.Write(farmPlantLogic.mPos.x);
			binaryWriter.Write(farmPlantLogic.mPos.y);
			binaryWriter.Write(farmPlantLogic.mPos.z);
			binaryWriter.Write(farmPlantLogic.mRot.x);
			binaryWriter.Write(farmPlantLogic.mRot.y);
			binaryWriter.Write(farmPlantLogic.mRot.z);
			binaryWriter.Write(farmPlantLogic.mRot.w);
			binaryWriter.Write(farmPlantLogic.mPutOutGameTime);
			binaryWriter.Write(farmPlantLogic.mLife);
			binaryWriter.Write(farmPlantLogic.mWater);
			binaryWriter.Write(farmPlantLogic.mClean);
			binaryWriter.Write(farmPlantLogic.mDead);
			binaryWriter.Write(farmPlantLogic.mGrowTimeIndex);
			binaryWriter.Write(farmPlantLogic.mCurGrowTime);
			binaryWriter.Write(farmPlantLogic.mTerrianType);
			binaryWriter.Write(farmPlantLogic.mGrowRate);
			binaryWriter.Write(farmPlantLogic.mExtraGrowRate);
			binaryWriter.Write(farmPlantLogic.mNpcGrowRate);
			binaryWriter.Write(farmPlantLogic.mLastUpdateTime);
		}
		binaryWriter.Close();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	public static void Import(byte[] buffer)
	{
		mPlantMap.Clear();
		mPlantHelpMap.Clear();
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		switch (num)
		{
		case 1:
		{
			for (int n = 0; n < num2; n++)
			{
				FarmPlantLogic farmPlantLogic6 = new FarmPlantLogic();
				farmPlantLogic6.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantLogic6._PlantType = binaryReader.ReadInt32();
				farmPlantLogic6.mPutOutGameTime = binaryReader.ReadSingle();
				farmPlantLogic6.mLife = binaryReader.ReadSingle();
				farmPlantLogic6.mWater = binaryReader.ReadSingle();
				farmPlantLogic6.mClean = binaryReader.ReadSingle();
				farmPlantLogic6.mDead = binaryReader.ReadBoolean();
				farmPlantLogic6.mGrowTimeIndex = binaryReader.ReadInt32();
				mPlantMap[farmPlantLogic6.mPlantInstanceId] = farmPlantLogic6;
				AddPlantToWorldIdMap(farmPlantLogic6.worldId, farmPlantLogic6);
			}
			break;
		}
		case 2:
		{
			for (int j = 0; j < num2; j++)
			{
				FarmPlantLogic farmPlantLogic2 = new FarmPlantLogic();
				farmPlantLogic2.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantLogic2._PlantType = binaryReader.ReadInt32();
				farmPlantLogic2.mPutOutGameTime = binaryReader.ReadDouble();
				farmPlantLogic2.mLife = binaryReader.ReadDouble();
				farmPlantLogic2.mWater = binaryReader.ReadDouble();
				farmPlantLogic2.mClean = binaryReader.ReadDouble();
				farmPlantLogic2.mDead = binaryReader.ReadBoolean();
				farmPlantLogic2.mGrowTimeIndex = binaryReader.ReadInt32();
				mPlantMap[farmPlantLogic2.mPlantInstanceId] = farmPlantLogic2;
				AddPlantToWorldIdMap(farmPlantLogic2.worldId, farmPlantLogic2);
			}
			break;
		}
		case 3:
		{
			for (int l = 0; l < num2; l++)
			{
				FarmPlantLogic farmPlantLogic4 = new FarmPlantLogic();
				farmPlantLogic4.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantLogic4._PlantType = binaryReader.ReadInt32();
				farmPlantLogic4.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantLogic4.mPutOutGameTime = binaryReader.ReadDouble();
				farmPlantLogic4.mLife = binaryReader.ReadDouble();
				farmPlantLogic4.mWater = binaryReader.ReadDouble();
				farmPlantLogic4.mClean = binaryReader.ReadDouble();
				farmPlantLogic4.mDead = binaryReader.ReadBoolean();
				farmPlantLogic4.mGrowTimeIndex = binaryReader.ReadInt32();
				mPlantMap[farmPlantLogic4.mPlantInstanceId] = farmPlantLogic4;
				mPlantHelpMap[new IntVec3(farmPlantLogic4.mPos)] = farmPlantLogic4.mPlantInstanceId;
				AddPlantToWorldIdMap(farmPlantLogic4.worldId, farmPlantLogic4);
			}
			break;
		}
		case 4:
		{
			for (int num3 = 0; num3 < num2; num3++)
			{
				FarmPlantLogic farmPlantLogic7 = new FarmPlantLogic();
				farmPlantLogic7.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantLogic7._PlantType = binaryReader.ReadInt32();
				farmPlantLogic7.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantLogic7.mRot = new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantLogic7.mPutOutGameTime = binaryReader.ReadDouble();
				farmPlantLogic7.mLife = binaryReader.ReadDouble();
				farmPlantLogic7.mWater = binaryReader.ReadDouble();
				farmPlantLogic7.mClean = binaryReader.ReadDouble();
				farmPlantLogic7.mDead = binaryReader.ReadBoolean();
				farmPlantLogic7.mGrowTimeIndex = binaryReader.ReadInt32();
				farmPlantLogic7.mCurGrowTime = binaryReader.ReadDouble();
				farmPlantLogic7.mTerrianType = binaryReader.ReadByte();
				farmPlantLogic7.mExtraGrowRate = binaryReader.ReadSingle();
				farmPlantLogic7.InitGrowRate(farmPlantLogic7.mExtraGrowRate);
				mPlantMap[farmPlantLogic7.mPlantInstanceId] = farmPlantLogic7;
				mPlantHelpMap[new IntVec3(farmPlantLogic7.mPos)] = farmPlantLogic7.mPlantInstanceId;
				AddPlantToWorldIdMap(farmPlantLogic7.worldId, farmPlantLogic7);
			}
			break;
		}
		case 5:
		{
			for (int m = 0; m < num2; m++)
			{
				FarmPlantLogic farmPlantLogic5 = new FarmPlantLogic();
				farmPlantLogic5.worldId = binaryReader.ReadInt32();
				farmPlantLogic5.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantLogic5._PlantType = binaryReader.ReadInt32();
				farmPlantLogic5.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantLogic5.mRot = new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantLogic5.mPutOutGameTime = binaryReader.ReadDouble();
				farmPlantLogic5.mLife = binaryReader.ReadDouble();
				farmPlantLogic5.mWater = binaryReader.ReadDouble();
				farmPlantLogic5.mClean = binaryReader.ReadDouble();
				farmPlantLogic5.mDead = binaryReader.ReadBoolean();
				farmPlantLogic5.mGrowTimeIndex = binaryReader.ReadInt32();
				farmPlantLogic5.mCurGrowTime = binaryReader.ReadDouble();
				Debug.LogError("import grow:" + farmPlantLogic5.mCurGrowTime);
				farmPlantLogic5.mTerrianType = binaryReader.ReadByte();
				farmPlantLogic5.mExtraGrowRate = binaryReader.ReadSingle();
				farmPlantLogic5.InitGrowRate(farmPlantLogic5.mExtraGrowRate);
				mPlantMap[farmPlantLogic5.mPlantInstanceId] = farmPlantLogic5;
				mPlantHelpMap[new IntVec3(farmPlantLogic5.mPos)] = farmPlantLogic5.mPlantInstanceId;
				AddPlantToWorldIdMap(farmPlantLogic5.worldId, farmPlantLogic5);
			}
			break;
		}
		case 6:
		{
			for (int k = 0; k < num2; k++)
			{
				FarmPlantLogic farmPlantLogic3 = new FarmPlantLogic();
				farmPlantLogic3.worldId = binaryReader.ReadInt32();
				farmPlantLogic3.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantLogic3._PlantType = binaryReader.ReadInt32();
				farmPlantLogic3.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantLogic3.mRot = new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantLogic3.mPutOutGameTime = binaryReader.ReadDouble();
				farmPlantLogic3.mLife = binaryReader.ReadDouble();
				farmPlantLogic3.mWater = binaryReader.ReadDouble();
				farmPlantLogic3.mClean = binaryReader.ReadDouble();
				farmPlantLogic3.mDead = binaryReader.ReadBoolean();
				farmPlantLogic3.mGrowTimeIndex = binaryReader.ReadInt32();
				farmPlantLogic3.mCurGrowTime = binaryReader.ReadDouble();
				farmPlantLogic3.mTerrianType = binaryReader.ReadByte();
				farmPlantLogic3.mExtraGrowRate = binaryReader.ReadSingle();
				farmPlantLogic3.mLastUpdateTime = binaryReader.ReadDouble();
				farmPlantLogic3.InitGrowRate(farmPlantLogic3.mExtraGrowRate);
				mPlantMap[farmPlantLogic3.mPlantInstanceId] = farmPlantLogic3;
				mPlantHelpMap[new IntVec3(farmPlantLogic3.mPos)] = farmPlantLogic3.mPlantInstanceId;
				AddPlantToWorldIdMap(farmPlantLogic3.worldId, farmPlantLogic3);
			}
			break;
		}
		case 2016110100:
		{
			for (int i = 0; i < num2; i++)
			{
				FarmPlantLogic farmPlantLogic = new FarmPlantLogic();
				farmPlantLogic.worldId = binaryReader.ReadInt32();
				farmPlantLogic.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantLogic._PlantType = binaryReader.ReadInt32();
				farmPlantLogic.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantLogic.mRot = new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantLogic.mPutOutGameTime = binaryReader.ReadDouble();
				farmPlantLogic.mLife = binaryReader.ReadDouble();
				farmPlantLogic.mWater = binaryReader.ReadDouble();
				farmPlantLogic.mClean = binaryReader.ReadDouble();
				farmPlantLogic.mDead = binaryReader.ReadBoolean();
				farmPlantLogic.mGrowTimeIndex = binaryReader.ReadInt32();
				farmPlantLogic.mCurGrowTime = binaryReader.ReadDouble();
				farmPlantLogic.mTerrianType = binaryReader.ReadByte();
				farmPlantLogic.mGrowRate = binaryReader.ReadSingle();
				farmPlantLogic.mExtraGrowRate = binaryReader.ReadSingle();
				farmPlantLogic.mNpcGrowRate = binaryReader.ReadSingle();
				farmPlantLogic.mLastUpdateTime = binaryReader.ReadDouble();
				mPlantMap[farmPlantLogic.mPlantInstanceId] = farmPlantLogic;
				mPlantHelpMap[new IntVec3(farmPlantLogic.mPos)] = farmPlantLogic.mPlantInstanceId;
				AddPlantToWorldIdMap(farmPlantLogic.worldId, farmPlantLogic);
			}
			break;
		}
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public FarmPlantLogic GetPlantByItemObjID(int itemObjID)
	{
		if (mPlantMap.ContainsKey(itemObjID))
		{
			return mPlantMap[itemObjID];
		}
		return null;
	}

	public FarmPlantLogic CreatePlant(int worldId, int itemObjID, int plantTypeID, Vector3 pos, Quaternion rot, byte terrainType)
	{
		FarmPlantLogic farmPlantLogic = new FarmPlantLogic();
		farmPlantLogic.worldId = worldId;
		farmPlantLogic.mPlantInstanceId = itemObjID;
		farmPlantLogic._PlantType = plantTypeID;
		farmPlantLogic.mLife = 100.0;
		farmPlantLogic.mPos = pos;
		farmPlantLogic.mRot = rot;
		farmPlantLogic.mPutOutGameTime = GameTime.Timer.Second;
		farmPlantLogic.mWater = farmPlantLogic.mPlantInfo.mDefaultWater;
		farmPlantLogic.mClean = farmPlantLogic.mPlantInfo.mDefaultClean;
		farmPlantLogic.mDead = false;
		farmPlantLogic.mGrowTimeIndex = 0;
		farmPlantLogic.mTerrianType = terrainType;
		farmPlantLogic.InitGrowRate(0f);
		farmPlantLogic.InitUpdateTime();
		mPlantMap[itemObjID] = farmPlantLogic;
		mPlantHelpMap[new IntVec3(farmPlantLogic.mPos)] = farmPlantLogic.mPlantInstanceId;
		AddPlantToWorldIdMap(farmPlantLogic.worldId, farmPlantLogic);
		if (this.CreatePlantEvent != null)
		{
			this.CreatePlantEvent(farmPlantLogic);
		}
		return farmPlantLogic;
	}

	public void RemovePlant(int itemObjID)
	{
		if (mPlantMap.ContainsKey(itemObjID))
		{
			if (this.RemovePlantEvent != null)
			{
				this.RemovePlantEvent(mPlantMap[itemObjID]);
			}
			mPlantHelpMap.Remove(new IntVec3(mPlantMap[itemObjID].mPos));
			mPlantWorldIdMap[mPlantMap[itemObjID].worldId].Remove(mPlantMap[itemObjID]);
			mPlantMap.Remove(itemObjID);
		}
	}

	private void Update()
	{
		frameCount++;
		if (frameCount % 64 != 0)
		{
			return;
		}
		Dictionary<int, List<FarmPlantLogic>> dictionary = new Dictionary<int, List<FarmPlantLogic>>();
		foreach (FarmPlantLogic value in mPlantMap.Values)
		{
			float num = 0f;
			List<ColonyFarm> list = ColonyMgr.AllWorkingMachine(1134, value.mPos);
			if (list.Count > 0)
			{
				foreach (ColonyFarm item in list)
				{
					if (item.curFarmerGrowRate > num)
					{
						num = item.curFarmerGrowRate;
					}
				}
			}
			if (num != value.mNpcGrowRate)
			{
				value.UpdateNpcGrowRate(num);
				if (!dictionary.ContainsKey(value.worldId))
				{
					dictionary.Add(value.worldId, new List<FarmPlantLogic>());
				}
				dictionary[value.worldId].Add(value);
			}
			else if (value.mNextUpdateTime > 0.0 && value.mNextUpdateTime < GameTime.Timer.Second)
			{
				value.UpdateStatus();
				if (!dictionary.ContainsKey(value.worldId))
				{
					dictionary.Add(value.worldId, new List<FarmPlantLogic>());
				}
				dictionary[value.worldId].Add(value);
			}
		}
		if (dictionary.Count > 0)
		{
			SyncPlantList(dictionary);
		}
		frameCount = 0;
	}

	public static void Init()
	{
		foreach (FarmPlantLogic value in mPlantMap.Values)
		{
			value.InitUpdateTime();
		}
	}

	private void OnDirtyVoxel(Vector3 pos, byte terrainType)
	{
		for (int i = 0; i < 2; i++)
		{
			IntVec3 key = new IntVec3(pos);
			if (mPlantHelpMap.ContainsKey(key))
			{
				FarmPlantLogic farmPlantLogic = mPlantMap[mPlantHelpMap[key]];
				if (farmPlantLogic.mTerrianType != terrainType)
				{
					farmPlantLogic.mTerrianType = terrainType;
					farmPlantLogic.UpdateGrowRate(0f);
				}
			}
		}
	}

	private static IEnumerator AsyncSave()
	{
		while (true)
		{
			yield return new WaitForSeconds(60f);
			SyncSave();
		}
	}

	public static void SyncSave()
	{
		FarmMgrData farmMgrData = new FarmMgrData();
		farmMgrData.ExportData(1, Export());
		AsyncSqlite.AddRecord(farmMgrData);
	}

	public static void LoadComplete(SqliteDataReader dataReader)
	{
		if (dataReader.Read())
		{
			dataReader.GetInt32(dataReader.GetOrdinal("ver"));
			byte[] buffer = (byte[])dataReader.GetValue(dataReader.GetOrdinal("data"));
			Import(buffer);
		}
	}

	public static void LoadData()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM farmdata WHERE id = 1;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public void SyncPlant(FarmPlantLogic plant)
	{
		ChannelNetwork.SyncChannel(plant.worldId, EPacketType.PT_InGame_Plant_UpdateInfo, plant);
	}

	public void SyncPlantList(Dictionary<int, List<FarmPlantLogic>> worldPlants)
	{
		foreach (KeyValuePair<int, List<FarmPlantLogic>> worldPlant in worldPlants)
		{
			ChannelNetwork.SyncChannel(worldPlant.Key, EPacketType.PT_InGame_Plant_UpdateInfoList, worldPlant.Value.ToArray(), false);
		}
	}
}
