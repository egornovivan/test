using System;
using System.Xml.Serialization;

namespace VArtifactTownXML;

[Serializable]
public class BuildingIdNum
{
	[XmlAttribute("posIndex")]
	public int posIndex = -1;

	[XmlAttribute("bid")]
	public int bid { get; set; }

	[XmlAttribute("num")]
	public int num { get; set; }
}
