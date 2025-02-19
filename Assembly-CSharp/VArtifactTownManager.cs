using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Pathea;
using TownData;
using uLink;
using UnityEngine;
using VANativeCampXML;
using VArtifactTownXML;

public class VArtifactTownManager : UnityEngine.MonoBehaviour
{
	public delegate void TownDestroyed(int allyId);

	private const int version000 = 20160902;

	private const int currentVersion = 20160902;

	private const int CACHE_COUNT = 2;

	public const int levelCount = 5;

	private const int detectedChunkAroundNum = 8;

	private static VArtifactTownManager mInstance;

	private List<IntVector2> renderReadyList = new List<IntVector2>();

	public Dictionary<int, VArtifactTown> townIdData = new Dictionary<int, VArtifactTown>();

	public Dictionary<IntVector2, VArtifactTown> townPosInfo = new Dictionary<IntVector2, VArtifactTown>();

	public Dictionary<IntVector2, VArtifactUnit> unitCenterData = new Dictionary<IntVector2, VArtifactUnit>();

	public Dictionary<IntVector2, VATileInfo> townTile = new Dictionary<IntVector2, VATileInfo>();

	public List<VArtifactUnit> OutputedTownList = new List<VArtifactUnit>();

	public VArtifactUnit outputedTown;

	private Dictionary<int, WeightPool> LevelPool = new Dictionary<int, WeightPool>();

	private int townDistanceX;

	private int townDistanceZ;

	private List<int> townNamePool = new List<int>();

	private VArtifactTownDesc VartifactTownXmlInfo = new VArtifactTownDesc();

	private VATown startTown;

	private List<VATown> vatownList;

	public int missionStartBuildingID = -1;

	public int missionStartNpcID = -1;

	public int missionStartNpcEntityId = -1;

	public Vector3 playerStartPos = default(Vector3);

	private int townTemplateNum;

	private int minX = -19200;

	private int minZ = -19200;

	private int maxX = 19200;

	private int maxZ = 19200;

	private int levelRadius = 3000;

	private int detectedChunkNum = 32;

	private Dictionary<int, List<VArtifactTown>> levelNpcTown = new Dictionary<int, List<VArtifactTown>>();

	public Dictionary<IntVector2, int> mRenderedChunk;

	public Dictionary<int, int> capturedCampId = new Dictionary<int, int>();

	public List<IntVector2> mDetectedTowns = new List<IntVector2>();

	public Dictionary<int, VATSaveData> mVATSaveData = new Dictionary<int, VATSaveData>();

	public Dictionary<int, List<int>> mAliveBuildings = new Dictionary<int, List<int>>();

	public Dictionary<int, List<SceneEntityPosAgent>> MonsterPointAgentDic = new Dictionary<int, List<SceneEntityPosAgent>>();

	private int counter;

	public List<IntVector2> renderTownList = new List<IntVector2>();

	public static VArtifactTownManager Instance => mInstance;

	public Dictionary<IntVector2, VArtifactTown> TownPosInfo => townPosInfo;

	public int MinX
	{
		get
		{
			return minX;
		}
		set
		{
			minX = value;
		}
	}

	public int MinZ
	{
		get
		{
			return minZ;
		}
		set
		{
			minZ = value;
		}
	}

	public int MaxX
	{
		get
		{
			return maxX;
		}
		set
		{
			maxX = value;
		}
	}

	public int MaxZ
	{
		get
		{
			return maxZ;
		}
		set
		{
			maxZ = value;
		}
	}

	public int LevelRadius
	{
		get
		{
			return levelRadius;
		}
		set
		{
			levelRadius = value;
		}
	}

	public int DetectedChunkNum
	{
		get
		{
			return detectedChunkNum;
		}
		set
		{
			detectedChunkNum = value;
		}
	}

	public event TownDestroyed TownDestroyedEvent;

	public void RegistTownDestryedEvent(TownDestroyed eventListener)
	{
		this.TownDestroyedEvent = (TownDestroyed)Delegate.Remove(this.TownDestroyedEvent, eventListener);
		this.TownDestroyedEvent = (TownDestroyed)Delegate.Combine(this.TownDestroyedEvent, eventListener);
	}

	public void UnRegistTownDestryedEvent(TownDestroyed eventListener)
	{
		this.TownDestroyedEvent = (TownDestroyed)Delegate.Remove(this.TownDestroyedEvent, eventListener);
	}

	public void SetSaveData(int townId, int ms_id)
	{
		if (!mVATSaveData.ContainsKey(townId))
		{
			mVATSaveData.Add(townId, new VATSaveData());
		}
		VATSaveData vATSaveData = mVATSaveData[townId];
		vATSaveData.townId = townId;
		vATSaveData.ms_id = ms_id;
	}

	public void SetSaveData(int townId, double lastHour, double nextHour)
	{
		if (!mVATSaveData.ContainsKey(townId))
		{
			mVATSaveData.Add(townId, new VATSaveData());
		}
		VATSaveData vATSaveData = mVATSaveData[townId];
		vATSaveData.townId = townId;
		vATSaveData.lastHour = lastHour;
		vATSaveData.nextHour = nextHour;
	}

	private void Awake()
	{
		mInstance = this;
		InitData();
	}

	private void Update()
	{
		counter++;
		if (counter % 120 == 0)
		{
			counter = 0;
			DetectTownsAround();
		}
		if (renderTownList.Count <= 0)
		{
			return;
		}
		lock (renderTownList)
		{
			foreach (IntVector2 renderTown in renderTownList)
			{
				RenderReady(renderTown);
			}
			renderTownList.Clear();
		}
	}

	private void InitData()
	{
	}

	private void InitVATData()
	{
		foreach (KeyValuePair<int, VATSaveData> mVATSaveDatum in mVATSaveData)
		{
			if (townIdData[mVATSaveDatum.Key] != null)
			{
				townIdData[mVATSaveDatum.Key].ms_id = mVATSaveDatum.Value.ms_id;
				townIdData[mVATSaveDatum.Key].lastHour = mVATSaveDatum.Value.lastHour;
				townIdData[mVATSaveDatum.Key].nextHour = mVATSaveDatum.Value.nextHour;
			}
			if (MonsterSiege_Town.Instance != null)
			{
				MonsterSiege_Town.Instance.OnNewTown(townIdData[mVATSaveDatum.Key]);
			}
		}
	}

