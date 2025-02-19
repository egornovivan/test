using System.Collections.Generic;
using System.IO;
using System.Xml;
using Behave.Runtime;
using UnityEngine;

public class MonsterXmlData
{
	private static Dictionary<int, ImxmlData> m_Data = new Dictionary<int, ImxmlData>();

	public static void InitializeData(int protoId, string btPath)
	{
		TextAsset textAsset = UnityEngine.Resources.Load(btPath) as TextAsset;
		if (!(textAsset != null) || string.IsNullOrEmpty(textAsset.text))
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = true;
		XmlReader xmlReader = XmlReader.Create(new StringReader(textAsset.text), xmlReaderSettings);
		xmlDocument.Load(xmlReader);
		XmlElement xmlElement = xmlDocument.SelectSingleNode("Tree") as XmlElement;
		XmlNodeList elementsByTagName = xmlElement.GetElementsByTagName("Action");
		foreach (XmlNode item in elementsByTagName)
		{
			XmlElement e = item as XmlElement;
			string attributeString = XmlUtil.GetAttributeString(e, "Type");
			if (attributeString.Equals("Pounce"))
			{
				PounceData value = new PounceData(e);
				m_Data.Add(protoId, value);
				break;
			}
		}
	}

	public static bool GetData<T>(int protoId, ref T t)
	{
		if (m_Data.ContainsKey(protoId))
		{
			t = (T)m_Data[protoId];
			return true;
		}
		return false;
	}
}
