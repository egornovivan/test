using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using VANativeCampXML;

public class VANativeCampManager
{
	public const int levelCount = 5;

	private const int zoneMin = 0;

	private const int zoneMax = 7;

	private static VANativeCampManager mInstance;

	public VANativeCampDesc randomCampInfo;

	public int templateCount;

	public List<NativeCamp> nativeCampsList;

	public Dictionary<int, WeightPool> LevelPoolPuja;

	public Dictionary<int, WeightPool> LevelPoolPaja;

	private int numMin;

	private int numMax;

	private int distanceMin;

	private int distanceMax;

	public static VANativeCampManager Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new VANativeCampManager();
			}
			return mInstance;
		}
	}

	public VANativeCampManager()
	{
		LoadXml();
	}

	public void LoadXml()
	{
		string path = "RandomTown/VANativeCamp";
		if (!GameConfig.IsMultiMode)
		{
			path = "RandomTown/VANativeCampSingleMode";
		}
		TextAsset textAsset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
		StringReader stringReader = new StringReader(textAsset.text);
		if (stringReader != null)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(VANativeCampDesc));
			randomCampInfo = (VANativeCampDesc)xmlSerializer.Deserialize(stringReader);
			stringReader.Close();
			nativeCampsList = randomCampInfo.nativeCamps.ToList();
			templateCount = randomCampInfo.nativeCamps.Count();
			numMin = randomCampInfo.numMin;
			numMax = randomCampInfo.numMax;
			distanceMin = randomCampInfo.distanceMin;
			distanceMax = randomCampInfo.distanceMax;
		}
	}

	public void Init()
	{
		LevelPoolPuja = new Dictionary<int, WeightPool>();
		LevelPoolPaja = new Dictionary<int, WeightPool>();
		for (int i = 0; i < randomCampInfo.nativeCamps.Count(); i++)
		{
			NativeCamp nativeCamp = randomCampInfo.nativeCamps[i];
			if (nativeCamp.nativeType == 0)
			{
				if (nativeCamp.level >= 5)
				{
					LogManager.Error("NativeCampXml error!", nativeCamp.cid);
				}
				if (!LevelPoolPuja.ContainsKey(nativeCamp.level))
				{
					LevelPoolPuja.Add(nativeCamp.level, new WeightPool());
				}
				LevelPoolPuja[nativeCamp.level].Add(nativeCamp.weight, nativeCamp.cid);
			}
			else
			{
				if (nativeCamp.level >= 5)
				{
					LogManager.Error("NativeCampXml error!", nativeCamp.cid);
				}
				if (!LevelPoolPaja.ContainsKey(nativeCamp.level))
				{
					LevelPoolPaja.Add(nativeCamp.level, new WeightPool());
				}
				LevelPoolPaja[nativeCamp.level].Add(nativeCamp.weight, nativeCamp.cid);
			}
		}
	}

	public void Clear()
	{
		LevelPoolPuja = new Dictionary<int, WeightPool>();
		LevelPoolPaja = new Dictionary<int, WeightPool>();
	}

	public void SetCampDistance(float scale)
	{
		distanceMin = (int)((float)randomCampInfo.distanceMin * scale);
		distanceMax = (int)((float)randomCampInfo.distanceMax * scale);
		VArtifactUtil.spawnRadius = Mathf.FloorToInt((float)VArtifactUtil.spawnRadius0 * scale);
	}
}
