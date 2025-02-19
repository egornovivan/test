using System;
using System.Xml.Serialization;

namespace VArtifactTownXML;

[Serializable]
public class VATown
{
	[XmlAttribute("tid")]
	public int tid { get; set; }

	[XmlAttribute("Level")]
	public int level { get; set; }

	[XmlAttribute("weight")]
	public int weight { get; set; }

	[XmlArrayItem("ArtifactUnit", typeof(ArtifactUnit))]
	[XmlArray("ArtifactUnitArray")]
	public ArtifactUnit[] artifactUnitArray { get; set; }
}
