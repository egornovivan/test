using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
public class HeightDescArrayCLS
{
	[XmlAttribute("start")]
	public int start { get; set; }

	[XmlAttribute("end")]
	public int end { get; set; }

	[XmlArray("GradientDescArray")]
	[XmlArrayItem("GradientDesc", typeof(GradientDescArrayCLS))]
	public GradientDescArrayCLS[] GradientDescArrayValues { get; set; }
}
