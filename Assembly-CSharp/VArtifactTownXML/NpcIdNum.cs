using System;
using System.Xml.Serialization;

namespace VArtifactTownXML;

[Serializable]
public class NpcIdNum
{
	[XmlAttribute("nid")]
	public int nid { get; set; }

	[XmlAttribute("num")]
	public int num { get; set; }
}
