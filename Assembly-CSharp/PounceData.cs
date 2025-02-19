using System.Xml;

public class PounceData : ImxmlData
{
	public string _name;

	public string _pounce;

	public float _startTime;

	public float _endTime;

	public float _stopTime;

	public int _skillID;

	public PounceData(XmlElement e)
	{
		XmlNodeList elementsByTagName = e.GetElementsByTagName("Data");
		foreach (XmlNode item in elementsByTagName)
		{
			_name = item.Attributes["Name"].Value;
			_pounce = item.Attributes["pounce"].Value;
			_startTime = XmlConvert.ToSingle(item.Attributes["startTime"].Value);
			_endTime = XmlConvert.ToSingle(item.Attributes["endTime"].Value);
			_stopTime = XmlConvert.ToSingle(item.Attributes["stopTime"].Value);
			_skillID = XmlConvert.ToInt32(item.Attributes["skillID"].Value);
		}
	}
}
