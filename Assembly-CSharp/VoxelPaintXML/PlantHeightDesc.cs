using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
public class PlantHeightDesc
{
	[XmlAttribute("start")]
	public int start { get; set; }

	[XmlAttribute("end")]
	public int end { get; set; }

	[XmlElement("PlantGradientDesc")]
	public PlantGradientDesc[] PlantGradientDescValues { get; set; }
}
