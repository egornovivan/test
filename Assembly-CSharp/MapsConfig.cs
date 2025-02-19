using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class MapsConfig
{
	public static MapsConfig Self;

	[XmlArrayItem]
	public List<MapConfig> PatheaMapConfig;

	public static void InitMapConfig()
	{
		string text = GameConfig.PEDataPath + "ConfigFiles";
		Directory.CreateDirectory(text);
		text += "/MapConfig.xml";
		if (File.Exists(text))
		{
			using (FileStream stream = new FileStream(text, FileMode.Open, FileAccess.Read))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(MapsConfig));
				Self = xmlSerializer.Deserialize(stream) as MapsConfig;
				return;
			}
		}
		using FileStream stream2 = new FileStream(text, FileMode.Create, FileAccess.Write);
		XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(MapsConfig));
		Self = new MapsConfig();
		Self.PatheaMapConfig = new List<MapConfig>();
		MapConfig mapConfig = new MapConfig();
		mapConfig.MapName = "PatheaMap";
		mapConfig.MapDescription = "OH, Come On";
		mapConfig.GameType = new string[3] { "Cooperation", "VS", "Survival" };
		mapConfig.GameMode = new string[4] { "Story", "Adventure", "Build", "Custom" };
		mapConfig.MapTeamNum = new string[4] { "1", "2", "3", "4" };
		mapConfig.MapCampBalance = new string[2] { "yes", "no" };
		mapConfig.MapAI = new string[2] { "yes", "no" };
		mapConfig.MapTerrainType = new string[8] { "Grassland", "Forest", "Desert", "Redstone", "Rainforest", "Mountain", "Swamp", "Crater" };
		mapConfig.MapWeatherType = new string[4] { "Random", "Dry", "Temperate", "Wet" };
		Self.PatheaMapConfig.Add(mapConfig);
		xmlSerializer2.Serialize(stream2, Self);
	}
}
