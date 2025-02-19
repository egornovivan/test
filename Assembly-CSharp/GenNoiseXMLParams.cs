using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class GenNoiseXMLParams
{
	[Serializable]
	[XmlRoot("root")]
	public class rootCLS
	{
		[XmlElement("BaseData")]
		public BaseDataCLS BaseDataValues { get; set; }

		[XmlArrayItem("HeightDesc", typeof(HeightDescCollectionCLS))]
		[XmlArray("HeightDescCollection")]
		public HeightDescCollectionCLS[] HeightDescCollectionValues { get; set; }

		[XmlElement("ArtifactInfo")]
		public ArtifactInfoCLS ArtifactInfoValues { get; set; }

		[XmlElement("PlateauInfo")]
		public PlateauInfoCLS PlateauInfoValues { get; set; }
	}

	[Serializable]
	public class BaseDataCLS
	{
		[XmlAttribute("TerrainHeight")]
		public int TerrainHeight { get; set; }

		[XmlAttribute("HMWidth")]
		public int HMWidth { get; set; }

		[XmlAttribute("HMHeight")]
		public int HMHeight { get; set; }

		[XmlAttribute("RandomSeed")]
		public int RandomSeed { get; set; }

		[XmlElement("TerrainHMSpec")]
		public TerrainHMSpecCLS TerrainHMSpecValues { get; set; }

		[XmlArray("CaveGroup")]
		[XmlArrayItem("CaveSpec", typeof(CaveGroupCLS))]
		public CaveGroupCLS[] CaveGroupValues { get; set; }
	}

	[Serializable]
	public class TerrainHMSpecCLS
	{
		[XmlAttribute("MinHeight")]
		public int MinHeight { get; set; }

		[XmlAttribute("MaxHeight")]
		public int MaxHeight { get; set; }

		[XmlAttribute("NumOfOctaves")]
		public int NumOfOctaves { get; set; }

		[XmlArrayItem("OctaveLength", typeof(OctaveLengthArrayCLS))]
		[XmlArray("OctaveLengthArray")]
		public OctaveLengthArrayCLS[] OctaveLengthArrayValues { get; set; }

		[XmlArrayItem("OctaveMagnitude", typeof(OctaveMagnitudeArrayCLS))]
		[XmlArray("OctaveMagnitudeArray")]
		public OctaveMagnitudeArrayCLS[] OctaveMagnitudeArrayValues { get; set; }
	}

	[Serializable]
	public class OctaveLengthArrayCLS
	{
		[XmlAttribute("value")]
		public int value { get; set; }
	}

	[Serializable]
	public class OctaveMagnitudeArrayCLS
	{
		[XmlAttribute("value")]
		public float value { get; set; }
	}

	[Serializable]
	public class CaveGroupCLS
	{
		[XmlAttribute("CaveWallVType")]
		public int CaveWallVType { get; set; }

		[XmlAttribute("CaveCeilingVType")]
		public int CaveCeilingVType { get; set; }

		[XmlAttribute("CaveFloorVType")]
		public int CaveFloorVType { get; set; }

		[XmlAttribute("Altitude")]
		public int Altitude { get; set; }

		[XmlAttribute("AltitudeVariance")]
		public float AltitudeVariance { get; set; }

		[XmlAttribute("MinerSpawnRate")]
		public float MinerSpawnRate { get; set; }

		[XmlAttribute("StopAtNumMiners")]
		public int StopAtNumMiners { get; set; }

		[XmlAttribute("NumSmoothOps")]
		public int NumSmoothOps { get; set; }

		[XmlAttribute("BelowHeight")]
		public int BelowHeight { get; set; }

		[XmlAttribute("AboveHeight")]
		public int AboveHeight { get; set; }

		[XmlAttribute("SamplingFactor")]
		public int SamplingFactor { get; set; }

		[XmlAttribute("bvNumIteration")]
		public int bvNumIteration { get; set; }

		[XmlAttribute("bvNoiseLength")]
		public float bvNoiseLength { get; set; }

		[XmlAttribute("bvNoiseStrength")]
		public float bvNoiseStrength { get; set; }

		[XmlAttribute("blNumIteration")]
		public int blNumIteration { get; set; }

		[XmlAttribute("blNoiseLength")]
		public float blNoiseLength { get; set; }

		[XmlAttribute("blNoiseStrength")]
		public float blNoiseStrength { get; set; }
	}

	[Serializable]
	public class HeightDescCollectionCLS
	{
		[XmlAttribute("start")]
		public int start { get; set; }

		[XmlAttribute("end")]
		public int end { get; set; }

		[XmlArray("GradientDescArray")]
		[XmlArrayItem("GradientDesc", typeof(GradientDescArrayCLS))]
		public GradientDescArrayCLS[] GradientDescArrayValues { get; set; }
	}

	[Serializable]
	public class GradientDescArrayCLS
	{
		[XmlAttribute("start")]
		public int start { get; set; }

		[XmlAttribute("end")]
		public int end { get; set; }

		[XmlArrayItem("VoxelRate", typeof(VoxelRateArrayCLS))]
		[XmlArray("VoxelRateArray")]
		public VoxelRateArrayCLS[] VoxelRateArrayValues { get; set; }
	}

	[Serializable]
	public class VoxelRateArrayCLS
	{
		[XmlAttribute("type")]
		public int type { get; set; }

		[XmlAttribute("perc")]
		public int perc { get; set; }
	}

	[Serializable]
	public class ArtifactInfoCLS
	{
		[XmlArrayItem("Artifact", typeof(ArtifactNameArrayCLS))]
		[XmlArray("ArtifactNameArray")]
		public ArtifactNameArrayCLS[] ArtifactNameArrayValues { get; set; }
	}

	[Serializable]
	public class ArtifactNameArrayCLS
	{
		[XmlAttribute("name")]
		public string name { get; set; }

		[XmlAttribute("perc")]
		public int perc { get; set; }
	}

	[Serializable]
	public class PlateauInfoCLS
	{
		[XmlArray("PlateauArray")]
		[XmlArrayItem("Plateau", typeof(PlateauArrayCLS))]
		public PlateauArrayCLS[] PlateauArrayValues { get; set; }
	}

	[Serializable]
	public class PlateauArrayCLS
	{
		[XmlAttribute("type")]
		public string type { get; set; }

		[XmlAttribute("num")]
		public int num { get; set; }

		[XmlAttribute("radius")]
		public int radius { get; set; }

		[XmlAttribute("focus")]
		public int focus { get; set; }
	}

	public rootCLS prms;

	public void LoadXML()
	{
		int randomMapID = (int)RandomMapConfig.RandomMapID;
		string text = "RandomMapXML/multiplayer_random_map_settings_";
		text += randomMapID;
		UnityEngine.Object @object = Resources.Load(text);
		TextAsset textAsset = ((!(null == @object)) ? (@object as TextAsset) : (Resources.Load("RandomMapXML/multiplayer_random_map_settings_1") as TextAsset));
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(rootCLS));
		StringReader stringReader = new StringReader(textAsset.text);
		if (stringReader != null)
		{
			prms = (rootCLS)xmlSerializer.Deserialize(stringReader);
			stringReader.Close();
		}
	}
}
