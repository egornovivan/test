using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.SqliteClient;
using Pathea;
using Pathea.Maths;
using UnityEngine;
using VANativeCampXML;
using VArtifactTownXML;

public class VArtifactUtil
{
	public static string ISOPath = string.Empty;

	public static Dictionary<int, Town_artifacts> townArtifactsData = new Dictionary<int, Town_artifacts>();

	public static Dictionary<int, int> townNameData = new Dictionary<int, int>();

	public static Dictionary<int, AllyName> allyNameData = new Dictionary<int, AllyName>();

	public static Dictionary<string, ulong> isoNameId = new Dictionary<string, ulong>();

	public static Dictionary<ulong, VArtifactData> isos = new Dictionary<ulong, VArtifactData>();

	public static Dictionary<int, ulong> townIdIso = new Dictionary<int, ulong>();

	public static Dictionary<Vector3, int> loadedPos = new Dictionary<Vector3, int>();

	public static Dictionary<IntVector3, VFVoxel> artTown = new Dictionary<IntVector3, VFVoxel>();

	public static int townCount = 0;

	public static int spawnRadius0 = 0;

	public static int spawnRadius = 0;

	public static string[] triplaner = new string[8] { "4,3,68,24,17,23,23,23", "8,6,66,21,22,22,22,66", "14,15,12,13,12,12,13,15", "26,27,65,25,65,25,65,65", "9,5,22,22,17,23,22,24", "8,17,22,66,66,24,23,66", "2,5,18,24,17,18,24,35", "20,59,67,67,20,67,67,67" };

	public static int triplanerIndex_grassLand = 0;

	public static int triplanerIndex_forest = 1;

	public static int triplanerIndex_dessert = 2;

	public static int triplanerIndex_redStone = 3;

	public static int triplanerIndex_rainforest = 4;

	public static int triplanerIndex_hill = 5;

	public static int triplanerIndex_swamp = 6;

	public static int triplanerIndex_crater = 7;

	public static string GetISONameFullPath(string filenameWithoutExtension)
	{
		return ISOPath + "/" + filenameWithoutExtension + ".art";
	}

	public static void Clear()
	{
		isoNameId.Clear();
		isos.Clear();
		townIdIso.Clear();
		loadedPos.Clear();
		artTown.Clear();
		townCount = 0;
	}

	public static void OutputTownVoxel(int x, int y, int z, VCVoxel voxel, VArtifactUnit town)
	{
		IntVector3 key = new IntVector3(x, y, z);
		VFVoxel value = default(VFVoxel);
		value.Type = voxel.Type;
		value.Volume = voxel.Volume;
		if (town.townVoxel.ContainsKey(key))
		{
			VFVoxel vFVoxel = town.townVoxel[key];
			value.Volume = (byte)Mathf.Clamp(value.Volume + vFVoxel.Volume, 0, 255);
		}
		town.townVoxel[key] = value;
	}

	public static bool LoadIso(string path)
	{
		long ticks = DateTime.Now.Ticks;
		try
		{
			VArtifactData vArtifactData = new VArtifactData();
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			byte[] array = new byte[(int)fileStream.Length];
			ulong num = CRC64.Compute(array);
			fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
			if (vArtifactData.Import(array, new VAOption(editor: false)))
			{
				isos[num] = vArtifactData;
				isoNameId[fileNameWithoutExtension] = num;
				Debug.Log("loadIso Time: " + (DateTime.Now.Ticks - ticks));
				return true;
			}
			vArtifactData = null;
			return false;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load file " + path);
			GameLog.HandleIOException(ex, GameLog.EIOFileType.InstallFiles);
			VArtifactData vArtifactData = null;
			return false;
		}
	}

	public static VArtifactData GetIsoData(string isoName, out ulong guid)
	{
		guid = GetGuidFromIsoName(isoName);
		if (guid == 0L)
		{
			Debug.LogError("isoName not exist!");
			return null;
		}
		if (!isos.ContainsKey(guid))
		{
			LoadIso(GetISONameFullPath(isoName));
		}
		return isos[guid];
	}

	public static void ClearAllISO()
	{
		isos.Clear();
	}

	public static void ClearISO(string filename)
	{
		isos.Remove(isoNameId[filename]);
	}

	public static void ClearISO(ulong isoGuId)
	{
		isos.Remove(isoGuId);
	}

