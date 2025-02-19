using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CustomData;
using Jboy;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using uLink;
using UnityEngine;
using Weather;

public class ServerConfig
{
	public const string DataBaseFile = "DataBase/localData";

	public const int CurrentVersion = 273;

	public static readonly int[] VersionCompatible = new int[15]
	{
		0, 256, 257, 258, 259, 260, 261, 262, 263, 264,
		265, 272, 273, 274, 275
	};

	public static string ServerShareDir = string.Empty;

	public static string ServerConfDir = string.Empty;

	public static string ServerLogDir = string.Empty;

	public static string ServerGameDir = string.Empty;

	public static string ServerDir = string.Empty;

	public static string ScenarioPath = string.Empty;

	public static string WorldPath = string.Empty;

	public static string ServerName = "PlanetExplorer";

	public static string LobbyIP = "119.28.73.40";

	public static string ProxyIP = "119.28.73.40";

	public static string MapSeed = "patheamaria";

	public static string MapName = string.Empty;

	public static string MasterRoleName = "Unknown";

	public static string Password = string.Empty;

	public static string UID = string.Empty;

	public static string ServerVersion = "V1.1.3";

	public static ESceneMode SceneMode = ESceneMode.Adventure;

	public static EGameType GameType = EGameType.VS;

	public static EClimateType ClimateType = EClimateType.CT_Temperate;

	public static RandomMapType TerrainType = RandomMapType.GrassLand;

	public static RandomMapType VegetationType = RandomMapType.GrassLand;

	public static EMoneyType MoneyType = EMoneyType.Digital;

	public static int LobbyPort = 12534;

	public static int ProxyPort = 12535;

	public static int ServerPort = 9900;

	public static int MaxConnections = 1000;

	public static int NumPerTeam = 16;

	public static int TeamNum = 2;

	public static int DropDeadPercent = 10;

	public static int TerrainHeight = 512;

	public static int MapSize = 2;

	public static int RiverDensity = 1;

	public static int RiverWidth = 1;

	public static int PlainHeight = 20;

	public static int Flatness = 25;

	public static int BridgeMaxHeight = 100;

	public static int AICount = 3;

	public static int rotation = 0;

	public static int pickedLineIndex = 0;

	public static int pickedLevelIndex = 0;

	public static int MaxNewNpcID;

	public static int MaxNewMonsterID;

	public static int MaxNewItemID;

	public static int MaxNewDoodadID;

	public static int ServerID;

	public static int PassWordState;

	public static int MasterId;

	public static int RecordVersion;

	public static float TrueTimeToGameTime = 60f;

	public static float LocalLatitude = 45f;

	public static float m_EquaAngle = 30f;

	public static float m_PeriodTime = 5E+09f;

	public static float WaterPlaneHeight = 97f;

	public static double GameTimeSecond = -1.0;

	public static ulong OwerSteamId;

	public static long ServerUID;

	public static bool PublicServer = false;

	public static bool MonsterYes = true;

	public static bool UnlimitedRes = false;

	public static bool UseProxy = false;

	public static bool IsNewServer = true;

	public static bool UseSkillTree = false;

	public static bool GameStarted = false;

	public static bool mirror = false;

	public static bool ScriptsAvailable = true;

	public static Dictionary<BuildingID, int> CreatedNpcItemBuildingIndex = new Dictionary<BuildingID, int>();

	public static Dictionary<int, List<int>> mAliveBuildings = new Dictionary<int, List<int>>();

	public static List<int> FoundMapLable = new List<int>();

	public static int AllyCount
	{
		get
		{
			return AICount + 1;
		}
		set
		{
			AICount = value - 1;
		}
	}

	public static bool IsAdventure => SceneMode == ESceneMode.Adventure;

	public static bool IsBuild => SceneMode == ESceneMode.Build;

	public static bool IsCustom => SceneMode == ESceneMode.Custom;

	public static bool IsTowerDefense => SceneMode == ESceneMode.TowerDefense;

