using System.IO;
using System.Xml.Serialization;
using RandomTownXML;
using UnityEngine;

public class RandomTownXMLParser
{
	private static RandomTownXMLParser instance;

	public static RandomTownXMLParser Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new RandomTownXMLParser();
			}
			return instance;
		}
	}

	public static void TestXxmlCreating()
	{
		string text = Application.dataPath + "/TestRandomTownXML";
		Directory.CreateDirectory(text);
		text += "/RandomTown.xml";
		using FileStream stream = new FileStream(text, FileMode.Create, FileAccess.Write);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(RandomTownDsc));
		NpcIdNum[] npcIdNum = new NpcIdNum[2]
		{
			new NpcIdNum(),
			new NpcIdNum()
		};
		Cell[] cell = new Cell[2]
		{
			new Cell(),
			new Cell()
		};
		BuildingNum[] buildingNum = new BuildingNum[2]
		{
			new BuildingNum(),
			new BuildingNum()
		};
		Town town = new Town();
		town.npcIdNum = npcIdNum;
		town.cell = cell;
		town.buildingNum = buildingNum;
		RandomTownDsc randomTownDsc = new RandomTownDsc();
		Town[] town2 = new Town[2] { town, town };
		randomTownDsc.town = town2;
		randomTownDsc.startTown = town;
		xmlSerializer.Serialize(stream, randomTownDsc);
	}
}
