using System;
using System.Xml;

namespace ScenarioRTL.IO;

public class MissionRaw
{
	public int id;

	public string name;

	public TriggerRaw[] triggers;

	public ParamRaw properties;

	public static MissionRaw Create(string xmlpath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(xmlpath);
		XmlElement documentElement = xmlDocument.DocumentElement;
		int count = documentElement.Attributes.Count;
		MissionRaw missionRaw = new MissionRaw();
		missionRaw.properties = new ParamRaw(count - 2);
		for (int i = 0; i < count; i++)
		{
			switch (i)
			{
			case 0:
				missionRaw.id = XmlConvert.ToInt32(documentElement.Attributes[i].Value);
				break;
			case 1:
				missionRaw.name = Uri.UnescapeDataString(documentElement.Attributes[i].Value);
				break;
			default:
				missionRaw.properties.Set(i - 2, documentElement.Attributes[i].Name, Uri.UnescapeDataString(documentElement.Attributes[i].Value));
				break;
			}
		}
		int count2 = documentElement.ChildNodes.Count;
		missionRaw.triggers = new TriggerRaw[count2];
		for (int j = 0; j < count2; j++)
		{
			missionRaw.triggers[j] = TriggerRaw.Create(documentElement.ChildNodes[j] as XmlElement);
		}
		return missionRaw;
	}

	public override string ToString()
	{
		return string.Format("MissionRaw " + name);
	}
}
