using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using Mono.Data.SqliteClient;
using NaturalResAsset;
using Pathea;
using PETools;
using SkillSystem;
using uLink;
using UnityEngine;

public class GameWorld
{
	public const int RoomWorldId = 100;

	public const int PlayerWorldId = 101;

	public const int MinWorldId = 200;

	public static float MinBrushSize = 0.5f;

	public static int BlockNumPerItem = (int)(1f / MinBrushSize * (1f / MinBrushSize) * (1f / MinBrushSize));

	protected static int _maxWorldId = 200;

	protected static Dictionary<int, GameWorld> _worldList = new Dictionary<int, GameWorld>();

	private static object m_SyncWorldSave = new object();

	private static bool m_SyncSaved = false;

	private Dictionary<int, Dictionary<IntVector3, BSVoxel>> cacheVoxels = new Dictionary<int, Dictionary<IntVector3, BSVoxel>>();

	private Dictionary<int, Dictionary<IntVector3, BSVoxel>> cacheBlocks = new Dictionary<int, Dictionary<IntVector3, BSVoxel>>();

	private Dictionary<int, List<GroundItemTarget>> cacheTrees = new Dictionary<int, List<GroundItemTarget>>();

	private Dictionary<int, List<Vector3>> cacheGrasses = new Dictionary<int, List<Vector3>>();

	private Dictionary<int, Dictionary<IntVector3, BSVoxel>> _voxels = new Dictionary<int, Dictionary<IntVector3, BSVoxel>>();

	private Dictionary<int, Dictionary<IntVector3, BSVoxel>> _blocks = new Dictionary<int, Dictionary<IntVector3, BSVoxel>>();

	private Dictionary<int, Dictionary<IntVector3, float>> _blockVolumes = new Dictionary<int, Dictionary<IntVector3, float>>();

	private Dictionary<int, List<GroundItemTarget>> _treeList = new Dictionary<int, List<GroundItemTarget>>();

	private Dictionary<int, List<Vector3>> _grassList = new Dictionary<int, List<Vector3>>();

	private Dictionary<int, FlagArea> _occupiedArea = new Dictionary<int, FlagArea>();

	private Dictionary<int, SceneObject> _sceneObjs = new Dictionary<int, SceneObject>();

	private Dictionary<int, List<ExploredArea>> _exploredAreas = new Dictionary<int, List<ExploredArea>>();

	private Dictionary<int, List<MaskArea>> _maskAreas = new Dictionary<int, List<MaskArea>>();

	private List<Vector3> mTownAreas = new List<Vector3>();

	private List<Vector3> mCampAreas = new List<Vector3>();

	private int _worldId;

	private string _worldName;

	public static int NewWorldId => _maxWorldId++;

	public int WorldId => _worldId;

	public string WorldName => _worldName;

	public event Action<int> OnVoxelDataChangedEventHandler;

	public event Action<int> OnBlockDataChangedEventHandler;

	public GameWorld(int id)
	{
		_worldId = id;
		_worldName = "Mainland";
	}

	public static void InitWorld()
	{
		uLink.Network.SetGroupFlags(100, NetworkGroupFlags.HideGameObjects);
		uLink.Network.SetGroupFlags(101, NetworkGroupFlags.HideGameObjects);
		uLink.Network.SetGroupFlags(200, NetworkGroupFlags.HideGameObjects);
		ChannelNetwork.InitChannel(100);
		ChannelNetwork.InitChannel(101);
		if (ServerConfig.IsCustom)
		{
			string[] worldNames = CustomGameData.Mgr.data.WorldNames;
			foreach (string worldName in worldNames)
			{
				NewGameWorld(worldName);
			}
		}
		NewGameWorld(200);
	}

	public static GameWorld NewGameWorld(string worldName)
	{
		GameWorld gameWorld = NewGameWorld();
		gameWorld.SetWorldName(worldName);
		return gameWorld;
	}

	public static GameWorld NewGameWorld()
	{
		return NewGameWorld(NewWorldId);
	}

	public static GameWorld NewGameWorld(int worldId)
	{
		if (_worldList.ContainsKey(worldId))
		{
			return _worldList[worldId];
		}
		NetworkGroup group = new NetworkGroup(worldId);
		uLink.Network.SetGroupFlags(group, NetworkGroupFlags.HideGameObjects);
		ChannelNetwork.InitChannel(worldId);
		GameWorld gameWorld = new GameWorld(worldId);
		_worldList[worldId] = gameWorld;
		return gameWorld;
	}

	public static GameWorld GetGameWorld(int id)
	{
		if (_worldList.ContainsKey(id))
		{
			return _worldList[id];
		}
		return null;
	}

	public static int LoginWorld(int worldId)
	{
		if (worldId <= 0)
		{
			NewGameWorld(200);
			return 200;
		}
		if (!_worldList.ContainsKey(worldId))
		{
			NewGameWorld(worldId);
		}
		return worldId;
	}

	public static int GetWorldId(int worldIndex)
	{
		if (_worldList.Count <= 0 || _worldList.Count <= worldIndex)
		{
			return -1;
		}
		return _worldList.ElementAt(worldIndex).Key;
	}

	public static void AddTownArea(int worldId, Vector3 pos)
	{
		GetGameWorld(worldId)?.AddTownArea(pos);
	}

	public static void AddCampArea(int worldId, Vector3 pos)
	{
		GetGameWorld(worldId)?.AddCampArea(pos);
	}

	public static Vector3[] GetTownAreas(int worldId)
	{
		return GetGameWorld(worldId)?.GetTownAreas();
	}

	public static Vector3[] GetCampAreas(int worldId)
	{
		return GetGameWorld(worldId)?.GetCampAreas();
	}

	public static void SaveAll()
	{
		SaveAreas();
		if (_worldList.Count == 0)
		{
			return;
		}
		lock (m_SyncWorldSave)
		{
			m_SyncSaved = true;
			foreach (KeyValuePair<int, GameWorld> world in _worldList)
			{
				world.Value.CacheSaveData();
				world.Value.SyncSave();
			}
		}
	}

	public static void LoadAll()
	{
		LoadGameWorld();
		LoadAreas();
		string path = Path.Combine(ServerConfig.ServerDir, "GameWorld");
		Directory.CreateDirectory(path);
		string[] files = Directory.GetFiles(path, "*.voxel");
		string[] array = files;
		foreach (string file in array)
		{
			LoadVoxel(file);
		}
		string[] files2 = Directory.GetFiles(path, "*.block");
		string[] array2 = files2;
		foreach (string file2 in array2)
		{
			LoadBlock(file2);
		}
		string[] files3 = Directory.GetFiles(path, "*.tree");
		string[] array3 = files3;
		foreach (string file3 in array3)
		{
			LoadTree(file3);
		}
		string[] files4 = Directory.GetFiles(path, "*.grass");
		string[] array4 = files4;
		foreach (string file4 in array4)
		{
			LoadGrass(file4);
		}
	}

	public static IEnumerator AsyncSave()
	{
		SaveAreas();
		if (_worldList.Count == 0)
		{
			yield break;
		}
		lock (m_SyncWorldSave)
		{
			if (m_SyncSaved)
			{
				yield break;
			}
			foreach (KeyValuePair<int, GameWorld> iter in _worldList)
			{
				iter.Value.CacheSaveData();
				Action saveQuery = iter.Value.SyncSave;
				IAsyncResult ar = saveQuery.BeginInvoke(null, null);
				while (!ar.IsCompleted)
				{
					yield return null;
				}
				saveQuery.EndInvoke(ar);
			}
		}
	}

	public static void SaveAreas()
	{
		AreasDbData areasDbData = new AreasDbData();
		areasDbData.ExportData(_worldList.Values);
		AsyncSqlite.AddRecord(areasDbData);
	}

