using System;
using System.Collections.Generic;
using System.IO;
using Jboy;
using Pathea;
using UnityEngine;

public class MyServer
{
	public string gameName;

	public string masterRoleName;

	public string gamePassword;

	public string mapName;

	public int gameMode;

	public int gameType;

	public string seedStr;

	public int teamNum;

	public int numPerTeam;

	public int terrainType;

	public int vegetationId;

	public int sceneClimate;

	public bool monsterYes;

	public bool isPrivate;

	public bool useSkillTree;

	public bool scriptsAvailable;

	public bool unlimitedRes;

	public int mapSize;

	public int riverDensity;

	public int riverWidth;

	public int terrainHeight;

	public bool proxyServer;

	public int dropDeadPercent;

	public string uid;

	public int plainHeight;

	public int flatness;

	public int bridgeMaxHeight;

	public int AICount;

	public int allyCount
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

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		MyServer myServer = obj as MyServer;
		if (myServer.gameName == gameName)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return gameName.GetHashCode();
	}

	public void Read(string configFile)
	{
		string jsonText = File.ReadAllText(configFile);
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
						gameName = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("MasterRoleName"))
					{
						masterRoleName = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("Password"))
					{
						gamePassword = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("MapName"))
					{
						mapName = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("GameMode"))
					{
						gameMode = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("GameType"))
					{
						gameType = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("MapSeed"))
					{
						seedStr = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("TeamNum"))
					{
						teamNum = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("NumPerTeam"))
					{
						numPerTeam = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("TerrainType"))
					{
						terrainType = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("VegetationType"))
					{
						vegetationId = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("ClimateType"))
					{
						sceneClimate = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("MonsterYes"))
					{
						monsterYes = Json.ReadObject<bool>(jsonReader);
					}
					else if (value.Equals("IsPrivate"))
					{
						isPrivate = Json.ReadObject<bool>(jsonReader);
					}
					else if (value.Equals("ProxyServer"))
					{
						proxyServer = Json.ReadObject<bool>(jsonReader);
					}
					else if (value.Equals("UnlimitedRes"))
					{
						unlimitedRes = Json.ReadObject<bool>(jsonReader);
					}
					else if (value.Equals("TerrainHeight"))
					{
						terrainHeight = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("MapSize"))
					{
						mapSize = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("RiverDensity"))
					{
						riverDensity = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("RiverWidth"))
					{
						riverWidth = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("PlainHeight"))
					{
						plainHeight = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("Flatness"))
					{
						flatness = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("BridgeMaxHeight"))
					{
						bridgeMaxHeight = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("AICount"))
					{
						AICount = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("UseSkillTree"))
					{
						useSkillTree = Json.ReadObject<bool>(jsonReader);
					}
					else if (value.Equals("DropDeadPercent"))
					{
						dropDeadPercent = Json.ReadObject<int>(jsonReader);
					}
					else if (value.Equals("UID"))
					{
						uid = Json.ReadObject<string>(jsonReader);
					}
					else if (value.Equals("ScriptsAvailable"))
					{
						scriptsAvailable = Json.ReadObject<bool>(jsonReader);
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("Read config file failed.\r\n{0}\r\n{1}\r\n{2}", configFile, ex.Message, ex.StackTrace);
			}
		}
		finally
		{
			jsonReader.Close();
		}
	}

	public void Create(string configFile)
	{
		JsonWriter jsonWriter = new JsonWriter(validate: true, prettyPrint: true, 8u);
		jsonWriter.WriteObjectStart();
		jsonWriter.WritePropertyName("ServerName");
		Json.WriteObject(gameName, jsonWriter);
		jsonWriter.WritePropertyName("MasterRoleName");
		Json.WriteObject(masterRoleName, jsonWriter);
		jsonWriter.WritePropertyName("Password");
		Json.WriteObject(gamePassword, jsonWriter);
		jsonWriter.WritePropertyName("MapName");
		Json.WriteObject(mapName, jsonWriter);
		jsonWriter.WritePropertyName("GameMode");
		Json.WriteObject(gameMode, jsonWriter);
		jsonWriter.WritePropertyName("GameType");
		Json.WriteObject(gameType, jsonWriter);
		jsonWriter.WritePropertyName("MapSeed");
		Json.WriteObject(seedStr, jsonWriter);
		jsonWriter.WritePropertyName("TeamNum");
		Json.WriteObject(teamNum, jsonWriter);
		jsonWriter.WritePropertyName("NumPerTeam");
		Json.WriteObject(numPerTeam, jsonWriter);
		jsonWriter.WritePropertyName("TerrainType");
		Json.WriteObject(terrainType, jsonWriter);
		jsonWriter.WritePropertyName("VegetationType");
		Json.WriteObject(vegetationId, jsonWriter);
		jsonWriter.WritePropertyName("ClimateType");
		Json.WriteObject(sceneClimate, jsonWriter);
		jsonWriter.WritePropertyName("MonsterYes");
		Json.WriteObject(monsterYes, jsonWriter);
		jsonWriter.WritePropertyName("IsPrivate");
		Json.WriteObject(isPrivate, jsonWriter);
		jsonWriter.WritePropertyName("ProxyServer");
		Json.WriteObject(proxyServer, jsonWriter);
		jsonWriter.WritePropertyName("UnlimitedRes");
		Json.WriteObject(unlimitedRes, jsonWriter);
		jsonWriter.WritePropertyName("TerrainHeight");
		Json.WriteObject(terrainHeight, jsonWriter);
		jsonWriter.WritePropertyName("MapSize");
		Json.WriteObject(mapSize, jsonWriter);
		jsonWriter.WritePropertyName("RiverDensity");
		Json.WriteObject(riverDensity, jsonWriter);
		jsonWriter.WritePropertyName("RiverWidth");
		Json.WriteObject(riverWidth, jsonWriter);
		jsonWriter.WritePropertyName("PlainHeight");
		Json.WriteObject(plainHeight, jsonWriter);
		jsonWriter.WritePropertyName("Flatness");
		Json.WriteObject(flatness, jsonWriter);
		jsonWriter.WritePropertyName("BridgeMaxHeight");
		Json.WriteObject(bridgeMaxHeight, jsonWriter);
		jsonWriter.WritePropertyName("AICount");
		Json.WriteObject(AICount, jsonWriter);
		jsonWriter.WritePropertyName("UseSkillTree");
		Json.WriteObject(useSkillTree, jsonWriter);
		jsonWriter.WritePropertyName("DropDeadPercent");
		Json.WriteObject(dropDeadPercent, jsonWriter);
		jsonWriter.WritePropertyName("UID");
		Json.WriteObject(uid, jsonWriter);
		jsonWriter.WritePropertyName("ScriptsAvailable");
		Json.WriteObject(scriptsAvailable, jsonWriter);
		jsonWriter.WriteObjectEnd();
		File.WriteAllText(configFile, jsonWriter.ToString());
	}

	public MyServer ReplaceStrForTrans()
	{
		MyServer myServer = new MyServer();
		myServer.gameName = ReplaceStr(gameName);
		myServer.masterRoleName = ReplaceStr(masterRoleName);
		myServer.gamePassword = ReplaceStr(gamePassword);
		myServer.mapName = ReplaceStr(mapName);
		myServer.seedStr = ReplaceStr(seedStr);
		myServer.gameMode = gameMode;
		myServer.gameType = gameType;
		myServer.teamNum = teamNum;
		myServer.numPerTeam = numPerTeam;
		myServer.terrainType = terrainType;
		myServer.vegetationId = vegetationId;
		myServer.sceneClimate = sceneClimate;
		myServer.monsterYes = monsterYes;
		myServer.isPrivate = isPrivate;
		myServer.unlimitedRes = unlimitedRes;
		myServer.terrainHeight = terrainHeight;
		myServer.mapSize = mapSize;
		myServer.riverDensity = riverDensity;
		myServer.riverWidth = riverWidth;
		myServer.plainHeight = plainHeight;
		myServer.flatness = flatness;
		myServer.bridgeMaxHeight = bridgeMaxHeight;
		myServer.allyCount = allyCount;
		myServer.useSkillTree = useSkillTree;
		myServer.dropDeadPercent = dropDeadPercent;
		myServer.scriptsAvailable = scriptsAvailable;
		return myServer;
	}

	public static string RecoverStr(string original)
	{
		if (string.IsNullOrEmpty(original))
		{
			return string.Empty;
		}
		if (original.Contains("@++@"))
		{
			original = original.Replace("@++@", " ");
		}
		if (original.Contains("@--@"))
		{
			original = original.Replace("@--@", ",");
		}
		if (original.Contains("@=@"))
		{
			original = original.Replace("@=@", "`");
		}
		if (original.Contains("@+@"))
		{
			original = original.Replace("@+@", "|");
		}
		if (original.Contains("@-@"))
		{
			original = original.Replace("@-@", "#");
		}
		return original;
	}

	public static string ReplaceStr(string original)
	{
		if (original == string.Empty || original == null)
		{
			return original;
		}
		if (original.Contains(" "))
		{
			original = original.Replace(" ", "@++@");
		}
		if (original.Contains(","))
		{
			original = original.Replace(",", "@--@");
		}
		if (original.Contains("`"))
		{
			original = original.Replace("`", "@=@");
		}
		if (original.Contains("|"))
		{
			original = original.Replace("|", "@+@");
		}
		if (original.Contains("#"))
		{
			original = original.Replace("#", "@-@");
		}
		return original;
	}

	public List<string> ToServerDataItem()
	{
		List<string> list = new List<string>();
		list.Add(gameName);
		list.Add(((PeGameMgr.EGameType)gameType).ToString());
		list.Add(AdventureOrBuild());
		if (gameType == 2)
		{
			list.Add(numPerTeam.ToString());
		}
		else
		{
			list.Add((numPerTeam * teamNum).ToString());
		}
		list.Add(teamNum.ToString());
		list.Add(seedStr);
		return list;
	}

	public List<string> ToServerInfo()
	{
		List<string> list = new List<string>();
		list.Add(gameName);
		list.Add(gamePassword);
		list.Add(((PeGameMgr.EGameType)gameType).ToString());
		list.Add(AdventureOrBuild());
		list.Add(MonsterOrNot());
		list.Add(mapName);
		list.Add(teamNum.ToString());
		list.Add(numPerTeam.ToString());
		list.Add(seedStr);
		list.Add(MajorBiomaString());
		list.Add(ClimateTypeString());
		return list;
	}

	public string AdventureOrBuild()
	{
		return (PeGameMgr.ESceneMode)gameMode switch
		{
			PeGameMgr.ESceneMode.Adventure => "Adventure", 
			PeGameMgr.ESceneMode.Build => "Build", 
			PeGameMgr.ESceneMode.Custom => "Custom", 
			PeGameMgr.ESceneMode.Story => "Story", 
			_ => "Adventure", 
		};
	}

	public static PeGameMgr.ESceneMode AdventureOrBuild(string mode)
	{
		if (string.IsNullOrEmpty(mode))
		{
			return PeGameMgr.ESceneMode.Adventure;
		}
		if (mode.Equals("Adventure"))
		{
			return PeGameMgr.ESceneMode.Adventure;
		}
		if (mode.Equals("Build"))
		{
			return PeGameMgr.ESceneMode.Build;
		}
		if (mode.Equals("Custom"))
		{
			return PeGameMgr.ESceneMode.Custom;
		}
		if (mode.Equals("Story"))
		{
			return PeGameMgr.ESceneMode.Story;
		}
		return PeGameMgr.ESceneMode.Adventure;
	}

	public string MonsterOrNot()
	{
		if (monsterYes)
		{
			return "Yes";
		}
		return "No";
	}

	public string MajorBiomaString()
	{
		return terrainType switch
		{
			1 => "GrassLand", 
			2 => "Forest", 
			3 => "Desert", 
			4 => "Redstone", 
			5 => "Rainforest", 
			_ => "Grassland", 
		};
	}

	public string ClimateTypeString()
	{
		return sceneClimate switch
		{
			0 => "Dry", 
			1 => "Temperate", 
			2 => "Wet", 
			_ => "Dry", 
		};
	}
}