	public void LoadXMLAtPath()
	{
		string path = "RandomTown/VArtifactTown";
		TextAsset textAsset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
		StringReader stringReader = new StringReader(textAsset.text);
		if (stringReader != null)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(VArtifactTownDesc));
			VartifactTownXmlInfo = (VArtifactTownDesc)xmlSerializer.Deserialize(stringReader);
			stringReader.Close();
			townDistanceX = VartifactTownXmlInfo.distanceX;
			townDistanceZ = VartifactTownXmlInfo.distanceZ;
		}
	}

	public void InitISO()
	{
		string iSOPath = GameConfig.PEDataPath + "RandomTownArt";
		VArtifactUtil.ISOPath = iSOPath;
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (Town_artifacts value in VArtifactUtil.townArtifactsData.Values)
		{
			if (!list2.Contains(value.isoName))
			{
				list2.Add(value.isoName);
				list.Add(VArtifactUtil.GetISONameFullPath(value.isoName));
			}
		}
		foreach (string item in list)
		{
			VArtifactUtil.LoadIso(item);
		}
	}

	public void Clear()
	{
		townIdData.Clear();
		townPosInfo.Clear();
		unitCenterData.Clear();
		townTile.Clear();
		OutputedTownList.Clear();
		LevelPool.Clear();
		levelNpcTown.Clear();
		capturedCampId.Clear();
		mAliveBuildings.Clear();
		MonsterPointAgentDic.Clear();
		VArtifactUtil.Clear();
		VATownGenerator.Instance.ClearData();
		this.TownDestroyedEvent = null;
	}

	public void GenTown()
	{
		VATownGenerator.Instance.ClearData();
		LoadXMLAtPath();
		VATownGenerator.Instance.InitAllyDistribution(new System.Random(RandomMapConfig.AllyGenSeed));
		System.Random random = new System.Random(RandomMapConfig.TownGenSeed);
		startTown = VartifactTownXmlInfo.vaStartTown;
		vatownList = VartifactTownXmlInfo.vaTown.ToList();
		townTemplateNum = vatownList.Count;
		for (int i = 0; i < vatownList.Count; i++)
		{
			VATown vATown = vatownList[i];
			if (vATown.level >= 5)
			{
				LogManager.Error("level >= levelCount Xml error!", vATown.tid);
			}
			if (!LevelPool.ContainsKey(vATown.level))
			{
				LevelPool.Add(vATown.level, new WeightPool());
			}
			LevelPool[vATown.level].Add(vATown.weight, vATown.tid);
		}
		townNamePool = new List<int>();
		townNamePool.AddRange(VArtifactUtil.townNameData.Keys);
		ArtifactUnit[] artifactUnitArray = VartifactTownXmlInfo.vaStartTown.artifactUnitArray;
		foreach (ArtifactUnit artifactUnit in artifactUnitArray)
		{
			Town_artifacts town_artifacts = VArtifactUtil.townArtifactsData[Convert.ToInt32(artifactUnit.id)];
			BuildingIdNum[] buildingIdNum = artifactUnit.buildingIdNum;
			foreach (BuildingIdNum buildingIdNum2 in buildingIdNum)
			{
				if (buildingIdNum2.posIndex >= town_artifacts.buildingCell.Count)
				{
					Debug.LogError("townXML&DataBase error! Tid: " + VartifactTownXmlInfo.vaStartTown.tid + ",vauId: " + artifactUnit.id);
					Debug.LogError("PosIndex Too large! bid: " + buildingIdNum2.bid + " PosIndex: " + buildingIdNum2.posIndex + " B_Position Count: " + town_artifacts.buildingCell.Count);
				}
			}
		}
		foreach (VATown vatown in vatownList)
		{
			ArtifactUnit[] artifactUnitArray2 = vatown.artifactUnitArray;
			foreach (ArtifactUnit artifactUnit2 in artifactUnitArray2)
			{
				Town_artifacts town_artifacts2 = VArtifactUtil.townArtifactsData[Convert.ToInt32(artifactUnit2.id)];
				BuildingIdNum[] buildingIdNum3 = artifactUnit2.buildingIdNum;
				foreach (BuildingIdNum buildingIdNum4 in buildingIdNum3)
				{
					if (buildingIdNum4.posIndex >= town_artifacts2.buildingCell.Count)
					{
						Debug.LogError("townXML&DataBase error! Tid: " + vatown.tid + ",vauId: " + artifactUnit2.id);
						Debug.LogError("PosIndex Too large! bid: " + buildingIdNum4.bid + " PosIndex: " + buildingIdNum4.posIndex + " B_Position Count: " + town_artifacts2.buildingCell.Count);
					}
				}
			}
		}
		VANativeCampManager.Instance.Init();
		InitParam();
		unitCenterData = new Dictionary<IntVector2, VArtifactUnit>();
		List<int> pickedGenLine = VATownGenerator.Instance.GetPickedGenLine();
		int townAmountMax = VATownGenerator.Instance.GetTownAmountMax();
		int townAmountMin = VATownGenerator.Instance.GetTownAmountMin();
		int num = 0;
		int townId = 0;
		for (int n = 0; n < pickedGenLine.Count; n++)
		{
			int levelByLineIndex = VATownGenerator.Instance.GetLevelByLineIndex(n);
			int generatedCount = 0;
			int num2 = pickedGenLine[n];
			if (n == 0)
			{
				IntVector2 intVector = VATownGenerator.Instance.GenTownPos(num2, random);
				num = -1;
				missionStartNpcID = startTown.artifactUnitArray[0].npcIdNum[0].nid;
				missionStartBuildingID = startTown.artifactUnitArray[0].buildingIdNum[0].bid;
				VArtifactTown vArtifactTown = new VArtifactTown(startTown, intVector);
				VArtifactUtil.GetArtifactUnit(vArtifactTown, startTown.artifactUnitArray, random);
				if (!VArtifactUtil.CheckTownAvailable(vArtifactTown))
				{
					continue;
				}
				vArtifactTown.townId = townId++;
				vArtifactTown.townNameId = PickATownName(random);
				vArtifactTown.areaId = num2;
				vArtifactTown.AllyId = 0;
				vArtifactTown.isMainTown = true;
				VATownGenerator.Instance.AddAllyTown(vArtifactTown);
				InitTownData(vArtifactTown, intVector, random);
				generatedCount++;
				GenEmptyTown(intVector, 0, ref townId, num2, random);
			}
			int genCount = townAmountMax - generatedCount;
			GenSomeTowns(genCount, levelByLineIndex, ref townId, num2, random, ref generatedCount);
			for (int num3 = 0; num3 < 3; num3++)
			{
				if (generatedCount >= townAmountMin)
				{
					break;
				}
				genCount = townAmountMax - generatedCount;
				GenSomeTowns(genCount, levelByLineIndex, ref townId, num2, random, ref generatedCount);
			}
		}
		int generatedCount2 = 0;
		for (int num4 = 0; num4 < TownGenData.AreaCount; num4++)
		{
			int levelByLineIndex2 = VATownGenerator.Instance.GetLevelByLineIndex(num4);
			int genAreaId = pickedGenLine[num4];
			GenBranchTowns(VATownGenerator.Instance.branchTownCountMax, levelByLineIndex2, ref townId, genAreaId, random, ref generatedCount2);
		}
		VATownGenerator.Instance.ConnectMainTowns();
		VATownGenerator.Instance.GenerateConnection(random);
		VArtifactUtil.ClearAllISO();
		InitTownHeight();
		if (Application.isEditor)
		{
			PrintTownPos();
		}
		if (!PeGameMgr.IsMulti)
		{
			return;
		}
		List<int> list = new List<int>();
		List<Vector3> list2 = new List<Vector3>();
		foreach (int key in townIdData.Keys)
		{
			list.Add(key);
			list2.Add(townIdData[key].TransPos);
		}
		NetworkManager.SyncServer(EPacketType.PT_Common_InitTownPos, list.ToArray(), list2.ToArray());
	}

	public void GenEmptyTown(IntVector2 townPos, int level, ref int townId, int genAreaId, System.Random myRand)
	{
		int num = 0;
		while (num < 3)
		{
			List<int> list = VArtifactUtil.RandomChoose(3, 0, 7, myRand);
			for (int i = 0; i < list.Count; i++)
			{
				IntVector2 intVector = VArtifactUtil.GenCampByZone(townPos, list[i], VATownGenerator.Instance.playerEmptyTownDistanceMin, VATownGenerator.Instance.playerEmptyTownDistanceMax, myRand);
				if (Mathf.Abs(intVector.x) > RandomMapConfig.Instance.mapRadius || Mathf.Abs(intVector.y) > RandomMapConfig.Instance.mapRadius)
				{
					continue;
				}
				IntVector2 intVector2 = VATownGenerator.Instance.GenTownPos(genAreaId, myRand);
				int templateId = LevelPool[level].GetRandID(myRand);
				startTown = vatownList.Find((VATown it) => it.tid == templateId);
				VArtifactTown vArtifactTown = new VArtifactTown(startTown, intVector2);
				VArtifactUtil.GetArtifactUnit(vArtifactTown, startTown.artifactUnitArray, myRand);
				if (VArtifactUtil.CheckTownAvailable(vArtifactTown))
				{
					vArtifactTown.townId = townId++;
					vArtifactTown.townNameId = 0;
					vArtifactTown.areaId = genAreaId;
					vArtifactTown.AllyId = 0;
					vArtifactTown.isEmpty = true;
					vArtifactTown.isMainTown = false;
					InitTownData(vArtifactTown, intVector2, myRand);
					num++;
					VATownGenerator.Instance.AddEmtpyTown(vArtifactTown);
					Debug.Log("emptyTown:" + intVector2);
					if (num >= 3)
					{
						break;
					}
				}
			}
		}
	}

	public void GenSomeTowns(int genCount, int level, ref int townId, int genAreaId, System.Random myRand, ref int generatedCount)
	{
		for (int i = 0; i < genCount; i++)
		{
			IntVector2 intVector = VATownGenerator.Instance.GenTownPos(genAreaId, myRand);
			AllyType allyTypeByAreaId = VATownGenerator.Instance.GetAllyTypeByAreaId(genAreaId);
			int templateId;
			VArtifactTown vArtifactTown;
			if (allyTypeByAreaId == AllyType.Player || allyTypeByAreaId == AllyType.Npc || VATownGenerator.Instance.GetAllyTownCount(0) < 4)
			{
				if (townId == 6)
				{
					templateId = LevelPool[2].GetRandID(myRand);
				}
				else if (townId == 4)
				{
					templateId = LevelPool[2].GetRandID(myRand);
				}
				else
				{
					templateId = LevelPool[level].GetRandID(myRand);
				}
				startTown = vatownList.Find((VATown it) => it.tid == templateId);
				vArtifactTown = new VArtifactTown(startTown, intVector);
				VArtifactUtil.GetArtifactUnit(vArtifactTown, startTown.artifactUnitArray, myRand);
				if (!VArtifactUtil.CheckTownAvailable(vArtifactTown))
				{
					continue;
				}
			}
			else
			{
				if (allyTypeByAreaId == AllyType.Puja)
				{
					templateId = VANativeCampManager.Instance.LevelPoolPuja[level].GetRandID(myRand);
				}
				else
				{
					templateId = VANativeCampManager.Instance.LevelPoolPaja[level].GetRandID(myRand);
				}
				NativeCamp nativeCamp = VANativeCampManager.Instance.nativeCampsList.Find((NativeCamp it) => it.cid == templateId);
				vArtifactTown = new VArtifactTown(nativeCamp, intVector);
				VArtifactUtil.GetArtifactUnit(vArtifactTown, nativeCamp.artifactUnitArray, myRand);
				if (!VArtifactUtil.CheckTownAvailable(vArtifactTown))
				{
					continue;
				}
			}
			vArtifactTown.townId = townId++;
			vArtifactTown.townNameId = PickATownName(myRand);
			vArtifactTown.areaId = genAreaId;
			if (VATownGenerator.Instance.GetAllyTownCount(0) < 4)
			{
				vArtifactTown.AllyId = 0;
			}
			else
			{
				vArtifactTown.AllyId = VATownGenerator.Instance.GetAllyIdByAreaId(genAreaId);
			}
			VATownGenerator.Instance.AddAllyTown(vArtifactTown);
			vArtifactTown.isMainTown = true;
			InitTownData(vArtifactTown, intVector, myRand);
			generatedCount++;
		}
	}

	public void GenBranchTowns(int genCount, int level, ref int townId, int genAreaId, System.Random myRand, ref int generatedCount)
	{
		for (int i = 0; i < genCount; i++)
		{
			IntVector2 intVector = VATownGenerator.Instance.GenTownPos(genAreaId, myRand);
			int templateId = LevelPool[level].GetRandID(myRand);
			startTown = vatownList.Find((VATown it) => it.tid == templateId);
			VArtifactTown vArtifactTown = new VArtifactTown(startTown, intVector);
			VArtifactUtil.GetArtifactUnit(vArtifactTown, startTown.artifactUnitArray, myRand);
			if (VArtifactUtil.CheckTownAvailable(vArtifactTown))
			{
				vArtifactTown.townId = townId++;
				vArtifactTown.townNameId = PickATownName(myRand);
				vArtifactTown.areaId = genAreaId;
				vArtifactTown.AllyId = 0;
				VATownGenerator.Instance.AddAllyTown(vArtifactTown);
				vArtifactTown.isMainTown = false;
				InitTownData(vArtifactTown, intVector, myRand);
				generatedCount++;
			}
		}
	}

	public void InitTownData(VArtifactTown townData, IntVector2 genPosStart, System.Random myRand)
	{
		AddTownData(townData);
		for (int i = 0; i < townData.VAUnits.Count; i++)
		{
			VArtifactUnit vau = townData.VAUnits[i];
			GenArtifactUnit(vau);
		}
	}

	public void AddTownData(VArtifactTown townData)
	{
		townIdData.Add(townData.townId, townData);
		townPosInfo.Add(townData.PosCenter, townData);
		if (townData.type == VArtifactType.NpcTown)
		{
			if (!levelNpcTown.ContainsKey(townData.level))
			{
				List<VArtifactTown> list = new List<VArtifactTown>();
				list.Add(townData);
				levelNpcTown.Add(townData.level, list);
			}
			else
			{
				levelNpcTown[townData.level].Add(townData);
			}
		}
		VATownGenerator.Instance.AddTown(townData.areaId, townData);
	}

	public void SwitchTownId(VArtifactTown t1, VArtifactTown t2)
	{
		int townId = t2.townId;
		t2.townId = t1.townId;
		t1.townId = townId;
		townIdData[t1.townId] = t1;
		townIdData[t2.townId] = t2;
	}

	private int PickATownName(System.Random rand)
	{
		int index = rand.Next(townNamePool.Count);
		int townNameId = VArtifactUtil.GetTownNameId(townNamePool[index]);
		townNamePool.RemoveAt(index);
		return townNameId;
	}

	private void InitTownHeight()
	{
		foreach (VArtifactTown value in townPosInfo.Values)
		{
			foreach (VArtifactUnit vAUnit in value.VAUnits)
			{
				vAUnit.SetHeight(GetMinHeight(vAUnit.PosStart, vAUnit.PosEnd) - 4f);
			}
			value.height = Mathf.CeilToInt(value.VAUnits[0].worldPos.y + (float)value.VAUnits[0].vaSize.z);
			if (value.templateId == -1)
			{
				VArtifactUnit vArtifactUnit = value.VAUnits[0];
				Quaternion quaternion = default(Quaternion);
				quaternion.eulerAngles = new Vector3(0f, vArtifactUnit.rot, 0f);
				Vector3 vector = vArtifactUnit.npcPos[0] + new Vector3(0f, 1f, -1f) - vArtifactUnit.worldPos;
				vector.x += vArtifactUnit.isoStartPos.x;
				vector.y += vArtifactUnit.worldPos.y;
				vector.z += vArtifactUnit.isoStartPos.y;
				Vector3 vector2 = vArtifactUnit.worldPos + quaternion * vector;
				playerStartPos = vector2;
			}
			if (value.templateId == -1)
			{
				AIErodeMap.AddErode(new Vector3(value.PosCenter.x, VFDataRTGen.GetPosHeight(value.PosCenter), value.PosCenter.y), value.radius + 96);
			}
			else
			{
				AIErodeMap.AddErode(new Vector3(value.PosCenter.x, VFDataRTGen.GetPosHeight(value.PosCenter), value.PosCenter.y), value.radius + 64);
			}
		}
	}

	private float GetAvgHeight(IntVector2 startPos, IntVector2 endPos)
	{
		return (float)(VFDataRTGen.GetPosHeight(startPos.x, (startPos.y + endPos.y) / 2) + VFDataRTGen.GetPosHeight((startPos.x + endPos.x) / 2, startPos.y) + VFDataRTGen.GetPosHeight(endPos.x, (startPos.y + endPos.y) / 2) + VFDataRTGen.GetPosHeight((startPos.x + endPos.x) / 2, endPos.y) + VFDataRTGen.GetPosHeight((startPos.x + endPos.x) / 2, (startPos.y + endPos.y) / 2)) / 5f;
	}

	private float GetMinHeight(IntVector2 startPos, IntVector2 endPos)
	{
		return Mathf.Min(VFDataRTGen.GetPosHeight(startPos.x, (startPos.y + endPos.y) / 2), VFDataRTGen.GetPosHeight((startPos.x + endPos.x) / 2, startPos.y), VFDataRTGen.GetPosHeight(endPos.x, (startPos.y + endPos.y) / 2), VFDataRTGen.GetPosHeight((startPos.x + endPos.x) / 2, endPos.y), VFDataRTGen.GetPosHeight((startPos.x + endPos.x) / 2, (startPos.y + endPos.y) / 2));
	}

	private void TestNewTownGen()
	{
		Dictionary<IntVector2, int> dictionary = new Dictionary<IntVector2, int>();
		int second = DateTime.Now.Second;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = -4000; i <= 4000; i += 64)
		{
			for (int j = -4000; j <= 4000; j += 64)
			{
				IntVector2 intVector = new IntVector2(i, j);
				int posHeight = VFDataRTGen.GetPosHeight(intVector, inWater: true);
				if (VFDataRTGen.IsSea(posHeight))
				{
					num2++;
				}
				if (posHeight > 20 && posHeight < 50)
				{
					num++;
					dictionary.Add(intVector, posHeight);
				}
				RandomMapType xZMapType = VFDataRTGen.GetXZMapType(intVector);
				if (xZMapType == RandomMapType.GrassLand)
				{
					num3++;
				}
				if (xZMapType == RandomMapType.Desert)
				{
					num4++;
				}
			}
		}
		Dictionary<IntVector2, int> dictionary2 = new Dictionary<IntVector2, int>();
		Dictionary<IntVector2, int> dictionary3 = new Dictionary<IntVector2, int>();
		Dictionary<int, Dictionary<IntVector2, int>> dictionary4 = new Dictionary<int, Dictionary<IntVector2, int>>();
		for (int k = 0; k < 16; k++)
		{
			dictionary4.Add(k, new Dictionary<IntVector2, int>());
		}
		foreach (KeyValuePair<IntVector2, int> item in dictionary)
		{
			if (dictionary.ContainsKey(item.Key + new IntVector2(-64, 0)) && dictionary.ContainsKey(item.Key + new IntVector2(64, 0)) && dictionary.ContainsKey(item.Key + new IntVector2(0, -64)) && dictionary.ContainsKey(item.Key + new IntVector2(0, 64)))
			{
				dictionary2.Add(item.Key, item.Value);
				int num5 = item.Key.x / 2000;
				if (item.Key.x < 0)
				{
					num5--;
				}
				int num6 = item.Key.y / 2000;
				if (item.Key.y < 0)
				{
					num6--;
				}
				int num7 = num5 + 2 + 4 * (1 - num6);
				if (dictionary4.ContainsKey(num7))
				{
					dictionary4[num7].Add(item.Key, item.Value);
				}
				else
				{
					Debug.LogError("not containsKey: " + num7);
				}
			}
		}
		foreach (KeyValuePair<IntVector2, int> item2 in dictionary)
		{
			if (dictionary.ContainsKey(new IntVector2(item2.Key.x + 64, item2.Key.y)))
			{
				dictionary3.Add(item2.Key, item2.Value);
			}
		}
		int second2 = DateTime.Now.Second;
		Debug.LogError("time: " + (second2 - second) + "seaCount:" + num2 + "plainCount:" + num + "grassland:" + num3 + "desert:" + num4 + "townCount:" + dictionary2.Count + "townCount2:" + dictionary3.Count);
		for (int l = 0; l < 16; l++)
		{
			Debug.LogError("districtTownCount: " + l + "_" + dictionary4[l].Count);
		}
		int num8 = -1;
		int num9 = 0;
		int num10 = 0;
		Debug.LogError("testX1:" + num8 + "testX2:" + num9 + "testX3:" + num10);
	}

	private void TestNewTerrain()
	{
		for (int i = -4000; i <= 4000; i += 64)
		{
			for (int j = -4000; j <= 4000; j += 64)
			{
				IntVector2 intVector = new IntVector2(i, j);
				float num = VFDataRTGen.GetfNoise12D1ten(i, j);
				if (num > 90f)
				{
					Debug.Log(string.Concat("pos:", intVector, " ftertype:", num));
				}
			}
		}
	}

	public void GenArtifactUnit(VArtifactUnit vau)
	{
		unitCenterData.Add(vau.PosCenter, vau);
		LinkToChunk(vau);
	}

	public void LinkToChunk(VArtifactUnit vaUnit)
	{
		List<IntVector2> list = VArtifactUtil.LinkedChunkIndex(vaUnit);
		for (int i = 0; i < list.Count; i++)
		{
			IntVector2 key = list[i];
			if (!townTile.ContainsKey(key))
			{
				List<VArtifactUnit> list2 = new List<VArtifactUnit>();
				list2.Add(vaUnit);
				VATileInfo value = new VATileInfo(list2, vaUnit.vat);
				townTile.Add(key, value);
			}
			else
			{
				townTile[key].AddUnit(vaUnit);
			}
		}
	}

	public List<VArtifactUnit> OutputTownData(IntVector2 tileIndex)
	{
		List<VArtifactUnit> tileUnitList = GetTileUnitList(tileIndex);
		if (tileUnitList == null)
		{
			return null;
		}
		tileUnitList.RemoveAll((VArtifactUnit it) => it.vat.isEmpty);
		if (tileUnitList.Count == 0)
		{
			return null;
		}
		List<VArtifactUnit> list = new List<VArtifactUnit>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < tileUnitList.Count; i++)
		{
			VArtifactUnit outputTown = tileUnitList[i];
			if (!OutputedTownList.Contains(outputTown))
			{
				list.Add(outputTown);
				continue;
			}
			list2.Add(OutputedTownList.FindIndex((VArtifactUnit it) => it == outputTown));
		}
		if (list.Count + OutputedTownList.Count <= 2)
		{
			for (int j = 0; j < list.Count; j++)
			{
				VArtifactUtil.OutputVoxels(list[j].worldPos, list[j], list[j].rot);
				OutputedTownList.Add(list[j]);
			}
		}
		else
		{
			List<int> list3 = new List<int>();
			for (int k = 0; k < 2; k++)
			{
				if (!list2.Contains(k))
				{
					list3.Add(k);
				}
			}
			int num = OutputedTownList.Count + list.Count - 2;
			for (int l = 0; l < num; l++)
			{
				int num2 = list3[l] - l;
				bool flag = true;
				for (int m = 0; m < OutputedTownList.Count; m++)
				{
					if (OutputedTownList[m].isoGuId == OutputedTownList[num2].isoGuId && m != num2)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					VArtifactUtil.isos.Remove(OutputedTownList[num2].isoGuId);
				}
				OutputedTownList[num2].Clear();
				OutputedTownList.RemoveAt(num2);
			}
			for (int n = 0; n < list.Count; n++)
			{
				VArtifactUtil.OutputVoxels(list[n].worldPos, list[n], list[n].rot);
				OutputedTownList.Add(list[n]);
			}
		}
		return tileUnitList;
	}

	public void GenTownFromTileIndex(IntVector2 tileIndex)
	{
		if (TileContainsTown(tileIndex))
		{
			List<VArtifactUnit> tileUnitList = GetTileUnitList(tileIndex);
			for (int i = 0; i < tileUnitList.Count; i++)
			{
				ArtifactAddToRender(tileUnitList[i], tileIndex);
			}
		}
	}

	public void RandomArtifactTown(int townId)
	{
		RandomArtifactTown(townIdData[townId]);
	}

	public void RandomArtifactTown(VArtifactTown vat)
	{
		if (!vat.isRandomed)
		{
			for (int i = 0; i < vat.VAUnits.Count; i++)
			{
				RandomArtifactUnit(vat.VAUnits[i]);
			}
			vat.isRandomed = true;
		}
	}

	public void RandomArtifactUnit(VArtifactUnit vau)
	{
		if (vau.isRandomed)
		{
			return;
		}
		List<int> list = new List<int>();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 0;
		int count = vau.buildingIdNum.Count;
		int num2 = 0;
		int count2 = vau.buildingCell.Count;
		for (int i = 0; i < count; i++)
		{
			if (vau.buildingIdNum[i].posIndex != -1)
			{
				if (vau.buildingIdNum[i].posIndex >= vau.buildingCell.Count)
				{
					Debug.LogError(string.Concat("PosIndex Too large! Tid: ", vau.vat.templateId, " bid: ", vau.buildingIdNum[i], " PosIndex: ", vau.buildingIdNum[i].posIndex, " B_Position Count: ", vau.buildingCell.Count));
					for (int j = 0; j < vau.buildingIdNum[i].num; j++)
					{
						list.Add(vau.buildingIdNum[i].bid);
					}
					num += vau.buildingIdNum[i].num;
				}
				else
				{
					dictionary.Add(vau.buildingIdNum[i].posIndex, vau.buildingIdNum[i].bid);
					num2++;
				}
			}
			else
			{
				for (int k = 0; k < vau.buildingIdNum[i].num; k++)
				{
					list.Add(vau.buildingIdNum[i].bid);
				}
				num += vau.buildingIdNum[i].num;
			}
		}
		int num3 = count2 - num2;
		if (num3 > num)
		{
			for (int l = 0; l < num3 - num; l++)
			{
				list.Add(-1);
			}
		}
		System.Random random = new System.Random(vau.PosCenter.x + vau.PosCenter.y);
		VArtifactUtil.Shuffle(list, random);
		if (vau.vat.templateId == -1 && !dictionary.ContainsValue(missionStartBuildingID))
		{
			if (num > num3)
			{
				list.RemoveRange(num3, num - num3);
			}
			if (!list.Contains(missionStartBuildingID))
			{
				int index = random.Next(num3);
				list[index] = missionStartBuildingID;
			}
		}
		List<int> list2 = new List<int>();
		for (int m = 0; m < count2; m++)
		{
			if (dictionary.ContainsKey(m))
			{
				list2.Add(dictionary[m]);
				continue;
			}
			list2.Add(list[0]);
			list.RemoveAt(0);
		}
		CreateBuildingInfo(vau, list2);
		List<Vector3> npcPos = vau.npcPos;
		List<NpcIdNum> npcIdNum = vau.npcIdNum;
		List<int> list3 = new List<int>();
		for (int n = 0; n < npcIdNum.Count; n++)
		{
			int nid = npcIdNum[n].nid;
			int num4 = npcIdNum[n].num;
			for (int num5 = 0; num5 < num4; num5++)
			{
				list3.Add(nid);
			}
		}
		int num6 = npcPos.Count;
		int num7 = list3.Count;
		if (vau.type == VArtifactType.NpcTown && vau.vat.templateId == -1 && vau.unitIndex == 0)
		{
			num6--;
			list3.Remove(missionStartNpcID);
			num7--;
		}
		if (num6 > num7)
		{
			for (int num8 = 0; num8 < num6 - num7; num8++)
			{
				list3.Add(-1);
			}
		}
		VArtifactUtil.Shuffle(list3, random);
		CreateTownNPCAndNative(vau, list3);
		vau.isRandomed = true;
	}

	public void CreateBuildingInfo(VArtifactUnit vau, List<int> buildingIdList)
	{
		if (vau.type == VArtifactType.NpcTown)
		{
			int num = vau.buildingCell.Count();
			for (int i = 0; i < num; i++)
			{
				if (buildingIdList[i] != -1)
				{
					int num2 = buildingIdList[i];
					if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(num2))
					{
						LogManager.Error("bid = [", num2, "] not exist in database!");
						break;
					}
					BuildingCell buildingCell = vau.buildingCell[i];
					Vector3 posAfterRotation = VArtifactUtil.GetPosAfterRotation(vau, buildingCell.cellPos);
					float rot = buildingCell.cellRot + vau.rot;
					int buildingNo = vau.vat.buildingNo++;
					BuildingID buildingID = new BuildingID(vau.vat.townId, buildingNo);
					Vector2 vector = new Vector2(20f, 20f);
					BlockBuilding blockBuilding = BlockBuilding.s_tblBlockBuildingMap[num2];
					vector = blockBuilding.mSize;
					VABuildingInfo vABuildingInfo = new VABuildingInfo(posAfterRotation, rot, num2, buildingID, VABuildingType.Prefeb, vau, vector);
					VABuildingManager.Instance.AddBuilding(vABuildingInfo);
					if (vau.vat.templateId == -1 && num2 == missionStartBuildingID)
					{
						VABuildingManager.Instance.missionBuilding.Add(0, buildingID);
						Debug.Log(string.Concat("<color=yellow>mission Building pos: ", posAfterRotation, "</color>"));
						PlayerMission.StoreBuildingPos(0, vABuildingInfo.frontDoorPos);
					}
					vau.buildingPosID[posAfterRotation] = buildingID;
				}
			}
			return;
		}
		vau.towerPos.y -= 0.5f;
		if (vau.unitIndex == 0)
		{
			vau.towerPos.y -= 0.5f;
			BuildingCell buildingCell2 = new BuildingCell();
			buildingCell2.cellPos = vau.towerPos;
			buildingCell2.cellRot = 0f;
			int buildingNo2 = -1;
			Vector3 posAfterRotation2 = VArtifactUtil.GetPosAfterRotation(vau, buildingCell2.cellPos);
			float rot2 = buildingCell2.cellRot + vau.rot;
			BuildingID buildingID2 = new BuildingID(vau.vat.townId, buildingNo2);
			Vector2 size = new Vector2(5f, 5f);
			VABuildingInfo vABuildingInfo2 = new VABuildingInfo(posAfterRotation2, rot2, 0, buildingID2, VABuildingType.Prefeb, vau, size);
			vABuildingInfo2.pathID = vau.vat.nativeTower.pathID;
			vABuildingInfo2.campID = vau.vat.nativeTower.campID;
			vABuildingInfo2.damageID = vau.vat.nativeTower.damageID;
			VABuildingManager.Instance.AddBuilding(vABuildingInfo2);
			vau.buildingPosID[posAfterRotation2] = buildingID2;
		}
		int num3 = vau.buildingCell.Count();
		int num4 = 0;
		for (int j = num4; j < num3; j++)
		{
			int index = j - num4;
			if (buildingIdList[index] != -1)
			{
				int num5 = buildingIdList[index];
				if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(num5))
				{
					LogManager.Error("bid = [", num5, "] not exist in database!");
					break;
				}
				BuildingCell buildingCell3 = vau.buildingCell[j];
				buildingCell3.cellPos.y -= 0.5f;
				Vector3 posAfterRotation3 = VArtifactUtil.GetPosAfterRotation(vau, buildingCell3.cellPos);
				float rot3 = buildingCell3.cellRot + vau.rot;
				int buildingNo3 = vau.vat.buildingNo++;
				BuildingID buildingID3 = new BuildingID(vau.vat.townId, buildingNo3);
				Vector2 vector2 = new Vector2(20f, 20f);
				BlockBuilding blockBuilding2 = BlockBuilding.s_tblBlockBuildingMap[num5];
				vector2 = blockBuilding2.mSize;
				VABuildingInfo bdinfo = new VABuildingInfo(posAfterRotation3, rot3, num5, buildingID3, VABuildingType.Prefeb, vau, vector2);
				VABuildingManager.Instance.AddBuilding(bdinfo);
				vau.buildingPosID[posAfterRotation3] = buildingID3;
			}
		}
	}

	public void CreateTownNPCAndNative(VArtifactUnit vau, List<int> npcIdList)
	{
		if (vau.type == VArtifactType.NpcTown)
		{
			List<Vector3> npcPos = vau.npcPos;
			int num = 0;
			if (vau.vat.templateId == -1 && vau.unitIndex == 0)
			{
				if (PeGameMgr.IsSingle || PeGameMgr.IsMultiAdventureCoop)
				{
					Vector3 vector = VArtifactUtil.GetPosAfterRotation(vau, npcPos[0]) + new Vector3(0f, 5f, 0f);
					int posHeight = VFDataRTGen.GetPosHeight(vector.x, vector.z);
					if ((float)posHeight >= vector.y - 1f)
					{
						vector.y = posHeight + 1;
					}
					if (!VATownNpcManager.Instance.IsCreated(vector))
					{
						VATownNpcInfo vATownNpcInfo = new VATownNpcInfo(vector, missionStartNpcID);
						vATownNpcInfo.townId = vau.vat.townId;
						VATownNpcManager.Instance.AddNpc(vATownNpcInfo);
						vau.npcPosInfo.Add(vector, vATownNpcInfo);
					}
				}
				num++;
			}
			for (int i = num; i < npcPos.Count; i++)
			{
				int index = i - num;
				if (npcIdList[index] != -1)
				{
					Vector3 vector2 = VArtifactUtil.GetPosAfterRotation(vau, npcPos[i]) + new Vector3(0f, 5f, 0f);
					int posHeight2 = VFDataRTGen.GetPosHeight(vector2.x, vector2.z);
					if ((float)posHeight2 >= vector2.y - 1f)
					{
						vector2.y = posHeight2 + 1;
					}
					if (!VATownNpcManager.Instance.IsCreated(vector2) || !vau.vat.IsPlayerTown)
					{
						VATownNpcInfo vATownNpcInfo2 = new VATownNpcInfo(vector2, npcIdList[index]);
						vATownNpcInfo2.townId = vau.vat.townId;
						VATownNpcManager.Instance.AddNpc(vATownNpcInfo2);
						vau.npcPosInfo.Add(vector2, vATownNpcInfo2);
					}
				}
			}
			return;
		}
		List<Vector3> npcPos2 = vau.npcPos;
		for (int j = 0; j < npcPos2.Count; j++)
		{
			if ((PeGameMgr.IsSingleAdventure || GameConfig.IsMultiMode) && npcIdList[j] != -1)
			{
				Vector3 vector3 = VArtifactUtil.GetPosAfterRotation(vau, npcPos2[j]) + new Vector3(0f, 5f, 0f);
				int posHeight3 = VFDataRTGen.GetPosHeight(vector3.x, vector3.z);
				if ((float)posHeight3 >= vector3.y - 1f)
				{
					vector3.y = posHeight3 + 1;
				}
				NativePointInfo nativePointInfo = new NativePointInfo(vector3, npcIdList[j]);
				nativePointInfo.townId = vau.vat.townId;
				VANativePointManager.Instance.AddNative(nativePointInfo);
				vau.nativePointInfo.Add(vector3, nativePointInfo);
			}
		}
	}

	public void ArtifactAddToRender(VArtifactUnit vau, IntVector2 tileIndex)
	{
		if (!vau.isAddedToRender)
		{
			AddToRenderReady(vau, tileIndex.x, tileIndex.y);
			vau.isAddedToRender = true;
		}
	}

	public void AddToRenderReady(VArtifactUnit vau, int x, int z)
	{
		lock (renderTownList)
		{
			renderTownList.Add(new IntVector2(x, z));
		}
	}

	public void RenderReady(IntVector2 tileIndexXZ)
	{
		if (TileContainsTown(tileIndexXZ) && !renderReadyList.Contains(tileIndexXZ))
		{
			renderReadyList.Add(tileIndexXZ);
			StartCoroutine(RenderAllWaitMainPlayer(tileIndexXZ));
		}
	}

	private IEnumerator RenderAllWaitMainPlayer(IntVector2 tileIndexXZ)
	{
		while (PeSingleton<PeCreature>.Instance.mainPlayer == null || PeLauncher.Instance.isLoading)
		{
			yield return null;
		}
		List<VArtifactUnit> vauList = GetTileUnitList(tileIndexXZ);
		for (int val = 0; val < vauList.Count; val++)
		{
			foreach (VArtifactUnit vau in vauList[val].vat.VAUnits)
			{
				RenderArtifactAllContent(vau);
				vau.isDoodadNpcRendered = true;
			}
			if (!vauList[val].vat.IsExplored)
			{
				AddTown(vauList[val].vat);
				vauList[val].vat.IsExplored = true;
			}
		}
		renderReadyList.Remove(tileIndexXZ);
	}

	public void RenderArtifactAllContent(VArtifactUnit vau)
	{
		if (vau.vat.isEmpty)
		{
			return;
		}
		if (!vau.vat.isRandomed)
		{
			RandomArtifactTown(vau.vat);
		}
		List<BuildingID> list = vau.buildingPosID.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			BuildingID buildingId = list[i];
			VABuildingManager.Instance.RenderBuilding(buildingId);
		}
		if (vau.type == VArtifactType.NpcTown)
		{
			if ((vau.vat.IsPlayerTown && !vau.isDoodadNpcRendered) || (!vau.vat.IsPlayerTown && !capturedCampId.ContainsKey(vau.vat.townId)))
			{
				List<VATownNpcInfo> list2 = vau.npcPosInfo.Values.ToList();
				for (int j = 0; j < list2.Count; j++)
				{
					VATownNpcInfo townNpcInfo = list2[j];
					VATownNpcManager.Instance.RenderTownNPC(townNpcInfo);
				}
			}
		}
		else if (!capturedCampId.ContainsKey(vau.vat.townId))
		{
			List<NativePointInfo> list3 = vau.nativePointInfo.Values.ToList();
			for (int k = 0; k < list3.Count; k++)
			{
				NativePointInfo nativePointInfo = list3[k];
				VANativePointManager.Instance.RenderNative(nativePointInfo);
			}
		}
		vau.buildingPosID.Clear();
		vau.npcPosInfo.Clear();
		vau.nativePointInfo.Clear();
	}

	public void SetCapturedCamp(List<int> townId)
	{
		for (int i = 0; i < townId.Count; i++)
		{
			if (!capturedCampId.ContainsKey(townId[i]))
			{
				capturedCampId.Add(townId[i], 0);
			}
		}
	}

	public List<VArtifactTown> GetLevelTowns(int level)
	{
		if (!levelNpcTown.ContainsKey(level))
		{
			return null;
		}
		return levelNpcTown[level];
	}

	public List<VArtifactTown> GetLevelMainTowns(int level)
	{
		List<VArtifactTown> list = new List<VArtifactTown>();
		if (levelNpcTown.ContainsKey(level))
		{
			foreach (VArtifactTown item in levelNpcTown[level])
			{
				if (item.isMainTown)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public VArtifactTown GetTownByID(int id)
	{
		if (!townIdData.ContainsKey(id))
		{
			return null;
		}
		return townIdData[id];
	}

	public void SetCaptured(VArtifactTown townInfo)
	{
		if (!capturedCampId.ContainsKey(townInfo.townId))
		{
			capturedCampId.Add(townInfo.townId, 0);
		}
	}

	public void SetCaptured(int townId)
	{
		Debug.Log("SetCaptured");
		if (!capturedCampId.ContainsKey(townId))
		{
			capturedCampId.Add(townId, 0);
		}
	}

	public bool IsCaptured(int campId)
	{
		return capturedCampId.ContainsKey(campId);
	}

	public void SetTownDistance(float scale)
	{
		townDistanceX = (int)((float)VartifactTownXmlInfo.distanceX * scale);
		townDistanceZ = (int)((float)VartifactTownXmlInfo.distanceZ * scale);
	}

	public void PrintTownPos()
	{
		List<VArtifactTown> list = new List<VArtifactTown>();
		list.AddRange(townPosInfo.Values);
		for (int i = 0; i < list.Count; i++)
		{
			VArtifactTown vArtifactTown = list[i];
			if (vArtifactTown.VAUnits[0].type == VArtifactType.NpcTown)
			{
				Debug.Log(string.Concat("<color=#007440FF>", vArtifactTown.VAUnits[0].type.ToString(), " start:", vArtifactTown.PosStart, " id:", vArtifactTown.townId, " tid:", vArtifactTown.templateId, " level:", vArtifactTown.level, " ally:", vArtifactTown.allyId, " isEmpty:", vArtifactTown.isEmpty.ToString(), "</color>"));
			}
			else
			{
				Debug.Log(string.Concat("<color=#AA00EAFF>|----", vArtifactTown.VAUnits[0].type.ToString(), " start:", vArtifactTown.PosCenter, " id:", vArtifactTown.townId, " cid:", vArtifactTown.templateId, " level:", vArtifactTown.level, " ally:", vArtifactTown.allyId, " isEmpty:", vArtifactTown.isEmpty.ToString(), "</color>"));
			}
		}
	}

	public bool IsTownChunk(IntVector2 tileIndex)
	{
		if (TileContainsTown(tileIndex))
		{
			return true;
		}
		return false;
	}

	public float GetTownCenterByTileAndPos(IntVector2 tileIndex, IntVector2 worldXZ)
	{
		List<VArtifactUnit> tileUnitList = GetTileUnitList(tileIndex);
		if (tileUnitList == null)
		{
			return 0f;
		}
		for (int i = 0; i < tileUnitList.Count; i++)
		{
			VArtifactUnit vArtifactUnit = tileUnitList[i];
			IntVector2 posStart = vArtifactUnit.PosStart;
			IntVector2 posEnd = vArtifactUnit.PosEnd;
			if (worldXZ.x >= posStart.x && worldXZ.y >= posStart.y && worldXZ.x <= posEnd.x && worldXZ.y <= posEnd.y)
			{
				return (float)vArtifactUnit.vaSize.z + vArtifactUnit.worldPos.y;
			}
		}
		return 0f;
	}

	public void AddTown(VArtifactTown vat)
	{
		if (!PeGameMgr.IsMulti)
		{
			DetectTowns(vat);
		}
		if (vat.type == VArtifactType.NpcTown)
		{
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_TownArea, vat.TransPos);
			}
			else if (PeGameMgr.IsSingleAdventure)
			{
				PeSingleton<DetectedTownMgr>.Instance.AddDetectedTown(vat);
				RandomMapIconMgr.AddTownIcon(vat);
			}
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_CampArea, vat.TransPos);
		}
		else if (PeGameMgr.IsSingleAdventure)
		{
			RandomMapIconMgr.AddNativeIcon(vat);
		}
		if (!mDetectedTowns.Contains(vat.PosCenter))
		{
			mDetectedTowns.Add(vat.PosCenter);
		}
	}

	public void RestoreTownIcon()
	{
		foreach (KeyValuePair<int, List<int>> mAliveBuilding in mAliveBuildings)
		{
			VArtifactTown townByID = GetTownByID(mAliveBuilding.Key);
			if (townByID == null)
			{
				continue;
			}
			if (mAliveBuilding.Value.Count == 0)
			{
				RandomMapIconMgr.AddDestroyedTownIcon(townByID);
				capturedCampId.Add(townByID.townId, 0);
			}
			else if (townByID.type == VArtifactType.NpcTown)
			{
				PeSingleton<DetectedTownMgr>.Instance.AddDetectedTown(townByID);
				RandomMapIconMgr.AddTownIcon(townByID);
			}
			else
			{
				RandomMapIconMgr.AddNativeIcon(townByID);
			}
			mDetectedTowns.Add(townByID.PosCenter);
			foreach (VArtifactUnit vAUnit in townByID.VAUnits)
			{
				vAUnit.isDoodadNpcRendered = true;
			}
			townByID.IsExplored = true;
		}
	}

	public void DetectTowns(VArtifactTown centerVat)
	{
		IntVector2 posCenter = centerVat.PosCenter;
		IntVector2 intVector = new IntVector2(posCenter.x >> 5, posCenter.y >> 5);
		for (int i = intVector.x - detectedChunkNum; i < intVector.x + detectedChunkNum + 1; i++)
		{
			for (int j = intVector.y - detectedChunkNum; j < intVector.y + detectedChunkNum + 1; j++)
			{
				IntVector2 intVector2 = new IntVector2(i, j);
				if (intVector.Equals(intVector2) || !TileContainsTown(intVector2))
				{
					continue;
				}
				VArtifactTown tileTown = GetTileTown(intVector2);
				if (centerVat.areaId != tileTown.areaId)
				{
					continue;
				}
				Vector3 transPos = tileTown.TransPos;
				if (!tileTown.IsExplored && !tileTown.PosCenter.Equals(posCenter) && !tileTown.isEmpty)
				{
					if (!RandomMapIconMgr.HasTownLabel(transPos))
					{
						UnknownLabel.AddMark(transPos);
					}
				}
				else
				{
					UnknownLabel.Remove(transPos);
				}
			}
		}
		if (townPosInfo.ContainsKey(posCenter))
		{
			VArtifactTown vArtifactTown = townPosInfo[posCenter];
			if (vArtifactTown != null)
			{
				vArtifactTown.IsExplored = true;
			}
		}
	}

	public void DetectTownsAround()
	{
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer != null))
		{
			return;
		}
		Vector3 position = PeSingleton<PeCreature>.Instance.mainPlayer.position;
		IntVector2 intVector = new IntVector2((int)position.x >> 5, (int)position.z >> 5);
		for (int i = intVector.x - 8; i < intVector.x + 8 + 1; i++)
		{
			for (int j = intVector.y - 8; j < intVector.y + 8 + 1; j++)
			{
				IntVector2 tileIndex = new IntVector2(i, j);
				if (!TileContainsTown(tileIndex))
				{
					continue;
				}
				VArtifactTown tileTown = GetTileTown(tileIndex);
				if (VATownGenerator.Instance.GetAreaIdByRealPos(new IntVector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y))) == tileTown.areaId)
				{
					Vector3 transPos = tileTown.TransPos;
					if (!tileTown.IsExplored && !tileTown.isEmpty && !RandomMapIconMgr.HasTownLabel(transPos))
					{
						UnknownLabel.AddMark(transPos);
					}
				}
			}
		}
	}

	public void Import(byte[] buffer)
	{
		if (buffer.Length == 0)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			VATSaveData vATSaveData = new VATSaveData();
			vATSaveData.townId = binaryReader.ReadInt32();
			vATSaveData.ms_id = binaryReader.ReadInt32();
			vATSaveData.lastHour = binaryReader.ReadDouble();
			vATSaveData.nextHour = binaryReader.ReadDouble();
			mVATSaveData.Add(vATSaveData.townId, vATSaveData);
		}
		InitVATData();
		int num3 = binaryReader.ReadInt32();
		for (int j = 0; j < num3; j++)
		{
			int key = binaryReader.ReadInt32();
			mAliveBuildings.Add(key, new List<int>());
			int num4 = binaryReader.ReadInt32();
			for (int k = 0; k < num4; k++)
			{
				mAliveBuildings[key].Add(binaryReader.ReadInt32());
			}
		}
		RestoreTownIcon();
		binaryReader.Close();
		memoryStream.Close();
	}

	public void Export(BinaryWriter bw)
	{
		bw.Write(20160902);
		bw.Write(mVATSaveData.Keys.Count);
		foreach (int key in mVATSaveData.Keys)
		{
			bw.Write(key);
			bw.Write(mVATSaveData[key].ms_id);
			bw.Write(mVATSaveData[key].lastHour);
			bw.Write(mVATSaveData[key].nextHour);
		}
		bw.Write(mAliveBuildings.Keys.Count);
		foreach (int key2 in mAliveBuildings.Keys)
		{
			bw.Write(key2);
			bw.Write(mAliveBuildings[key2].Count);
			foreach (int item in mAliveBuildings[key2])
			{
				bw.Write(item);
			}
		}
	}

	public bool TileContainsTown(IntVector2 tileIndex)
	{
		return townTile.ContainsKey(tileIndex);
	}

	public List<VArtifactUnit> GetTileUnitList(IntVector2 tileIndex)
	{
		if (townTile.ContainsKey(tileIndex))
		{
			return townTile[tileIndex].unitList;
		}
		return null;
	}

	public VArtifactTown GetTileTown(IntVector2 tileIndex)
	{
		if (townTile.ContainsKey(tileIndex))
		{
			return townTile[tileIndex].town;
		}
		return null;
	}

	public VArtifactTown GetTown(Vector3 position)
	{
		return null;
	}

	private static void ClearRandomTownSystem()
	{
		Instance.Clear();
		VABuildingManager.Instance.Clear();
		VATownNpcManager.Instance.Clear();
	}

	public static IEnumerator WaitForArtifactTown(int[] capturedCampId)
	{
		while (null == Instance || null == VABuildingManager.Instance || null == VATownNpcManager.Instance)
		{
			yield return null;
		}
		if (capturedCampId != null)
		{
			Instance.SetCapturedCamp(capturedCampId.ToList());
		}
	}

	public static void RPC_S2C_NativeTowerDestroyed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		if (!Instance.capturedCampId.ContainsKey(value))
		{
			Instance.capturedCampId.Add(value, 0);
		}
	}

	private void InitParam()
	{
		switch (RandomMapConfig.mapSize)
		{
		case 0:
			SetTownBoundary(-19200, 19200, -19200, 19200);
			Instance.LevelRadius = 4000;
			SetTownDistance(1f, 1f);
			Instance.DetectedChunkNum = 32;
			break;
		case 1:
			SetTownBoundary(-9600, 9600, -9600, 9600);
			Instance.LevelRadius = 2000;
			SetTownDistance(1f, 1f);
			Instance.DetectedChunkNum = 32;
			break;
		case 2:
			SetTownBoundary(-3860, 3860, -3860, 3860);
			Instance.LevelRadius = 800;
			SetTownDistance(0.5f, 0.8f);
			Instance.DetectedChunkNum = 16;
			break;
		case 3:
			SetTownBoundary(-1920, 1920, -1920, 1920);
			Instance.LevelRadius = 400;
			SetTownDistance(0.5f, 0.8f);
			Instance.DetectedChunkNum = 12;
			break;
		case 4:
			SetTownBoundary(-960, 960, -960, 960);
			Instance.LevelRadius = 200;
			SetTownDistance(0.25f, 0.8f);
			Instance.DetectedChunkNum = 6;
			break;
		}
	}

	public void SetTownBoundary(int minX, int maxX, int minZ, int maxZ)
	{
		Instance.MinX = minX;
		Instance.MaxX = maxX;
		Instance.MinZ = minZ;
		Instance.MaxZ = maxZ;
	}

	public void SetTownDistance(float townDistanceScale, float campDistanceScale)
	{
		Instance.SetTownDistance(townDistanceScale);
		VANativeCampManager.Instance.SetCampDistance(campDistanceScale);
	}

	public void AddAliveBuilding(int townId, int entityId)
	{
		if (!mAliveBuildings.ContainsKey(townId))
		{
			mAliveBuildings.Add(townId, new List<int>());
		}
		mAliveBuildings[townId].Add(entityId);
	}

	public void OnTownDestroyed(VArtifactTown vat)
	{
		RandomMapIconMgr.DestroyTownIcon(vat);
		Debug.Log("OnTownDestroyed id:" + vat.townId);
		SetCaptured(vat.townId);
		RemoveNativePointAgent(vat.townId);
		PeSingleton<DetectedTownMgr>.Instance.RemoveDetectedTown(vat);
		if (this.TownDestroyedEvent != null)
		{
			this.TownDestroyedEvent(vat.AllyId);
		}
	}

	public void OnTownDestroyed(int townId)
	{
		VArtifactTown townByID = GetTownByID(townId);
		RandomMapIconMgr.DestroyTownIcon(townByID);
		Debug.Log("OnTownDestroyed id:" + townByID.townId);
		SetCaptured(townId);
		RemoveNativePointAgent(townId);
		PeSingleton<DetectedTownMgr>.Instance.RemoveDetectedTown(townByID);
		if (this.TownDestroyedEvent != null)
		{
			this.TownDestroyedEvent(townByID.AllyId);
		}
	}

	public void OnBuildingDeath(int townId, int entityId, bool isSignalTower)
	{
		Debug.Log("OnBuildingDeath id:" + townId + " ," + entityId);
		if (mAliveBuildings.ContainsKey(townId))
		{
			List<int> list = mAliveBuildings[townId];
			list.Remove(entityId);
			if (list.Count == 0)
			{
				OnTownDestroyed(townId);
			}
		}
	}

	public void AddMonsterPointAgent(int townId, SceneEntityPosAgent nativeAgent)
	{
		if (!MonsterPointAgentDic.ContainsKey(townId))
		{
			MonsterPointAgentDic.Add(townId, new List<SceneEntityPosAgent>());
		}
		MonsterPointAgentDic[townId].Add(nativeAgent);
	}

	public void RemoveNativePointAgent(int townId)
	{
		if (!MonsterPointAgentDic.ContainsKey(townId))
		{
			return;
		}
		foreach (SceneEntityPosAgent item in MonsterPointAgentDic[townId])
		{
			if ((item.protoId & 0x40000000) != 0)
			{
				(item.entity as EntityGrp).RemoveAllAgent();
			}
			SceneMan.RemoveSceneObj(item);
		}
		MonsterPointAgentDic.Remove(townId);
	}
}
