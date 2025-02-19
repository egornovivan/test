using System.IO;
using Jboy;

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
		jsonWriter.TryWritePropertyName("ProxyServer");
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
}
