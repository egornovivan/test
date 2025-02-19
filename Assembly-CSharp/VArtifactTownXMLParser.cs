using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using VArtifactTownXML;

public class VArtifactTownXMLParser
{
	private static VArtifactTownXMLParser instance;

	public static VArtifactTownXMLParser Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new VArtifactTownXMLParser();
			}
			return instance;
		}
	}

	public static void TestXxmlCreating()
	{
		string text = Application.dataPath + "/TestVATownXML";
		Directory.CreateDirectory(text);
		text += "/VATown.xml";
		using FileStream stream = new FileStream(text, FileMode.Create, FileAccess.Write);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(VArtifactTownDesc));
		NpcIdNum[] npcIdNum = new NpcIdNum[2]
		{
			new NpcIdNum(),
			new NpcIdNum()
		};
		BuildingIdNum[] buildingIdNum = new BuildingIdNum[2]
		{
			new BuildingIdNum(),
			new BuildingIdNum()
		};
		ArtifactUnit artifactUnit = new ArtifactUnit();
		artifactUnit.id = "-1";
		artifactUnit.pos = "100,200";
		artifactUnit.rot = "-1";
		artifactUnit.npcIdNum = npcIdNum;
		artifactUnit.buildingIdNum = buildingIdNum;
		ArtifactUnit[] artifactUnitArray = new ArtifactUnit[2] { artifactUnit, artifactUnit };
		VATown vATown = new VATown();
		vATown.artifactUnitArray = artifactUnitArray;
		VArtifactTownDesc vArtifactTownDesc = new VArtifactTownDesc();
		VATown[] vaTown = new VATown[2] { vATown, vATown };
		vArtifactTownDesc.vaStartTown = vATown;
		vArtifactTownDesc.vaTown = vaTown;
		xmlSerializer.Serialize(stream, vArtifactTownDesc);
	}
}
