using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using VANativeCampXML;
using VArtifactTownXML;

public class VANativeCampXMLParser
{
	private static VANativeCampXMLParser instance;

	public static VANativeCampXMLParser Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new VANativeCampXMLParser();
			}
			return instance;
		}
	}

	public static void TestXxmlCreating()
	{
		string text = Application.dataPath + "/TestNativeCampXML";
		Directory.CreateDirectory(text);
		text += "/VANativeCamp.xml";
		using FileStream stream = new FileStream(text, FileMode.Create, FileAccess.Write);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(VANativeCampDesc));
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
		NativeCamp nativeCamp = new NativeCamp();
		nativeCamp.artifactUnitArray = artifactUnitArray;
		DynamicNative dynamicNative = new DynamicNative();
		NativeTower nativeTower = new NativeTower();
		nativeTower.dynamicNatives = new DynamicNative[2] { dynamicNative, dynamicNative };
		nativeCamp.nativeTower = nativeTower;
		VANativeCampDesc vANativeCampDesc = new VANativeCampDesc();
		NativeCamp[] nativeCamps = new NativeCamp[2] { nativeCamp, nativeCamp };
		vANativeCampDesc.nativeCamps = nativeCamps;
		xmlSerializer.Serialize(stream, vANativeCampDesc);
	}
}
