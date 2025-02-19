using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public class FarmManager : MonoBehaviour
{
	public delegate void PlantEvent(FarmPlantLogic plant);

	private const int Version = 4;

	private static FarmManager mInstance;

	private int frameCount;

	public Dictionary<int, FarmPlantLogic> mPlantMap = new Dictionary<int, FarmPlantLogic>();

	public Dictionary<IntVec3, int> mPlantHelpMap = new Dictionary<IntVec3, int>();

	public static FarmManager Instance => mInstance;

	public event PlantEvent CreatePlantEvent;

	public event PlantEvent RemovePlantEvent;

	private void Awake()
	{
		mInstance = this;
		DigTerrainManager.onDirtyVoxel += OnDirtyVoxel;
	}

	private void OnDestroy()
	{
		DigTerrainManager.onDirtyVoxel -= OnDirtyVoxel;
	}

	public void Export(BinaryWriter bw)
	{
	}

	public void Import(byte[] buffer)
	{
	}

	public List<FarmPlantInitData> ImportPlantData(byte[] buffer)
	{
		List<FarmPlantInitData> list = new List<FarmPlantInitData>();
		mPlantMap.Clear();
		mPlantHelpMap.Clear();
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		switch (num)
		{
		case 4:
		case 5:
		{
			for (int j = 0; j < num2; j++)
			{
				FarmPlantInitData farmPlantInitData2 = new FarmPlantInitData();
				farmPlantInitData2.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantInitData2.mTypeID = binaryReader.ReadInt32();
				farmPlantInitData2.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantInitData2.mRot = new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantInitData2.mPutOutGameTime = binaryReader.ReadDouble();
				farmPlantInitData2.mLife = binaryReader.ReadDouble();
				farmPlantInitData2.mWater = binaryReader.ReadDouble();
				farmPlantInitData2.mClean = binaryReader.ReadDouble();
				farmPlantInitData2.mDead = binaryReader.ReadBoolean();
				farmPlantInitData2.mGrowTimeIndex = binaryReader.ReadInt32();
				farmPlantInitData2.mCurGrowTime = binaryReader.ReadDouble();
				farmPlantInitData2.mTerrianType = binaryReader.ReadByte();
				farmPlantInitData2.mExtraGrowRate = binaryReader.ReadSingle();
				list.Add(farmPlantInitData2);
			}
			break;
		}
		case 6:
		{
			for (int k = 0; k < num2; k++)
			{
				FarmPlantInitData farmPlantInitData3 = new FarmPlantInitData();
				farmPlantInitData3.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantInitData3.mTypeID = binaryReader.ReadInt32();
				farmPlantInitData3.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantInitData3.mRot = new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantInitData3.mPutOutGameTime = binaryReader.ReadDouble();
				farmPlantInitData3.mLife = binaryReader.ReadDouble();
				farmPlantInitData3.mWater = binaryReader.ReadDouble();
				farmPlantInitData3.mClean = binaryReader.ReadDouble();
				farmPlantInitData3.mDead = binaryReader.ReadBoolean();
				farmPlantInitData3.mGrowTimeIndex = binaryReader.ReadInt32();
				farmPlantInitData3.mCurGrowTime = binaryReader.ReadDouble();
				farmPlantInitData3.mTerrianType = binaryReader.ReadByte();
				farmPlantInitData3.mExtraGrowRate = binaryReader.ReadSingle();
				farmPlantInitData3.mLastUpdateTime = binaryReader.ReadDouble();
				list.Add(farmPlantInitData3);
			}
			break;
		}
		case 2016110100:
		{
			for (int i = 0; i < num2; i++)
			{
				FarmPlantInitData farmPlantInitData = new FarmPlantInitData();
				farmPlantInitData.mPlantInstanceId = binaryReader.ReadInt32();
				farmPlantInitData.mTypeID = binaryReader.ReadInt32();
				farmPlantInitData.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantInitData.mRot = new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				farmPlantInitData.mPutOutGameTime = binaryReader.ReadDouble();
				farmPlantInitData.mLife = binaryReader.ReadDouble();
				farmPlantInitData.mWater = binaryReader.ReadDouble();
				farmPlantInitData.mClean = binaryReader.ReadDouble();
				farmPlantInitData.mDead = binaryReader.ReadBoolean();
				farmPlantInitData.mGrowTimeIndex = binaryReader.ReadInt32();
				farmPlantInitData.mCurGrowTime = binaryReader.ReadDouble();
				farmPlantInitData.mTerrianType = binaryReader.ReadByte();
				farmPlantInitData.mGrowRate = binaryReader.ReadSingle();
				farmPlantInitData.mExtraGrowRate = binaryReader.ReadSingle();
				farmPlantInitData.mNpcGrowRate = binaryReader.ReadSingle();
				farmPlantInitData.mLastUpdateTime = binaryReader.ReadDouble();
				list.Add(farmPlantInitData);
			}
			break;
		}
		}
		binaryReader.Close();
		memoryStream.Close();
		return list;
	}

	public FarmPlantLogic GetPlantByItemObjID(int itemObjID)
	{
		if (mPlantMap.ContainsKey(itemObjID))
		{
			return mPlantMap[itemObjID];
		}
		return null;
	}

	public void AddPlant(FarmPlantLogic addPlant)
	{
		mPlantMap[addPlant.mPlantInstanceId] = addPlant;
		mPlantHelpMap[new IntVec3(addPlant.mPos)] = addPlant.mPlantInstanceId;
		if (this.CreatePlantEvent != null)
		{
			this.CreatePlantEvent(addPlant);
		}
	}

	public void InitPlant(FarmPlantLogic addPlant)
	{
		addPlant._PlantType = addPlant.mPlantInfo.mTypeID;
		addPlant.mLife = 100.0;
		addPlant.mPutOutGameTime = GameTime.Timer.Second;
		addPlant.mWater = addPlant.mPlantInfo.mDefaultWater;
		addPlant.mClean = addPlant.mPlantInfo.mDefaultClean;
		addPlant.mDead = false;
		addPlant.mGrowTimeIndex = 0;
		IntVector3 intVector = new IntVector3(addPlant.transform.position + 0.1f * Vector3.down);
		addPlant.mTerrianType = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z).Type;
		addPlant.InitGrowRate(0f);
		addPlant.InitUpdateTime();
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
			mPlantMap.Remove(itemObjID);
		}
	}

	private void Update()
	{
		if (GameConfig.IsMultiMode)
		{
			return;
		}
		frameCount++;
		if (frameCount % 64 != 0)
		{
			return;
		}
		List<CSCreator> list = new List<CSCreator>();
		if (CSMain.Instance != null)
		{
			if (PeGameMgr.IsSingle)
			{
				list.Add(CSMain.s_MgCreator);
			}
			else
			{
				list.AddRange(CSMain.Instance.otherCreators.Values);
			}
		}
		foreach (FarmPlantLogic value in mPlantMap.Values)
		{
			if (null == value || value.transform == null)
			{
				continue;
			}
			if (WeatherConfig.IsRaining)
			{
				value.GetRain();
			}
			float num = 0f;
			if (list.Count > 0)
			{
				foreach (CSCreator item in list)
				{
					CSMgCreator cSMgCreator = item as CSMgCreator;
					if (!(cSMgCreator == null) && item.Assembly != null && item.Assembly.InRange(value.mPos) && item.Assembly.Farm != null && item.Assembly.Farm.IsRunning && !(item.Assembly.Farm.FarmerGrowRate <= num))
					{
						num = item.Assembly.Farm.FarmerGrowRate;
					}
				}
			}
			if (num != value.mNpcGrowRate)
			{
				value.UpdateNpcGrowRate(num);
			}
			else if (value.mNextUpdateTime > 0.0 && value.mNextUpdateTime < GameTime.Timer.Second)
			{
				value.UpdateStatus();
			}
			frameCount = 0;
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
					farmPlantLogic.UpdateGrowRate(0f, bReset: false);
				}
			}
		}
	}
}
