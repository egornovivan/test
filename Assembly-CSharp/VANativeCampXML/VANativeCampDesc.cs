using System;
using System.Xml.Serialization;

namespace VANativeCampXML;

[Serializable]
[XmlRoot("VANativeCampDesc")]
public class VANativeCampDesc
{
	[XmlAttribute("NumMin")]
	public int numMin { get; set; }

	[XmlAttribute("NumMax")]
	public int numMax { get; set; }

	[XmlAttribute("DistanceMin")]
	public int distanceMin { get; set; }

	[XmlAttribute("DistanceMax")]
	public int distanceMax { get; set; }

	[XmlArray("NativeCampArray")]
	[XmlArrayItem("NativeCamp", typeof(NativeCamp))]
	public NativeCamp[] nativeCamps { get; set; }
}
