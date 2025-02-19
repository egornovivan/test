using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
public class PlantDescArrayCLS
{
	[XmlAttribute("startFadeIn")]
	public float startFadeIn { get; set; }

	[XmlAttribute("start")]
	public float start { get; set; }

	[XmlAttribute("cellSize")]
	public float cellSize { get; set; }

	[XmlElement("PlantHeightDesc")]
	public PlantHeightDesc[] PlantHeightDescValues { get; set; }
}