	public static void OutputVoxels(Vector3 worldPos, VArtifactUnit newTown, float rotation = 0f)
	{
		if (!isos.ContainsKey(newTown.isoGuId))
		{
			LoadIso(GetISONameFullPath(newTown.isoName));
		}
		if (!isos.ContainsKey(newTown.isoGuId))
		{
			Debug.LogError("isoGuId error: " + newTown.isoGuId + "isoName: " + newTown.isoName);
			return;
		}
		VArtifactData vArtifactData = isos[newTown.isoGuId];
		Quaternion quaternion = default(Quaternion);
		quaternion.eulerAngles = new Vector3(0f, rotation, 0f);
		Vector3 normalized = (quaternion * Vector3.right).normalized;
		Vector3 normalized2 = (quaternion * Vector3.up).normalized;
		Vector3 normalized3 = (quaternion * Vector3.forward).normalized;
		Vector3 vector = new Vector3(vArtifactData.m_HeadInfo.xSize, 0f, vArtifactData.m_HeadInfo.zSize) * -0.5f;
		Vector3 vector2 = worldPos + quaternion * vector;
		foreach (KeyValuePair<int, VCVoxel> voxel in vArtifactData.m_Voxels)
		{
			Vector3 vector3 = new Vector3(voxel.Key & 0x3FF, voxel.Key >> 20, (voxel.Key >> 10) & 0x3FF);
			Vector3 vector4 = vector2 + vector3.x * normalized + vector3.y * normalized2 + vector3.z * normalized3;
			INTVECTOR3 iNTVECTOR = new INTVECTOR3(Mathf.FloorToInt(vector4.x), Mathf.FloorToInt(vector4.y), Mathf.FloorToInt(vector4.z));
			INTVECTOR3 iNTVECTOR2 = new INTVECTOR3(Mathf.CeilToInt(vector4.x), Mathf.CeilToInt(vector4.y), Mathf.CeilToInt(vector4.z));
			if (iNTVECTOR == iNTVECTOR2)
			{
				OutputTownVoxel(iNTVECTOR.x, iNTVECTOR.y, iNTVECTOR.z, voxel.Value, newTown);
				continue;
			}
			for (int i = iNTVECTOR.x; i <= iNTVECTOR2.x; i++)
			{
				for (int j = iNTVECTOR.y; j <= iNTVECTOR2.y; j++)
				{
					for (int k = iNTVECTOR.z; k <= iNTVECTOR2.z; k++)
					{
						float num = 1f - Mathf.Abs(vector4.x - (float)i);
						float num2 = 1f - Mathf.Abs(vector4.y - (float)j);
						float num3 = 1f - Mathf.Abs(vector4.z - (float)k);
						float num4 = num * num2 * num3;
						num4 = ((!(num4 < 0.5f)) ? (0.5f / (1.5f - num4)) : (num4 / (0.5f + num4)));
						VCVoxel value = voxel.Value;
						value.Volume = (byte)Mathf.CeilToInt((float)(int)value.Volume * num4);
						if (value.Volume > 1)
						{
							OutputTownVoxel(i, j, k, value, newTown);
						}
					}
				}
			}
		}
	}

	public static List<IntVector2> OccupiedTile(List<VArtifactUnit> artifactList)
	{
		List<IntVector2> list = new List<IntVector2>();
		for (int i = 0; i < artifactList.Count; i++)
		{
			List<IntVector2> list2 = LinkedChunkIndex(artifactList[i]);
			for (int j = 0; j < list2.Count; j++)
			{
				if (!list.Contains(list2[j]))
				{
					list.Add(list2[j]);
				}
			}
		}
		return list;
	}

	public static List<IntVector2> LinkedChunkIndex(VArtifactUnit townInfo)
	{
		IntVector2 posStart = townInfo.PosStart;
		IntVector2 posEnd = townInfo.PosEnd;
		List<IntVector2> startIndexList = Link1PointToChunk(posStart);
		List<IntVector2> endIndexList = Link1PointToChunk(posEnd);
		IntVector2 minChunkIndex = GetMinChunkIndex(startIndexList);
		IntVector2 maxChunkIndex = GetMaxChunkIndex(endIndexList);
		return GetChunkIndexListFromStartEnd(minChunkIndex, maxChunkIndex);
	}

