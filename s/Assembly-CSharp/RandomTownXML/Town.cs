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

	[XmlArrayItem("NPC", typeof(NpcIdNum))]
	[XmlArray("NPCArray")]
	public NpcIdNum[] npcIdNum { get; set; }

	[XmlArray("BuildingNumArray")]
	[XmlArrayItem("BuildingNum", typeof(BuildingNum))]
	public BuildingNum[] buildingNum { get; set; }

	[XmlArrayItem("Cell", typeof(Cell))]
	[XmlArray("CellArray")]
	public Cell[] cell { get; set; }
}
