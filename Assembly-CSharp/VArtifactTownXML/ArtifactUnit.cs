using System.Xml.Serialization;

namespace VArtifactTownXML;

public class ArtifactUnit
{
	[XmlAttribute("id")]
	public string id { get; set; }

	[XmlAttribute("pos")]
	public string pos { get; set; }

	[XmlAttribute("rot")]
	public string rot { get; set; }

	[XmlAttribute("type")]
	public int type { get; set; }

	[XmlArrayItem("NPC", typeof(NpcIdNum))]
	[XmlArray("NPCArray")]
	public NpcIdNum[] npcIdNum { get; set; }

	[XmlArrayItem("BuildingNum", typeof(BuildingIdNum))]
	[XmlArray("BuildingNumArray")]
	public BuildingIdNum[] buildingIdNum { get; set; }
}
