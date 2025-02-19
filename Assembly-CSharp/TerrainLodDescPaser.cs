using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class TerrainLodDescPaser
{
	[Serializable]
	public class TerrainLodDesc
	{
		[XmlElement("MaxLodLv")]
		public int lod { get; set; }

		[XmlElement("Lod0SizeX")]
		public int x { get; set; }

		[XmlElement("Lod0SizeY")]
		public int y { get; set; }

		[XmlElement("Lod0SizeZ")]
		public int z { get; set; }
	}

	public static TerrainLodDesc terLodDesc;

	public static void LoadTerLodDesc(string xmlPath)
	{
		TextAsset textAsset = Resources.Load(xmlPath, typeof(TextAsset)) as TextAsset;
		StringReader stringReader = new StringReader(textAsset.text);
		if (stringReader != null)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TerrainLodDesc));
			terLodDesc = (TerrainLodDesc)xmlSerializer.Deserialize(stringReader);
			stringReader.Close();
		}
	}
}