	public static void LoadGameWorld()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM playergameworld;");
			pEDbOp.BindReaderHandler(LoadWorldDataComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private static void LoadWorldDataComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			int roleId = reader.GetInt32(reader.GetOrdinal("roleid"));
			int worldId = reader.GetInt32(reader.GetOrdinal("worldid"));
			byte[] buff = (byte[])reader.GetValue(reader.GetOrdinal("data"));
			PETools.Serialize.Import(buff, delegate(BinaryReader r)
			{
				int num = BufferHelper.ReadInt32(r);
				for (int i = 0; i < num; i++)
				{
					int index = BufferHelper.ReadInt32(r);
					int teamId = BufferHelper.ReadInt32(r);
					AddExploredArea(roleId, worldId, teamId, index);
				}
				num = BufferHelper.ReadInt32(r);
				for (int j = 0; j < num; j++)
				{
					byte index2 = BufferHelper.ReadByte(r);
					int iconId = BufferHelper.ReadInt32(r);
					BufferHelper.ReadVector3(r, out var _value);
					string desc = BufferHelper.ReadString(r);
					int teamId2 = BufferHelper.ReadInt32(r);
					AddMaskArea(roleId, worldId, teamId2, iconId, _value, desc, index2);
				}
			});
		}
	}

	public static void LoadAreas()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM areainfo;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private static void LoadComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			int int2 = reader.GetInt32(reader.GetOrdinal("worldid"));
			byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("townarea"));
			using (MemoryStream input = new MemoryStream(buffer))
			{
				using BinaryReader reader2 = new BinaryReader(input);
				int num = BufferHelper.ReadInt32(reader2);
				for (int i = 0; i < num; i++)
				{
					BufferHelper.ReadVector3(reader2, out var _value);
					AddTownArea(int2, _value);
				}
			}
			byte[] buffer2 = (byte[])reader.GetValue(reader.GetOrdinal("camparea"));
			using MemoryStream input2 = new MemoryStream(buffer2);
			using BinaryReader reader3 = new BinaryReader(input2);
			int num2 = BufferHelper.ReadInt32(reader3);
			for (int j = 0; j < num2; j++)
			{
				BufferHelper.ReadVector3(reader3, out var _value2);
				AddCampArea(int2, _value2);
			}
		}
	}

	public static int AddMaskArea(int playerId, int worldId, int teamId, int iconId, Vector3 pos, string desc)
	{
		return GetGameWorld(worldId)?.AddMaskArea(playerId, teamId, iconId, pos, desc) ?? (-1);
	}

	public static bool AddMaskArea(int playerId, int worldId, int teamId, int iconId, Vector3 pos, string desc, int index)
	{
		return GetGameWorld(worldId)?.AddMaskArea(playerId, teamId, iconId, pos, desc, index) ?? false;
	}

	public static bool RemoveMaskArea(int playerId, int worldId, int teamId, int index)
	{
		return GetGameWorld(worldId)?.RemoveMaskArea(playerId, teamId, index) ?? false;
	}

	public static MaskArea GetMaskArea(int playerId, int worldId, int teamId, int index)
	{
		return GetGameWorld(worldId)?.GetMaskArea(playerId, teamId, index);
	}

	public static List<MaskArea> GetPlayerMaskAreas(int playerId, int worldId, int teamId)
	{
		List<MaskArea> areas = new List<MaskArea>();
		GetGameWorld(worldId)?.GetPlayerMaskAreas(playerId, teamId, ref areas);
		return areas;
	}

	public static List<MaskArea> GetPlayerMaskAreas(int playerId, int worldId)
	{
		List<MaskArea> areas = new List<MaskArea>();
		GetGameWorld(worldId)?.GetPlayerMaskAreas(playerId, ref areas);
		return areas;
	}

	public static List<MaskArea> GetTeamMaskAreas(int worldId, int teamId)
	{
		List<MaskArea> areas = new List<MaskArea>();
		GetGameWorld(worldId)?.GetTeamMaskAreas(teamId, ref areas);
		return areas;
	}

	public static void Serialize(int playerId, int worldId, BinaryWriter w)
	{
		GetGameWorld(worldId)?.Serialize(playerId, w);
	}

	public static bool AddExploredArea(int playerId, int worldId, int teamId, Vector3 pos)
	{
		int index = AreaHelper.Vector2Int(pos);
		return AddExploredArea(playerId, worldId, teamId, index);
	}

	public static bool AddExploredArea(int playerId, int worldId, int teamId, int index)
	{
		return GetGameWorld(worldId)?.AddExploredArea(playerId, teamId, index) ?? false;
	}

	public static bool IsPlayerExploredArea(int playerId, int worldId, int teamId, int index)
	{
		return GetGameWorld(worldId)?.IsPlayerExploredArea(playerId, teamId, index) ?? false;
	}

	public static bool IsTeamExploredArea(int worldId, int teamId, int index)
	{
		return GetGameWorld(worldId)?.IsTeamExploredArea(teamId, index) ?? false;
	}

	public static List<ExploredArea> GetPlayerExploredAreas(int playerId, int worldId, int teamId)
	{
		List<ExploredArea> areas = new List<ExploredArea>();
		GetGameWorld(worldId)?.GetPlayerExploredAreas(playerId, teamId, ref areas);
		return areas;
	}

	public static List<ExploredArea> GetPlayerExploredAreas(int playerId, int worldId)
	{
		List<ExploredArea> areas = new List<ExploredArea>();
		GetGameWorld(worldId)?.GetPlayerExploredAreas(playerId, ref areas);
		return areas;
	}

	public static List<ExploredArea> GetTeamExploredAreas(int teamId, int worldId)
	{
		List<ExploredArea> areas = new List<ExploredArea>();
		GetGameWorld(worldId)?.GetTeamExploredAreas(teamId, ref areas);
		return areas;
	}

	public static void LoadVoxel(string file, bool bak = false)
	{
		if (!File.Exists(file))
		{
			return;
		}
		FileStream fileStream = null;
		BinaryReader binaryReader = null;
		bool flag = false;
		try
		{
			fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
			binaryReader = new BinaryReader(fileStream);
			int num = BufferHelper.ReadInt32(binaryReader);
			GameWorld gameWorld = GetGameWorld(num);
			if (gameWorld == null)
			{
				gameWorld = NewGameWorld(num);
			}
			int num2 = BufferHelper.ReadInt32(binaryReader);
			for (int i = 0; i < num2; i++)
			{
				int areaIndex = BufferHelper.ReadInt32(binaryReader);
				int num3 = BufferHelper.ReadInt32(binaryReader);
				for (int j = 0; j < num3; j++)
				{
					IntVector3 pos = BufferHelper.ReadIntVector3(binaryReader);
					BufferHelper.ReadBSVoxel(binaryReader, out var _value);
					gameWorld.ApplyVoxel(areaIndex, pos, _value);
				}
			}
			flag = true;
		}
		catch (Exception exception)
		{
			if (LogFilter.logError)
			{
				Debug.LogException(exception);
			}
			flag = false;
		}
		finally
		{
			if (binaryReader != null)
			{
				binaryReader.Close();
				binaryReader = null;
			}
			if (fileStream != null)
			{
				fileStream.Close();
				fileStream = null;
			}
		}
		if (!flag && !bak)
		{
			string file2 = file + ".bak";
			LoadVoxel(file2, bak: true);
		}
	}

	public static void LoadBlock(string file, bool bak = false)
	{
		if (!File.Exists(file))
		{
			return;
		}
		FileStream fileStream = null;
		BinaryReader binaryReader = null;
		bool flag = false;
		try
		{
			fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
			binaryReader = new BinaryReader(fileStream);
			int num = BufferHelper.ReadInt32(binaryReader);
			GameWorld gameWorld = GetGameWorld(num);
			if (gameWorld == null)
			{
				gameWorld = NewGameWorld(num);
			}
			int num2 = BufferHelper.ReadInt32(binaryReader);
			for (int i = 0; i < num2; i++)
			{
				int areaIndex = BufferHelper.ReadInt32(binaryReader);
				int num3 = BufferHelper.ReadInt32(binaryReader);
				for (int j = 0; j < num3; j++)
				{
					IntVector3 pos = BufferHelper.ReadIntVector3(binaryReader);
					BufferHelper.ReadBSVoxel(binaryReader, out var _value);
					gameWorld.ApplyBlock(areaIndex, pos, _value);
				}
			}
			flag = true;
		}
		catch (Exception exception)
		{
			if (LogFilter.logError)
			{
				Debug.LogException(exception);
			}
			flag = false;
		}
		finally
		{
			if (binaryReader != null)
			{
				binaryReader.Close();
				binaryReader = null;
			}
			if (fileStream != null)
			{
				fileStream.Close();
				fileStream = null;
			}
		}
		if (!flag && !bak)
		{
			string file2 = file + ".bak";
			LoadBlock(file2, bak: true);
		}
	}

	public static void LoadGrass(string file, bool bak = false)
	{
		if (!File.Exists(file))
		{
			return;
		}
		FileStream fileStream = null;
		BinaryReader binaryReader = null;
		bool flag = false;
		try
		{
			fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 8192);
			binaryReader = new BinaryReader(fileStream);
			int num = BufferHelper.ReadInt32(binaryReader);
			GameWorld gameWorld = GetGameWorld(num);
			if (gameWorld == null)
			{
				gameWorld = NewGameWorld(num);
			}
			int num2 = BufferHelper.ReadInt32(binaryReader);
			for (int i = 0; i < num2; i++)
			{
				int num3 = BufferHelper.ReadInt32(binaryReader);
				for (int j = 0; j < num3; j++)
				{
					BufferHelper.ReadVector3(binaryReader, out var _value);
					int index = AreaHelper.Vector2Int(_value);
					gameWorld.ApplyGrass(index, _value);
				}
			}
			flag = true;
		}
		catch (Exception exception)
		{
			if (LogFilter.logError)
			{
				Debug.LogException(exception);
			}
			flag = false;
		}
		finally
		{
			if (binaryReader != null)
			{
				binaryReader.Close();
				binaryReader = null;
			}
			if (fileStream != null)
			{
				fileStream.Close();
				fileStream = null;
			}
		}
		if (!flag && !bak)
		{
			string file2 = file + ".bak";
			LoadGrass(file2, bak: true);
		}
	}

	public static void LoadTree(string file, bool bak = false)
	{
		if (!File.Exists(file))
		{
			return;
		}
		FileStream fileStream = null;
		BinaryReader binaryReader = null;
		bool flag = false;
		try
		{
			fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 8192);
			binaryReader = new BinaryReader(fileStream);
			int num = BufferHelper.ReadInt32(binaryReader);
			GameWorld gameWorld = GetGameWorld(num);
			if (gameWorld == null)
			{
				gameWorld = NewGameWorld(num);
			}
			int num2 = BufferHelper.ReadInt32(binaryReader);
			for (int i = 0; i < num2; i++)
			{
				int num3 = BufferHelper.ReadInt32(binaryReader);
				for (int j = 0; j < num3; j++)
				{
					BufferHelper.ReadVector3(binaryReader, out var _value);
					int typeIndex = BufferHelper.ReadInt32(binaryReader);
					float hP = BufferHelper.ReadSingle(binaryReader);
					float maxHP = BufferHelper.ReadSingle(binaryReader);
					float height = BufferHelper.ReadSingle(binaryReader);
					float width = BufferHelper.ReadSingle(binaryReader);
					GroundItemTarget groundItemTarget = new GroundItemTarget(_value, typeIndex, height, width);
					groundItemTarget.HP = hP;
					groundItemTarget.MaxHP = maxHP;
					int index = AreaHelper.Vector2Int(_value);
					gameWorld.ApplyTree(index, groundItemTarget);
				}
			}
			flag = true;
		}
		catch (Exception exception)
		{
			if (LogFilter.logError)
			{
				Debug.LogException(exception);
			}
			flag = false;
		}
		finally
		{
			if (binaryReader != null)
			{
				binaryReader.Close();
				binaryReader = null;
			}
			if (fileStream != null)
			{
				fileStream.Close();
				fileStream = null;
			}
		}
		if (!flag && !bak)
		{
			string file2 = file + ".bak";
			LoadTree(file2, bak: true);
		}
	}

	internal static byte[] GenBlockData(IEnumerable<KeyValuePair<IntVector3, BSVoxel>> blocks)
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, blocks.Count());
			foreach (KeyValuePair<IntVector3, BSVoxel> block in blocks)
			{
				BufferHelper.Serialize(w, block.Key);
				BufferHelper.Serialize(w, block.Value);
			}
		});
	}

	public static Dictionary<IntVector3, B45Block> BuildBuilding(Vector3 worldPos, int ID, int rot)
	{
		BlockBuilding building = BlockBuilding.GetBuilding(ID);
		building.GetBuildingInfo(out var blocks);
		Dictionary<IntVector3, B45Block> dictionary = new Dictionary<IntVector3, B45Block>();
		worldPos = BestMatchPosition(worldPos);
		IntVector3 intVector = WorldPosToBuildIndex(worldPos);
		intVector.y--;
		foreach (IntVector3 key in blocks.Keys)
		{
			Vector3 vector = BuildIndexToWorldPos(key);
			vector = Quaternion.Euler(0f, rot * 90, 0f) * vector;
			vector = BestMatchPosition(vector);
			IntVector3 intVector2 = WorldPosToBuildIndex(vector);
			dictionary[intVector + intVector2] = new B45Block(B45Block.MakeBlockType(blocks[key].blockType >> 2, (rot + (blocks[key].blockType & 3)) % 4), blocks[key].materialType);
		}
		Vector3 zero = Vector3.zero;
		if (rot == 1 || rot == 2)
		{
			zero.z = MinBrushSize;
		}
		if (rot == 2 || rot == 3)
		{
			zero.x = MinBrushSize;
		}
		zero.y = 0f - MinBrushSize;
		return dictionary;
	}

	public static Dictionary<int, int> GetResouce(List<VFVoxel> removeList, float bouns, bool bGetSpItems = false)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		if (removeList.Count == 0)
		{
			return dictionary2;
		}
		foreach (VFVoxel remove in removeList)
		{
			if (dictionary.ContainsKey(remove.Type))
			{
				Dictionary<int, int> dictionary3;
				Dictionary<int, int> dictionary4 = (dictionary3 = dictionary);
				int type;
				int key = (type = remove.Type);
				type = dictionary3[type];
				dictionary4[key] = type + 1;
			}
			else
			{
				dictionary[remove.Type] = 1;
			}
		}
		foreach (int key5 in dictionary.Keys)
		{
			NaturalRes terrainResData;
			if ((terrainResData = NaturalRes.GetTerrainResData(key5)) == null || terrainResData.m_itemsGot.Count <= 0)
			{
				continue;
			}
			float num = 0f;
			num = ((!(terrainResData.mFixedNum > 0f)) ? (bouns + terrainResData.mSelfGetNum) : terrainResData.mFixedNum);
			num *= (float)dictionary[key5];
			for (int i = 0; (float)i < num; i++)
			{
				int num2 = UnityEngine.Random.Range(0, 100);
				for (int j = 0; j < terrainResData.m_itemsGot.Count; j++)
				{
					if ((float)num2 < terrainResData.m_itemsGot[j].m_probablity)
					{
						if (dictionary2.ContainsKey(terrainResData.m_itemsGot[j].m_id))
						{
							Dictionary<int, int> dictionary5;
							Dictionary<int, int> dictionary6 = (dictionary5 = dictionary2);
							int type;
							int key2 = (type = terrainResData.m_itemsGot[j].m_id);
							type = dictionary5[type];
							dictionary6[key2] = type + 1;
						}
						else
						{
							dictionary2[terrainResData.m_itemsGot[j].m_id] = 1;
						}
						break;
					}
				}
			}
			if (terrainResData.m_extraGot.extraPercent > 0f && UnityEngine.Random.value < num * terrainResData.m_extraGot.extraPercent)
			{
				num *= terrainResData.m_extraGot.extraPercent;
				for (int k = 0; (float)k < num; k++)
				{
					int num3 = UnityEngine.Random.Range(0, 100);
					for (int l = 0; l < terrainResData.m_extraGot.m_extraGot.Count; l++)
					{
						if ((float)num3 < terrainResData.m_extraGot.m_extraGot[l].m_probablity)
						{
							if (dictionary2.ContainsKey(terrainResData.m_extraGot.m_extraGot[l].m_id))
							{
								Dictionary<int, int> dictionary7;
								Dictionary<int, int> dictionary8 = (dictionary7 = dictionary2);
								int type;
								int key3 = (type = terrainResData.m_extraGot.m_extraGot[l].m_id);
								type = dictionary7[type];
								dictionary8[key3] = type + 1;
							}
							else
							{
								dictionary2[terrainResData.m_extraGot.m_extraGot[l].m_id] = 1;
							}
							break;
						}
					}
				}
			}
			if (!bGetSpItems)
			{
				continue;
			}
			num = ((!(terrainResData.mFixedNum > 0f)) ? (bouns + terrainResData.mSelfGetNum) : terrainResData.mFixedNum);
			num *= (float)dictionary[key5];
			if (!(terrainResData.m_extraSpGot.extraPercent > 0f) || !(UnityEngine.Random.value < num * terrainResData.m_extraSpGot.extraPercent))
			{
				continue;
			}
			num *= terrainResData.m_extraSpGot.extraPercent;
			for (int m = 0; (float)m < num; m++)
			{
				int num4 = UnityEngine.Random.Range(0, 100);
				for (int n = 0; n < terrainResData.m_extraSpGot.m_extraGot.Count; n++)
				{
					if ((float)num4 < terrainResData.m_extraSpGot.m_extraGot[n].m_probablity)
					{
						if (dictionary2.ContainsKey(terrainResData.m_extraSpGot.m_extraGot[n].m_id))
						{
							Dictionary<int, int> dictionary9;
							Dictionary<int, int> dictionary10 = (dictionary9 = dictionary2);
							int type;
							int key4 = (type = terrainResData.m_extraSpGot.m_extraGot[n].m_id);
							type = dictionary9[type];
							dictionary10[key4] = type + 1;
						}
						else
						{
							dictionary2[terrainResData.m_extraSpGot.m_extraGot[n].m_id] = 1;
						}
						break;
					}
				}
			}
		}
		return dictionary2;
	}

	public static bool AddSceneObj(SceneObject so, int worldId)
	{
		if (_worldList.ContainsKey(worldId))
		{
			return _worldList[worldId].AddSceneObj(so);
		}
		return false;
	}

	public static bool DelSceneObj(SceneObject so, int worldId)
	{
		if (_worldList.ContainsKey(worldId))
		{
			return _worldList[worldId].DelSceneObj(so);
		}
		return false;
	}

	public static bool DelSceneObj(int id, int worldId)
	{
		if (_worldList.ContainsKey(worldId))
		{
			return _worldList[worldId].DelSceneObj(id);
		}
		return false;
	}

	public static bool ExistedObj(SceneObject so, int worldId)
	{
		if (!_worldList.ContainsKey(worldId))
		{
			return false;
		}
		return _worldList[worldId].ExistedObj(so);
	}

	public static bool ExistedObj(int id, int worldId)
	{
		if (!_worldList.ContainsKey(worldId))
		{
			return false;
		}
		return _worldList[worldId].ExistedObj(id);
	}

	public static SceneObject[] GetSceneObjs(int worldId)
	{
		if (!_worldList.ContainsKey(worldId))
		{
			return null;
		}
		return _worldList[worldId].GetSceneObjs();
	}

	public static SceneObject GetSceneObj(int id, int worldId)
	{
		if (!_worldList.ContainsKey(worldId))
		{
			return null;
		}
		return _worldList[worldId].GetSceneObj(id);
	}

	public static void RegisterVoxelDataChangedEvent(int worldId, Action<int> handler)
	{
		if (_worldList.ContainsKey(worldId))
		{
			GameWorld gameWorld = _worldList[worldId];
			gameWorld.OnVoxelDataChangedEventHandler = (Action<int>)Delegate.Combine(gameWorld.OnVoxelDataChangedEventHandler, handler);
		}
	}

	public static void UnregisterVoxelDataChangedEvent(int worldId, Action<int> handler)
	{
		if (_worldList.ContainsKey(worldId))
		{
			GameWorld gameWorld = _worldList[worldId];
			gameWorld.OnVoxelDataChangedEventHandler = (Action<int>)Delegate.Remove(gameWorld.OnVoxelDataChangedEventHandler, handler);
		}
	}

	public static void RegisterBlockDataChangedEvent(int worldId, Action<int> handler)
	{
		if (_worldList.ContainsKey(worldId))
		{
			GameWorld gameWorld = _worldList[worldId];
			gameWorld.OnBlockDataChangedEventHandler = (Action<int>)Delegate.Combine(gameWorld.OnBlockDataChangedEventHandler, handler);
		}
	}

	public static void UnregisterBlockDataChangedEvent(int worldId, Action<int> handler)
	{
		if (_worldList.ContainsKey(worldId))
		{
			GameWorld gameWorld = _worldList[worldId];
			gameWorld.OnBlockDataChangedEventHandler = (Action<int>)Delegate.Remove(gameWorld.OnBlockDataChangedEventHandler, handler);
		}
	}

	public void SyncSave()
	{
		SaveVoxel();
		SaveBlock();
		SaveTree();
		SaveGrass();
	}

	private void OnVoxelDataChanged(int areaIndex)
	{
		if (this.OnVoxelDataChangedEventHandler != null)
		{
			this.OnVoxelDataChangedEventHandler(areaIndex);
		}
	}

	private void OnBlockDataChanged(int areaIndex)
	{
		if (this.OnBlockDataChangedEventHandler != null)
		{
			this.OnBlockDataChangedEventHandler(areaIndex);
		}
	}

	public void AddTownArea(Vector3 pos)
	{
		if (!mTownAreas.Contains(pos))
		{
			mTownAreas.Add(pos);
		}
	}

	public void AddCampArea(Vector3 pos)
	{
		if (!mCampAreas.Contains(pos))
		{
			mCampAreas.Add(pos);
		}
	}

	public Vector3[] GetTownAreas()
	{
		return mTownAreas.ToArray();
	}

	public Vector3[] GetCampAreas()
	{
		return mCampAreas.ToArray();
	}

	public byte[] GetTownAreaData()
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, mTownAreas.Count);
			foreach (Vector3 mTownArea in mTownAreas)
			{
				BufferHelper.Serialize(w, mTownArea);
			}
		});
	}

	public byte[] GetCampAreaData()
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, mCampAreas.Count);
			foreach (Vector3 mCampArea in mCampAreas)
			{
				BufferHelper.Serialize(w, mCampArea);
			}
		});
	}

	public int AddMaskArea(int playerId, int teamId, int iconId, Vector3 pos, string desc)
	{
		int validMaskIndex = GetValidMaskIndex(playerId, teamId);
		if (AddMaskArea(playerId, teamId, iconId, pos, desc, validMaskIndex))
		{
			return validMaskIndex;
		}
		return -1;
	}

	public bool AddMaskArea(int playerId, int teamId, int iconId, Vector3 pos, string desc, int index)
	{
		if (index == -1)
		{
			return false;
		}
		if (!_maskAreas.ContainsKey(playerId))
		{
			_maskAreas.Add(playerId, new List<MaskArea>());
		}
		MaskArea maskArea = new MaskArea();
		maskArea.IconID = iconId;
		maskArea.Index = (byte)index;
		maskArea.Pos = pos;
		maskArea.Description = desc;
		maskArea.TeamId = teamId;
		_maskAreas[playerId].Add(maskArea);
		return true;
	}

	public MaskArea GetMaskArea(int playerId, int teamId, int index)
	{
		if (!_maskAreas.ContainsKey(playerId))
		{
			return null;
		}
		return _maskAreas[playerId].Find((MaskArea iter) => iter.Index == index && iter.TeamId == teamId);
	}

	public bool RemoveMaskArea(int playerId, int teamId, int index)
	{
		if (!_maskAreas.ContainsKey(playerId))
		{
			return false;
		}
		return _maskAreas[playerId].RemoveAll((MaskArea iter) => iter.Index == index && iter.TeamId == teamId) >= 1;
	}

	public void GetPlayerMaskAreas(int playerId, int teamId, ref List<MaskArea> areas)
	{
		if (!_maskAreas.ContainsKey(playerId))
		{
			return;
		}
		foreach (MaskArea item in _maskAreas[playerId])
		{
			if (item.TeamId == teamId)
			{
				areas.Add(item);
			}
		}
	}

	public void GetPlayerMaskAreas(int playerId, ref List<MaskArea> areas)
	{
		if (_maskAreas.ContainsKey(playerId) && _maskAreas[playerId].Count > 0)
		{
			areas.AddRange(_maskAreas[playerId]);
		}
	}

	public void GetTeamMaskAreas(int teamId, ref List<MaskArea> areas)
	{
		foreach (KeyValuePair<int, List<MaskArea>> maskArea in _maskAreas)
		{
			foreach (MaskArea item in maskArea.Value)
			{
				if (item.TeamId == teamId)
				{
					areas.Add(item);
				}
			}
		}
	}

	public bool AddExploredArea(int playerId, int teamId, Vector3 pos)
	{
		int index = AreaHelper.Vector2Int(pos);
		return AddExploredArea(playerId, teamId, index);
	}

	public bool AddExploredArea(int playerId, int teamId, int index)
	{
		if (!IsPlayerExploredArea(playerId, teamId, index))
		{
			_exploredAreas[playerId].Add(new ExploredArea(index, teamId));
			return true;
		}
		return false;
	}

	public bool IsPlayerExploredArea(int playerId, int teamId, int index)
	{
		if (!_exploredAreas.ContainsKey(playerId))
		{
			_exploredAreas.Add(playerId, new List<ExploredArea>());
		}
		return _exploredAreas[playerId].Exists((ExploredArea iter) => iter.Index == index && iter.TeamId == teamId);
	}

	public bool IsTeamExploredArea(int teamId, int index)
	{
		foreach (KeyValuePair<int, List<ExploredArea>> exploredArea in _exploredAreas)
		{
			foreach (ExploredArea item in exploredArea.Value)
			{
				if (item.TeamId == teamId && item.Index == index)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void GetTeamExploredAreas(int teamId, ref List<ExploredArea> areas)
	{
		foreach (KeyValuePair<int, List<ExploredArea>> exploredArea in _exploredAreas)
		{
			foreach (ExploredArea item in exploredArea.Value)
			{
				if (item.TeamId == teamId)
				{
					areas.Add(item);
				}
			}
		}
	}

	public void GetPlayerExploredAreas(int playerId, ref List<ExploredArea> areas)
	{
		if (_exploredAreas.ContainsKey(playerId) && _exploredAreas[playerId].Count > 0)
		{
			areas.AddRange(_exploredAreas[playerId]);
		}
	}

	public void GetPlayerExploredAreas(int playerId, int teamId, ref List<ExploredArea> areas)
	{
		if (!_exploredAreas.ContainsKey(playerId) || _exploredAreas[playerId].Count <= 0)
		{
			return;
		}
		foreach (ExploredArea item in _exploredAreas[playerId])
		{
			if (item.TeamId == teamId)
			{
				areas.Add(item);
			}
		}
	}

	public void Serialize(int playerId, BinaryWriter w)
	{
		if (_exploredAreas.ContainsKey(playerId))
		{
			BufferHelper.Serialize(w, _exploredAreas[playerId].Count);
			foreach (ExploredArea item in _exploredAreas[playerId])
			{
				BufferHelper.Serialize(w, item.Index);
				BufferHelper.Serialize(w, item.TeamId);
			}
		}
		else
		{
			BufferHelper.Serialize(w, 0);
		}
		if (_maskAreas.ContainsKey(playerId))
		{
			BufferHelper.Serialize(w, _maskAreas[playerId].Count);
			{
				foreach (MaskArea item2 in _maskAreas[playerId])
				{
					BufferHelper.Serialize(w, item2.Index);
					BufferHelper.Serialize(w, item2.IconID);
					BufferHelper.Serialize(w, item2.Pos);
					BufferHelper.Serialize(w, item2.Description);
					BufferHelper.Serialize(w, item2.TeamId);
				}
				return;
			}
		}
		BufferHelper.Serialize(w, 0);
	}

	public byte GetValidMaskIndex(int playerId, int teamId)
	{
		if (!_maskAreas.ContainsKey(playerId))
		{
			_maskAreas.Add(playerId, new List<MaskArea>());
		}
		int num = -1;
		for (int i = 0; i <= 255; i++)
		{
			if (!_maskAreas[playerId].Exists((MaskArea iter) => iter.Index == i && iter.TeamId == teamId))
			{
				num = i;
				break;
			}
		}
		return (byte)num;
	}

	public void CacheSaveData()
	{
		cacheVoxels.Clear();
		cacheBlocks.Clear();
		cacheTrees.Clear();
		cacheGrasses.Clear();
		foreach (KeyValuePair<int, Dictionary<IntVector3, BSVoxel>> voxel in _voxels)
		{
			Dictionary<IntVector3, BSVoxel> dictionary = new Dictionary<IntVector3, BSVoxel>();
			foreach (KeyValuePair<IntVector3, BSVoxel> item in voxel.Value)
			{
				dictionary.Add(item.Key, item.Value);
			}
			cacheVoxels.Add(voxel.Key, dictionary);
		}
		foreach (KeyValuePair<int, Dictionary<IntVector3, BSVoxel>> block in _blocks)
		{
			Dictionary<IntVector3, BSVoxel> dictionary2 = new Dictionary<IntVector3, BSVoxel>();
			foreach (KeyValuePair<IntVector3, BSVoxel> item2 in block.Value)
			{
				dictionary2.Add(item2.Key, item2.Value);
			}
			cacheBlocks.Add(block.Key, dictionary2);
		}
		foreach (KeyValuePair<int, List<GroundItemTarget>> tree in _treeList)
		{
			List<GroundItemTarget> list = new List<GroundItemTarget>();
			list.AddRange(tree.Value);
			cacheTrees.Add(tree.Key, list);
		}
		foreach (KeyValuePair<int, List<Vector3>> grass in _grassList)
		{
			List<Vector3> list2 = new List<Vector3>();
			list2.AddRange(grass.Value);
			cacheGrasses.Add(grass.Key, list2);
		}
	}

	public void SaveVoxel()
	{
		if (cacheVoxels.Count <= 0)
		{
			return;
		}
		string text = Path.Combine(ServerConfig.ServerDir, "GameWorld");
		Directory.CreateDirectory(text);
		FileStream fileStream = null;
		BinaryWriter binaryWriter = null;
		try
		{
			string text2 = WorldId + ".voxel";
			string text3 = Path.Combine(text, text2);
			if (File.Exists(text3))
			{
				string path = text2 + ".bak";
				string destFileName = Path.Combine(text, path);
				File.Copy(text3, destFileName, overwrite: true);
			}
			fileStream = new FileStream(text3, FileMode.Create, FileAccess.Write, FileShare.Read, 8192);
			binaryWriter = new BinaryWriter(fileStream);
			BufferHelper.Serialize(binaryWriter, WorldId);
			BufferHelper.Serialize(binaryWriter, cacheVoxels.Count);
			foreach (KeyValuePair<int, Dictionary<IntVector3, BSVoxel>> cacheVoxel in cacheVoxels)
			{
				BufferHelper.Serialize(binaryWriter, cacheVoxel.Key);
				BufferHelper.Serialize(binaryWriter, cacheVoxel.Value.Count);
				foreach (KeyValuePair<IntVector3, BSVoxel> item in cacheVoxel.Value)
				{
					BufferHelper.Serialize(binaryWriter, item.Key);
					BufferHelper.Serialize(binaryWriter, item.Value);
				}
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("Save voxel error\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}
		finally
		{
			binaryWriter?.Close();
			fileStream?.Close();
		}
	}

	public void SaveBlock()
	{
		if (cacheBlocks.Count <= 0)
		{
			return;
		}
		string text = Path.Combine(ServerConfig.ServerDir, "GameWorld");
		Directory.CreateDirectory(text);
		FileStream fileStream = null;
		BinaryWriter binaryWriter = null;
		try
		{
			string text2 = WorldId + ".block";
			string text3 = Path.Combine(text, text2);
			if (File.Exists(text3))
			{
				string path = text2 + ".bak";
				string destFileName = Path.Combine(text, path);
				File.Copy(text3, destFileName, overwrite: true);
			}
			fileStream = new FileStream(text3, FileMode.Create, FileAccess.Write, FileShare.Read, 8192);
			binaryWriter = new BinaryWriter(fileStream);
			BufferHelper.Serialize(binaryWriter, WorldId);
			BufferHelper.Serialize(binaryWriter, cacheBlocks.Count);
			foreach (KeyValuePair<int, Dictionary<IntVector3, BSVoxel>> cacheBlock in cacheBlocks)
			{
				BufferHelper.Serialize(binaryWriter, cacheBlock.Key);
				BufferHelper.Serialize(binaryWriter, cacheBlock.Value.Count);
				foreach (KeyValuePair<IntVector3, BSVoxel> item in cacheBlock.Value)
				{
					BufferHelper.Serialize(binaryWriter, item.Key);
					BufferHelper.Serialize(binaryWriter, item.Value);
				}
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("Save block error\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}
		finally
		{
			binaryWriter?.Close();
			fileStream?.Close();
		}
	}

	public void SaveGrass()
	{
		if (cacheGrasses.Count == 0)
		{
			return;
		}
		string text = Path.Combine(ServerConfig.ServerDir, "GameWorld");
		Directory.CreateDirectory(text);
		FileStream fileStream = null;
		BinaryWriter binaryWriter = null;
		try
		{
			string text2 = WorldId + ".grass";
			string text3 = Path.Combine(text, text2);
			if (File.Exists(text3))
			{
				string path = text2 + ".bak";
				string destFileName = Path.Combine(text, path);
				File.Copy(text3, destFileName, overwrite: true);
			}
			fileStream = new FileStream(text3, FileMode.Create, FileAccess.Write, FileShare.Read, 8192);
			binaryWriter = new BinaryWriter(fileStream);
			BufferHelper.Serialize(binaryWriter, WorldId);
			BufferHelper.Serialize(binaryWriter, cacheGrasses.Count);
			foreach (KeyValuePair<int, List<Vector3>> cacheGrass in cacheGrasses)
			{
				BufferHelper.Serialize(binaryWriter, cacheGrass.Value.Count);
				foreach (Vector3 item in cacheGrass.Value)
				{
					BufferHelper.Serialize(binaryWriter, item);
				}
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("Save grass error\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}
		finally
		{
			binaryWriter?.Close();
			fileStream?.Close();
		}
	}

	public void SaveTree()
	{
		if (cacheTrees.Count == 0)
		{
			return;
		}
		string text = Path.Combine(ServerConfig.ServerDir, "GameWorld");
		Directory.CreateDirectory(text);
		FileStream fileStream = null;
		BinaryWriter binaryWriter = null;
		try
		{
			string text2 = WorldId + ".tree";
			string text3 = Path.Combine(text, text2);
			if (File.Exists(text3))
			{
				string path = text2 + ".bak";
				string destFileName = Path.Combine(text, path);
				File.Copy(text3, destFileName, overwrite: true);
			}
			fileStream = new FileStream(text3, FileMode.Create, FileAccess.Write, FileShare.Read, 8192);
			binaryWriter = new BinaryWriter(fileStream);
			BufferHelper.Serialize(binaryWriter, WorldId);
			BufferHelper.Serialize(binaryWriter, cacheTrees.Count);
			foreach (KeyValuePair<int, List<GroundItemTarget>> cacheTree in cacheTrees)
			{
				BufferHelper.Serialize(binaryWriter, cacheTree.Value.Count);
				foreach (GroundItemTarget item in cacheTree.Value)
				{
					BufferHelper.Serialize(binaryWriter, item.Pos);
					BufferHelper.Serialize(binaryWriter, item.TypeIndex);
					BufferHelper.Serialize(binaryWriter, item.HP);
					BufferHelper.Serialize(binaryWriter, item.MaxHP);
					BufferHelper.Serialize(binaryWriter, item.Height);
					BufferHelper.Serialize(binaryWriter, item.Width);
				}
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("Save tree error\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}
		finally
		{
			binaryWriter?.Close();
			fileStream?.Close();
		}
	}

	public void ChangeTerrain(IntVector3 pos, byte targetType, VFVoxel vf)
	{
		int num = AreaHelper.IntVector2Int(pos);
		if (!_voxels.ContainsKey(num))
		{
			_voxels[num] = new Dictionary<IntVector3, BSVoxel>();
		}
		if (!_voxels[num].ContainsKey(pos))
		{
			_voxels[num][pos] = new BSVoxel(vf);
		}
		BSVoxel bSVoxel = _voxels[num][pos];
		if (bSVoxel.type != 0 && bSVoxel.type != targetType)
		{
			if (bSVoxel.volume > 128)
			{
				_voxels[num][pos] = new BSVoxel((byte)((bSVoxel.volume >= 235) ? 255u : ((uint)(bSVoxel.volume + 20))), targetType);
			}
			else
			{
				_voxels[num][pos] = new BSVoxel(bSVoxel.volume, targetType);
			}
			OnVoxelDataChanged(num);
		}
	}

	public bool ChangeVoxelData(IntVector3 pos, byte type, byte volume, float durDec)
	{
		if (volume == 0)
		{
			return false;
		}
		float num = durDec;
		NaturalRes terrainResData = NaturalRes.GetTerrainResData(type);
		if (terrainResData != null)
		{
			num *= terrainResData.m_duration;
		}
		int num2 = AreaHelper.IntVector2Int(pos);
		if (!_voxels.ContainsKey(num2))
		{
			_voxels[num2] = new Dictionary<IntVector3, BSVoxel>();
		}
		if (!_voxels[num2].ContainsKey(pos))
		{
			_voxels[num2][pos] = new BSVoxel(volume, type);
		}
		BSVoxel value = _voxels[num2][pos];
		byte b = (byte)num;
		if (num >= 255f)
		{
			value.volume = 0;
		}
		else if (value.volume > b)
		{
			value.volume -= b;
		}
		else
		{
			value.volume = 0;
		}
		OnVoxelDataChanged(num2);
		_voxels[num2][pos] = value;
		if (value.volume <= 127)
		{
			value.volume = 0;
			_voxels[num2][pos] = value;
			return true;
		}
		return false;
	}

	internal int ChangeVoxelDataInRange(IntVector3 digPos, Vector3 pos, byte type, byte volume, float durDec, float range)
	{
		NaturalRes terrainResData = NaturalRes.GetTerrainResData(type);
		if (terrainResData != null)
		{
			int num = AreaHelper.IntVector2Int(digPos);
			if (!_voxels.ContainsKey(num))
			{
				_voxels[num] = new Dictionary<IntVector3, BSVoxel>();
			}
			if (!_voxels[num].ContainsKey(digPos))
			{
				_voxels[num][pos] = new BSVoxel(volume, type);
			}
			else if (_voxels[num][digPos].volume <= 0)
			{
				return 0;
			}
			float num2 = durDec * terrainResData.m_duration * (1f - Mathf.Clamp01(Vector3.Distance(pos, digPos.ToVector3()) / range) * 0.25f);
			BSVoxel value = _voxels[num][digPos];
			byte b = (byte)num2;
			if (b >= byte.MaxValue)
			{
				value.volume = 0;
			}
			else if (value.volume > b)
			{
				value.volume -= b;
			}
			else
			{
				value.volume = 0;
			}
			OnVoxelDataChanged(num);
			_voxels[num][pos] = value;
			if (value.volume <= 127)
			{
				value.volume = 0;
				_voxels[num][pos] = value;
				return 1;
			}
		}
		return 0;
	}

	internal Dictionary<IntVector3, BSVoxel> ChangeBlockDataInRange(Vector3 pos, float power, float range, float minBrush)
	{
		Dictionary<IntVector3, BSVoxel> dictionary = new Dictionary<IntVector3, BSVoxel>();
		MinBrushSize = minBrush;
		BlockNumPerItem = (int)(1f / minBrush * (1f / minBrush) * (1f / minBrush));
		for (float num = 0f - range; num < range; num += MinBrushSize)
		{
			for (float num2 = 0f - range; num2 < range; num2 += MinBrushSize)
			{
				for (float num3 = 0f - range; num3 < range; num3 += MinBrushSize)
				{
					Vector3 vector = new Vector3(num, num2, num3);
					Vector3 vector2 = BestMatchPosition(pos + vector);
					if (!(Vector3.Distance(vector2, pos) < range))
					{
						continue;
					}
					IntVector3 intVector = WorldPosToBuildIndex(vector2);
					int num4 = AreaHelper.Vector2Int(vector2);
					if (!_blocks.ContainsKey(num4) || !_blocks[num4].ContainsKey(intVector))
					{
						continue;
					}
					BSVoxel value = _blocks[num4][intVector];
					if (value.blockType >> 2 != 0)
					{
						int blockItemProtoID = BSBlockMatMap.GetBlockItemProtoID(value.materialType);
						NaturalRes terrainResData = NaturalRes.GetTerrainResData(ItemProto.GetItemData(blockItemProtoID).setUp);
						float num5 = power * terrainResData.m_duration * (1f - Mathf.Clamp01(vector.magnitude / range) * 0.25f);
						if (!_blockVolumes.ContainsKey(num4))
						{
							_blockVolumes[num4] = new Dictionary<IntVector3, float>();
						}
						if (!_blockVolumes[num4].ContainsKey(intVector))
						{
							_blockVolumes[num4][intVector] = 255f;
						}
						Dictionary<IntVector3, float> dictionary2;
						Dictionary<IntVector3, float> dictionary3 = (dictionary2 = _blockVolumes[num4]);
						IntVector3 key;
						IntVector3 key2 = (key = intVector);
						float num6 = dictionary2[key];
						dictionary3[key2] = num6 - num5;
						if (_blockVolumes[num4][intVector] <= 0f)
						{
							_blockVolumes[num4].Remove(intVector);
							_blocks[num4].Remove(intVector);
							value.blockType = 0;
							value.volume = 0;
							dictionary.Add(intVector, value);
							OnBlockDataChanged(num4);
						}
					}
				}
			}
		}
		return dictionary;
	}

	public byte[] GenBSVoxelData(int dsType, int index)
	{
		Dictionary<IntVector3, BSVoxel> ds;
		if (dsType == 0)
		{
			if (!_voxels.ContainsKey(index))
			{
				return null;
			}
			ds = _voxels[index];
		}
		else
		{
			if (!_blocks.ContainsKey(index))
			{
				return null;
			}
			ds = _blocks[index];
		}
		if (ds.Count == 0)
		{
			return null;
		}
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, ds.Count);
			foreach (KeyValuePair<IntVector3, BSVoxel> item in ds)
			{
				BufferHelper.Serialize(w, item.Key);
				BufferHelper.Serialize(w, item.Value);
			}
		});
	}

	public void BuildBuildingBlock(Dictionary<IntVector3, B45Block> buildDic)
	{
		Dictionary<int, Dictionary<IntVector3, B45Block>> dictionary = new Dictionary<int, Dictionary<IntVector3, B45Block>>();
		foreach (IntVector3 key2 in buildDic.Keys)
		{
			Vector3 pos = BuildIndexToWorldPos(key2);
			int key = AreaHelper.Vector2Int(pos);
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = new Dictionary<IntVector3, B45Block>();
			}
			dictionary[key][key2] = buildDic[key2];
		}
		if (LogFilter.logDebug)
		{
			Debug.Log("areaBuildDic: " + dictionary.Keys.Count);
		}
		Dictionary<IntVector3, B45Block> dictionary2 = new Dictionary<IntVector3, B45Block>();
		foreach (int key3 in dictionary.Keys)
		{
			foreach (IntVector3 key4 in dictionary[key3].Keys)
			{
				dictionary2[key4] = dictionary[key3][key4];
				AddBlockList(key3, key4, dictionary[key3][key4]);
			}
		}
	}

	public static IntVector3 WorldPosToBuildIndex(Vector3 worldPos)
	{
		return new IntVector3(worldPos / MinBrushSize);
	}

	public static Vector3 BuildIndexToWorldPos(IntVector3 index)
	{
		return index.ToVector3() * MinBrushSize + 0.001f * Vector3.one;
	}

	public static Vector3 BestMatchPosition(Vector3 pos)
	{
		Vector3 vector = (pos + 0.001f * Vector3.one) / MinBrushSize;
		vector.x = Mathf.RoundToInt(vector.x);
		vector.y = Mathf.RoundToInt(vector.y);
		vector.z = Mathf.RoundToInt(vector.z);
		return vector * MinBrushSize + 0.001f * Vector3.one;
	}

	public static bool CheckArea(int worldId, int teamId, Vector3 pos, int protoId)
	{
		int index = AreaHelper.Vector2Int(pos);
		return CheckArea(worldId, teamId, index, protoId);
	}

	public static bool CheckArea(int worldId, int teamId, int index, int protoId)
	{
		return GetGameWorld(worldId)?.CanPutOut(teamId, index, protoId) ?? false;
	}

	public static void OccupyArea(int worldId, int teamId, Vector3 pos, int lvl)
	{
		int index = AreaHelper.Vector2Int(pos);
		GetGameWorld(worldId)?.AddOccupiedArea(teamId, index, lvl);
	}

	public static void LoseArea(int worldId, int teamId, Vector3 pos)
	{
		int index = AreaHelper.Vector2Int(pos);
		GetGameWorld(worldId)?.DelOccupiedArea(teamId, index);
	}

	internal void AddBlockList(int mapIndex, IntVector3 index, B45Block block)
	{
		if (!_blocks.ContainsKey(mapIndex))
		{
			_blocks[mapIndex] = new Dictionary<IntVector3, BSVoxel>();
		}
		BSVoxel value = new BSVoxel(block);
		_blocks[mapIndex][index] = value;
	}

	public void ApplyVoxel(int areaIndex, IntVector3 pos, BSVoxel voxel)
	{
		if (!_voxels.ContainsKey(areaIndex))
		{
			_voxels[areaIndex] = new Dictionary<IntVector3, BSVoxel>();
		}
		_voxels[areaIndex][pos] = voxel;
	}

	public void ApplyBlock(int areaIndex, IntVector3 pos, BSVoxel block)
	{
		if (!_blocks.ContainsKey(areaIndex))
		{
			_blocks[areaIndex] = new Dictionary<IntVector3, BSVoxel>();
		}
		_blocks[areaIndex][pos] = block;
	}

	public void ApplyTree(int index, GroundItemTarget item)
	{
		if (!_treeList.ContainsKey(index))
		{
			_treeList[index] = new List<GroundItemTarget>();
		}
		_treeList[index].Add(item);
	}

	public void ApplyGrass(int index, Vector3 pos)
	{
		if (!_grassList.ContainsKey(index))
		{
			_grassList[index] = new List<Vector3>();
		}
		_grassList[index].Add(pos);
	}

	public void ApplyData(int dsType, Dictionary<IntVector3, BSVoxel> effVoxel)
	{
		if (dsType == 0)
		{
			foreach (KeyValuePair<IntVector3, BSVoxel> item in effVoxel)
			{
				int num = AreaHelper.IntVector2Int(item.Key);
				if (!_voxels.ContainsKey(num))
				{
					_voxels[num] = new Dictionary<IntVector3, BSVoxel>();
				}
				if (item.Value.value0 == 0 && item.Value.value1 == 0)
				{
					_voxels[num].Remove(item.Key);
				}
				else
				{
					if (_blocks.ContainsKey(num) && _blocks[num].ContainsKey(item.Key))
					{
						_blocks[num].Remove(item.Key);
					}
					_voxels[num][item.Key] = item.Value;
				}
				OnVoxelDataChanged(num);
			}
		}
		else
		{
			foreach (KeyValuePair<IntVector3, BSVoxel> item2 in effVoxel)
			{
				IntVector3 intVector = new IntVector3((float)item2.Key.x * 0.5f, (float)item2.Key.y * 0.5f, (float)item2.Key.z * 0.5f);
				int num2 = AreaHelper.Vector2Int(intVector);
				if (!_blocks.ContainsKey(num2))
				{
					_blocks[num2] = new Dictionary<IntVector3, BSVoxel>();
				}
				if (item2.Value.value0 == 0 && item2.Value.value1 == 0)
				{
					_blocks[num2].Remove(item2.Key);
				}
				else
				{
					if (_voxels.ContainsKey(num2) && _voxels[num2].ContainsKey(item2.Key))
					{
						_voxels[num2].Remove(item2.Key);
					}
					_blocks[num2][item2.Key] = item2.Value;
				}
				OnBlockDataChanged(num2);
			}
		}
		SyncVoxelRedo(dsType, effVoxel);
	}

	private void SyncVoxelRedo(int dsType, Dictionary<IntVector3, BSVoxel> voxels)
	{
		byte[] array = PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, dsType);
			BufferHelper.Serialize(w, voxels.Count);
			foreach (KeyValuePair<IntVector3, BSVoxel> voxel in voxels)
			{
				BufferHelper.Serialize(w, voxel.Key);
				BufferHelper.Serialize(w, voxel.Value);
			}
		});
		ChannelNetwork.SyncChannel(WorldId, EPacketType.PT_InGame_BlockRedo, array);
	}

	public void DeleteGrass(byte[] data)
	{
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				BufferHelper.ReadVector3(r, out var _value);
				DeleteGrass(_value);
			}
		});
	}

	public void DeleteTree(byte[] data)
	{
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				BufferHelper.ReadVector3(r, out var _value);
				int typeIndex = BufferHelper.ReadInt32(r);
				DeleteTree(_value, typeIndex);
			}
		});
	}

	private void DeleteGrass(Vector3 pos)
	{
		int key = AreaHelper.Vector2Int(pos);
		if (!_grassList.ContainsKey(key))
		{
			_grassList[key] = new List<Vector3>();
		}
		if (!_grassList[key].Contains(pos))
		{
			_grassList[key].Add(pos);
		}
	}

	public void DeleteTree(Vector3 pos, int typeIndex)
	{
		int key = AreaHelper.Vector2Int(pos);
		if (!_treeList.ContainsKey(key))
		{
			_treeList[key] = new List<GroundItemTarget>();
		}
		GroundItemTarget groundItemTarget = _treeList[key].Find((GroundItemTarget iter) => (iter.Pos - pos).sqrMagnitude <= 0.01f);
		if (groundItemTarget == null)
		{
			groundItemTarget = new GroundItemTarget(pos, typeIndex, 1f, 1f);
			groundItemTarget.MaxHP = 0f;
			groundItemTarget.HP = 0f;
			_treeList[key].Add(groundItemTarget);
		}
		else
		{
			groundItemTarget.HP = 0f;
		}
	}

	public void FellTree(SkEntity caster, Vector3 pos, int typeIndex, float height, float width)
	{
		if (!(null != caster))
		{
			return;
		}
		int key = AreaHelper.Vector2Int(pos);
		if (!_treeList.ContainsKey(key))
		{
			_treeList[key] = new List<GroundItemTarget>();
		}
		NaturalRes terrainResData = NaturalRes.GetTerrainResData(typeIndex + 1000);
		if (terrainResData == null)
		{
			return;
		}
		GroundItemTarget groundItemTarget = _treeList[key].Find((GroundItemTarget iter) => (iter.Pos - pos).sqrMagnitude <= 0.01f);
		if (groundItemTarget == null)
		{
			groundItemTarget = new GroundItemTarget(pos, typeIndex, height, width);
			_treeList[key].Add(groundItemTarget);
		}
		if (groundItemTarget.HP <= 0f)
		{
			caster._net.RPCOthers(EPacketType.PT_InGame_SKFellTree, groundItemTarget.TypeIndex, groundItemTarget.Pos, groundItemTarget.MaxHP, groundItemTarget.HP);
			return;
		}
		float num = 0f;
		if (groundItemTarget.GetTargetType() == ESkillTargetType.TYPE_Wood)
		{
			num = groundItemTarget.GetDestroyed(caster.GetAttribute(AttribType.CutDamage), terrainResData.m_duration);
		}
		else if (groundItemTarget.GetTargetType() == ESkillTargetType.TYPE_Herb)
		{
			num = groundItemTarget.GetDestroyed(caster.GetAttribute(AttribType.Atk), terrainResData.m_duration);
		}
		if (num <= 0f)
		{
			if (caster._net is Player)
			{
				bool bGetSpItems = ((Player)caster._net).CheckCutterGetRare();
				if (groundItemTarget.GetTargetType() == ESkillTargetType.TYPE_Wood)
				{
					ActionEventsMgr._self.ProcessAction(OperatorEnum.Oper_Tree, ActionOpportunity.Opp_OnDeath, (SkNetworkInterface)caster._net, caster.ID);
				}
				else if (groundItemTarget.GetTargetType() == ESkillTargetType.TYPE_Herb)
				{
					ActionEventsMgr._self.ProcessAction(OperatorEnum.Oper_Herb, ActionOpportunity.Opp_OnDeath, (SkNetworkInterface)caster._net, caster.ID);
				}
				GetResouce((Player)caster._net, groundItemTarget, bGetSpItems);
			}
			else if (caster._net is AiAdNpcNetwork)
			{
				Player lordPlayer = (caster._net as AiAdNpcNetwork).lordPlayer;
				if (lordPlayer != null)
				{
					bool bGetSpItems2 = lordPlayer.CheckCutterGetRare();
					GetResouce(lordPlayer, groundItemTarget, bGetSpItems2);
				}
			}
		}
		caster._net.RPCOthers(EPacketType.PT_InGame_SKFellTree, groundItemTarget.TypeIndex, groundItemTarget.Pos, groundItemTarget.MaxHP, groundItemTarget.HP);
	}

	public int GenTreeData(out byte[] data)
	{
		int count = 0;
		data = PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			foreach (KeyValuePair<int, List<GroundItemTarget>> tree in _treeList)
			{
				foreach (GroundItemTarget item in tree.Value)
				{
					if (item.HP <= 0f)
					{
						count++;
						BufferHelper.Serialize(w, item.Pos);
					}
				}
			}
		});
		return count;
	}

	public int GenGrassData(out byte[] data)
	{
		int count = 0;
		data = PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			foreach (KeyValuePair<int, List<Vector3>> grass in _grassList)
			{
				foreach (Vector3 item in grass.Value)
				{
					count++;
					BufferHelper.Serialize(w, item);
				}
			}
		});
		return count;
	}

	public void Clear(int index)
	{
		_blockVolumes.Remove(index);
		_blocks.Remove(index);
		_voxels.Remove(index);
	}

	public void Clear()
	{
		_blockVolumes.Clear();
		_blocks.Clear();
		_voxels.Clear();
	}

	public void AddOccupiedArea(int teamId, int index, int flagLvl)
	{
		if (!_occupiedArea.ContainsKey(index))
		{
			_occupiedArea[index] = new FlagArea(index, teamId);
		}
		_occupiedArea[index].SetLevel(flagLvl);
	}

	public void DelOccupiedArea(int teamId, int index)
	{
		_occupiedArea.Remove(index);
	}

	public int GetOccupiedAreaNum(int teamId)
	{
		int num = 0;
		for (int i = 0; i < _occupiedArea.Count; i++)
		{
			if (_occupiedArea.ElementAt(i).Value.TeamId == teamId)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsOccupiedArea(int teamId, int index)
	{
		if (!_occupiedArea.ContainsKey(index))
		{
			return false;
		}
		return ForceSetting.Conflict(_occupiedArea[index].TeamId, teamId);
	}

	public bool CanPutOut(int teamId, int index, int protoId)
	{
		if (!_occupiedArea.ContainsKey(index))
		{
			return true;
		}
		if (protoId == 376 || protoId == 377 || protoId == 378)
		{
			return false;
		}
		if (ForceSetting.Conflict(_occupiedArea[index].TeamId, teamId))
		{
			return _occupiedArea[index].FlagLv <= 377;
		}
		return true;
	}

	public bool CanDig(int teamId, int index)
	{
		if (!_occupiedArea.ContainsKey(index))
		{
			return true;
		}
		if (!ForceSetting.Conflict(_occupiedArea[index].TeamId, teamId))
		{
			return true;
		}
		return _occupiedArea[index].FlagLv <= 376;
	}

	public void DigTerrain(SkNetworkInterface net, IntVector3 intPos, float durDec, float radius, float resourceBonus, byte[] data, bool bReturnItem, bool bGetSpItems, float height)
	{
		if (net == null)
		{
			return;
		}
		int index = AreaHelper.Vector2Int(net.transform.position);
		if (!CanDig(net.TeamId, index))
		{
			return;
		}
		VFVoxel voxel = new VFVoxel(0, 0);
		VFTerrainTarget target = new VFTerrainTarget(Vector3.zero, intPos, ref voxel);
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			for (float num = 0f - radius; num <= radius; num += 1f)
			{
				for (float num2 = 0f - radius; num2 <= radius; num2 += 1f)
				{
					for (float num3 = 0f - height; num3 <= height; num3 += 1f)
					{
						IntVector3 pos = new IntVector3((float)intPos.x + num, (float)intPos.y + num3, (float)intPos.z + num2);
						float num4 = num * num + num3 * num3 + num2 * num2;
						if (bReturnItem || !(num4 > radius * radius))
						{
							byte type = BufferHelper.ReadByte(r);
							byte volume = BufferHelper.ReadByte(r);
							if (ChangeVoxelData(pos, type, volume, durDec))
							{
								target.mRemoveList.Add(new VFVoxel(0, type));
							}
						}
					}
				}
			}
		});
		if (bReturnItem)
		{
			Dictionary<int, int> resouce = GetResouce(target.mRemoveList, resourceBonus, bGetSpItems);
			ItemSample[] array = resouce.Select((KeyValuePair<int, int> iter) => new ItemSample(iter.Key, iter.Value)).ToArray();
			if (net is Player)
			{
				Player player = (Player)net;
				player.GetPoint(BattleConstData.Instance._points_dig);
				BattleManager.SyncBattleInfo(player.TeamId);
				BattleManager.SyncBattleInfos();
				if (array != null && array.Length >= 1 && player.Package.CanAdd(array))
				{
					ItemObject[] items = player.Package.AddSameItems(array);
					player.SyncItemList(items);
					player.SyncNewItem(array);
					player.SyncPackageIndex();
					ActionEventsMgr._self.ProcessAction(OperatorEnum.Oper_Voxel, ActionOpportunity.Opp_OnDeath, player, player.Id);
				}
			}
		}
		ChannelNetwork.SyncChannel(net.WorldId, EPacketType.PT_InGame_SKDigTerrain, intPos, durDec, radius, height, bReturnItem);
	}

	public void GetResouce(Player player, GroundItemTarget treeInfo, bool bGetSpItems = false)
	{
		if (!(null != player._skEntity) || player.Package == null)
		{
			return;
		}
		NaturalRes terrainResData = NaturalRes.GetTerrainResData(treeInfo.TypeIndex + 1000);
		Dictionary<int, int> plantList = new Dictionary<int, int>();
		float num = 0f;
		num = ((!(terrainResData.mFixedNum > 0f)) ? (player._skEntity.GetAttribute(AttribType.ResBouns) + terrainResData.mSelfGetNum * treeInfo.Width * treeInfo.Width * treeInfo.Height) : terrainResData.mFixedNum);
		for (int i = 0; i < (int)num; i++)
		{
			int num2 = UnityEngine.Random.Range(0, 100);
			for (int j = 0; j < terrainResData.m_itemsGot.Count; j++)
			{
				if ((float)num2 < terrainResData.m_itemsGot[j].m_probablity)
				{
					if (plantList.ContainsKey(terrainResData.m_itemsGot[j].m_id))
					{
						Dictionary<int, int> dictionary;
						Dictionary<int, int> dictionary2 = (dictionary = plantList);
						int id;
						int key = (id = terrainResData.m_itemsGot[j].m_id);
						id = dictionary[id];
						dictionary2[key] = id + 1;
					}
					else
					{
						plantList[terrainResData.m_itemsGot[j].m_id] = 1;
					}
					break;
				}
			}
		}
		if (terrainResData.m_extraGot.extraPercent > 0f && UnityEngine.Random.value < num * terrainResData.m_extraGot.extraPercent)
		{
			num *= terrainResData.m_extraGot.extraPercent;
			for (int k = 0; (float)k < num; k++)
			{
				int num3 = UnityEngine.Random.Range(0, 100);
				for (int l = 0; l < terrainResData.m_extraGot.m_extraGot.Count; l++)
				{
					if ((float)num3 < terrainResData.m_extraGot.m_extraGot[l].m_probablity)
					{
						if (plantList.ContainsKey(terrainResData.m_extraGot.m_extraGot[l].m_id))
						{
							Dictionary<int, int> dictionary3;
							Dictionary<int, int> dictionary4 = (dictionary3 = plantList);
							int id;
							int key2 = (id = terrainResData.m_extraGot.m_extraGot[l].m_id);
							id = dictionary3[id];
							dictionary4[key2] = id + 1;
						}
						else
						{
							plantList[terrainResData.m_extraGot.m_extraGot[l].m_id] = 1;
						}
						break;
					}
				}
			}
		}
		num = ((!(terrainResData.mFixedNum > 0f)) ? (player._skEntity.GetAttribute(AttribType.ResBouns) + terrainResData.mSelfGetNum * treeInfo.Width * treeInfo.Width * treeInfo.Height) : terrainResData.mFixedNum);
		if (bGetSpItems && terrainResData.m_extraSpGot.extraPercent > 0f && UnityEngine.Random.value < num * terrainResData.m_extraSpGot.extraPercent && player.CheckCutterGetRare())
		{
			num *= terrainResData.m_extraSpGot.extraPercent;
			for (int m = 0; (float)m < num; m++)
			{
				int num4 = UnityEngine.Random.Range(0, 100);
				for (int n = 0; n < terrainResData.m_extraSpGot.m_extraGot.Count; n++)
				{
					if ((float)num4 < terrainResData.m_extraSpGot.m_extraGot[n].m_probablity)
					{
						if (plantList.ContainsKey(terrainResData.m_extraGot.m_extraGot[n].m_id))
						{
							Dictionary<int, int> dictionary5;
							Dictionary<int, int> dictionary6 = (dictionary5 = plantList);
							int id;
							int key3 = (id = terrainResData.m_extraSpGot.m_extraGot[n].m_id);
							id = dictionary5[id];
							dictionary6[key3] = id + 1;
						}
						else
						{
							plantList[terrainResData.m_extraSpGot.m_extraGot[n].m_id] = 1;
						}
						break;
					}
				}
			}
		}
		GetSpecialItem.PlantItemAdd(ref plantList, player);
		foreach (int key4 in plantList.Keys)
		{
			ItemObject itemObject = player.CreateItem(key4, plantList[key4], syn: true);
			if (itemObject != null)
			{
				player.Package.AddItem(itemObject);
				player.SyncItem(itemObject);
				player.SyncPackageIndex();
			}
		}
		ItemSample[] items = plantList.Select((KeyValuePair<int, int> iter) => new ItemSample(iter.Key, iter.Value)).ToArray();
		player.SyncNewItem(items);
	}

	public bool AddSceneObj(SceneObject so)
	{
		if (so == null)
		{
			return false;
		}
		_sceneObjs[so.Id] = so;
		return true;
	}

	public bool DelSceneObj(SceneObject so)
	{
		if (so == null)
		{
			return false;
		}
		return DelSceneObj(so.Id);
	}

	public bool DelSceneObj(int id)
	{
		if (_sceneObjs.ContainsKey(id) && _sceneObjs.Remove(id))
		{
			SceneObjMgr.Delete(id);
			return true;
		}
		return false;
	}

	public bool ExistedObj(SceneObject so)
	{
		if (so == null)
		{
			return false;
		}
		return ExistedObj(so.Id);
	}

	public bool ExistedObj(int id)
	{
		return _sceneObjs.ContainsKey(id);
	}

	public SceneObject[] GetSceneObjs()
	{
		return _sceneObjs.Values.ToArray();
	}

	public SceneObject GetSceneObj(int id)
	{
		return (!ExistedObj(id)) ? null : _sceneObjs[id];
	}

	public void SetWorldName(string worldName)
	{
		_worldName = worldName;
	}
}
