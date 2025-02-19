using System;
using System.Xml.Serialization;

namespace RandomTownXML;

[Serializable]
[XmlRoot("RandomTownDsc")]
public class RandomTownDsc
{
	[XmlAttribute("DistanceX")]
	public int distanceX { get; set; }

	[XmlAttribute("DistanceZ")]
	public int distanceZ { get; set; }

	[XmlElement("StartTown")]
	public Town startTown { get; set; }

	[XmlArray("TownArray")]
	[XmlArrayItem("Town", typeof(Town))]
	public Town[] town { get; set; }
}
