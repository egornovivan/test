using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class FarmPlantLogic : DragItemLogic, ISaveDataInScene
{
	public delegate void EventDel(FarmPlantLogic plant, int event_type);

	public const int cEvent_NeedWater = 1;

	public const int cEvent_NoNeedWater = 2;

	public const int cEvent_NeedClean = 3;

	public const int cEvent_NoNeedClean = 4;

	public const int cEvent_Dead = 5;

	public const int cEvent_Ripe = 6;

	private const double VarPerOp = 30.0;

	private const int Version000 = 20150409;

	private const int Version = 20160222;

	public const int Version001 = 2016110100;

	private const float LifeUp = 20f;

	private const float LifeDeMin = 0f;

	private const float LifeDeMax = 40f;

	private const double LifeMax = 100.0;

	private GameObject _mainGo;

	private int mTypeID;

	public double mPutOutGameTime;

	public double mLife;

	public double mWater;

	public double mClean;

	public bool mDead;

	public int mGrowTimeIndex;

	public double mCurGrowTime;

	public byte mTerrianType;

	public float mGrowRate = 1f;

	public float mExtraGrowRate;

	public float mNpcGrowRate;

	public PlantInfo mPlantInfo;

	public Bounds mPlantBounds;

	public double mLastUpdateTime;

	public double mNextUpdateTime;

	private bool mInit;

	public bool NeedWater;

	public bool NeedClean;

	public bool IsRipe;

	private double OneDaySecond = 93600.0;

	public int curVersion = 2016110100;

	public int mPlantInstanceId
	{
		get
		{
			return itemDrag.itemObj.instanceId;
		}
		set
		{
			itemDrag = PeSingleton<ItemMgr>.Instance.Get(value).GetCmpt<Drag>();
		}
	}

	public int protoTypeId => itemDrag.itemObj.protoId;

	public Vector3 mPos => base.transform.position;

	public int _PlantType
	{
		get
		{
			return mTypeID;
		}
		set
		{
			mTypeID = value;
			mPlantInfo = PlantInfo.GetInfo(mTypeID);
			mPlantBounds = PlantInfo.GetPlantBounds(protoTypeId, mPos);
		}
	}

	private static event EventDel EventListener;

	void ISaveDataInScene.ImportData(byte[] data)
	{
		MemoryStream input = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(input);
		switch (binaryReader.ReadInt32())
		{
		case 20150409:
			id = binaryReader.ReadInt32();
			mPlantInstanceId = binaryReader.ReadInt32();
			_PlantType = binaryReader.ReadInt32();
			mPutOutGameTime = binaryReader.ReadDouble();
			mLife = binaryReader.ReadDouble();
			mWater = binaryReader.ReadDouble();
			mClean = binaryReader.ReadDouble();
			mDead = binaryReader.ReadBoolean();
			mGrowTimeIndex = binaryReader.ReadInt32();
			mCurGrowTime = binaryReader.ReadDouble();
			mTerrianType = binaryReader.ReadByte();
			mExtraGrowRate = binaryReader.ReadSingle();
			mInit = binaryReader.ReadBoolean();
			InitGrowRate(mExtraGrowRate);
			break;
		case 20160222:
			id = binaryReader.ReadInt32();
			mPlantInstanceId = binaryReader.ReadInt32();
			_PlantType = binaryReader.ReadInt32();
			mPutOutGameTime = binaryReader.ReadDouble();
			mLife = binaryReader.ReadDouble();
			mWater = binaryReader.ReadDouble();
			mClean = binaryReader.ReadDouble();
			mDead = binaryReader.ReadBoolean();
			mGrowTimeIndex = binaryReader.ReadInt32();
			mCurGrowTime = binaryReader.ReadDouble();
			mTerrianType = binaryReader.ReadByte();
			mExtraGrowRate = binaryReader.ReadSingle();
			mInit = binaryReader.ReadBoolean();
			mLastUpdateTime = binaryReader.ReadDouble();
			InitGrowRate(mExtraGrowRate);
			break;
		case 2016110100:
			id = binaryReader.ReadInt32();
			mPlantInstanceId = binaryReader.ReadInt32();
			_PlantType = binaryReader.ReadInt32();
			mPutOutGameTime = binaryReader.ReadDouble();
			mLife = binaryReader.ReadDouble();
			mWater = binaryReader.ReadDouble();
			mClean = binaryReader.ReadDouble();
			mDead = binaryReader.ReadBoolean();
			mGrowTimeIndex = binaryReader.ReadInt32();
			mCurGrowTime = binaryReader.ReadDouble();
			mTerrianType = binaryReader.ReadByte();
			mGrowRate = binaryReader.ReadSingle();
			mExtraGrowRate = binaryReader.ReadSingle();
			mNpcGrowRate = binaryReader.ReadSingle();
			mInit = binaryReader.ReadBoolean();
			mLastUpdateTime = binaryReader.ReadDouble();
			break;
		}
	}

	byte[] ISaveDataInScene.ExportData()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(curVersion);
		binaryWriter.Write(id);
		binaryWriter.Write(mPlantInstanceId);
		binaryWriter.Write(_PlantType);
		binaryWriter.Write(mPutOutGameTime);
		binaryWriter.Write(mLife);
		binaryWriter.Write(mWater);
		binaryWriter.Write(mClean);
		binaryWriter.Write(mDead);
		binaryWriter.Write(mGrowTimeIndex);
		binaryWriter.Write(mCurGrowTime);
		binaryWriter.Write(mTerrianType);
		binaryWriter.Write(mGrowRate);
		binaryWriter.Write(mExtraGrowRate);
		binaryWriter.Write(mNpcGrowRate);
		binaryWriter.Write(mInit);
		binaryWriter.Write(mLastUpdateTime);
		binaryWriter.Close();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	public static void RegisterEventListener(EventDel eventListener)
	{
		FarmPlantLogic.EventListener = (EventDel)Delegate.Combine(FarmPlantLogic.EventListener, eventListener);
	}

	public static void UnregisterEventListener(EventDel eventListener)
	{
		FarmPlantLogic.EventListener = (EventDel)Delegate.Remove(FarmPlantLogic.EventListener, eventListener);
	}

	public static void ExcuteEventListener(FarmPlantLogic plant, int event_type)
	{
		if (FarmPlantLogic.EventListener != null)
		{
			FarmPlantLogic.EventListener(plant, event_type);
		}
	}

	public void InitGrowRate(float extraRate)
	{
		float num = 0f;
		mGrowRate = mTerrianType switch
		{
			19 => 1f, 
			63 => 1.3f, 
			_ => 1f, 
		} + extraRate + mNpcGrowRate;
		mExtraGrowRate = extraRate;
	}

	public void UpdateGrowRate(float extraRate, bool bReset = true)
	{
		UpdateStatus();
		float num = 0f;
		num = mTerrianType switch
		{
			19 => 1f, 
			63 => 1.3f, 
			_ => 1f, 
		};
		if (bReset)
		{
			mGrowRate = num + extraRate + mNpcGrowRate;
			mExtraGrowRate = extraRate;
		}
		else
		{
			mGrowRate = num + mExtraGrowRate + extraRate + mNpcGrowRate;
			mExtraGrowRate += extraRate;
		}
		UpdateStatus();
	}

	public void UpdateNpcGrowRate(float npcRate)
	{
		UpdateStatus();
		float num = 0f;
		mGrowRate = mTerrianType switch
		{
			19 => 1f, 
			63 => 1.3f, 
			_ => 1f, 
		} + mExtraGrowRate + npcRate;
		mNpcGrowRate = npcRate;
		UpdateStatus();
	}

	public float GetRipePercent()
	{
		float num = (float)mCurGrowTime;
		float num2 = (float)(GameTime.Timer.Second - mLastUpdateTime) * mGrowRate + num;
		float num3 = mPlantInfo.mGrowTime[mPlantInfo.mGrowTime.Length - 1];
		return Mathf.Clamp(num2 / num3, 0f, 1f);
	}

	public int GetWaterItemCount()
	{
		UpdateStatus();
		return (int)(((double)mPlantInfo.mWaterLevel[1] - mWater) / 30.0);
	}

	public void GetRain()
	{
		if (NeedWater)
		{
			UpdateStatus();
			int num = (int)(((double)mPlantInfo.mWaterLevel[1] - mWater) / 30.0);
			if (num > 0)
			{
				mWater += 30.0 * (double)num;
				UpdateStatus();
			}
		}
	}

	public void Watering(int water_count)
	{
		UpdateStatus();
		mWater += 30.0 * (double)water_count;
		UpdateStatus();
	}

	public int GetCleaningItemCount()
	{
		UpdateStatus();
		return (int)(((double)mPlantInfo.mCleanLevel[1] - mClean) / 30.0);
	}

	public void Cleaning(int weeding_count)
	{
		UpdateStatus();
		mClean += 30.0 * (double)weeding_count;
		UpdateStatus();
	}

	public int GetHarvestItemNum(float harvestAbility = 1f)
	{
		UpdateStatus();
		return (int)((float)((int)(mLife / 20.0) + 1) * 0.2f * (float)mPlantInfo.mItemGetNum * harvestAbility);
	}

	public Dictionary<int, int> GetHarvestItemIds(int itemGetNum)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < itemGetNum; i++)
		{
			float num = UnityEngine.Random.Range(0f, 1f);
			for (int j = 0; j < mPlantInfo.mItemGetPro.Count; j++)
			{
				if (num < mPlantInfo.mItemGetPro[j].m_probablity)
				{
					if (!dictionary.ContainsKey(mPlantInfo.mItemGetPro[j].m_id))
					{
						dictionary[mPlantInfo.mItemGetPro[j].m_id] = 0;
					}
					Dictionary<int, int> dictionary2;
					Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
					int key;
					int key2 = (key = mPlantInfo.mItemGetPro[j].m_id);
					key = dictionary2[key];
					dictionary3[key2] = key + 1;
				}
			}
		}
		return dictionary;
	}

	public int GetHarvestItemNumMax()
	{
		return (int)(1.2f * (float)mPlantInfo.mItemGetNum);
	}

	public Dictionary<int, int> GetHarvestItemIdsMax(int itemGetNum)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < itemGetNum; i++)
		{
			float num = 0f;
			for (int j = 0; j < mPlantInfo.mItemGetPro.Count; j++)
			{
				if (num < mPlantInfo.mItemGetPro[j].m_probablity)
				{
					if (!dictionary.ContainsKey(mPlantInfo.mItemGetPro[j].m_id))
					{
						dictionary[mPlantInfo.mItemGetPro[j].m_id] = 0;
					}
					Dictionary<int, int> dictionary2;
					Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
					int key;
					int key2 = (key = mPlantInfo.mItemGetPro[j].m_id);
					key = dictionary2[key];
					dictionary3[key2] = key + 1;
				}
			}
		}
		return dictionary;
	}

	public void UpdateStatus()
	{
		if (mDead || IsRipe)
		{
			mNextUpdateTime = -1.0;
			return;
		}
		double second = GameTime.Timer.Second;
		double num = second - mLastUpdateTime;
		double timePassedDay = num / (double)GameTime.Timer.Day2Sec;
		mLastUpdateTime = second;
		UpdateLife(timePassedDay);
		UpdateGrowTime(num);
		UpdateGrowIndex();
		UpdateEvent();
		UpdateModel();
		if (mDead || IsRipe)
		{
			mNextUpdateTime = -1.0;
			return;
		}
		double num2 = CountNextUpdateTime();
		mNextUpdateTime = mLastUpdateTime + num2;
	}

	public void UpdateWater(double timePassedDay)
	{
		mWater -= timePassedDay * (double)mPlantInfo.mWaterDS;
	}

	public void UpdateClean(double timePassedDay)
	{
		mClean -= timePassedDay * (double)mPlantInfo.mCleanDS;
	}

	public void UpdateGrowTime(double timePassedSecond)
	{
		if (!mDead)
		{
			mCurGrowTime += timePassedSecond * (double)mGrowRate;
		}
	}

	public void UpdateGrowIndex()
	{
		if (!mDead)
		{
			while (mGrowTimeIndex < mPlantInfo.mGrowTime.Length && mCurGrowTime > (double)mPlantInfo.mGrowTime[mGrowTimeIndex])
			{
				mGrowTimeIndex++;
			}
		}
	}

	public void UpdateLife(double timePassedDay)
	{
		double num = CountNextWatertime();
		double num2 = CountNextCleanTime();
		bool flag = false;
		bool flag2 = false;
		if (num >= 0.0 && num2 >= 0.0)
		{
			double num3 = ((!(num > num2)) ? num : num2);
			if (num3 >= timePassedDay)
			{
				num3 = timePassedDay;
				timePassedDay = 0.0;
			}
			else
			{
				timePassedDay -= num3;
			}
			mWater -= (double)mPlantInfo.mWaterDS * num3;
			mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
			mClean -= (double)mPlantInfo.mCleanDS * num3;
			mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
			mLife += 20.0 * num3 * 2.0;
			if (mLife > 100.0)
			{
				mLife = 100.0;
			}
			flag = true;
		}
		else if (num >= 0.0 && num2 < -2.0)
		{
			double num3 = num;
			if (num3 >= timePassedDay)
			{
				num3 = timePassedDay;
				timePassedDay = 0.0;
			}
			else
			{
				timePassedDay -= num3;
			}
			mWater -= (double)mPlantInfo.mWaterDS * num3;
			mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
			mClean -= (double)mPlantInfo.mCleanDS * num3;
			mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
			mLife += 20.0 * num3 * 2.0;
			if (mLife > 100.0)
			{
				mLife = 100.0;
			}
			flag = true;
		}
		if (timePassedDay < 1.401298464324817E-45)
		{
			return;
		}
		if (flag || (num < num2 && num > -2.0 && num2 > -2.0))
		{
			double num4 = mWater / (double)mPlantInfo.mWaterDS;
			double num5 = (mClean - (double)mPlantInfo.mCleanLevel[0]) / (double)mPlantInfo.mCleanDS;
			if (num4 >= num5)
			{
				double num3 = num5;
				if (num3 >= timePassedDay)
				{
					num3 = timePassedDay;
					timePassedDay = 0.0;
				}
				else
				{
					timePassedDay -= num3;
				}
				double num6 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - mWater * 2.0 + num3 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num3 / 2.0;
				double num7 = 20.0 * num3;
				if (mLife + num6 + num7 <= 1.401298464324817E-45)
				{
					mLife = 0.0;
					mDead = true;
					return;
				}
				mWater -= (double)mPlantInfo.mWaterDS * num3;
				mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
				mClean -= (double)mPlantInfo.mCleanDS * num3;
				mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
				mLife += num6 + num7;
				if (mLife > 100.0)
				{
					mLife = 100.0;
				}
			}
			else
			{
				double num8 = num4;
				if (num8 >= timePassedDay)
				{
					num8 = timePassedDay;
					timePassedDay = 0.0;
				}
				else
				{
					timePassedDay -= num8;
				}
				double num9 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - mWater * 2.0 + num8 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num8 / 2.0;
				double num10 = 20.0 * num8;
				if (mLife + num9 + num10 <= 1.401298464324817E-45)
				{
					mLife = 0.0;
					mDead = true;
					return;
				}
				mWater -= (double)mPlantInfo.mWaterDS * num8;
				mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
				mClean -= (double)mPlantInfo.mCleanDS * num8;
				mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
				mLife += num9 + num10;
				if (mLife > 100.0)
				{
					mLife = 100.0;
				}
				if (timePassedDay < 1.401298464324817E-45)
				{
					return;
				}
				double num11 = num5 - num4;
				if (num11 >= timePassedDay)
				{
					num11 = timePassedDay;
					timePassedDay = 0.0;
				}
				else
				{
					timePassedDay -= num11;
				}
				num9 = -40.0 * num11;
				num10 = 20.0 * num11;
				if (mLife + num9 + num10 <= 1.401298464324817E-45)
				{
					mLife = 0.0;
					mDead = true;
					return;
				}
				mWater -= (double)mPlantInfo.mWaterDS * num11;
				mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
				mClean -= (double)mPlantInfo.mCleanDS * num11;
				mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
				mLife += num9 + num10;
				if (mLife > 100.0)
				{
					mLife = 100.0;
				}
			}
			flag2 = true;
		}
		else if (num >= num2 && num != -2.0 && num2 > -2.0)
		{
			double num12 = mClean / (double)mPlantInfo.mCleanDS;
			double num13 = (mWater - (double)mPlantInfo.mWaterLevel[0]) / (double)mPlantInfo.mWaterDS;
			if (num12 >= num13)
			{
				double num3 = num13;
				if (num3 >= timePassedDay)
				{
					num3 = timePassedDay;
					timePassedDay = 0.0;
				}
				else
				{
					timePassedDay -= num3;
				}
				double num14 = (0.0 - 40.0 * (((double)(mPlantInfo.mCleanLevel[0] * 2f) - mClean * 2.0 + num3 * (double)mPlantInfo.mCleanDS) / (double)mPlantInfo.mCleanLevel[0])) * num3 / 2.0;
				double num15 = 20.0 * num3;
				if (mLife + num15 + num14 <= 1.401298464324817E-45)
				{
					mLife = 0.0;
					mDead = true;
					return;
				}
				mWater -= (double)mPlantInfo.mWaterDS * num3;
				mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
				mClean -= (double)mPlantInfo.mCleanDS * num3;
				mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
				mLife += num15 + num14;
				if (mLife > 100.0)
				{
					mLife = 100.0;
				}
			}
			else
			{
				double num16 = num12;
				if (num16 >= timePassedDay)
				{
					num16 = timePassedDay;
					timePassedDay = 0.0;
				}
				else
				{
					timePassedDay -= num16;
				}
				double num17 = (0.0 - 40.0 * (((double)(mPlantInfo.mCleanLevel[0] * 2f) - mClean * 2.0 + num16 * (double)mPlantInfo.mCleanDS) / (double)mPlantInfo.mCleanLevel[0])) * num16 / 2.0;
				double num18 = 20.0 * num16;
				if (mLife + num18 + num17 <= 1.401298464324817E-45)
				{
					mLife = 0.0;
					mDead = true;
					return;
				}
				mWater -= (double)mPlantInfo.mWaterDS * num16;
				mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
				mClean -= (double)mPlantInfo.mCleanDS * num16;
				mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
				mLife += num18 + num17;
				if (mLife > 100.0)
				{
					mLife = 100.0;
				}
				if (timePassedDay < 1.401298464324817E-45)
				{
					return;
				}
				double num19 = num13 - num12;
				if (num19 >= timePassedDay)
				{
					num19 = timePassedDay;
					timePassedDay = 0.0;
				}
				else
				{
					timePassedDay -= num19;
				}
				num17 = -40.0 * num19;
				num18 = 20.0 * num19;
				if (mLife + num18 + num17 <= 1.401298464324817E-45)
				{
					mLife = 0.0;
					mDead = true;
					return;
				}
				mWater -= (double)mPlantInfo.mWaterDS * num19;
				mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
				mClean -= (double)mPlantInfo.mCleanDS * num19;
				mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
				mLife += num18 + num17;
				if (mLife > 100.0)
				{
					mLife = 100.0;
				}
			}
			flag2 = true;
		}
		else if (num2 < -2.0)
		{
			if (num < 0.0)
			{
				double num20 = mWater / (double)mPlantInfo.mWaterDS;
				if (num20 > 0.0)
				{
					double num3 = num20;
					if (num3 >= timePassedDay)
					{
						num3 = timePassedDay;
						timePassedDay = 0.0;
					}
					else
					{
						timePassedDay -= num3;
					}
					double num21 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - mWater * 2.0 + num3 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num3 / 2.0;
					double num22 = 20.0 * num3;
					if (mLife + num21 + num22 <= 1.401298464324817E-45)
					{
						mLife = 0.0;
						mDead = true;
						return;
					}
					mWater -= (double)mPlantInfo.mWaterDS * num3;
					mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
					mClean -= (double)mPlantInfo.mCleanDS * num3;
					mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
					mLife += num21 + num22;
					if (mLife > 100.0)
					{
						mLife = 100.0;
					}
				}
				if (!(timePassedDay < 1.401298464324817E-45))
				{
					mLife -= 20.0 * timePassedDay;
					if (mLife <= 1.401298464324817E-45)
					{
						mLife = 0.0;
						mDead = true;
					}
				}
				return;
			}
			flag2 = true;
		}
		if (timePassedDay < 1.401298464324817E-45 || (!flag2 && (!(num < 0.0) || !(num2 < 0.0))))
		{
			return;
		}
		if (mWater > 0.0 && mClean > 0.0)
		{
			double num23 = mWater / (double)mPlantInfo.mWaterDS;
			double num24 = mClean / (double)mPlantInfo.mCleanDS;
			double num3 = ((!(num23 > num24)) ? num23 : num24);
			if (num3 >= timePassedDay)
			{
				num3 = timePassedDay;
				timePassedDay = 0.0;
			}
			else
			{
				timePassedDay -= num3;
			}
			double num25 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - mWater * 2.0 + num3 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num3 / 2.0;
			double num26 = (0.0 - 40.0 * (((double)(mPlantInfo.mCleanLevel[0] * 2f) - mClean * 2.0 + num3 * (double)mPlantInfo.mCleanDS) / (double)mPlantInfo.mCleanLevel[0])) * num3 / 2.0;
			if (mLife + num25 + num26 <= 1.401298464324817E-45)
			{
				mLife = 0.0;
				mDead = true;
				return;
			}
			mWater -= (double)mPlantInfo.mWaterDS * num3;
			mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
			mClean -= (double)mPlantInfo.mCleanDS * num3;
			mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
			mLife += num25 + num26;
			if (mLife > 100.0)
			{
				mLife = 100.0;
			}
		}
		if (timePassedDay < 1.401298464324817E-45)
		{
			return;
		}
		if (mWater > 0.0 && mClean <= 1.401298464324817E-45)
		{
			double num3 = mWater / (double)mPlantInfo.mWaterDS;
			if (num3 >= timePassedDay)
			{
				num3 = timePassedDay;
				timePassedDay = 0.0;
			}
			else
			{
				timePassedDay -= num3;
			}
			double num27 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - mWater * 2.0 + num3 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num3 / 2.0;
			double num28 = -40.0 * num3;
			if (mLife + num27 + num28 <= 1.401298464324817E-45)
			{
				mLife = 0.0;
				mDead = true;
			}
			else
			{
				mWater -= (double)mPlantInfo.mWaterDS * num3;
				mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
				mClean -= (double)mPlantInfo.mCleanDS * num3;
				mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
				mLife += num27 + num28;
				if (mLife > 100.0)
				{
					mLife = 100.0;
				}
			}
		}
		else if (mWater <= 1.401298464324817E-45 && mClean > 0.0)
		{
			double num3 = mClean / (double)mPlantInfo.mCleanDS;
			if (num3 >= timePassedDay)
			{
				num3 = timePassedDay;
				timePassedDay = 0.0;
			}
			else
			{
				timePassedDay -= num3;
			}
			double num29 = (0.0 - 40.0 * (((double)(mPlantInfo.mCleanLevel[0] * 2f) - mClean * 2.0 + num3 * (double)mPlantInfo.mCleanDS) / (double)mPlantInfo.mCleanLevel[0])) * num3 / 2.0;
			double num30 = -40.0 * num3;
			if (mLife + num30 + num29 <= 1.401298464324817E-45)
			{
				mLife = 0.0;
				mDead = true;
				return;
			}
			mWater -= (double)mPlantInfo.mWaterDS * num3;
			mWater = ((!(mWater < 0.0)) ? mWater : 0.0);
			mClean -= (double)mPlantInfo.mCleanDS * num3;
			mClean = ((!(mClean < 0.0)) ? mClean : 0.0);
			mLife += num30 + num29;
			if (mLife > 100.0)
			{
				mLife = 100.0;
			}
		}
		if (!(timePassedDay < 1.401298464324817E-45))
		{
			mLife -= 80.0 * timePassedDay;
			if (mLife < 1.401298464324817E-45)
			{
				mLife = 0.0;
				mDead = true;
			}
		}
	}

	public void UpdateEvent()
	{
		if (mDead)
		{
			ExcuteEventListener(this, 5);
		}
		bool needWater = NeedWater;
		NeedWater = mWater < (double)mPlantInfo.mWaterLevel[0];
		if (NeedWater != needWater)
		{
			int event_type = (NeedWater ? 1 : 2);
			ExcuteEventListener(this, event_type);
		}
		bool needClean = NeedClean;
		NeedClean = mClean < (double)mPlantInfo.mCleanLevel[0];
		if (NeedClean != needClean)
		{
			int event_type2 = ((!NeedClean) ? 4 : 3);
			ExcuteEventListener(this, event_type2);
		}
		bool isRipe = IsRipe;
		IsRipe = mGrowTimeIndex == mPlantInfo.mGrowTime.Length;
		if (isRipe != IsRipe && IsRipe)
		{
			ExcuteEventListener(this, 6);
		}
	}

	public void UpdateModel()
	{
		if (!(base.gameObject == null))
		{
			ItemScript_Plant componentInChildren = GetComponentInChildren<ItemScript_Plant>();
			if (componentInChildren != null)
			{
				componentInChildren.UpdatModel();
			}
		}
	}

	public double CountNextUpdateTime()
	{
		if (!mDead && !IsRipe)
		{
			double num = 0.0;
			double num2 = CountNextWatertime();
			double num3 = CountNextCleanTime();
			num = ((num2 < 0.0 && num3 > 0.0) ? num3 : ((num3 < 0.0 && num2 > 0.0) ? num2 : ((!(num3 > 0.0) || !(num2 > 0.0)) ? (-1.0) : ((!(num2 > num3)) ? num2 : num3))));
			num *= (double)GameTime.Timer.Day2Sec;
			double num4 = CountNextGrowTime();
			if (num4 < 0.0)
			{
				return -2.0;
			}
			if (num4 < num || num < 0.0)
			{
				num = num4;
			}
			if (!(num2 > 0.0) || !(num3 > 0.0))
			{
				double num5 = CountNextDeadTime() * (double)GameTime.Timer.Day2Sec;
				if (num5 < num)
				{
					num = num5;
				}
			}
			return num;
		}
		if (mDead)
		{
			return -1.0;
		}
		return -2.0;
	}

	public double CountNextWatertime()
	{
		if (mWater <= (double)mPlantInfo.mWaterLevel[0])
		{
			return -1.0;
		}
		double num = mWater - (double)mPlantInfo.mWaterLevel[0];
		return num / (double)mPlantInfo.mWaterDS;
	}

	public double CountNextCleanTime()
	{
		if (mPlantInfo.mCleanLevel[0] == 0f)
		{
			return -4.0;
		}
		if (mClean <= (double)mPlantInfo.mCleanLevel[0])
		{
			return -1.0;
		}
		double num = mClean - (double)mPlantInfo.mCleanLevel[0];
		return num / (double)mPlantInfo.mCleanDS;
	}

	public double CountNextGrowTime()
	{
		if (mCurGrowTime < (double)mPlantInfo.mGrowTime[0])
		{
			return ((double)mPlantInfo.mGrowTime[0] - mCurGrowTime) / (double)mGrowRate;
		}
		if (mCurGrowTime >= (double)mPlantInfo.mGrowTime[0] && mCurGrowTime < (double)mPlantInfo.mGrowTime[1])
		{
			return ((double)mPlantInfo.mGrowTime[1] - mCurGrowTime) / (double)mGrowRate;
		}
		if (mCurGrowTime >= (double)mPlantInfo.mGrowTime[1] && mCurGrowTime < (double)mPlantInfo.mGrowTime[2])
		{
			return ((double)mPlantInfo.mGrowTime[2] - mCurGrowTime) / (double)mGrowRate;
		}
		return -1.0;
	}

	public double CountNextDeadTime()
	{
		double num = 0.0;
		double num2 = CountNextWatertime();
		double num3 = CountNextCleanTime();
		bool flag = false;
		bool flag2 = false;
		double num4 = mWater;
		double num5 = mClean;
		double num6 = mLife;
		if (num2 >= 0.0 && num3 >= 0.0)
		{
			double num7 = ((!(num2 > num3)) ? num2 : num3);
			num += num7;
			num4 -= (double)mPlantInfo.mWaterDS * num7;
			num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
			num5 -= (double)mPlantInfo.mCleanDS * num7;
			num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
			num6 += 20.0 * num7 * 2.0;
			if (num6 > 100.0)
			{
				num6 = 100.0;
			}
			flag = true;
		}
		else if (num2 >= 0.0 && num3 < -2.0)
		{
			double num7 = num2;
			num += num7;
			num4 -= (double)mPlantInfo.mWaterDS * num7;
			num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
			num5 -= (double)mPlantInfo.mCleanDS * num7;
			num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
			num6 += 20.0 * num7 * 2.0;
			if (num6 > 100.0)
			{
				num6 = 100.0;
			}
			flag = true;
		}
		if (flag || (num2 < num3 && num2 > -2.0 && num3 > -2.0))
		{
			double num8 = num4 / (double)mPlantInfo.mWaterDS;
			double num9 = (num5 - (double)mPlantInfo.mCleanLevel[0]) / (double)mPlantInfo.mCleanDS;
			if (num8 >= num9)
			{
				double num7 = num9;
				double num10 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - num4 * 2.0 + num7 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num7 / 2.0;
				double num11 = 20.0 * num7;
				if (num6 + num10 + num11 <= 1.401298464324817E-45)
				{
					double a = -40f * mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] / 2f;
					double b = -40.0 * ((double)mPlantInfo.mWaterLevel[0] - num4) / (double)mPlantInfo.mWaterLevel[0] + 20.0;
					double c = num6;
					double largerResult = GetLargerResult(a, b, c);
					return num + largerResult;
				}
				num4 -= (double)mPlantInfo.mWaterDS * num7;
				num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
				num5 -= (double)mPlantInfo.mCleanDS * num7;
				num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
				num6 += num10 + num11;
				num += num7;
			}
			else
			{
				double num12 = num8;
				double num13 = num9 - num8;
				double num14 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - num4 * 2.0 + num12 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num12 / 2.0;
				double num15 = 20.0 * num12;
				if (num6 + num14 + num15 <= 1.401298464324817E-45)
				{
					double a2 = -40f * mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] / 2f;
					double b2 = -40.0 * ((double)mPlantInfo.mWaterLevel[0] - num4) / (double)mPlantInfo.mWaterLevel[0] + 20.0;
					double c2 = num6;
					double largerResult2 = GetLargerResult(a2, b2, c2);
					return num + largerResult2;
				}
				num4 -= (double)mPlantInfo.mWaterDS * num12;
				num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
				num5 -= (double)mPlantInfo.mCleanDS * num12;
				num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
				num6 += num14 + num15;
				num += num12;
				num14 = -40.0 * num13;
				num15 = 20.0 * num13;
				if (num6 + num14 + num15 <= 1.401298464324817E-45)
				{
					double num16 = num6 / 20.0;
					return num + num16;
				}
				num4 -= (double)mPlantInfo.mWaterDS * num13;
				num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
				num5 -= (double)mPlantInfo.mCleanDS * num13;
				num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
				num6 += num14 + num15;
				num += num13;
			}
			flag2 = true;
		}
		else if (num2 >= num3 && num2 != -2.0 && num3 > -2.0)
		{
			double num17 = num5 / (double)mPlantInfo.mCleanDS;
			double num18 = (num4 - (double)mPlantInfo.mWaterLevel[0]) / (double)mPlantInfo.mWaterDS;
			if (num17 >= num18)
			{
				double num7 = num18;
				double num19 = (0.0 - 40.0 * (((double)(mPlantInfo.mCleanLevel[0] * 2f) - num5 * 2.0 + num7 * (double)mPlantInfo.mCleanDS) / (double)mPlantInfo.mCleanLevel[0])) * num7 / 2.0;
				double num20 = 20.0 * num7;
				if (num6 + num20 + num19 <= 1.401298464324817E-45)
				{
					double a3 = -40f * mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0] / 2f;
					double b3 = -40.0 * ((double)mPlantInfo.mCleanLevel[0] - num5) / (double)mPlantInfo.mCleanLevel[0] + 20.0;
					double c3 = num6;
					double largerResult3 = GetLargerResult(a3, b3, c3);
					return num + largerResult3;
				}
				num4 -= (double)mPlantInfo.mWaterDS * num7;
				num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
				num5 -= (double)mPlantInfo.mCleanDS * num7;
				num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
				num6 += num20 + num19;
				num += num7;
			}
			else
			{
				double num21 = num17;
				double num22 = num18 - num17;
				double num23 = (0.0 - 40.0 * (((double)(mPlantInfo.mCleanLevel[0] * 2f) - num5 * 2.0 + num21 * (double)mPlantInfo.mCleanDS) / (double)mPlantInfo.mCleanLevel[0])) * num21 / 2.0;
				double num24 = 20.0 * num21;
				if (num6 + num24 + num23 <= 1.401298464324817E-45)
				{
					double a4 = -40f * mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0] / 2f;
					double b4 = -40.0 * ((double)mPlantInfo.mCleanLevel[0] - num5) / (double)mPlantInfo.mCleanLevel[0] + 20.0;
					double c4 = num6;
					double largerResult4 = GetLargerResult(a4, b4, c4);
					return num + largerResult4;
				}
				num4 -= (double)mPlantInfo.mWaterDS * num21;
				num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
				num5 -= (double)mPlantInfo.mCleanDS * num21;
				num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
				num6 += num24 + num23;
				num += num21;
				num23 = -40.0 * num22;
				num24 = 20.0 * num22;
				if (num6 + num24 + num23 <= 1.401298464324817E-45)
				{
					double num25 = num6 / 20.0;
					return num + num25;
				}
				num4 -= (double)mPlantInfo.mWaterDS * num22;
				num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
				num5 -= (double)mPlantInfo.mCleanDS * num22;
				num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
				num6 += num24 + num23;
				num += num22;
			}
			flag2 = true;
		}
		else if (num3 < -2.0)
		{
			if (num2 < 0.0)
			{
				double num26 = num4 / (double)mPlantInfo.mWaterDS;
				if (num26 > 0.0)
				{
					double num7 = num26;
					double num27 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - num4 * 2.0 + num7 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num7 / 2.0;
					double num28 = 20.0 * num7;
					if (num6 + num27 + num28 <= 1.401298464324817E-45)
					{
						double a5 = -40f * mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] / 2f;
						double b5 = -40.0 * ((double)mPlantInfo.mWaterLevel[0] - num4) / (double)mPlantInfo.mWaterLevel[0] + 20.0;
						double c5 = num6;
						double largerResult5 = GetLargerResult(a5, b5, c5);
						return num + largerResult5;
					}
					num4 -= (double)mPlantInfo.mWaterDS * num7;
					num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
					num5 -= (double)mPlantInfo.mCleanDS * num7;
					num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
					num6 += num27 + num28;
					num += num7;
				}
				double num29 = num6 / 20.0;
				return num + num29;
			}
			flag2 = true;
		}
		if (flag2 || (num2 < 0.0 && num3 < 0.0))
		{
			if (num4 > 0.0 && num5 > 0.0)
			{
				double num30 = num4 / (double)mPlantInfo.mWaterDS;
				double num31 = num5 / (double)mPlantInfo.mCleanDS;
				double num7 = ((!(num30 > num31)) ? num30 : num31);
				double num32 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - num4 * 2.0 + num7 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num7 / 2.0;
				double num33 = (0.0 - 40.0 * (((double)(mPlantInfo.mCleanLevel[0] * 2f) - num5 * 2.0 + num7 * (double)mPlantInfo.mCleanDS) / (double)mPlantInfo.mCleanLevel[0])) * num7 / 2.0;
				if (num6 + num32 + num33 <= 1.401298464324817E-45)
				{
					double a6 = -40f * (mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] + mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0]) / 2f;
					double b6 = -40.0 * (((double)mPlantInfo.mWaterLevel[0] - num4) / (double)mPlantInfo.mWaterLevel[0] + ((double)mPlantInfo.mCleanLevel[0] - num5) / (double)mPlantInfo.mCleanLevel[0]);
					double c6 = num6;
					double largerResult6 = GetLargerResult(a6, b6, c6);
					return num + largerResult6;
				}
				num4 -= (double)mPlantInfo.mWaterDS * num7;
				num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
				num5 -= (double)mPlantInfo.mCleanDS * num7;
				num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
				num6 += num32 + num33;
				num += num7;
			}
			if (num4 > 0.0 && num5 <= 1.401298464324817E-45)
			{
				double num7 = num4 / (double)mPlantInfo.mWaterDS;
				double num34 = (0.0 - 40.0 * (((double)(mPlantInfo.mWaterLevel[0] * 2f) - num4 * 2.0 + num7 * (double)mPlantInfo.mWaterDS) / (double)mPlantInfo.mWaterLevel[0])) * num7 / 2.0;
				double num35 = -40.0 * num7;
				if (num6 + num34 + num35 <= 1.401298464324817E-45)
				{
					double a7 = -40f * (mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0]) / 2f;
					double b7 = -40.0 * ((double)mPlantInfo.mWaterLevel[0] - num4) / (double)mPlantInfo.mWaterLevel[0] - 40.0;
					double c7 = num6;
					double largerResult7 = GetLargerResult(a7, b7, c7);
					return num + largerResult7;
				}
				num4 -= (double)mPlantInfo.mWaterDS * num7;
				num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
				num5 -= (double)mPlantInfo.mCleanDS * num7;
				num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
				num6 += num34 + num35;
				num += num7;
			}
			else if (num4 <= 1.401298464324817E-45 && num5 > 0.0)
			{
				double num7 = num5 / (double)mPlantInfo.mCleanDS;
				double num36 = (0.0 - 40.0 * (((double)(mPlantInfo.mCleanLevel[0] * 2f) - num5 * 2.0 + num7 * (double)mPlantInfo.mCleanDS) / (double)mPlantInfo.mCleanLevel[0])) * num7 / 2.0;
				double num37 = -40.0 * num7;
				if (num6 + num37 + num36 <= 1.401298464324817E-45)
				{
					double a8 = -40f * (mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0]) / 2f;
					double b8 = -40.0 * ((double)mPlantInfo.mCleanLevel[0] - num5) / (double)mPlantInfo.mCleanLevel[0] - 40.0;
					double c8 = num6;
					double largerResult8 = GetLargerResult(a8, b8, c8);
					return num + largerResult8;
				}
				num4 -= (double)mPlantInfo.mWaterDS * num7;
				num4 = ((!(num4 < 0.0)) ? num4 : 0.0);
				num5 -= (double)mPlantInfo.mCleanDS * num7;
				num5 = ((!(num5 < 0.0)) ? num5 : 0.0);
				num6 += num37 + num36;
				num += num7;
			}
			double num38 = num6 / 80.0;
			num += num38;
		}
		return num;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		ItemScript componentInChildren = GetComponentInChildren<ItemScript>();
		if (null != componentInChildren)
		{
			componentInChildren.OnActivate();
		}
	}

	public override void OnDeactivate()
	{
		ItemScript componentInChildren = GetComponentInChildren<ItemScript>();
		if (null != componentInChildren)
		{
			componentInChildren.OnDeactivate();
		}
	}

	public override void OnConstruct()
	{
		base.OnDeactivate();
		_mainGo = itemDrag.CreateViewGameObject(null);
		if (!(_mainGo == null))
		{
			_mainGo.transform.parent = base.transform;
			_mainGo.transform.position = base.transform.position;
			_mainGo.transform.rotation = base.transform.rotation;
			_mainGo.transform.localScale = base.transform.localScale;
			ItemScript component = _mainGo.GetComponent<ItemScript>();
			if (null != component)
			{
				component.SetItemObject(itemDrag.itemObj);
				component.InitNetlayer(mNetlayer);
				component.id = id;
				component.OnConstruct();
			}
		}
	}

	public override void OnDestruct()
	{
		ItemScript componentInChildren = GetComponentInChildren<ItemScript>();
		if (null != componentInChildren)
		{
			componentInChildren.OnDestruct();
		}
		if (_mainGo != null)
		{
			UnityEngine.Object.Destroy(_mainGo);
		}
		base.OnDestruct();
	}

	public double GetLargerResult(double a, double b, double c)
	{
		if (a > 0.0)
		{
			return (0.0 - b + (double)Mathf.Sqrt((float)(b * b - 4.0 * a * c))) / (2.0 * a);
		}
		return (0.0 - b - (double)Mathf.Sqrt((float)(b * b - 4.0 * a * c))) / (2.0 * a);
	}

	public void InitInMultiMode()
	{
		mPlantInfo = PlantInfo.GetPlantInfoByItemId(protoTypeId);
		mPlantBounds = PlantInfo.GetPlantBounds(protoTypeId, mPos);
		FarmManager.Instance.AddPlant(this);
	}

	public void InitDataFromPlant(FarmPlantInitData plant)
	{
		mPlantInfo = PlantInfo.GetInfo(plant.mTypeID);
		mLife = plant.mLife;
		mWater = plant.mWater;
		mClean = plant.mClean;
		mCurGrowTime = plant.mCurGrowTime;
		mDead = plant.mDead;
		mLastUpdateTime = plant.mLastUpdateTime;
		mPlantBounds = PlantInfo.GetPlantBounds(protoTypeId, mPos);
	}

	public void UpdateInMultiMode()
	{
		UpdateGrowIndex();
		UpdateEvent();
		UpdateModel();
	}

	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		try
		{
			int itemObjID = stream.ReadInt32();
			double num = stream.ReadDouble();
			double num2 = stream.ReadDouble();
			double num3 = stream.ReadDouble();
			double num4 = stream.ReadDouble();
			bool flag = stream.ReadBoolean();
			double num5 = stream.ReadDouble();
			FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(itemObjID);
			if (plantByItemObjID != null)
			{
				plantByItemObjID.mLife = num;
				plantByItemObjID.mWater = num2;
				plantByItemObjID.mClean = num3;
				plantByItemObjID.mCurGrowTime = num4;
				plantByItemObjID.mDead = flag;
				plantByItemObjID.mLastUpdateTime = num5;
			}
			return plantByItemObjID;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		try
		{
			FarmPlantLogic farmPlantLogic = value as FarmPlantLogic;
			stream.WriteInt32(farmPlantLogic.mPlantInstanceId);
			stream.WriteDouble(farmPlantLogic.mLife);
			stream.WriteDouble(farmPlantLogic.mWater);
			stream.WriteDouble(farmPlantLogic.mClean);
			stream.WriteDouble(farmPlantLogic.mCurGrowTime);
			stream.WriteBoolean(farmPlantLogic.mDead);
			stream.WriteDouble(farmPlantLogic.mLastUpdateTime);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void Start()
	{
		if (!GameConfig.IsMultiMode)
		{
			mPlantInfo = PlantInfo.GetPlantInfoByItemId(protoTypeId);
			FarmManager.Instance.AddPlant(this);
			if (!mInit)
			{
				FarmManager.Instance.InitPlant(this);
				mInit = true;
			}
			InitUpdateTime();
		}
	}

	public void InitUpdateTime()
	{
		if (mLastUpdateTime < 1.401298464324817E-45 && mLastUpdateTime > -1.401298464324817E-45)
		{
			mLastUpdateTime = GameTime.Timer.Second;
		}
		IsRipe = mGrowTimeIndex == mPlantInfo.mGrowTime.Length;
		if (mDead || IsRipe)
		{
			mNextUpdateTime = -1.0;
		}
		else
		{
			UpdateStatus();
		}
	}
}
