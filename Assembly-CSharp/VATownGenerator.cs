using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VATownGenerator
{
	private const int LinkAreaIndex = 8;

	private static VATownGenerator mInstance;

	private Dictionary<int, List<VArtifactTown>> MainTownDic = new Dictionary<int, List<VArtifactTown>>();

	private List<VArtifactTown> mainTownList = new List<VArtifactTown>();

	private List<VArtifactTown> branchTownList = new List<VArtifactTown>();

	private Dictionary<IntVector2, List<VArtifactTown>> areaTown = new Dictionary<IntVector2, List<VArtifactTown>>();

	private List<VArtifactTown> emptyTowns = new List<VArtifactTown>();

	private List<VATConnectionLine> townConnection = new List<VATConnectionLine>();

	private Dictionary<IntVector2, List<VATConnectionLine>> areaConnections = new Dictionary<IntVector2, List<VATConnectionLine>>();

	private Dictionary<IntVector2, List<VATConnectionLine>> areaTypeConnections = new Dictionary<IntVector2, List<VATConnectionLine>>();

	private Dictionary<int, List<VArtifactTown>> allyTownDic = new Dictionary<int, List<VArtifactTown>>();

	public int EnemyNpcAllyCount;

	public int PujaAllyCount;

	public int PajaAllyCount;

	private Dictionary<int, int> allyPlayerIdDic = new Dictionary<int, int>();

	public int playerStartTownCount;

	public int branchTownCountMax;

	public int playerEmptyTownDistanceMin;

	public int playerEmptyTownDistanceMax;

	private Dictionary<int, int> allyAreaCount = new Dictionary<int, int>();

	private Dictionary<int, int> allyAreaThreashold = new Dictionary<int, int>();

	private Dictionary<int, int> allyColor = new Dictionary<int, int>();

	private Dictionary<int, int> allyName = new Dictionary<int, int>();

	private Dictionary<int, List<int>> allyNamePool = new Dictionary<int, List<int>>();

	public static VATownGenerator Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new VATownGenerator();
			}
			return mInstance;
		}
	}

	private static float ConnectionAreaWidth => VFDataRTGen.TownConnectionAreaWidth;

	private static float ConnectionAreaTypeWidth => VFDataRTGen.TownConnectionAreaTypeWidth;

	private static float TownAreaMaxDistance => VFDataRTGen.TownChangeMaxDistance;

	private static float TownAreaMaxFactor => VFDataRTGen.TownChangeMaxFactor;

	private bool Mirror => RandomMapConfig.mirror;

	private float RotationF => RandomMapConfig.rotation * 90;

	private int MapRadius => RandomMapConfig.Instance.mapRadius;

	private int MapSizeId => RandomMapConfig.mapSize;

	private int PickedLineIndex => RandomMapConfig.pickedLineIndex;

	private int PickedLevelIndex => RandomMapConfig.pickedLevelIndex;

	private int BiomaCount => RandomMapConfig.RandomMapTypeCount;

	private int AllyCount => RandomMapConfig.allyCount;

	public void ClearData()
	{
		MainTownDic = new Dictionary<int, List<VArtifactTown>>();
		mainTownList = new List<VArtifactTown>();
		branchTownList = new List<VArtifactTown>();
		areaTown = new Dictionary<IntVector2, List<VArtifactTown>>();
		townConnection = new List<VATConnectionLine>();
		areaConnections = new Dictionary<IntVector2, List<VATConnectionLine>>();
		areaTypeConnections = new Dictionary<IntVector2, List<VATConnectionLine>>();
		allyTownDic = new Dictionary<int, List<VArtifactTown>>();
		allyPlayerIdDic = new Dictionary<int, int>();
		emptyTowns = new List<VArtifactTown>();
		allyNamePool = new Dictionary<int, List<int>>();
	}

	public List<RandomMapTypePoint> InitBiomaPos()
	{
		System.Random random = new System.Random(RandomMapConfig.TownGenSeed);
		IntVector2 intVector = GenTownPos(TownGenData.GenerationLine[PickedLineIndex].ToList()[0], random);
		List<IntVector2> list = new List<IntVector2>();
		List<RandomMapTypePoint> list2 = new List<RandomMapTypePoint>();
		int num = BiomaCount;
		for (int i = 1; i <= BiomaCount; i++)
		{
			if (i == (int)RandomMapConfig.RandomMapID)
			{
				list2.Add(new RandomMapTypePoint((RandomMapType)i, intVector));
			}
			else
			{
				list2.Add(new RandomMapTypePoint((RandomMapType)i));
			}
		}
		if (MapSizeId > 1)
		{
			for (int j = 0; j < num; j++)
			{
				IntVector2 realPos = GetRealPos(new IntVector2(random.Next(-MapRadius, MapRadius), random.Next(-MapRadius, MapRadius)));
				if (list.Contains(realPos))
				{
					j--;
					continue;
				}
				if (intVector.Distance(realPos) < 500f)
				{
					j--;
					continue;
				}
				bool flag = false;
				foreach (IntVector2 item in list)
				{
					if (item.Distance(realPos) < VFDataRTGen.changeBiomaDiff)
					{
						flag = true;
						j--;
						break;
					}
				}
				if (!flag)
				{
					list.Add(realPos);
				}
			}
		}
		else
		{
			num = ((MapSizeId != 1) ? (BiomaCount * 4) : (BiomaCount * 2));
			for (int k = 0; k < num; k++)
			{
				IntVector2 realPos2 = GetRealPos(new IntVector2(random.Next(-MapRadius, MapRadius), random.Next(-MapRadius, MapRadius)));
				if (list.Contains(realPos2))
				{
					k--;
					continue;
				}
				if (intVector.Distance(realPos2) < 1000f)
				{
					k--;
					continue;
				}
				bool flag2 = false;
				foreach (IntVector2 item2 in list)
				{
					if (item2.Distance(realPos2) < VFDataRTGen.changeBiomaDiff)
					{
						flag2 = true;
						k--;
						break;
					}
				}
				if (!flag2)
				{
					list.Add(realPos2);
				}
			}
		}
		for (int l = 0; l < num; l++)
		{
			list2[l % BiomaCount].AddPos(list[l]);
		}
		return list2;
	}

	public List<int> GetPickedGenLine()
	{
		return TownGenData.GenerationLine[PickedLineIndex].ToList();
	}

	public IntVector2 GetInitPos()
	{
		System.Random myRand = new System.Random(RandomMapConfig.TownGenSeed);
		return GenTownPos(TownGenData.GenerationLine[PickedLineIndex].ToList()[0], myRand);
	}

	public IntVector2 GetInitPos(System.Random myRand)
	{
		return GenTownPos(TownGenData.GenerationLine[PickedLineIndex].ToList()[0], myRand);
	}

	public IntVector2 GenTownPos(int areaId, System.Random myRand)
	{
		IntVector2 realPos = GetRealPos(TownGenData.AreaIndex[areaId]);
		int minValue = ((realPos.x <= 0) ? (realPos.x * TownGenData.AreaRadius) : ((realPos.x - 1) * TownGenData.AreaRadius));
		int minValue2 = ((realPos.y <= 0) ? (realPos.y * TownGenData.AreaRadius) : ((realPos.y - 1) * TownGenData.AreaRadius));
		int maxValue = ((realPos.x <= 0) ? ((realPos.x + 1) * TownGenData.AreaRadius) : (realPos.x * TownGenData.AreaRadius));
		int maxValue2 = ((realPos.y <= 0) ? ((realPos.y + 1) * TownGenData.AreaRadius) : (realPos.y * TownGenData.AreaRadius));
		return new IntVector2(myRand.Next(minValue, maxValue), myRand.Next(minValue2, maxValue2));
	}

	private void InitStartTownCount()
	{
		if (MapSizeId > 2)
		{
			playerStartTownCount = 4;
		}
		else if (MapSizeId == 2)
		{
			playerStartTownCount = 2;
		}
		else
		{
			playerStartTownCount = 1;
		}
		Debug.Log("playerStartTown: " + playerStartTownCount);
	}

	private void InitBranchTownCount()
	{
		if (MapSizeId > 2)
		{
			branchTownCountMax = 0;
		}
		else if (MapSizeId == 2)
		{
			branchTownCountMax = 1;
		}
		else if (MapSizeId == 1)
		{
			branchTownCountMax = 2;
		}
		else
		{
			branchTownCountMax = 3;
		}
		Debug.Log("branchTownCountMax: " + branchTownCountMax);
	}

	private void InitEmptyTownDistance()
	{
		if (MapSizeId > 2)
		{
			playerEmptyTownDistanceMin = 300;
			playerEmptyTownDistanceMax = 800;
		}
		else if (MapSizeId == 2)
		{
			playerEmptyTownDistanceMin = 500;
			playerEmptyTownDistanceMax = 1000;
		}
		else
		{
			playerEmptyTownDistanceMin = 1000;
			playerEmptyTownDistanceMax = 1500;
		}
		Debug.Log("EmptyTownDistanceMin: " + playerEmptyTownDistanceMin + " max:" + playerEmptyTownDistanceMax);
	}

	public void InitAllyDistribution(System.Random myRand)
	{
		Debug.Log("mirror:" + Mirror + " rotation:" + RotationF + " pickedLineIndex:" + PickedLineIndex + " pickedLevelIndex:" + PickedLevelIndex);
		InitStartTownCount();
		InitBranchTownCount();
		EnemyNpcAllyCount = 1;
		PujaAllyCount = 1;
		PajaAllyCount = 1;
		for (int i = 0; i < AllyCount - 4; i++)
		{
			if (myRand.NextDouble() < 0.33329999446868896)
			{
				EnemyNpcAllyCount++;
			}
			else if (myRand.NextDouble() < 0.6665999889373779)
			{
				PujaAllyCount++;
			}
			else
			{
				PajaAllyCount++;
			}
		}
		Debug.Log("EnemyNpc: " + EnemyNpcAllyCount + " Puja: " + PujaAllyCount + " Paja: " + PajaAllyCount);
		InitAllyColor(myRand);
		InitAllyPlayerId(myRand);
		InitAllyArea(myRand);
		InitAllyName(myRand);
	}

	public void InitAllyColor(System.Random myRand)
	{
		allyColor.Clear();
		allyColor.Add(0, 0);
		List<int> list = new List<int>();
		for (int i = 1; i < AllyCount; i++)
		{
			list.Add(i);
		}
		VArtifactUtil.Shuffle(list, myRand);
		for (int j = 1; j < AllyCount; j++)
		{
			allyColor.Add(j, list[j - 1]);
		}
		foreach (KeyValuePair<int, int> item in allyColor)
		{
			Debug.Log("ally: " + item.Key + " color:" + item.Value);
		}
	}

	public void InitAllyArea(System.Random myRand)
	{
		allyAreaCount.Clear();
		allyAreaThreashold.Clear();
		int num = TownGenData.AreaCount - playerStartTownCount - AllyCount + 1;
		int num2 = AllyCount - 1;
		allyAreaCount.Add(0, playerStartTownCount);
		for (int i = 1; i < AllyCount; i++)
		{
			allyAreaCount.Add(i, 1);
		}
		for (int j = 0; j < num; j++)
		{
			double num3 = myRand.NextDouble();
			float num4 = 1f / (float)num2;
			for (int k = 1; k < AllyCount; k++)
			{
				if (num3 < (double)(num4 * (float)k))
				{
					Dictionary<int, int> dictionary;
					Dictionary<int, int> dictionary2 = (dictionary = allyAreaCount);
					int key;
					int key2 = (key = k);
					key = dictionary[key];
					dictionary2[key2] = key + 1;
					break;
				}
			}
		}
		allyAreaThreashold.Add(0, allyAreaCount[0]);
		for (int l = 1; l < AllyCount; l++)
		{
			allyAreaThreashold.Add(l, allyAreaCount[l] + allyAreaThreashold[l - 1]);
		}
		foreach (KeyValuePair<int, int> item in allyAreaThreashold)
		{
			Debug.Log("ally: " + item.Key + " AreaThreashold:" + item.Value);
		}
	}

	public void InitAllyPlayerId(System.Random rand)
	{
		allyPlayerIdDic.Clear();
		allyPlayerIdDic.Add(0, 1);
		allyPlayerIdDic.Add(1, 20);
		int i = 2;
		List<int> list = new List<int>();
		for (int j = 0; j < PujaAllyCount - 1; j++)
		{
			list.Add(21 + j);
		}
		for (int k = 0; k < PajaAllyCount; k++)
		{
			list.Add(30 + k);
		}
		for (int l = 0; l < EnemyNpcAllyCount; l++)
		{
			list.Add(40 + l);
		}
		VArtifactUtil.Shuffle(list, rand);
		for (; i < AllyCount; i++)
		{
			allyPlayerIdDic.Add(i, list[i - 2]);
		}
		foreach (KeyValuePair<int, int> item in allyPlayerIdDic)
		{
			Debug.Log("ally: " + item.Key + " playerId:" + item.Value);
		}
	}

	public void InitAllyName(System.Random rand)
	{
		allyName = new Dictionary<int, int>();
		foreach (AllyName value2 in VArtifactUtil.allyNameData.Values)
		{
			if (!allyNamePool.ContainsKey(value2.raceId))
			{
				allyNamePool.Add(value2.raceId, new List<int>());
			}
			allyNamePool[value2.raceId].Add(value2.nameId);
		}
		for (int i = 1; i < AllyCount; i++)
		{
			AllyType allyType = GetAllyType(i);
			List<int> list = allyNamePool[(int)allyType];
			int num = rand.Next(list.Count);
			int value = list[num];
			allyName.Add(i, value);
			list.Remove(num);
		}
		foreach (KeyValuePair<int, int> item in allyName)
		{
			Debug.Log("ally: " + item.Key + " nameId:" + item.Value);
		}
	}

	public AllyType GetAllyType(int allyId)
	{
		int playerId = GetPlayerId(allyId);
		switch (playerId)
		{
		case 0:
			return AllyType.Player;
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
			return AllyType.Puja;
		default:
			if (playerId >= 30 && playerId < 40)
			{
				return AllyType.Paja;
			}
			if (playerId >= 40)
			{
				return AllyType.Npc;
			}
			return AllyType.Npc;
		}
	}

	public int GetAllyNum(int allyId)
	{
		int playerId = GetPlayerId(allyId);
		if (playerId >= 20 && playerId < 30)
		{
			return playerId - 20 + 1;
		}
		if (playerId >= 30 && playerId < 40)
		{
			return playerId - 30 + 1;
		}
		if (playerId >= 40)
		{
			return playerId - 40 + 1;
		}
		return 0;
	}

	public int GetPlayerId(int allyId)
	{
		if (allyPlayerIdDic.ContainsKey(allyId))
		{
			return allyPlayerIdDic[allyId];
		}
		return 0;
	}

	public int GetAllyIdByPlayerId(int playerId)
	{
		foreach (KeyValuePair<int, int> item in allyPlayerIdDic)
		{
			if (item.Value == playerId)
			{
				return item.Key;
			}
		}
		return 0;
	}

	public int GetAllyIdByAreaIndex(int areaIndex)
	{
		int result = 0;
		for (int i = 0; i < AllyCount; i++)
		{
			if (areaIndex < allyAreaThreashold[i])
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public int GetAllyIdByAreaId(int areaId)
	{
		return GetAllyIdByAreaIndex(GetGenLineIndexByAreaId(areaId));
	}

	public AllyType GetAllyTypeByAreaIndex(int areaIndex)
	{
		return GetAllyType(GetAllyIdByAreaIndex(areaIndex));
	}

	public int GetFirstEnemyNpcAllyColor()
	{
		for (int i = 1; i < AllyCount; i++)
		{
			if (GetAllyType(i) == AllyType.Npc)
			{
				return GetAllyColor(i);
			}
		}
		return 1;
	}

	public int GetFirstEnemyNpcAllyPlayerId()
	{
		return 40;
	}

	public AllyType GetAllyTypeByAreaId(int areaId)
	{
		return GetAllyTypeByAreaIndex(GetGenLineIndexByAreaId(areaId));
	}

	public int GetAllyColor(int allyId)
	{
		if (allyColor.ContainsKey(allyId))
		{
			return allyColor[allyId];
		}
		return 0;
	}

	public int GetAllyTownCount(int allyId)
	{
		if (!allyTownDic.ContainsKey(allyId))
		{
			return 0;
		}
		return allyTownDic[allyId].Count;
	}

	public int GetAllyTownDestroyedCount(int allyId)
	{
		if (!allyTownDic.ContainsKey(allyId))
		{
			return 0;
		}
		List<VArtifactTown> list = allyTownDic[allyId];
		return list.FindAll((VArtifactTown it) => VArtifactTownManager.Instance.capturedCampId.ContainsKey(it.townId)).Count;
	}

	public int GetAllyTownExistCount(int allyId)
	{
		if (!allyTownDic.ContainsKey(allyId))
		{
			return 0;
		}
		List<VArtifactTown> list = allyTownDic[allyId];
		return list.FindAll((VArtifactTown it) => !VArtifactTownManager.Instance.capturedCampId.ContainsKey(it.townId)).Count;
	}

	public List<VArtifactTown> GetAllyTowns(int allyId)
	{
		if (!allyTownDic.ContainsKey(allyId))
		{
			return new List<VArtifactTown>();
		}
		return allyTownDic[allyId];
	}

	public AllyType GetRandomExistEnemyType(out int color)
	{
		List<int> list = new List<int>();
		for (int i = 1; i < AllyCount; i++)
		{
			if (GetAllyTownExistCount(i) > 0)
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			color = -1;
			return AllyType.Player;
		}
		int allyId = list[new System.Random().Next(list.Count)];
		color = GetAllyColor(allyId);
		return GetAllyType(allyId);
	}

	public int GetAllyName(int allyId)
	{
		if (!allyName.ContainsKey(allyId))
		{
			return -1;
		}
		return allyName[allyId];
	}

	public float GetNearestAllyDistance(Vector3 pos, out int allyId, out Vector3 townPos)
	{
		float num = float.MaxValue;
		allyId = 0;
		townPos = Vector3.zero;
		for (int i = 1; i < AllyCount; i++)
		{
			if (!allyTownDic.ContainsKey(i))
			{
				continue;
			}
			List<VArtifactTown> list = allyTownDic[i].FindAll((VArtifactTown it) => !VArtifactTownManager.Instance.capturedCampId.ContainsKey(it.townId));
			foreach (VArtifactTown item in list)
			{
				float num2 = Vector3.Distance(pos, item.TransPos);
				if (num2 < num)
				{
					num = num2;
					allyId = i;
					townPos = item.TransPos;
				}
			}
		}
		return num;
	}

	public void AddTown(int areaId, VArtifactTown townData)
	{
		if (townData.isMainTown)
		{
			AddMainTown(areaId, townData);
		}
		else
		{
			AddBranchTown(townData);
		}
		LinkTownToArea(townData);
	}

	public void AddMainTown(int areaId, VArtifactTown townData)
	{
		if (MainTownDic.ContainsKey(areaId))
		{
			MainTownDic[areaId].Add(townData);
			return;
		}
		MainTownDic.Add(areaId, new List<VArtifactTown> { townData });
	}

	public void AddBranchTown(VArtifactTown townData)
	{
		branchTownList.Add(townData);
	}

	public void AddAllyTown(VArtifactTown townData)
	{
		if (!allyTownDic.ContainsKey(townData.AllyId))
		{
			allyTownDic.Add(townData.AllyId, new List<VArtifactTown>());
		}
		allyTownDic[townData.AllyId].Add(townData);
	}

	public void ChangeAlliance(VArtifactTown vat)
	{
		allyTownDic[vat.allyId].Remove(vat);
		vat.allyId = 0;
		allyTownDic[vat.allyId].Add(vat);
	}

	public void RestoreAlliance(VArtifactTown vat)
	{
		allyTownDic[vat.allyId].Remove(vat);
		vat.allyId = vat.genAllyId;
		allyTownDic[vat.allyId].Add(vat);
	}

	public void AddEmtpyTown(VArtifactTown vat)
	{
		emptyTowns.Add(vat);
	}

	public VArtifactTown GetEmptyTown(int id)
	{
		return emptyTowns.Find((VArtifactTown it) => it.townId == id);
	}

	public void AddTownConnection(IntVector2 v1, IntVector2 v2, System.Random rand)
	{
		float num = v1.Distance(v2);
		if (num > 2000f)
		{
			if (rand.NextDouble() < 1.0)
			{
				GenCutPointNewLine(v1, v2, rand);
			}
			else
			{
				AddConnection(v1, v2);
			}
		}
		else if (num > 1500f)
		{
			if (rand.NextDouble() < 0.8999999761581421)
			{
				GenCutPointNewLine(v1, v2, rand);
			}
			else
			{
				AddConnection(v1, v2);
			}
		}
		else if (num > 1000f)
		{
			if (rand.NextDouble() < 0.800000011920929)
			{
				GenCutPointNewLine(v1, v2, rand);
			}
			else
			{
				AddConnection(v1, v2);
			}
		}
		else if (num > 500f)
		{
			if (rand.NextDouble() < 0.699999988079071)
			{
				GenCutPointNewLine(v1, v2, rand);
			}
			else
			{
				AddConnection(v1, v2);
			}
		}
		else
		{
			AddConnection(v1, v2);
		}
	}

	public void GenCutPointNewLine(IntVector2 v1, IntVector2 v2, System.Random rand)
	{
		float num = v1.Distance(v2);
		IntVector2 centerPoint = new IntVector2((v1.x + v2.x) / 2, (v1.y + v2.y) / 2);
		float maxRadius = num / 4f;
		IntVector2 randomPointFromPoint = VArtifactUtil.GetRandomPointFromPoint(centerPoint, maxRadius, rand);
		AddTownConnection(v1, randomPointFromPoint, rand);
		AddTownConnection(randomPointFromPoint, v2, rand);
	}

	public void AddConnection(IntVector2 v1, IntVector2 v2)
	{
		VATConnectionLine vATConnectionLine = new VATConnectionLine(v1, v2);
		if (!townConnection.Contains(vATConnectionLine))
		{
			townConnection.Add(vATConnectionLine);
			LinkConnectionToArea(vATConnectionLine);
		}
	}

	public void ConnectMainTowns()
	{
		List<int> pickedGenLine = GetPickedGenLine();
		for (int i = 0; i < pickedGenLine.Count; i++)
		{
			if (i == 0)
			{
				List<VArtifactTown> list = new List<VArtifactTown>();
				list.AddRange(MainTownDic[pickedGenLine[i]]);
				VArtifactTown item = list.First();
				mainTownList.Add(item);
				list.Remove(item);
				int count = list.Count;
				for (int j = 0; j < count; j++)
				{
					VArtifactTown vArtifactTown = mainTownList.Last();
					item = GetNearestTown(vArtifactTown.PosCenter, list);
					if (vArtifactTown.townId == 0 && item.townId != 4)
					{
						VArtifactTownManager.Instance.SwitchTownId(item, VArtifactTownManager.Instance.GetTownByID(4));
					}
					else if (vArtifactTown.townId == 4 && item.townId != 5)
					{
						VArtifactTownManager.Instance.SwitchTownId(item, VArtifactTownManager.Instance.GetTownByID(5));
					}
					else if (vArtifactTown.townId == 5 && item.townId != 6)
					{
						VArtifactTownManager.Instance.SwitchTownId(item, VArtifactTownManager.Instance.GetTownByID(6));
					}
					else if (vArtifactTown.townId == 6 && item.townId != 7)
					{
						VArtifactTownManager.Instance.SwitchTownId(item, VArtifactTownManager.Instance.GetTownByID(7));
					}
					mainTownList.Add(item);
					list.Remove(item);
				}
			}
			else
			{
				if (!MainTownDic.ContainsKey(pickedGenLine[i]))
				{
					continue;
				}
				List<VArtifactTown> list2 = new List<VArtifactTown>();
				list2.AddRange(MainTownDic[pickedGenLine[i]]);
				int count2 = list2.Count;
				for (int k = 0; k < count2; k++)
				{
					VArtifactTown vArtifactTown2 = mainTownList.Last();
					VArtifactTown nearestTown = GetNearestTown(vArtifactTown2.PosCenter, list2);
					if (vArtifactTown2.townId == 0 && nearestTown.townId != 4)
					{
						VArtifactTownManager.Instance.SwitchTownId(nearestTown, VArtifactTownManager.Instance.GetTownByID(4));
					}
					else if (vArtifactTown2.townId == 4 && nearestTown.townId != 5)
					{
						VArtifactTownManager.Instance.SwitchTownId(nearestTown, VArtifactTownManager.Instance.GetTownByID(5));
					}
					else if (vArtifactTown2.townId == 5 && nearestTown.townId != 6)
					{
						VArtifactTownManager.Instance.SwitchTownId(nearestTown, VArtifactTownManager.Instance.GetTownByID(6));
					}
					else if (vArtifactTown2.townId == 6 && nearestTown.townId != 7)
					{
						VArtifactTownManager.Instance.SwitchTownId(nearestTown, VArtifactTownManager.Instance.GetTownByID(7));
					}
					mainTownList.Add(nearestTown);
					list2.Remove(nearestTown);
				}
			}
		}
	}

	public VArtifactTown GetNearestTown(IntVector2 pos, List<VArtifactTown> vatList)
	{
		float num = 999999f;
		VArtifactTown result = null;
		foreach (VArtifactTown vat in vatList)
		{
			float num2 = vat.PosCenter.Distance(pos);
			if (num2 < num)
			{
				num = num2;
				result = vat;
			}
		}
		return result;
	}

	public void GenerateConnection(System.Random rand)
	{
		for (int i = 0; i < mainTownList.Count - 1; i++)
		{
			VArtifactTown vArtifactTown = mainTownList[i];
			VArtifactTown vArtifactTown2 = mainTownList[i + 1];
			AddTownConnection(vArtifactTown.PosCenter, vArtifactTown.PosEntrance, rand);
			if (vArtifactTown2.PosStart.y > vArtifactTown.PosStart.y)
			{
				if (vArtifactTown2.PosCenter.x < vArtifactTown.PosCenter.x)
				{
					AddTownConnection(vArtifactTown.PosEntrance, vArtifactTown.PosEntranceLeft, rand);
					AddTownConnection(vArtifactTown.PosEntranceLeft, vArtifactTown2.PosEntrance, rand);
				}
				else
				{
					AddTownConnection(vArtifactTown.PosEntrance, vArtifactTown.PosEntranceRight, rand);
					AddTownConnection(vArtifactTown.PosEntranceRight, vArtifactTown2.PosEntrance, rand);
				}
			}
			else if (vArtifactTown2.PosCenter.x < vArtifactTown.PosCenter.x)
			{
				AddTownConnection(vArtifactTown.PosEntrance, vArtifactTown2.PosEntranceRight, rand);
				AddTownConnection(vArtifactTown2.PosEntranceRight, vArtifactTown2.PosEntrance, rand);
			}
			else
			{
				AddTownConnection(vArtifactTown.PosEntrance, vArtifactTown2.PosEntranceLeft, rand);
				AddTownConnection(vArtifactTown2.PosEntranceLeft, vArtifactTown2.PosEntrance, rand);
			}
		}
		if (mainTownList.Count > 1)
		{
			AddTownConnection(mainTownList[mainTownList.Count - 1].PosCenter, mainTownList[mainTownList.Count - 1].PosEntrance, rand);
		}
	}

	public float GetAreaTownDistance(int x, int z, out VArtifactTown vaTown)
	{
		float num = float.MaxValue;
		vaTown = null;
		IntVector2 key = new IntVector2(x >> 8, z >> 8);
		if (!areaTown.ContainsKey(key))
		{
			return num;
		}
		foreach (VArtifactTown item in areaTown[key])
		{
			float num2 = (item.PosCenter.x - x) * (item.PosCenter.x - x) + (item.PosCenter.y - z) * (item.PosCenter.y - z);
			if (num2 < num)
			{
				num = num2;
				vaTown = item;
			}
		}
		return Mathf.Sqrt(num);
	}

	public float GetConnectionLineDistance(IntVector2 pos, bool onConnection = false)
	{
		float num = float.MaxValue;
		IntVector2 key = new IntVector2(pos.x >> 8, pos.y >> 8);
		List<VATConnectionLine> list = new List<VATConnectionLine>();
		if (onConnection)
		{
			if (areaTypeConnections.ContainsKey(key))
			{
				list.AddRange(areaTypeConnections[key]);
			}
		}
		else if (areaConnections.ContainsKey(key))
		{
			list.AddRange(areaConnections[key]);
		}
		foreach (VATConnectionLine item in list)
		{
			IntVector2 leftPos = item.leftPos;
			IntVector2 rightPos = item.rightPos;
			float num2 = PointToSegDist(pos.x, pos.y, leftPos.x, leftPos.y, rightPos.x, rightPos.y);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public int GetTownAmountMin()
	{
		return TownGenData.GetTownAmountMin();
	}

	public int GetTownAmountMax()
	{
		return TownGenData.GetTownAmountMax();
	}

	public int GetLevelByLineIndex(int lineIndex)
	{
		return TownGenData.GetLevel(PickedLevelIndex, lineIndex);
	}

	public int GetLevelByAreaId(int areaId)
	{
		return TownGenData.GetLevel(PickedLineIndex, PickedLevelIndex, areaId);
	}

	public int GetLevelByRealPos(IntVector2 realPosXZ)
	{
		IntVector2 originPos = GetOriginPos(realPosXZ);
		int areaId = TownGenData.GetAreaId(originPos);
		return TownGenData.GetLevel(PickedLineIndex, PickedLevelIndex, areaId);
	}

	public int GetAreaIdByRealPos(IntVector2 realPosXZ)
	{
		IntVector2 originPos = GetOriginPos(realPosXZ);
		return TownGenData.GetAreaId(originPos);
	}

	public int GetNextAreaId(int curAreaId)
	{
		List<int> pickedGenLine = GetPickedGenLine();
		int num = pickedGenLine.FindIndex((int it) => it == curAreaId) + 1;
		if (num < pickedGenLine.Count - 1)
		{
			return pickedGenLine[num + 1];
		}
		return -1;
	}

	public int GetGenLineIndexByAreaId(int areaId)
	{
		List<int> pickedGenLine = GetPickedGenLine();
		return pickedGenLine.FindIndex((int it) => it == areaId);
	}

	public IntVector2 GetRealPos(IntVector2 originPos)
	{
		IntVector2 intVector = new IntVector2(originPos.x, originPos.y);
		if (Mirror)
		{
			intVector.x = -intVector.x;
		}
		Vector3 vector = new Vector3(intVector.x, 0f, intVector.y);
		Quaternion quaternion = Quaternion.Euler(0f, RotationF, 0f);
		vector = quaternion * vector;
		intVector.x = Mathf.RoundToInt(vector.x);
		intVector.y = Mathf.RoundToInt(vector.z);
		return intVector;
	}

	public IntVector2 GetOriginPos(IntVector2 realPos)
	{
		Vector3 vector = new Vector3(realPos.x, 0f, realPos.y);
		Quaternion quaternion = Quaternion.Euler(0f, 0f - RotationF, 0f);
		vector = quaternion * vector;
		IntVector2 intVector = new IntVector2();
		intVector.x = Mathf.RoundToInt(vector.x);
		intVector.y = Mathf.RoundToInt(vector.z);
		if (Mirror)
		{
			intVector.x = -intVector.x;
		}
		return intVector;
	}

	public static float PointToSegDist(float x, float y, float x1, float y1, float x2, float y2)
	{
		float num = (x2 - x1) * (x - x1) + (y2 - y1) * (y - y1);
		if (num <= 0f)
		{
			return Mathf.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
		}
		float num2 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
		if (num >= num2)
		{
			return Mathf.Sqrt((x - x2) * (x - x2) + (y - y2) * (y - y2));
		}
		float num3 = num / num2;
		float num4 = x1 + (x2 - x1) * num3;
		float num5 = y1 + (y2 - y1) * num3;
		return Mathf.Sqrt((x - num4) * (x - num4) + (num5 - y) * (num5 - y));
	}

	public void AddTownArea(IntVector2 areaIndex, VArtifactTown vat)
	{
		if (areaTown.ContainsKey(areaIndex))
		{
			areaTown[areaIndex].Add(vat);
			return;
		}
		areaTown.Add(areaIndex, new List<VArtifactTown> { vat });
	}

	public void LinkTownToArea(VArtifactTown vat)
	{
		int num = Mathf.CeilToInt(((float)vat.MiddleRadius + TownAreaMaxDistance) * TownAreaMaxFactor);
		int num2 = vat.PosCenter.x - num >> 8;
		int num3 = vat.PosCenter.y - num >> 8;
		int num4 = vat.PosCenter.x + num >> 8;
		int num5 = vat.PosCenter.y + num >> 8;
		for (int i = num2; i <= num4; i++)
		{
			for (int j = num3; j <= num5; j++)
			{
				AddTownArea(new IntVector2(i, j), vat);
			}
		}
	}

	public void AddConnectionArea(IntVector2 areaIndex, VATConnectionLine vatc)
	{
		if (areaConnections.ContainsKey(areaIndex))
		{
			areaConnections[areaIndex].Add(vatc);
			return;
		}
		areaConnections.Add(areaIndex, new List<VATConnectionLine> { vatc });
	}

	public void AddConnectionTypeArea(IntVector2 areaIndex, VATConnectionLine vatc)
	{
		if (areaTypeConnections.ContainsKey(areaIndex))
		{
			areaTypeConnections[areaIndex].Add(vatc);
			return;
		}
		areaTypeConnections.Add(areaIndex, new List<VATConnectionLine> { vatc });
	}

	public void LinkConnectionToArea(VATConnectionLine vatc)
	{
		List<IntVector2> list = AreaConnectionRect(vatc, ConnectionAreaWidth);
		int num = GetMinX(list) >> 8;
		int num2 = GetMinY(list) >> 8;
		int num3 = GetMaxX(list) >> 8;
		int num4 = GetMaxY(list) >> 8;
		List<IntVector2> rect = AreaConnectionRect(vatc, ConnectionAreaTypeWidth);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				List<IntVector2> list2 = new List<IntVector2>();
				list2.Add(new IntVector2(i << 8, j << 8));
				list2.Add(new IntVector2(i + 1 << 8, j + 1 << 8));
				List<IntVector2> rect2 = list2;
				if (i == 11 && j == -49)
				{
					Debug.Log("pause");
				}
				if (CheckRectIntersection(list, rect2))
				{
					AddConnectionArea(new IntVector2(i, j), vatc);
				}
				if (CheckRectIntersection(rect, rect2))
				{
					AddConnectionTypeArea(new IntVector2(i, j), vatc);
				}
			}
		}
	}

	private static int GetMinX(List<IntVector2> posList)
	{
		if (posList == null || posList.Count == 0)
		{
			Debug.LogError("VATownGenerator.GetMinx(),no param!");
			return -999999;
		}
		int x = posList[0].x;
		foreach (IntVector2 pos in posList)
		{
			if (pos.x < x)
			{
				x = pos.x;
			}
		}
		return x;
	}

	private static int GetMinY(List<IntVector2> posList)
	{
		if (posList == null || posList.Count == 0)
		{
			Debug.LogError("VATownGenerator.GetMinY(),no param!");
			return -999999;
		}
		int y = posList[0].y;
		foreach (IntVector2 pos in posList)
		{
			if (pos.y < y)
			{
				y = pos.y;
			}
		}
		return y;
	}

	private static int GetMaxX(List<IntVector2> posList)
	{
		if (posList == null || posList.Count == 0)
		{
			Debug.LogError("VATownGenerator.GetMinx(),no param!");
			return -999999;
		}
		int x = posList[0].x;
		foreach (IntVector2 pos in posList)
		{
			if (pos.x > x)
			{
				x = pos.x;
			}
		}
		return x;
	}

	private static int GetMaxY(List<IntVector2> posList)
	{
		if (posList == null || posList.Count == 0)
		{
			Debug.LogError("VATownGenerator.GetMinx(),no param!");
			return -999999;
		}
		int y = posList[0].y;
		foreach (IntVector2 pos in posList)
		{
			if (pos.y > y)
			{
				y = pos.y;
			}
		}
		return y;
	}

	public static List<IntVector2> AreaConnectionRect(VATConnectionLine vatc, float halfWidth)
	{
		List<IntVector2> list = new List<IntVector2>();
		if (vatc.leftPos.y == vatc.rightPos.y)
		{
			list.Add(new IntVector2(vatc.leftPos.x, Mathf.FloorToInt((float)vatc.leftPos.y - halfWidth)));
			list.Add(new IntVector2(vatc.leftPos.x, Mathf.CeilToInt((float)vatc.leftPos.y + halfWidth)));
			list.Add(new IntVector2(vatc.rightPos.x, Mathf.CeilToInt((float)vatc.rightPos.y + halfWidth)));
			list.Add(new IntVector2(vatc.rightPos.x, Mathf.FloorToInt((float)vatc.rightPos.y - halfWidth)));
			return list;
		}
		if (vatc.leftPos.x == vatc.rightPos.x)
		{
			list.Add(new IntVector2(Mathf.CeilToInt((float)vatc.leftPos.x + halfWidth), vatc.leftPos.y));
			list.Add(new IntVector2(Mathf.FloorToInt((float)vatc.leftPos.x - halfWidth), vatc.leftPos.y));
			list.Add(new IntVector2(Mathf.FloorToInt((float)vatc.rightPos.x - halfWidth), vatc.rightPos.y));
			list.Add(new IntVector2(Mathf.CeilToInt((float)vatc.rightPos.x + halfWidth), vatc.rightPos.y));
			return list;
		}
		if (vatc.leftPos.y > vatc.rightPos.y)
		{
			float num = vatc.leftPos.Distance(vatc.rightPos);
			float num2 = halfWidth * (float)(vatc.leftPos.y - vatc.rightPos.y) / num;
			float num3 = halfWidth * (float)(vatc.rightPos.x - vatc.leftPos.x) / num;
			list.Add(new IntVector2(Mathf.FloorToInt((float)vatc.leftPos.x - num2), Mathf.FloorToInt((float)vatc.leftPos.y - num3)));
			list.Add(new IntVector2(Mathf.CeilToInt((float)vatc.leftPos.x + num2), Mathf.CeilToInt((float)vatc.leftPos.y + num3)));
			list.Add(new IntVector2(Mathf.CeilToInt((float)vatc.rightPos.x + num2), Mathf.CeilToInt((float)vatc.rightPos.y + num3)));
			list.Add(new IntVector2(Mathf.FloorToInt((float)vatc.rightPos.x - num2), Mathf.FloorToInt((float)vatc.rightPos.y - num3)));
		}
		else
		{
			float num4 = vatc.leftPos.Distance(vatc.rightPos);
			float num5 = halfWidth * (float)(vatc.rightPos.y - vatc.leftPos.y) / num4;
			float num6 = halfWidth * (float)(vatc.rightPos.x - vatc.leftPos.x) / num4;
			list.Add(new IntVector2(Mathf.CeilToInt((float)vatc.leftPos.x + num5), Mathf.FloorToInt((float)vatc.leftPos.y - num6)));
			list.Add(new IntVector2(Mathf.FloorToInt((float)vatc.leftPos.x - num5), Mathf.CeilToInt((float)vatc.leftPos.y + num6)));
			list.Add(new IntVector2(Mathf.FloorToInt((float)vatc.rightPos.x - num5), Mathf.CeilToInt((float)vatc.rightPos.y + num6)));
			list.Add(new IntVector2(Mathf.CeilToInt((float)vatc.rightPos.x + num5), Mathf.FloorToInt((float)vatc.rightPos.y - num6)));
		}
		return list;
	}

	public static bool CheckRectIntersection(List<IntVector2> rect01, List<IntVector2> rect02)
	{
		for (int i = 0; i < 4; i++)
		{
			IntVector2 aa = rect01[i];
			IntVector2 bb = ((i >= 3) ? rect01[0] : rect01[i + 1]);
			for (int j = 0; j < 4; j++)
			{
				IntVector2 cc = rect02[j / 2];
				IntVector2 dd = ((j % 2 != 0) ? new IntVector2(rect02[1].x, rect02[0].y) : new IntVector2(rect02[0].x, rect02[1].y));
				if (CheckLineintersect(aa, bb, cc, dd))
				{
					return true;
				}
			}
		}
		bool flag = true;
		for (int k = 0; k < 4; k++)
		{
			if (rect01[k].x > rect02[1].x || rect01[k].x < rect02[0].x || rect01[k].y > rect02[1].y || rect01[k].y < rect02[0].y)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return true;
		}
		flag = true;
		for (int l = 0; l < 4; l++)
		{
			IntVector2 intVector = new IntVector2();
			intVector.x = rect02[l / 2].x;
			intVector.y = rect02[l % 2].y;
			for (int m = 0; m < 4; m++)
			{
				int num;
				int num2;
				if (m < 3)
				{
					num = rect01[m + 1].x - rect01[m].x;
					num2 = rect01[m + 1].y - rect01[m].y;
				}
				else
				{
					num = rect01[0].x - rect01[m].x;
					num2 = rect01[0].y - rect01[m].y;
				}
				if (determinant(intVector.x - rect01[m].x, intVector.y - rect01[m].y, num, num2) < 0.0)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}

	private static double determinant(double v1, double v2, double v3, double v4)
	{
		return v1 * v4 - v2 * v3;
	}

	public static bool CheckPointOnSegment(IntVector2 aa, IntVector2 bb, IntVector2 p)
	{
		if (p.Equals(aa) || p.Equals(bb))
		{
			return true;
		}
		if (aa.x == bb.x)
		{
			if (p.x != aa.x)
			{
				return false;
			}
			if ((p.y < bb.y && p.y > aa.y) || (p.y > bb.y && p.y < aa.y))
			{
				return true;
			}
		}
		if (bb.x == p.x)
		{
			return bb.y == p.y;
		}
		if ((bb.y - p.y) / (bb.x - p.x) != (bb.y - aa.y) / (bb.x - aa.x))
		{
			return false;
		}
		if (((p.y < bb.y && p.y > aa.y) || (p.y > bb.y && p.y < aa.y)) && ((p.x < bb.x && p.x > aa.x) || (p.x > bb.x && p.y < aa.x)))
		{
			return true;
		}
		return false;
	}

	public static bool CheckLineintersect(IntVector2 aa, IntVector2 bb, IntVector2 cc, IntVector2 dd)
	{
		double num = determinant(bb.x - aa.x, cc.x - dd.x, bb.y - aa.y, cc.y - dd.y);
		if (num <= 1E-06 && num >= -1E-06)
		{
			IntVector2 p;
			IntVector2 p2;
			IntVector2 aa2;
			IntVector2 bb2;
			if (aa.Distance(bb) <= cc.Distance(dd))
			{
				p = aa;
				p2 = bb;
				aa2 = cc;
				bb2 = dd;
			}
			else
			{
				aa2 = aa;
				bb2 = bb;
				p = cc;
				p2 = dd;
			}
			if (CheckPointOnSegment(aa2, bb2, p))
			{
				return true;
			}
			if (CheckPointOnSegment(aa2, bb2, p2))
			{
				return true;
			}
			return false;
		}
		double num2 = determinant(cc.x - aa.x, cc.x - dd.x, cc.y - aa.y, cc.y - dd.y) / num;
		if (num2 > 1.0 || num2 < 0.0)
		{
			return false;
		}
		double num3 = determinant(bb.x - aa.x, cc.x - aa.x, bb.y - aa.y, cc.y - aa.y) / num;
		if (num3 > 1.0 || num3 < 0.0)
		{
			return false;
		}
		return true;
	}
}
