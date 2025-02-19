using System.Xml.Serialization;

public class AssetBundleDesc
{
	[XmlElement("Pos")]
	public AssetPRS[] pos;

	[XmlAttribute("PathName")]
	public string pathName { get; set; }

	[XmlAttribute("CampName")]
	public string campName { get; set; }
}