	public static List<IntVector2> GetChunkIndexListFromStartEnd(IntVector2 startIndex, IntVector2 endIndex)
	{
		List<IntVector2> list = new List<IntVector2>();
		for (int i = startIndex.x; i <= endIndex.x; i++)
		{
			for (int j = startIndex.y; j <= endIndex.y; j++)
			{
				IntVector2 item = new IntVector2(i, j);
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public static IntVector2 GetMinChunkIndex(List<IntVector2> startIndexList)
	{
		if (startIndexList == null || startIndexList.Count <= 0)
		{
			return null;
		}
		IntVector2 intVector = new IntVector2();
		intVector = startIndexList[0];
		for (int i = 0; i < startIndexList.Count; i++)
		{
			if (startIndexList[i].x <= intVector.x && startIndexList[i].y <= intVector.y)
			{
				intVector = startIndexList[i];
			}
		}
		return intVector;
	}

	public static IntVector2 GetMaxChunkIndex(List<IntVector2> endIndexList)
	{
		if (endIndexList == null || endIndexList.Count <= 0)
		{
			return null;
		}
		IntVector2 intVector = new IntVector2();
		intVector = endIndexList[0];
		for (int i = 0; i < endIndexList.Count; i++)
		{
			if (endIndexList[i].x >= intVector.x && endIndexList[i].y >= intVector.y)
			{
				intVector = endIndexList[i];
			}
		}
		return intVector;
	}

	public static List<IntVector2> Link1PointToChunk(IntVector2 pos)
	{
		List<IntVector2> list = new List<IntVector2>();
		int x = pos.x;
		int y = pos.y;
		int num = x >> 5;
		int num2 = y >> 5;
		IntVector2 item = new IntVector2(num, num2);
		list.Add(item);
		int num3 = num;
		if ((x + 1) % 32 == 0)
		{
			num3 = x + 1 >> 5;
		}
		else if (x % 32 < 2)
		{
			num3 = x - 2 >> 5;
		}
		int num4 = num2;
		if ((y + 1) % 32 == 0)
		{
			num4 = y + 1 >> 5;
		}
		else if (y % 32 < 2)
		{
			num4 = y - 2 >> 5;
		}
		if (num3 != num && num4 == num2)
		{
			item = new IntVector2(num3, num2);
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		else if (num3 == num && num4 != num2)
		{
			item = new IntVector2(num, num4);
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		else if (num3 != num && num4 != num2)
		{
			item = new IntVector2(num3, num2);
			if (!list.Contains(item))
			{
				list.Add(item);
			}
			item = new IntVector2(num, num4);
			if (!list.Contains(item))
			{
				list.Add(item);
			}
			item = new IntVector2(num3, num4);
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static VArtifactData GetVartifactDataFromIsoName(string isoName)
	{
		if (isoNameId.ContainsKey(isoName))
		{
			ulong key = isoNameId[isoName];
			if (isos.ContainsKey(key))
			{
				return isos[key];
			}
		}
		return null;
	}

	public static ulong GetGuidFromIsoName(string isoName)
	{
		if (isoNameId.ContainsKey(isoName))
		{
			return isoNameId[isoName];
		}
		return 0uL;
	}

	public static float IsInTown(IntVector2 posXZ)
	{
		if (VArtifactTownManager.Instance == null)
		{
			return 0f;
		}
		int x_ = posXZ.x >> 5;
		int y_ = posXZ.y >> 5;
		IntVector2 tileIndex = new IntVector2(x_, y_);
		if (!VArtifactTownManager.Instance.IsTownChunk(tileIndex))
		{
			return 0f;
		}
		return VArtifactTownManager.Instance.GetTownCenterByTileAndPos(tileIndex, posXZ);
	}

	public static bool IsInTown(IntVector2 posXZ, out IntVector2 townPosCenter)
	{
		townPosCenter = new IntVector2(-9999999, -9999999);
		if (VArtifactTownManager.Instance == null)
		{
			return false;
		}
		if (VArtifactTownManager.Instance == null)
		{
			return false;
		}
		foreach (VArtifactTown value in VArtifactTownManager.Instance.townPosInfo.Values)
		{
			if (value.isEmpty || IntVector2.SqrMagnitude(posXZ - value.PosCenter) > value.radius * value.radius)
			{
				continue;
			}
			townPosCenter = value.PosCenter;
			return true;
		}
		return false;
	}

	public static VArtifactTown GetPosTown(Vector3 pos)
	{
		IntVector2 intVector = new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
		int x_ = intVector.x >> 5;
		int y_ = intVector.y >> 5;
		IntVector2 tileIndex = new IntVector2(x_, y_);
		if (VArtifactTownManager.Instance == null || !VArtifactTownManager.Instance.IsTownChunk(tileIndex))
		{
			return null;
		}
		return VArtifactTownManager.Instance.GetTileTown(tileIndex);
	}

	public static bool CheckTownAvailable(VArtifactTown vaTowndata)
	{
		if (!IsContained(vaTowndata))
		{
			for (int i = 0; i < vaTowndata.VAUnits.Count; i++)
			{
				VArtifactUnit vArtifactUnit = vaTowndata.VAUnits[i];
				vArtifactUnit.worldPos = new Vector3(vArtifactUnit.PosCenter.x, -1f, vArtifactUnit.PosCenter.y);
			}
			return true;
		}
		return false;
	}

	public static void GetArtifactUnit(VArtifactTown townData, ArtifactUnit[] artifactUnitArray, System.Random myRand)
	{
		int x = townData.PosGen.x;
		int y = townData.PosGen.y;
		int x2 = townData.PosGen.x;
		int y2 = townData.PosGen.y;
		int num = 0;
		for (int i = 0; i < artifactUnitArray.Count(); i++)
		{
			IntVector2 intVector2FromStr = GetIntVector2FromStr(artifactUnitArray[i].pos);
			int num2;
			if (artifactUnitArray[i].id.Equals("-1"))
			{
				List<int> list = townArtifactsData.Keys.ToList();
				num2 = list[myRand.Next(list.Count)];
			}
			else
			{
				num2 = RandIntFromStr(artifactUnitArray[i].id, myRand);
			}
			Town_artifacts town_artifacts = townArtifactsData[num2];
			string isoName = town_artifacts.isoName;
			float rot = ((!artifactUnitArray[i].rot.Equals("-1")) ? RandFloatFromStr(artifactUnitArray[i].rot, myRand) : ((float)(myRand.NextDouble() * 360.0)));
			VArtifactUnit vArtifactUnit = new VArtifactUnit();
			ulong guid;
			VArtifactData isoData = GetIsoData(isoName, out guid);
			if (isoData == null)
			{
				Debug.LogError("unitID:" + num2 + " isoName not found! IsoName: " + isoName);
				continue;
			}
			vArtifactUnit.isoName = isoName;
			vArtifactUnit.unitIndex = num++;
			vArtifactUnit.isoId = num2;
			vArtifactUnit.isoGuId = guid;
			vArtifactUnit.vat = townData;
			vArtifactUnit.rot = rot;
			vArtifactUnit.PosCenter = intVector2FromStr + townData.PosGen;
			int xSize = isoData.m_HeadInfo.xSize;
			int zSize = isoData.m_HeadInfo.zSize;
			vArtifactUnit.isoStartPos = new IntVector2(intVector2FromStr.x - xSize / 2, intVector2FromStr.y - zSize / 2) + townData.PosGen;
			vArtifactUnit.isoEndPos = new IntVector2(intVector2FromStr.x + xSize / 2, intVector2FromStr.y + zSize / 2) + townData.PosGen;
			int x3 = town_artifacts.vaSize.x;
			int y3 = town_artifacts.vaSize.y;
			vArtifactUnit.PosStart = new IntVector2(intVector2FromStr.x - x3 / 2, intVector2FromStr.y - y3 / 2) + townData.PosGen;
			vArtifactUnit.PosEnd = new IntVector2(intVector2FromStr.x + x3 / 2, intVector2FromStr.y + y3 / 2) + townData.PosGen;
			vArtifactUnit.level = townData.level;
			vArtifactUnit.type = townData.type;
			vArtifactUnit.buildingIdNum = artifactUnitArray[i].buildingIdNum.ToList();
			vArtifactUnit.npcIdNum = artifactUnitArray[i].npcIdNum.ToList();
			vArtifactUnit.buildingCell = town_artifacts.buildingCell;
			vArtifactUnit.npcPos = town_artifacts.npcPos;
			vArtifactUnit.vaSize = town_artifacts.vaSize;
			vArtifactUnit.towerPos = town_artifacts.towerPos;
			if (vArtifactUnit.PosStart.x < x)
			{
				x = vArtifactUnit.PosStart.x;
			}
			if (vArtifactUnit.PosStart.y < y)
			{
				y = vArtifactUnit.PosStart.y;
			}
			if (vArtifactUnit.PosEnd.x > x2)
			{
				x2 = vArtifactUnit.PosEnd.x;
			}
			if (vArtifactUnit.PosEnd.y > y2)
			{
				y2 = vArtifactUnit.PosEnd.y;
			}
			townData.VAUnits.Add(vArtifactUnit);
		}
		townData.PosStart = new IntVector2(x, y);
		townData.PosEnd = new IntVector2(x2, y2);
		townData.PosCenter = new IntVector2((x + x2) / 2, (y + y2) / 2);
		townData.radius = (int)Mathf.Sqrt(Mathf.Pow((x2 - x) / 2, 2f) + Mathf.Pow((y2 - y) / 2, 2f));
	}

	public static Vector3 GetPosAfterRotation(VArtifactUnit vau, Vector3 relativePos)
	{
		Quaternion quaternion = default(Quaternion);
		quaternion.eulerAngles = new Vector3(0f, vau.rot, 0f);
		Vector3 vector = relativePos - vau.worldPos;
		vector.x += vau.isoStartPos.x;
		vector.y += vau.worldPos.y;
		vector.z += vau.isoStartPos.y;
		return vau.worldPos + quaternion * vector;
	}

	public static DynamicNativePoint GetDynamicNativePoint(int townId)
	{
		VArtifactTown townByID = VArtifactTownManager.Instance.GetTownByID(townId);
		DynamicNative[] dynamicNatives = townByID.nativeTower.dynamicNatives;
		System.Random random = new System.Random(DateTime.Now.Millisecond);
		DynamicNative dynamicNative = dynamicNatives[random.Next(dynamicNatives.Count())];
		DynamicNativePoint result = default(DynamicNativePoint);
		result.id = dynamicNative.did;
		result.type = dynamicNative.type;
		if (dynamicNative.type == 1)
		{
			result.point = GetDynamicNativeSinglePoint(townByID, random);
		}
		else
		{
			result.point = GetDynamicNativeGroupPoint(townByID, random);
		}
		return result;
	}

	public static Vector3 GetDynamicNativeSinglePoint(VArtifactTown vat, System.Random randSeed)
	{
		VArtifactUnit vArtifactUnit = vat.VAUnits[randSeed.Next(vat.VAUnits.Count)];
		Vector3 relativePos = vArtifactUnit.npcPos[randSeed.Next(vArtifactUnit.npcPos.Count)];
		return GetPosAfterRotation(vArtifactUnit, relativePos);
	}

	public static Vector3 GetDynamicNativeGroupPoint(VArtifactTown vat, System.Random randSeed)
	{
		return GetDynamicNativeSinglePoint(vat, randSeed);
	}

	public static DynamicNative[] GetAllDynamicNativePoint(int townId, out List<Vector3> posList)
	{
		DynamicNative[] result = null;
		posList = new List<Vector3>();
		if (VArtifactTownManager.Instance != null)
		{
			VArtifactTown townByID = VArtifactTownManager.Instance.GetTownByID(townId);
			if (townByID != null)
			{
				if (townByID.nativeTower != null)
				{
					result = townByID.nativeTower.dynamicNatives;
					if (townByID.VAUnits != null && townByID.VAUnits.Count > 0)
					{
						VArtifactUnit vArtifactUnit = townByID.VAUnits[0];
						List<Vector3> npcPos = vArtifactUnit.npcPos;
						foreach (Vector3 item in npcPos)
						{
							posList.Add(GetPosAfterRotation(vArtifactUnit, item));
						}
					}
					else
					{
						Debug.LogError("GetAllDynamicNativePoint: vat.VAUnits==null||vat.VAUnits.Count=0");
					}
				}
				else
				{
					Debug.LogError("GetAllDynamicNativePoint: vat.nativeTower==null");
				}
			}
			else
			{
				Debug.LogError("GetAllDynamicNativePoint: vat==null");
			}
		}
		else
		{
			Debug.LogError("GetAllDynamicNativePoint: ArtifactTownManager.Instance==null");
		}
		return result;
	}

	public static void LoadData()
	{
		LoadTownArtifact();
		LoadTownNameData();
		LoadAllyName();
	}

	public static void LoadTownArtifact()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Town_artifacts");
		while (sqliteDataReader.Read())
		{
			Town_artifacts town_artifacts = new Town_artifacts();
			town_artifacts.ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Id")));
			town_artifacts.isoName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Name"));
			town_artifacts.vaSize = GetIntVector3FromStr(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Size")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("B_position"));
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPC_born"));
			string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Tower"));
			town_artifacts.buildingCell = new List<BuildingCell>();
			string[] array = @string.Split('_');
			for (int i = 0; i < array.Count(); i++)
			{
				BuildingCell buildingCell = new BuildingCell();
				string[] array2 = array[i].Split(';');
				buildingCell.cellPos = GetVector3FromStr(array2[0]);
				buildingCell.cellRot = float.Parse(array2[1]);
				town_artifacts.buildingCell.Add(buildingCell);
			}
			town_artifacts.npcPos = new List<Vector3>();
			string[] array3 = string2.Split('_');
			for (int j = 0; j < array3.Count(); j++)
			{
				Vector3 vector3FromStr = GetVector3FromStr(array3[j]);
				town_artifacts.npcPos.Add(vector3FromStr);
			}
			town_artifacts.towerPos = GetVector3FromStr(string3);
			townArtifactsData.Add(town_artifacts.ID, town_artifacts);
		}
	}

	public static void LoadTownNameData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("TownName");
		while (sqliteDataReader.Read())
		{
			int key = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Id")));
			int value = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TranslationId")));
			townNameData.Add(key, value);
		}
	}

	public static int GetTownNameId(int id)
	{
		if (townNameData.ContainsKey(id))
		{
			return townNameData[id];
		}
		return -1;
	}

	public static void LoadAllyName()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdvCampName");
		while (sqliteDataReader.Read())
		{
			AllyName value = default(AllyName);
			value.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			value.raceId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Race")));
			value.nameId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TranslationID")));
			allyNameData.Add(value.id, value);
		}
	}

	public static bool IsContained(VArtifactTown townInfo)
	{
		for (int i = 0; i < townInfo.VAUnits.Count; i++)
		{
			List<IntVector2> list = LinkedChunkIndex(townInfo.VAUnits[i]);
			for (int j = 0; j < list.Count; j++)
			{
				IntVector2 tileIndex = list[j];
				if (VArtifactTownManager.Instance.TileContainsTown(tileIndex))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Vector3 GetStartPos()
	{
		if (PeGameMgr.IsSingleAdventure)
		{
			return VArtifactTownManager.Instance.playerStartPos;
		}
		IntVector2 spawnPos = GetSpawnPos();
		return new Vector3(spawnPos.x, VFDataRTGen.GetPosTop(spawnPos), spawnPos.y);
	}

	public static IntVector2 GetSpawnPos()
	{
		System.Random random = new System.Random(DateTime.Now.Millisecond);
		if (PeGameMgr.IsMultiAdventure)
		{
			if (PeGameMgr.IsMultiCoop)
			{
				return new IntVector2(Mathf.RoundToInt(VArtifactTownManager.Instance.playerStartPos.x), Mathf.RoundToInt(VArtifactTownManager.Instance.playerStartPos.z));
			}
			if (PeGameMgr.IsMultiVS)
			{
				List<VArtifactTown> allyTowns = VATownGenerator.Instance.GetAllyTowns(0);
				allyTowns = allyTowns.FindAll((VArtifactTown it) => it.level <= 1 && it.isMainTown);
				if (allyTowns.Count == 0)
				{
					LogManager.Error("No town! ");
					return VATownGenerator.Instance.GetInitPos(random);
				}
				int count = allyTowns.Count;
				List<int> list = new List<int>();
				for (int i = 0; i < count; i++)
				{
					list.Add(i);
				}
				Shuffle(list, new System.Random(RandomMapConfig.RandSeed));
				int index = list[BaseNetwork.MainPlayer.TeamId % count];
				VArtifactTown vArtifactTown = allyTowns[index];
				IntVector2 posCenter = vArtifactTown.PosCenter;
				return new IntVector2(posCenter.x + random.Next(-spawnRadius, spawnRadius), posCenter.y + random.Next(-spawnRadius, spawnRadius));
			}
			List<VArtifactTown> allyTowns2 = VATownGenerator.Instance.GetAllyTowns(0);
			allyTowns2 = allyTowns2.FindAll((VArtifactTown it) => it.level <= 1 && it.isMainTown);
			if (allyTowns2.Count == 0)
			{
				LogManager.Error("No town! ");
				return VATownGenerator.Instance.GetInitPos(random);
			}
			int index2 = random.Next(allyTowns2.Count);
			VArtifactTown vArtifactTown2 = allyTowns2[index2];
			IntVector2 posCenter2 = vArtifactTown2.PosCenter;
			return new IntVector2(posCenter2.x + random.Next(-spawnRadius, spawnRadius), posCenter2.y + random.Next(-spawnRadius, spawnRadius));
		}
		return VATownGenerator.Instance.GetInitPos();
	}

	public static void SetNpcStandRot(PeEntity npc, float rot, bool isStand)
	{
		if (!(npc == null))
		{
			NpcCmpt cmpt = npc.GetCmpt<NpcCmpt>();
			if (cmpt != null)
			{
				cmpt.Req_Rotation(Quaternion.Euler(0f, rot, 0f));
				cmpt.StandRotate = rot;
			}
		}
	}

	public static void GetPosRotFromPointRot(ref Vector3 pos, ref float rot, Vector3 refPos, float refRot)
	{
		Quaternion quaternion = default(Quaternion);
		quaternion.eulerAngles = new Vector3(0f, refRot, 0f);
		pos = refPos + quaternion * pos;
		rot = refRot + rot;
	}

	public static void ShowAllTowns()
	{
		List<VArtifactTown> list = VArtifactTownManager.Instance.townPosInfo.Values.ToList();
		foreach (VArtifactTown item in list)
		{
			if (item.Type == VArtifactType.NpcTown)
			{
				RandomMapIconMgr.AddTownIcon(item);
			}
			else
			{
				RandomMapIconMgr.AddNativeIcon(item);
			}
		}
	}

	public static void RemoveAllTowns()
	{
		RandomMapIconMgr.ClearAll();
	}

	public static bool HasTown()
	{
		if (VArtifactTownManager.Instance == null)
		{
			return false;
		}
		return VArtifactTownManager.Instance.townPosInfo.Values.Count > 0;
	}

	public static float GetNearestTownDistance(int x, int z, out VArtifactTown vaTown)
	{
		float num = float.MaxValue;
		vaTown = null;
		if (VArtifactTownManager.Instance == null)
		{
			return num;
		}
		foreach (VArtifactTown value in VArtifactTownManager.Instance.townPosInfo.Values)
		{
			if (!value.isEmpty)
			{
				float num2 = (value.PosCenter.x - x) * (value.PosCenter.x - x) + (value.PosCenter.y - z) * (value.PosCenter.y - z);
				if (num2 < num)
				{
					num = num2;
					vaTown = value;
				}
			}
		}
		return Mathf.Sqrt(num);
	}

	public static bool IsInTownBallArea(Vector3 pos)
	{
		if (VArtifactTownManager.Instance == null)
		{
			return false;
		}
		foreach (VArtifactTown value in VArtifactTownManager.Instance.townPosInfo.Values)
		{
			if (value.isEmpty || !((pos - value.TransPos).sqrMagnitude <= (float)(value.radius * value.radius)))
			{
				continue;
			}
			return true;
		}
		return false;
	}

	public static int IsInNativeCampArea(Vector3 pos)
	{
		if (VArtifactTownManager.Instance == null)
		{
			return -1;
		}
		IntVector2 intVector = new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
		IntVector2 tileIndex = new IntVector2(intVector.x >> 5, intVector.y >> 5);
		VArtifactTown tileTown = VArtifactTownManager.Instance.GetTileTown(tileIndex);
		if (tileTown == null)
		{
			return -1;
		}
		if (tileTown.type == VArtifactType.NativeCamp)
		{
			if (tileTown.PosCenter.Distance(intVector) < (float)tileTown.radius && tileTown.InYArea(pos.y))
			{
				if (tileTown.nativeType == NativeType.Puja)
				{
					return 0;
				}
				return 1;
			}
			return -1;
		}
		return -1;
	}

	public static void ChangeTownAlliance(int townId)
	{
		VArtifactTown townByID = VArtifactTownManager.Instance.GetTownByID(townId);
		if (townByID != null)
		{
			VATownGenerator.Instance.ChangeAlliance(townByID);
		}
	}

	public static void RestoreTownAlliance(int townId)
	{
		VArtifactTown townByID = VArtifactTownManager.Instance.GetTownByID(townId);
		if (townByID != null)
		{
			VATownGenerator.Instance.RestoreAlliance(townByID);
		}
	}

	public static bool GetTownPos(int townId, out Vector3 pos)
	{
		if (VArtifactTownManager.Instance == null)
		{
			pos = Vector3.zero;
			return false;
		}
		VArtifactTown townByID = VArtifactTownManager.Instance.GetTownByID(townId);
		if (townByID == null)
		{
			pos = Vector3.zero;
			return false;
		}
		pos = townByID.TransPos;
		return true;
	}

	public static bool GetTownName(int townId, out string name)
	{
		if (VArtifactTownManager.Instance == null)
		{
			name = string.Empty;
			return false;
		}
		VArtifactTown townByID = VArtifactTownManager.Instance.GetTownByID(townId);
		if (townByID == null)
		{
			name = string.Empty;
			return false;
		}
		name = PELocalization.GetString(townByID.townNameId);
		return true;
	}

	public static int GetFirstEnemyNpcColor()
	{
		if (VATownGenerator.Instance == null)
		{
			return -1;
		}
		return VATownGenerator.Instance.GetFirstEnemyNpcAllyColor();
	}

	public static int GetFirstEnemyNpcPlayerId()
	{
		if (VATownGenerator.Instance == null)
		{
			return -1;
		}
		return VATownGenerator.Instance.GetFirstEnemyNpcAllyPlayerId();
	}

	public static void RegistTownDestryedEvent(VArtifactTownManager.TownDestroyed eventListener)
	{
		if (VArtifactTownManager.Instance != null)
		{
			VArtifactTownManager.Instance.TownDestroyedEvent -= eventListener;
			VArtifactTownManager.Instance.TownDestroyedEvent += eventListener;
		}
	}

	public static void UnRegistTownDestryedEvent(VArtifactTownManager.TownDestroyed eventListener)
	{
		if (VArtifactTownManager.Instance != null)
		{
			VArtifactTownManager.Instance.TownDestroyedEvent -= eventListener;
		}
	}

	public static void Shuffle(List<int> id, System.Random myRand)
	{
		int count = id.Count;
		List<int> list = new List<int>();
		for (int i = 0; i < count; i++)
		{
			list.Add(id[i]);
		}
		int num = 0;
		while (list.Count > 0)
		{
			int index = myRand.Next(list.Count);
			id[num] = list[index];
			num++;
			list.RemoveAt(index);
		}
	}

	public static List<int> RandomChoose(int num, int minValue, int maxValue, System.Random randomSeed)
	{
		List<int> list = new List<int>();
		for (int i = minValue; i < maxValue + 1; i++)
		{
			list.Add(i);
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < num; j++)
		{
			int index = randomSeed.Next(maxValue - minValue + 1 - j);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	public static Vector3 GetVector3FromStr(string value)
	{
		if (value == "0")
		{
			return Vector3.zero;
		}
		string[] array = value.Split(',');
		float x = float.Parse(array[0]);
		float y = float.Parse(array[1]);
		float z = float.Parse(array[2]);
		return new Vector3(x, y, z);
	}

	public static IntVector2 GetIntVector2FromStr(string value)
	{
		string[] array = value.Split(',');
		int x_ = Convert.ToInt32(array[0]);
		int y_ = Convert.ToInt32(array[1]);
		return new IntVector2(x_, y_);
	}

	public static IntVector3 GetIntVector3FromStr(string value)
	{
		string[] array = value.Split(',');
		int x_ = Convert.ToInt32(array[0]);
		int y_ = Convert.ToInt32(array[1]);
		int z_ = Convert.ToInt32(array[2]);
		return new IntVector3(x_, y_, z_);
	}

	public static int RandIntFromStr(string value, System.Random rand)
	{
		string[] array = value.Split(',');
		int num = rand.Next(array.Count());
		return Convert.ToInt32(array[num]);
	}

	public static float RandFloatFromStr(string value, System.Random rand)
	{
		string[] array = value.Split(',');
		int num = rand.Next(array.Count());
		return float.Parse(array[num]);
	}

	public static IntVector2 GenCampByZone(IntVector2 center, int zoneNo, int distanceMin, int distanceMax, System.Random randomSeed)
	{
		IntVector2 intVector = new IntVector2(center.x, center.y);
		switch (zoneNo)
		{
		case 0:
			intVector.x += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
			intVector.y += randomSeed.Next(distanceMin, distanceMax);
			break;
		case 1:
			intVector.x += randomSeed.Next(distanceMin, distanceMax);
			intVector.y += randomSeed.Next(distanceMin, distanceMax);
			break;
		case 2:
			intVector.x += randomSeed.Next(distanceMin, distanceMax);
			intVector.y += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
			break;
		case 3:
			intVector.x += randomSeed.Next(distanceMin, distanceMax);
			intVector.y -= randomSeed.Next(distanceMin, distanceMax);
			break;
		case 4:
			intVector.x += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
			intVector.y -= randomSeed.Next(distanceMin, distanceMax);
			break;
		case 5:
			intVector.x -= randomSeed.Next(distanceMin, distanceMax);
			intVector.y -= randomSeed.Next(distanceMin, distanceMax);
			break;
		case 6:
			intVector.x -= randomSeed.Next(distanceMin, distanceMax);
			intVector.y += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
			break;
		case 7:
			intVector.x -= randomSeed.Next(distanceMin, distanceMax);
			intVector.y += randomSeed.Next(distanceMin, distanceMax);
			break;
		default:
			intVector.x += randomSeed.Next(distanceMin, distanceMax);
			intVector.y += randomSeed.Next(distanceMin, distanceMax);
			break;
		}
		return intVector;
	}

	public static IntVector2 GetRandomPointFromPoint(IntVector2 centerPoint, float MaxRadius, System.Random rand)
	{
		IntVector2 intVector = null;
		double num = rand.NextDouble() * (double)MaxRadius * 4.0 / 5.0 + (double)(MaxRadius / 5f);
		double num2 = 6.2831854820251465 * rand.NextDouble();
		int num3 = Mathf.RoundToInt((float)(num * Math.Cos(num2)));
		int num4 = Mathf.RoundToInt((float)(num * Math.Sin(num2)));
		return new IntVector2(centerPoint.x + num3, centerPoint.y + num4);
	}
}
