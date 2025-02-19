using System.Xml.Serialization;

namespace VANativeCampXML;

public class NativeTower
{
	[XmlAttribute("pathID")]
	public int pathID { get; set; }

	[XmlAttribute("campID")]
	public int campID { get; set; }

	[XmlAttribute("damageID")]
	public int damageID { get; set; }

	[XmlElement("DynamicNative")]
	public DynamicNative[] dynamicNatives { get; set; }
}
