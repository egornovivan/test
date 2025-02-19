using System;
using System.Xml.Serialization;
using VArtifactTownXML;

namespace VANativeCampXML;

[Serializable]
public class NativeCamp
{
	[XmlAttribute("cid")]
	public int cid { get; set; }

	[XmlAttribute("weight")]
	public int weight { get; set; }

	[XmlAttribute("Level")]
	public int level { get; set; }

	[XmlAttribute("NativeType")]
	public int nativeType { get; set; }

	[XmlElement("NativeTower")]
	public NativeTower nativeTower { get; set; }

	[XmlArray("ArtifactUnitArray")]
	[XmlArrayItem("ArtifactUnit", typeof(ArtifactUnit))]
	public ArtifactUnit[] artifactUnitArray { get; set; }
}
