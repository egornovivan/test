using System;
using System.Xml.Serialization;

namespace VArtifactTownXML;

[Serializable]
[XmlRoot("VArtifactTownDesc")]
public class VArtifactTownDesc
{
	[XmlAttribute("townsize")]
	public int townsize { get; set; }

	[XmlAttribute("DistanceX")]
	public int distanceX { get; set; }

	[XmlAttribute("DistanceZ")]
	public int distanceZ { get; set; }

	[XmlElement("VAStartTown")]
	public VATown vaStartTown { get; set; }

	[XmlArrayItem("VATown", typeof(VATown))]
	[XmlArray("VATownArray")]
	public VATown[] vaTown { get; set; }
}
