using System;
using System.Xml.Serialization;

namespace RandomTownXML;

[Serializable]
public class Cell
{
	[XmlAttribute("x")]
	public int x { get; set; }

	[XmlAttribute("z")]
	public int z { get; set; }

	[XmlAttribute("rot")]
	public int rot { get; set; }
}
