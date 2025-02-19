using System;
using System.IO;
using Jboy;

public class ClientConfig
{
	public static string LobbyIP = "74.81.173.196";

	public static string ProxyIP = "119.28.5.150";

	public static int LobbyPort = 12534;

	public static int ProxyPort = 12535;

	public static void InitClientConfig()
	{
		string text = Path.Combine(GameConfig.PEDataPath, "ConfigFiles");
		Directory.CreateDirectory(text);
		string configFile = Path.Combine(text, "ClientConfig.conf");
		try
		{
			ReadConfig(configFile);
		}
		catch (Exception ex)
		{
			LogManager.Warning("Incompatible config file, default config file created.", ex.Source, ex.StackTrace);
			CreateConfig(configFile);
		}
	}

	private static void ReadConfig(string configFile)
	{
		string jsonText = File.ReadAllText(configFile);
		JsonReader jsonReader = new JsonReader(jsonText);
		jsonReader.ReadObjectStart();
		jsonReader.ReadPropertyName("LobbyIP");
		LobbyIP = Json.ReadObject<string>(jsonReader);
		jsonReader.ReadPropertyName("LobbyPort");
		LobbyPort = Json.ReadObject<int>(jsonReader);
		jsonReader.ReadPropertyName("ProxyIP");
		ProxyIP = Json.ReadObject<string>(jsonReader);
		jsonReader.ReadPropertyName("ProxyPort");
		ProxyPort = Json.ReadObject<int>(jsonReader);
		jsonReader.ReadObjectEnd();
		jsonReader.Close();
	}

	private static void CreateConfig(string configFile)
	{
		JsonWriter jsonWriter = new JsonWriter(validate: true, prettyPrint: true, 2u);
		jsonWriter.WriteObjectStart();
		jsonWriter.WritePropertyName("LobbyIP");
		Json.WriteObject(LobbyIP, jsonWriter);
		jsonWriter.WritePropertyName("LobbyPort");
		Json.WriteObject(LobbyPort, jsonWriter);
		jsonWriter.WritePropertyName("ProxyIP");
		Json.WriteObject(ProxyIP, jsonWriter);
		jsonWriter.WritePropertyName("ProxyPort");
		Json.WriteObject(ProxyPort, jsonWriter);
		jsonWriter.WriteObjectEnd();
		File.WriteAllText(configFile, jsonWriter.ToString());
	}
}
