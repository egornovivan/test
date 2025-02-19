using System;
using System.Xml.Serialization;

namespace RandomTownXML;

[Serializable]
public class BuildingNum
{
	[XmlAttribute("bid")]
	public int bid { get; set; }

	[XmlAttribute("num")]
	public int num { get; set; }
}
