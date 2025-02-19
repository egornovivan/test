using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
public class PlantGradientDesc
{
	public float startTan;

	public float endTan;

	[XmlAttribute("start")]
	public int start { get; set; }

	[XmlAttribute("end")]
	public int end { get; set; }

	[XmlElement("Plant")]
	public PlantDescCLS[] PlantDescArrayValues { get; set; }
}
