using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
public class GradientDescArrayCLS
{
	public float startTan;

	public float endTan;

	[XmlAttribute("start")]
	public int start { get; set; }

	[XmlAttribute("end")]
	public int end { get; set; }

	[XmlArrayItem("VoxelRate", typeof(VoxelRateArrayCLS))]
	[XmlArray("VoxelRateArray")]
	public VoxelRateArrayCLS[] VoxelRateArrayValues { get; set; }
}
