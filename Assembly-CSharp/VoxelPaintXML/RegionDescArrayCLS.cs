using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
public class RegionDescArrayCLS
{
	[XmlAttribute("color")]
	public string color { get; set; }

	[XmlAttribute("name")]
	public string name { get; set; }

	[XmlAttribute("mineChance")]
	public float mineChance { get; set; }

	[XmlElement("Trees")]
	public PlantDescArrayCLS trees { get; set; }

	[XmlElement("Grasses")]
	public PlantDescArrayCLS grasses { get; set; }

	[XmlElement("NewGrasses")]
	public PlantDescArrayCLS newgrasses { get; set; }

	[XmlArrayItem("HeightDesc", typeof(HeightDescArrayCLS))]
	[XmlArray("HeightDescArray")]
	public HeightDescArrayCLS[] HeightDescArrayValues { get; set; }

	[XmlArrayItem("TerrainDesc", typeof(TerrainDesc))]
	[XmlArray("TerrainDescArray")]
	public TerrainDesc[] TerrainDescArrayValues { get; set; }

	[XmlArray("MineChanceArrayCLS")]
	[XmlArrayItem("MineChance", typeof(MineChanceArrayCLS))]
	public MineChanceArrayCLS[] MineChanceArrayValues { get; set; }
}
