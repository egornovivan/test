using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
public class VoxelRateArrayCLS
{
	[XmlAttribute("type")]
	public int type { get; set; }

	[XmlAttribute("perc")]
	public int perc { get; set; }
}
