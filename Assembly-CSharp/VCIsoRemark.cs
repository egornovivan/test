using System;
using System.Collections.Generic;
using System.Xml;

public class VCIsoRemark
{
	public CreationAttr m_Attribute;

	public string m_Error;

	public string xml
	{
		get
		{
			string empty = string.Empty;
			empty += "<REMARKS>\r\n";
			empty += "\t<ATTR>\r\n";
			string text = $"<COMMON type=\"{(int)m_Attribute.m_Type}\" vol=\"{m_Attribute.m_Volume}\" weight=\"{m_Attribute.m_Weight}\" dur=\"{m_Attribute.m_Durability}\" sp=\"{m_Attribute.m_SellPrice}\">";
			empty = empty + "\t\t" + text + "\r\n";
			foreach (KeyValuePair<int, int> item in m_Attribute.m_Cost)
			{
				if (item.Value > 0)
				{
					string text2 = $"\t\t\t<COST id=\"{item.Key}\" cnt=\"{item.Value}\" />\r\n";
					empty += text2;
				}
			}
			empty += "\t\t</COMMON>\r\n";
			string text3 = $"\t\t<PROP atk=\"{m_Attribute.m_Attack}\" def=\"{m_Attribute.m_Defense}\" inc=\"{m_Attribute.m_MuzzleAtkInc}\" fs=\"{m_Attribute.m_FireSpeed}\" acc=\"{m_Attribute.m_Accuracy}\" dc=\"{m_Attribute.m_DragCoef}\" fuel=\"{m_Attribute.m_MaxFuel}\" />\r\n";
			empty += text3;
			empty += "\t</ATTR>\r\n";
			return empty + "</REMARKS>\r\n";
		}
		set
		{
			m_Attribute = new CreationAttr();
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(value);
				XmlNode xmlNode = xmlDocument.DocumentElement["ATTR"];
				XmlNode xmlNode2 = xmlNode["COMMON"];
				XmlNode xmlNode3 = xmlNode["PROP"];
				m_Attribute.m_Type = (ECreation)XmlConvert.ToInt32(xmlNode2.Attributes["type"].Value);
				m_Attribute.m_Volume = XmlConvert.ToSingle(xmlNode2.Attributes["vol"].Value);
				m_Attribute.m_Weight = XmlConvert.ToSingle(xmlNode2.Attributes["weight"].Value);
				m_Attribute.m_Durability = XmlConvert.ToSingle(xmlNode2.Attributes["dur"].Value);
				m_Attribute.m_SellPrice = XmlConvert.ToSingle(xmlNode2.Attributes["sp"].Value);
				m_Attribute.m_Attack = XmlConvert.ToSingle(xmlNode3.Attributes["atk"].Value);
				m_Attribute.m_Defense = XmlConvert.ToSingle(xmlNode3.Attributes["def"].Value);
				m_Attribute.m_MuzzleAtkInc = XmlConvert.ToSingle(xmlNode3.Attributes["inc"].Value);
				m_Attribute.m_FireSpeed = XmlConvert.ToSingle(xmlNode3.Attributes["fs"].Value);
				m_Attribute.m_Accuracy = XmlConvert.ToSingle(xmlNode3.Attributes["acc"].Value);
				m_Attribute.m_DragCoef = XmlConvert.ToSingle(xmlNode3.Attributes["dc"].Value);
				m_Attribute.m_MaxFuel = XmlConvert.ToSingle(xmlNode3.Attributes["fuel"].Value);
				foreach (XmlNode childNode in xmlNode2.ChildNodes)
				{
					int key = XmlConvert.ToInt32(childNode.Attributes["id"].Value);
					int value2 = XmlConvert.ToInt32(childNode.Attributes["cnt"].Value);
					m_Attribute.m_Cost.Add(key, value2);
				}
			}
			catch (Exception ex)
			{
				m_Attribute = null;
				m_Error = "Reading remarks error" + ex.ToString();
			}
		}
	}

	public VCIsoRemark()
	{
		m_Attribute = new CreationAttr();
		m_Error = string.Empty;
	}
}
