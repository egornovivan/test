using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
public class PlantDescCLS
{
	[XmlAttribute("index")]
	public int idx { get; set; }

	[XmlAttribute("pct")]
	public float pct { get; set; }

	[XmlAttribute("wScaleMin")]
	public float wScaleMin { get; set; }

	[XmlAttribute("wScaleMax")]
	public float wScaleMax { get; set; }

	[XmlAttribute("hScaleMin")]
	public float hScaleMin { get; set; }

	[XmlAttribute("hScaleMax")]
	public float hScaleMax { get; set; }
}
