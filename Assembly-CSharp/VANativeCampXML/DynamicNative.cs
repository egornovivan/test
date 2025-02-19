using System.Xml.Serialization;

namespace VANativeCampXML;

public class DynamicNative
{
	[XmlAttribute("did")]
	public int did { get; set; }

	[XmlAttribute("type")]
	public int type { get; set; }
}