	public static bool IsStory => SceneMode == ESceneMode.Story;

	public static bool IsVS => GameType == EGameType.VS;

	public static bool IsCooperation => GameType == EGameType.Cooperation;

	public static bool IsSurvive => GameType == EGameType.Survive;

	public static bool IsAdventureCoop => IsAdventure && IsCooperation;

	public static bool IsAdventureVS => IsAdventure && IsVS;

	public static bool IsAdventureSurvive => IsAdventure && IsSurvive;

	public static bool IsBuildCoop => IsBuild && IsCooperation;

	public static bool IsBuildVS => IsBuild && IsVS;

	public static bool IsBuildSurvive => IsBuild && IsSurvive;

	public static bool isCompatible
	{
		get
		{
			if (RecordVersion == 0)
			{
				return true;
			}
			for (int i = 0; i < VersionCompatible.Length; i++)
			{
				if (VersionCompatible[i] == RecordVersion)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static void InitConfig()
	{
		InitFromDefConfig();
		InitFromCmd();
		InitServerLog();
	}

	public static void InitServerConfig(string dir)
	{
		string text = Path.Combine(dir, "ConfigFiles");
		Directory.CreateDirectory(text);
		string configPath = Path.Combine(text, "ServerConfig.conf");
		CreateConfig(configPath);
		string path = Path.Combine(text, "version.txt");
		File.WriteAllText(path, ServerVersion);
	}

	private static void InitFromDefConfig()
	{
		ServerShareDir = Path.Combine(Application.dataPath, "..");
		if (Application.platform == RuntimePlatform.OSXPlayer)
		{
			ServerShareDir = Path.Combine(ServerShareDir, "..");
		}
		ServerConfDir = Path.Combine(ServerShareDir, "ConfigFiles");
		Directory.CreateDirectory(ServerConfDir);
		string text = Path.Combine(ServerConfDir, "ServerConfig.conf");
		if (File.Exists(text))
		{
			ReadConfig(text);
		}
		else
		{
			CreateConfig(text);
		}
		ServerUID = DateTime.UtcNow.Ticks;
		LoadServerVersion();
	}

	private static void LoadServerVersion()
	{
		string path = Path.Combine(ServerConfDir, "version.txt");
		if (File.Exists(path))
		{
			ServerVersion = File.ReadAllText(path);
		}
		else
		{
			File.WriteAllText(path, ServerVersion);
		}
	}

	private static void InitServerLog()
	{
		ServerLogDir = Path.Combine(ServerDir, "Log");
		Directory.CreateDirectory(ServerLogDir);
		LogManager.InitLogManager(ServerLogDir);
		ScenarioPath = Path.Combine(ServerDir, "Scenario");
		Directory.CreateDirectory(ScenarioPath);
		WorldPath = Path.Combine(ServerDir, "Worlds");
		Directory.CreateDirectory(WorldPath);
	}

	private static void InitFromConfig()
	{
		if (!string.IsNullOrEmpty(ServerName))
		{
			string path = Path.Combine(ServerShareDir, "ServerData");
			if (IsCustom)
			{
				ServerGameDir = Path.Combine(path, "CustomGames");
			}
			else
			{
				ServerGameDir = Path.Combine(path, "CommonGames");
			}
			ServerDir = Path.Combine(ServerGameDir, ServerName);
			Directory.CreateDirectory(ServerDir);
			string text = Path.Combine(ServerDir, "config.json");
			if (!File.Exists(text))
			{
				SaveConfig(text);
			}
			else
			{
				LoadConfig(text);
			}
		}
	}

	private static void InitFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (LogFilter.logDebug)
		{
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				Debug.Log(commandLineArgs[i]);
			}
		}
		int cmdIndex = GetCmdIndex(commandLineArgs, "startwithargs");
		if (cmdIndex == -1 || commandLineArgs.Length <= cmdIndex)
		{
			InitFromConfig();
		}
		else
		{
			string text = commandLineArgs[++cmdIndex];
			string[] array = text.Split('#');
			if (LogFilter.logDebug)
			{
				for (int j = 0; j < array.Length; j++)
				{
					Debug.Log(array[j]);
				}
			}
			ServerName = MyServer.RecoverStr(array[0]);
			SceneMode = (ESceneMode)int.Parse(array[1]);
			OwerSteamId = ulong.Parse(array[2]);
			InitFromConfig();
		}
		ServerPort = NetworkUtility.FindAvailablePort(9900, 9915);
		if (!NetworkUtility.IsPortAvailable(ServerPort))
		{
			throw new Exception("Invalid port.");
		}
	}

	private static int GetCmdIndex(string[] cmds, string cmd)
	{
		for (int i = 0; i < cmds.Length; i++)
		{
			if (!string.IsNullOrEmpty(cmd) && !string.IsNullOrEmpty(cmds[i]) && cmds[i].Equals(cmd))
			{
				return i;
			}
		}
		return -1;
	}

	private static void SaveConfig(string configFile)
	{
		MyServer myServer = new MyServer();
		myServer.gameName = ServerName;
		myServer.masterRoleName = MasterRoleName;
		myServer.gamePassword = Password;
		myServer.mapName = MapName;
		myServer.gameMode = (int)SceneMode;
		myServer.gameType = (int)GameType;
		myServer.seedStr = MapSeed;
		myServer.teamNum = TeamNum;
		myServer.numPerTeam = NumPerTeam;
		myServer.terrainType = (int)TerrainType;
		myServer.vegetationId = (int)VegetationType;
		myServer.sceneClimate = (int)ClimateType;
		myServer.monsterYes = MonsterYes;
		myServer.isPrivate = !PublicServer;
		myServer.proxyServer = UseProxy;
		myServer.unlimitedRes = UnlimitedRes;
		myServer.terrainHeight = TerrainHeight;
		myServer.mapSize = MapSize;
		myServer.riverDensity = RiverDensity;
		myServer.riverWidth = RiverWidth;
		myServer.plainHeight = PlainHeight;
		myServer.flatness = Flatness;
		myServer.bridgeMaxHeight = BridgeMaxHeight;
		myServer.dropDeadPercent = DropDeadPercent;
		myServer.useSkillTree = UseSkillTree;
		myServer.uid = UID;
		myServer.allyCount = AllyCount;
		myServer.scriptsAvailable = ScriptsAvailable;
		myServer.Create(configFile);
	}

	public static void LoadConfig(string configFile)
	{
		string jsonText = File.ReadAllText(configFile, Encoding.UTF8);
		JsonReader jsonReader = new JsonReader(jsonText);
		try
		{
			JsonToken jsonToken = JsonToken.None;
			if ((jsonToken = jsonReader.Read(out var value)) != JsonToken.ObjectStart)
			{
				return;
			}
			while ((jsonToken = jsonReader.Read(out value)) != JsonToken.ObjectEnd)
			{
				if (jsonToken == JsonToken.PropertyName)
				{
					if (value.Equals("ServerName"))
					{
						ServerName = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("MasterRoleName"))
					{
						MasterRoleName = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("Password"))
					{
						Password = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("MapName"))
					{
						MapName = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("GameMode"))
					{
						SceneMode = (ESceneMode)Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("GameType"))
					{
						GameType = (EGameType)Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("MapSeed"))
					{
						MapSeed = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("TeamNum"))
					{
						TeamNum = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("NumPerTeam"))
					{
						NumPerTeam = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("TerrainType"))
					{
						TerrainType = (RandomMapType)Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("VegetationType"))
					{
						VegetationType = (RandomMapType)Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("ClimateType"))
					{
						ClimateType = (EClimateType)Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("MonsterYes"))
					{
						MonsterYes = Json.ReadObject<bool>(jsonReader);
					}
					else if (value.Equals("IsPrivate"))
					{
						bool flag = Json.ReadObject<bool>(jsonReader);
						PublicServer = !flag;
					}
					else if (value.Equals("ProxyServer"))
					{
						UseProxy = Json.ReadObject<bool>(jsonReader);
					}
					else if (value.Equals("UnlimitedRes"))
					{
						UnlimitedRes = Json.ReadObject<bool>(jsonReader);
					}
					else if (value.Equals("TerrainHeight"))
					{
						TerrainHeight = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("MapSize"))
					{
						MapSize = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("RiverDensity"))
					{
						RiverDensity = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("RiverWidth"))
					{
						RiverWidth = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("PlainHeight"))
					{
						PlainHeight = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("Flatness"))
					{
						Flatness = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("BridgeMaxHeight"))
					{
						BridgeMaxHeight = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("UseSkillTree"))
					{
						UseSkillTree = Json.ReadObject<bool>(jsonReader);
					}
					else if (value.Equals("DropDeadPercent"))
					{
						DropDeadPercent = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("UID"))
					{
						UID = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("AICount"))
					{
						AICount = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("ScriptsAvailable"))
					{
						ScriptsAvailable = Json.ReadObject<bool>(jsonReader);
					}
				}
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			jsonReader.Close();
		}
	}

	public static int GenerateMapSeed(string strSeed)
	{
		int num = 0;
		char[] array = strSeed.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			num += (i + 1) * array[i];
		}
		return num;
	}

	public static void SetMasterId(string roleName, int id)
	{
		if (!string.IsNullOrEmpty(MasterRoleName) && string.Equals(roleName, MasterRoleName))
		{
			MasterId = id;
		}
	}

	private static void PrepareData()
	{
		NumPerTeam = Mathf.Clamp(NumPerTeam, 1, 32);
		MapSize = Mathf.Clamp(MapSize, 0, 4);
		switch (GameType)
		{
		case EGameType.Cooperation:
			TeamNum = 1;
			MaxConnections = NumPerTeam;
			break;
		case EGameType.Survive:
			TeamNum = 0;
			MaxConnections = NumPerTeam;
			break;
		case EGameType.VS:
		{
			TeamNum = Mathf.Clamp(TeamNum, 2, 4);
			NumPerTeam = Mathf.Clamp(NumPerTeam, 1, 16);
			int num = Mathf.Min(TeamNum * NumPerTeam, 32);
			int num2 = num / TeamNum;
			NumPerTeam = (int)Mathf.Pow(2f, (int)Mathf.Log(num2, 2f));
			MaxConnections = TeamNum * NumPerTeam;
			break;
		}
		}
		switch (SceneMode)
		{
		case ESceneMode.Custom:
			if (!CustomGameData.Mgr.Load(ServerDir))
			{
				throw new Exception("Load custom data error.");
			}
			GameType = EGameType.Survive;
			MaxConnections = GroupNetwork.InitTeam();
			TeamNum = 0;
			NumPerTeam = 0;
			MapSize = 0;
			ScriptsAvailable = false;
			break;
		case ESceneMode.Story:
			GameType = EGameType.Cooperation;
			TeamNum = 1;
			MaxConnections = NumPerTeam;
			MapSize = 0;
			MonsterYes = true;
			ScriptsAvailable = false;
			break;
		case ESceneMode.Build:
			ScriptsAvailable = false;
			break;
		}
		PassWordState = ((!string.IsNullOrEmpty(Password)) ? 1 : 0);
		GroupNetwork.InitTeam(TeamNum);
		WeatherConfig.SetClimateType(ClimateType);
		AreaHelper.SetMapParam();
	}

	public static void SyncSave()
	{
		MaxNewNpcID = IdGenerator.CurNpcId;
		MaxNewMonsterID = IdGenerator.CurMonsterId;
		MaxNewItemID = IdGenerator.CurItemId;
		MaxNewDoodadID = IdGenerator.CurDoodadId;
		CreatedNpcItemBuildingIndex.Clear();
		foreach (KeyValuePair<BuildingID, int> generatedBuilding in BuildingInfoManager.Instance.GeneratedBuildings)
		{
			CreatedNpcItemBuildingIndex[generatedBuilding.Key] = generatedBuilding.Value;
		}
		mAliveBuildings.Clear();
		foreach (KeyValuePair<int, List<int>> mAliveBuilding in BuildingInfoManager.Instance.mAliveBuildings)
		{
			mAliveBuildings[mAliveBuilding.Key] = mAliveBuilding.Value;
		}
		ConfigDbData configDbData = new ConfigDbData();
		configDbData.ExportData();
		AsyncSqlite.AddRecord(configDbData);
	}

	private static void LoadComplete(SqliteDataReader reader)
	{
		if (!reader.Read())
		{
			return;
		}
		RecordVersion = reader.GetInt32(reader.GetOrdinal("ver"));
		if (LogFilter.logFatal)
		{
			Debug.LogFormat("Current record version:{0}", RecordVersion);
		}
		ServerName = reader.GetString(reader.GetOrdinal("servername"));
		SceneMode = (ESceneMode)reader.GetInt32(reader.GetOrdinal("mode"));
		GameType = (EGameType)reader.GetInt32(reader.GetOrdinal("type"));
		MapSeed = reader.GetString(reader.GetOrdinal("seed"));
		MaxNewNpcID = reader.GetInt32(reader.GetOrdinal("maxnpcid"));
		MaxNewMonsterID = reader.GetInt32(reader.GetOrdinal("maxmonsterid"));
		MaxNewItemID = reader.GetInt32(reader.GetOrdinal("maxitemid"));
		MaxNewDoodadID = reader.GetInt32(reader.GetOrdinal("maxdoodadid"));
		int @int = reader.GetInt32(reader.GetOrdinal("curteamid"));
		TerrainType = (RandomMapType)reader.GetInt32(reader.GetOrdinal("terraintype"));
		VegetationType = (RandomMapType)reader.GetInt32(reader.GetOrdinal("vegetationtype"));
		ClimateType = (EClimateType)reader.GetInt32(reader.GetOrdinal("scenceclimate"));
		ServerUID = reader.GetInt64(reader.GetOrdinal("serveruid"));
		TerrainHeight = reader.GetInt32(reader.GetOrdinal("terrainheight"));
		MapSize = reader.GetInt32(reader.GetOrdinal("mapsize"));
		RiverDensity = reader.GetInt32(reader.GetOrdinal("riverdensity"));
		RiverWidth = reader.GetInt32(reader.GetOrdinal("riverwidth"));
		PlainHeight = reader.GetInt32(reader.GetOrdinal("plainheight"));
		Flatness = reader.GetInt32(reader.GetOrdinal("flatness"));
		BridgeMaxHeight = reader.GetInt32(reader.GetOrdinal("bridgemaxheight"));
		GameStarted = reader.GetBoolean(reader.GetOrdinal("gamestarted"));
		IdGenerator.InitCurId(MaxNewNpcID, MaxNewMonsterID, MaxNewItemID, MaxNewDoodadID);
		WeatherConfig.SetClimateType(ClimateType);
		GroupNetwork.SetCurTeamId(@int);
		byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("blobdata"));
		using (MemoryStream input = new MemoryStream(buffer))
		{
			using BinaryReader reader2 = new BinaryReader(input);
			int num = BufferHelper.ReadInt32(reader2);
			for (int i = 0; i < num; i++)
			{
				int townId = BufferHelper.ReadInt32(reader2);
				int buildingNo = BufferHelper.ReadInt32(reader2);
				CreatedNpcItemBuildingIndex.Add(new BuildingID(townId, buildingNo), 0);
				BuildingInfoManager.Instance.GeneratedBuildings.Add(new BuildingID(townId, buildingNo), 0);
			}
			num = BufferHelper.ReadInt32(reader2);
			for (int j = 0; j < num; j++)
			{
				int num2 = BufferHelper.ReadInt32(reader2);
				mAliveBuildings.Add(num2, new List<int>());
				BuildingInfoManager.Instance.mAliveBuildings.Add(num2, new List<int>());
				int num3 = BufferHelper.ReadInt32(reader2);
				for (int k = 0; k < num3; k++)
				{
					BuildingInfoManager.Instance.AddAliveBuilding(num2, BufferHelper.ReadInt32(reader2));
				}
			}
			GameTimeSecond = BufferHelper.ReadDouble(reader2);
			if (IsStory)
			{
				int num4 = BufferHelper.ReadInt32(reader2);
				for (int l = 0; l < num4; l++)
				{
					AddFoundMapLable(BufferHelper.ReadInt32(reader2));
				}
			}
			VersionMgr.MAINTAIN_VER = (EConstVersion)BufferHelper.ReadInt32(reader2);
			MoneyType = (EMoneyType)BufferHelper.ReadInt32(reader2);
			DropDeadPercent = BufferHelper.ReadInt32(reader2);
			UID = BufferHelper.ReadString(reader2);
			if (!IsStory && VersionMgr.MAINTAIN_VER >= EConstVersion.VER_2016072900)
			{
				mirror = BufferHelper.ReadBoolean(reader2);
				rotation = BufferHelper.ReadInt32(reader2);
				pickedLineIndex = BufferHelper.ReadInt32(reader2);
				pickedLevelIndex = BufferHelper.ReadInt32(reader2);
				AllyCount = BufferHelper.ReadInt32(reader2);
			}
			if (VersionMgr.MAINTAIN_VER > EConstVersion.VER_2016101800)
			{
				num = BufferHelper.ReadInt32(reader2);
				if (num > 0)
				{
					int[] array = new int[num];
					Vector3[] array2 = new Vector3[num];
					for (int m = 0; m < num; m++)
					{
						array[m] = BufferHelper.ReadInt32(reader2);
						array2[m] = default(Vector3);
						BufferHelper.ReadVector3(reader2, out array2[m]);
					}
					BuildingInfoManager.Instance.InitAllTownPos(array, array2);
				}
			}
		}
		IsNewServer = false;
	}

