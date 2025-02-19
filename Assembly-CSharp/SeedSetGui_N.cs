using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class SeedSetGui_N : UIStaticWnd
{
	public class LocalKeyValue
	{
		public int Key { get; private set; }

		public string Value { get; private set; }

		public LocalKeyValue(int key)
		{
			Key = key;
			Value = PELocalization.GetString(key);
		}
	}

	private static SeedSetGui_N mInstance;

	public UIInput mSeedNumIP;

	public UIPopupList mBiomePL;

	public UIPopupList mWeatherPL;

	public UIScrollBar mSbTerrainHeight;

	public UIScrollBar mSbMapSzie;

	public UIScrollBar mSbHostilityAllyCount;

	public UIScrollBar mSbRiverDensity;

	public UIScrollBar mSbRiverWidth;

	public UIScrollBar mSbPlainHeight;

	public UIScrollBar mSbFlatness;

	public UIScrollBar mSbBridgeMaxHeight;

	public UILabel mLbTerrainHeightInfo;

	public UILabel mLbMapSizeInfo;

	public UILabel mLbHostilityAllyCount;

	public UILabel mLbRiverDensityInfo;

	public UILabel mLbRiverWidthInfo;

	public UILabel mLbPlainHeight;

	public UILabel mLbFlatness;

	public UILabel mLbBridgeMaxHeight;

	public UICheckbox mCbUseSkillTree;

	public GameObject mUseSkillRoot;

	public UICheckbox mOpenAllScripts;

	public GameObject mOpenAllScriptsRoot;

	private List<LocalKeyValue> m_BiomeItemList = new List<LocalKeyValue>();

	private List<LocalKeyValue> m_WeatherItemList = new List<LocalKeyValue>();

	private int mTerrainHeight = 512;

	private int mMapSize = 2;

	private int mHostilityAllyCount = 3;

	private int mRiverDensity = 5;

	private int mRiverWidth = 10;

	private int plainHeight = 5;

	private int flatness = 50;

	private int bridgeMaxHeight = 20;

	public static SeedSetGui_N Instance => mInstance;

	private int mAllyCount => mHostilityAllyCount + 1;

	private void Awake()
	{
		mInstance = this;
		m_BiomeItemList.Add(new LocalKeyValue(8000609));
		m_BiomeItemList.Add(new LocalKeyValue(8000610));
		m_BiomeItemList.Add(new LocalKeyValue(8000611));
		m_BiomeItemList.Add(new LocalKeyValue(8000612));
		m_BiomeItemList.Add(new LocalKeyValue(8000613));
		m_BiomeItemList.Add(new LocalKeyValue(8000614));
		m_BiomeItemList.Add(new LocalKeyValue(8000615));
		m_BiomeItemList.Add(new LocalKeyValue(8000616));
		mBiomePL.items.Clear();
		for (int i = 0; i < m_BiomeItemList.Count; i++)
		{
			mBiomePL.items.Add(m_BiomeItemList[i].Value);
		}
		mSeedNumIP.text = "Planet Maria";
		m_WeatherItemList.Add(new LocalKeyValue(8000617));
		m_WeatherItemList.Add(new LocalKeyValue(8000618));
		m_WeatherItemList.Add(new LocalKeyValue(8000619));
		m_WeatherItemList.Add(new LocalKeyValue(8000620));
		mWeatherPL.items.Clear();
		for (int j = 0; j < m_WeatherItemList.Count; j++)
		{
			mWeatherPL.items.Add(m_WeatherItemList[j].Value);
		}
		if (PeGameMgr.IsAdventure)
		{
			mUseSkillRoot.SetActive(value: true);
			mOpenAllScriptsRoot.SetActive(value: true);
		}
		else
		{
			mUseSkillRoot.SetActive(value: false);
			mOpenAllScriptsRoot.SetActive(value: false);
		}
	}

	private void Start()
	{
		mSbTerrainHeight.scrollValue = 1f;
		mSbMapSzie.scrollValue = 0.5f;
		mSbHostilityAllyCount.scrollValue = 1f;
		mSbRiverDensity.scrollValue = 0.25f;
		mSbRiverWidth.scrollValue = 0.1f;
		mSbPlainHeight.scrollValue = 0.15f;
		mSbFlatness.scrollValue = 0.25f;
		mSbBridgeMaxHeight.scrollValue = 0f;
		mWeatherPL.textLabel.text = m_WeatherItemList[1].Value;
		mBiomePL.textLabel.text = m_BiomeItemList[0].Value;
		UpdateScrollValueChage();
	}

	private void SeedSetEnd()
	{
		LocalKeyValue localKeyValue = m_BiomeItemList.Find((LocalKeyValue kv) => kv.Value == mBiomePL.textLabel.text);
		if (localKeyValue == null)
		{
			return;
		}
		switch (localKeyValue.Key)
		{
		case 8000609:
			RandomMapConfig.RandomMapID = RandomMapType.GrassLand;
			RandomMapConfig.vegetationId = RandomMapType.GrassLand;
			break;
		case 8000610:
			RandomMapConfig.RandomMapID = RandomMapType.Forest;
			RandomMapConfig.vegetationId = RandomMapType.Forest;
			break;
		case 8000611:
			RandomMapConfig.RandomMapID = RandomMapType.Desert;
			RandomMapConfig.vegetationId = RandomMapType.Desert;
			break;
		case 8000612:
			RandomMapConfig.RandomMapID = RandomMapType.Redstone;
			RandomMapConfig.vegetationId = RandomMapType.Redstone;
			break;
		case 8000613:
			RandomMapConfig.RandomMapID = RandomMapType.Rainforest;
			RandomMapConfig.vegetationId = RandomMapType.Rainforest;
			break;
		case 8000614:
			RandomMapConfig.RandomMapID = RandomMapType.Mountain;
			RandomMapConfig.vegetationId = RandomMapType.Mountain;
			break;
		case 8000615:
			RandomMapConfig.RandomMapID = RandomMapType.Swamp;
			RandomMapConfig.vegetationId = RandomMapType.Swamp;
			break;
		case 8000616:
			RandomMapConfig.RandomMapID = RandomMapType.Crater;
			RandomMapConfig.vegetationId = RandomMapType.Crater;
			break;
		}
		LocalKeyValue localKeyValue2 = m_WeatherItemList.Find((LocalKeyValue kv) => kv.Value == mWeatherPL.textLabel.text);
		if (localKeyValue2 != null)
		{
			switch (localKeyValue2.Key)
			{
			case 8000617:
				RandomMapConfig.ScenceClimate = ClimateType.CT_Dry;
				break;
			case 8000618:
				RandomMapConfig.ScenceClimate = ClimateType.CT_Temperate;
				break;
			case 8000619:
				RandomMapConfig.ScenceClimate = ClimateType.CT_Wet;
				break;
			case 8000620:
				RandomMapConfig.ScenceClimate = ClimateType.CT_Random;
				break;
			}
			RandomMapConfig.SeedString = mSeedNumIP.text;
			int num = 0;
			char[] array = mSeedNumIP.text.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				num += (i + 1) * array[i];
			}
			RandomMapConfig.RandSeed = Convert.ToInt32(num);
			RandomMapConfig.TerrainHeight = mTerrainHeight;
			RandomMapConfig.mapSize = mMapSize;
			RandomMapConfig.allyCount = mAllyCount;
			RandomMapConfig.riverDensity = mRiverDensity;
			RandomMapConfig.riverWidth = mRiverWidth;
			RandomMapConfig.plainHeight = plainHeight;
			RandomMapConfig.flatness = flatness;
			RandomMapConfig.bridgeMaxHeight = bridgeMaxHeight;
			if (mUseSkillRoot.activeSelf)
			{
				RandomMapConfig.useSkillTree = mCbUseSkillTree.isChecked;
			}
			else
			{
				RandomMapConfig.useSkillTree = false;
			}
			if (mOpenAllScriptsRoot.activeSelf)
			{
				RandomMapConfig.openAllScripts = mOpenAllScripts.isChecked;
			}
			else
			{
				RandomMapConfig.openAllScripts = false;
			}
			RandomMapConfig.InitTownAreaPara();
			PeSceneCtrl.Instance.GotoGameSence();
		}
	}

	private void OnOkBtn()
	{
		if (Input.GetMouseButtonUp(0))
		{
			SeedSetEnd();
		}
	}

	private void OnCancelBtn()
	{
		if (Input.GetMouseButtonUp(0))
		{
			Hide();
		}
	}

	private void OnRandomBtn()
	{
		int index = UnityEngine.Random.Range(0, m_BiomeItemList.Count - 1);
		mBiomePL.selection = m_BiomeItemList[index].Value;
		index = UnityEngine.Random.Range(0, m_WeatherItemList.Count - 1);
		mWeatherPL.selection = m_WeatherItemList[index].Value;
		mSeedNumIP.text = UnityEngine.Random.Range(0, 1000000).ToString();
		mSbTerrainHeight.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100f;
		mSbMapSzie.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100f;
		mSbHostilityAllyCount.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100f;
		mSbRiverDensity.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100f;
		mSbRiverWidth.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100f;
		mSbPlainHeight.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100f;
		mSbFlatness.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(1, 100)) / 100f;
		mSbBridgeMaxHeight.scrollValue = Convert.ToSingle(UnityEngine.Random.Range(0, 100)) / 100f;
		UpdateScrollValueChage();
	}

	private void OnSeedSubmit(string text)
	{
		SeedSetEnd();
	}

	private void Update()
	{
		UpdateScrollValueChage();
	}

	private void UpdateScrollValueChage()
	{
		float scrollValue = mSbTerrainHeight.scrollValue;
		if (scrollValue >= 0f && (double)scrollValue < 0.33)
		{
			mSbTerrainHeight.scrollValue = 0f;
			mTerrainHeight = 128;
			mLbTerrainHeightInfo.text = "128m";
		}
		else if ((double)scrollValue >= 0.33 && (double)scrollValue < 0.66)
		{
			mSbTerrainHeight.scrollValue = 0.5f;
			mTerrainHeight = 256;
			mLbTerrainHeightInfo.text = "256m";
		}
		else if ((double)scrollValue >= 0.66 && scrollValue <= 1f)
		{
			mSbTerrainHeight.scrollValue = 1f;
			mTerrainHeight = 512;
			mLbTerrainHeightInfo.text = "512m";
		}
		scrollValue = mSbMapSzie.scrollValue;
		if (scrollValue >= 0f && (double)scrollValue < 0.125)
		{
			mSbMapSzie.scrollValue = 0f;
			mMapSize = 4;
			mLbMapSizeInfo.text = "2km * 2km";
			UpdateHostilityAllyCountRangByMapSize(2);
		}
		else if ((double)scrollValue >= 0.125 && (double)scrollValue < 0.375)
		{
			mSbMapSzie.scrollValue = 0.25f;
			mMapSize = 3;
			mLbMapSizeInfo.text = "4km * 4km";
			UpdateHostilityAllyCountRangByMapSize(4);
		}
		else if ((double)scrollValue >= 0.375 && (double)scrollValue < 0.625)
		{
			mSbMapSzie.scrollValue = 0.5f;
			mMapSize = 2;
			mLbMapSizeInfo.text = "8km * 8km";
			UpdateHostilityAllyCountRangByMapSize(8);
		}
		else if ((double)scrollValue >= 0.625 && (double)scrollValue < 0.875)
		{
			mSbMapSzie.scrollValue = 0.75f;
			mMapSize = 1;
			mLbMapSizeInfo.text = "20km * 20km";
			UpdateHostilityAllyCountRangByMapSize(20);
		}
		else if ((double)scrollValue >= 0.875 && scrollValue <= 1f)
		{
			mSbMapSzie.scrollValue = 1f;
			mMapSize = 0;
			mLbMapSizeInfo.text = "40km * 40km";
			UpdateHostilityAllyCountRangByMapSize(40);
		}
		int num = Convert.ToInt32(mSbRiverDensity.scrollValue * 100f);
		if (num < 1)
		{
			num = 1;
		}
		mLbRiverDensityInfo.text = num.ToString();
		mRiverDensity = num;
		num = Convert.ToInt32(mSbRiverWidth.scrollValue * 100f);
		if (num < 1)
		{
			num = 1;
		}
		mLbRiverWidthInfo.text = num.ToString();
		mRiverWidth = num;
		num = Convert.ToInt32(mSbPlainHeight.scrollValue * 100f);
		if (num < 1)
		{
			num = 1;
		}
		mLbPlainHeight.text = num.ToString();
		plainHeight = num;
		num = Convert.ToInt32(mSbFlatness.scrollValue * 100f);
		if (num < 1)
		{
			num = 1;
		}
		mLbFlatness.text = num.ToString();
		flatness = num;
		num = Convert.ToInt32(mSbBridgeMaxHeight.scrollValue * 100f);
		mLbBridgeMaxHeight.text = num.ToString();
		bridgeMaxHeight = num;
	}

	private void UpdateHostilityAllyCountRangByMapSize(int mapSize)
	{
		mSbHostilityAllyCount.enabled = true;
		float scrollValue = mSbHostilityAllyCount.scrollValue;
		switch (mapSize)
		{
		case 2:
			mHostilityAllyCount = 3;
			mSbHostilityAllyCount.scrollValue = 1f;
			break;
		case 4:
			if (scrollValue <= 0.1f)
			{
				mHostilityAllyCount = 3;
				mSbHostilityAllyCount.scrollValue = 0f;
			}
			else if (scrollValue <= 0.51f)
			{
				mHostilityAllyCount = 4;
				mSbHostilityAllyCount.scrollValue = 0.5f;
			}
			else
			{
				mHostilityAllyCount = 5;
				mSbHostilityAllyCount.scrollValue = 1f;
			}
			break;
		case 8:
			if (scrollValue <= 0.1f)
			{
				mHostilityAllyCount = 3;
				mSbHostilityAllyCount.scrollValue = 0f;
			}
			else if (scrollValue <= 0.34f)
			{
				mHostilityAllyCount = 4;
				mSbHostilityAllyCount.scrollValue = 0.33f;
			}
			else if (scrollValue <= 0.67f)
			{
				mHostilityAllyCount = 5;
				mSbHostilityAllyCount.scrollValue = 0.66f;
			}
			else
			{
				mHostilityAllyCount = 6;
				mSbHostilityAllyCount.scrollValue = 1f;
			}
			break;
		case 20:
		case 40:
			if (scrollValue <= 0.1f)
			{
				mHostilityAllyCount = 3;
				mSbHostilityAllyCount.scrollValue = 0f;
			}
			else if (scrollValue <= 0.26f)
			{
				mHostilityAllyCount = 4;
				mSbHostilityAllyCount.scrollValue = 0.25f;
			}
			else if (scrollValue <= 0.51f)
			{
				mHostilityAllyCount = 5;
				mSbHostilityAllyCount.scrollValue = 0.5f;
			}
			else if (scrollValue <= 0.76f)
			{
				mHostilityAllyCount = 6;
				mSbHostilityAllyCount.scrollValue = 0.75f;
			}
			else
			{
				mHostilityAllyCount = 7;
				mSbHostilityAllyCount.scrollValue = 1f;
			}
			break;
		}
		mLbHostilityAllyCount.text = mHostilityAllyCount.ToString();
	}
}
