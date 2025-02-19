using System;
using System.Xml.Serialization;

namespace VoxelPaintXML;

[Serializable]
public class TerrainDesc
{
	[XmlAttribute("terChance")]
	public float terChance { get; set; }
}
