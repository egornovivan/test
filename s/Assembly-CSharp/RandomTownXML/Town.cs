using System;
using System.Xml.Serialization;

namespace RandomTownXML;

[Serializable]
public class Town
{
	[XmlAttribute("tid")]
	public int tid { get; set; }

	[XmlAttribute("CellNumX")]
	public int cellNumX { get; set; }

	[XmlAttribute("CellNumZ")]
	public int cellNumZ { get; set; }

	[XmlAttribute("CellSizeX")]
	public int cellSizeX { get; set; }

	[XmlAttribute("CellSizeZ")]
	public int cellSizeZ { get; set; }

	[XmlAttribute("weight")]
	public int weight { get; set; }

	[XmlArray("NPCArray")]
	[XmlArrayItem("NPC", typeof(NpcIdNum))]
	public NpcIdNum[] npcIdNum { get; set; }

	[XmlArrayItem("BuildingNum", typeof(BuildingNum))]
	[XmlArray("BuildingNumArray")]
	public BuildingNum[] buildingNum { get; set; }

	[XmlArray("CellArray")]
	[XmlArrayItem("Cell", typeof(Cell))]
	public Cell[] cell { get; set; }
}