	public static void InitTownAreaPara()
	{
		System.Random random = new System.Random(GenerateMapSeed(MapSeed));
		mirror = random.NextDouble() >= 0.5;
		rotation = random.Next(4);
		pickedLineIndex = random.Next(TownGenData.GenerationLine.Length);
		pickedLevelIndex = random.Next(TownGenData.AreaLevel.Length);
	}

	public static bool AddFoundMapLable(int lableId)
	{
		if (!FoundMapLable.Contains(lableId))
		{
			FoundMapLable.Add(lableId);
			return true;
		}
		return false;
	}

	public static void InitFromDB()
	{
		PrepareMoneyType();
		IsNewServer = true;
		LoadConfig();
		if (LogFilter.logFatal)
		{
			Debug.LogFormat("Current server version:{0}", 273);
		}
		if (!IsNewServer && !isCompatible && LogFilter.logError)
		{
			Debug.LogErrorFormat("Incompatible version:[{0}] with [{1}]", 273, RecordVersion);
		}
		if (IsNewServer)
		{
			SyncSave();
		}
		PrepareData();
	}

	private static void PrepareMoneyType()
	{
		if (SceneMode == ESceneMode.Story)
		{
			MoneyType = EMoneyType.Meat;
		}
	}

	private static void LoadConfig()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM serverinfo WHERE servername=@servername;");
			pEDbOp.BindParam("@servername", ServerName);
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private static void ReadConfig(string configPath)
	{
		string jsonText = File.ReadAllText(configPath);
		JsonReader jsonReader = new JsonReader(jsonText);
		try
		{
			JsonToken jsonToken = JsonToken.None;
			if ((jsonToken = jsonReader.Read(out var value)) != JsonToken.ObjectStart)
			{
				return;
			}
			while ((jsonToken = jsonReader.Read(out value)) != JsonToken.ObjectEnd)
			{
				if (jsonToken != JsonToken.PropertyName)
				{
					continue;
				}
				if (value.Equals("LobbyIP"))
				{
					LobbyIP = Json.ReadObject<string>(jsonReader);
					continue;
				}
				if (value.Equals("LobbyPort"))
				{
					LobbyPort = Json.ReadObject<int>(jsonReader);
					continue;
				}
				if (value.Equals("ProxyIP"))
				{
					ProxyIP = Json.ReadObject<string>(jsonReader);
					continue;
				}
				if (value.Equals("ProxyPort"))
				{
					ProxyPort = Json.ReadObject<int>(jsonReader);
					continue;
				}
				if (value.Equals("TeamNum"))
				{
					TeamNum = Json.ReadObject<int>(jsonReader);
					continue;
				}
				if (value.Equals("NumPerTeam"))
				{
					NumPerTeam = Json.ReadObject<int>(jsonReader);
					continue;
				}
				if (value.Equals("GameMode"))
				{
					int sceneMode = Json.ReadObject<int>(jsonReader);
					SceneMode = (ESceneMode)sceneMode;
					continue;
				}
				if (value.Equals("GameType"))
				{
					int gameType = Json.ReadObject<int>(jsonReader);
					GameType = (EGameType)gameType;
					continue;
				}
				if (value.Equals("MonsterYes"))
				{
					MonsterYes = Json.ReadObject<bool>(jsonReader);
					continue;
				}
				if (value.Equals("ServerName"))
				{
					ServerName = Json.ReadObject<string>(jsonReader);
					continue;
				}
				if (value.Equals("MapName"))
				{
					MapName = Json.ReadObject<string>(jsonReader);
				}
				if (value.Equals("MapSeed"))
				{
					MapSeed = Json.ReadObject<string>(jsonReader);
				}
				else if (value.Equals("TerrainType"))
				{
					TerrainType = (RandomMapType)Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("ClimateType"))
				{
					ClimateType = (EClimateType)Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("VegetationType"))
				{
					VegetationType = (RandomMapType)Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("MasterRoleName"))
				{
					MasterRoleName = Json.ReadObject<string>(jsonReader);
				}
				else if (value.Equals("Password"))
				{
					Password = Json.ReadObject<string>(jsonReader);
				}
				else if (value.Equals("PublicServer"))
				{
					PublicServer = Json.ReadObject<bool>(jsonReader);
				}
				else if (value.Equals("Proxy"))
				{
					UseProxy = Json.ReadObject<bool>(jsonReader);
				}
				else if (value.Equals("UseSkillTree"))
				{
					UseSkillTree = Json.ReadObject<bool>(jsonReader);
				}
				else if (value.Equals("DropDeadPercent"))
				{
					DropDeadPercent = Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("UnlimitedRes"))
				{
					UnlimitedRes = Json.ReadObject<bool>(jsonReader);
				}
				else if (value.Equals("TerrainHeight"))
				{
					TerrainHeight = Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("MapSize"))
				{
					MapSize = Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("RiverDensity"))
				{
					RiverDensity = Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("RiverWidth"))
				{
					RiverWidth = Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("PlainHeight"))
				{
					PlainHeight = Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("Flatness"))
				{
					Flatness = Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("BridgeMaxHeight"))
				{
					BridgeMaxHeight = Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("AICount"))
				{
					AICount = Json.ReadObject<int>(jsonReader);
				}
				else if (value.Equals("ScriptsAvailable"))
				{
					ScriptsAvailable = Json.ReadObject<bool>(jsonReader);
				}
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			jsonReader.Close();
		}
	}

	private static void CreateConfig(string configPath)
	{
		JsonWriter jsonWriter = new JsonWriter(validate: true, prettyPrint: true, 8u);
		jsonWriter.WriteObjectStart();
		jsonWriter.WritePropertyName("LobbyIP");
		Json.WriteObject(LobbyIP, jsonWriter);
		jsonWriter.WritePropertyName("LobbyPort");
		Json.WriteObject(LobbyPort, jsonWriter);
		jsonWriter.WritePropertyName("ProxyIP");
		Json.WriteObject(ProxyIP, jsonWriter);
		jsonWriter.WritePropertyName("ProxyPort");
		Json.WriteObject(ProxyPort, jsonWriter);
		jsonWriter.WritePropertyName("TeamNum");
		Json.WriteObject(TeamNum, jsonWriter);
		jsonWriter.WritePropertyName("NumPerTeam");
		Json.WriteObject(NumPerTeam, jsonWriter);
		jsonWriter.WritePropertyName("GameMode");
		Json.WriteObject((int)SceneMode, jsonWriter);
		jsonWriter.WritePropertyName("GameType");
		Json.WriteObject((int)GameType, jsonWriter);
		jsonWriter.WritePropertyName("MonsterYes");
		Json.WriteObject(MonsterYes, jsonWriter);
		jsonWriter.WritePropertyName("ServerName");
		Json.WriteObject(ServerName, jsonWriter);
		jsonWriter.WritePropertyName("MapName");
		Json.WriteObject(MapName, jsonWriter);
		jsonWriter.WritePropertyName("MapSeed");
		Json.WriteObject(MapSeed, jsonWriter);
		jsonWriter.WritePropertyName("TerrainType");
		Json.WriteObject((int)TerrainType, jsonWriter);
		jsonWriter.WritePropertyName("ClimateType");
		Json.WriteObject((int)ClimateType, jsonWriter);
		jsonWriter.WritePropertyName("VegetationType");
		Json.WriteObject((int)VegetationType, jsonWriter);
		jsonWriter.WritePropertyName("MasterRoleName");
		Json.WriteObject(MasterRoleName, jsonWriter);
		jsonWriter.WritePropertyName("Password");
		Json.WriteObject(Password, jsonWriter);
		jsonWriter.WritePropertyName("PublicServer");
		Json.WriteObject(PublicServer, jsonWriter);
		jsonWriter.WritePropertyName("Proxy");
		Json.WriteObject(UseProxy, jsonWriter);
		jsonWriter.WritePropertyName("UseSkillTree");
		Json.WriteObject(UseSkillTree, jsonWriter);
		jsonWriter.WritePropertyName("DropDeadPercent");
		Json.WriteObject(DropDeadPercent, jsonWriter);
		jsonWriter.WritePropertyName("UnlimitedRes");
		Json.WriteObject(UnlimitedRes, jsonWriter);
		jsonWriter.WritePropertyName("TerrainHeight");
		Json.WriteObject(TerrainHeight, jsonWriter);
		jsonWriter.WritePropertyName("MapSize");
		Json.WriteObject(MapSize, jsonWriter);
		jsonWriter.WritePropertyName("RiverDensity");
		Json.WriteObject(RiverDensity, jsonWriter);
		jsonWriter.WritePropertyName("RiverWidth");
		Json.WriteObject(RiverWidth, jsonWriter);
		jsonWriter.WritePropertyName("PlainHeight");
		Json.WriteObject(PlainHeight, jsonWriter);
		jsonWriter.WritePropertyName("Flatness");
		Json.WriteObject(Flatness, jsonWriter);
		jsonWriter.WritePropertyName("BridgeMaxHeight");
		Json.WriteObject(BridgeMaxHeight, jsonWriter);
		jsonWriter.WritePropertyName("AICount");
		Json.WriteObject(AICount, jsonWriter);
		jsonWriter.WritePropertyName("ScriptsAvailable");
		Json.WriteObject(ScriptsAvailable, jsonWriter);
		jsonWriter.WriteObjectEnd();
		File.WriteAllText(configPath, jsonWriter.ToString());
	}
}
