using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
[XmlRoot("AutoPaintDesc")]
public class rootCLS
{
	[XmlAttribute("regionsMap")]
	public string regionsMap { get; set; }

	[XmlArrayItem("Region", typeof(RegionDescArrayCLS))]
	[XmlArray("RegionArray")]
	public RegionDescArrayCLS[] RegionDescArrayValues { get; set; }
}
