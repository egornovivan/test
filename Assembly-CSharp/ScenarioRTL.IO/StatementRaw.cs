using System;
using System.Xml;

namespace ScenarioRTL.IO;

public class StatementRaw
{
	public string classname = string.Empty;

	public int order;

	public ParamRaw parameters;

	public static StatementRaw Create(XmlElement elem)
	{
		int count = elem.Attributes.Count;
		bool flag = elem.HasAttribute("order");
		StatementRaw statementRaw = new StatementRaw();
		statementRaw.parameters = new ParamRaw((!flag) ? (count - 1) : (count - 2));
		if (flag)
		{
			for (int i = 0; i < count; i++)
			{
				switch (i)
				{
				case 0:
					statementRaw.classname = elem.Attributes[i].Value;
					break;
				case 1:
					statementRaw.order = XmlConvert.ToInt32(elem.Attributes[i].Value);
					break;
				default:
					statementRaw.parameters.Set(i - 2, elem.Attributes[i].Name, Uri.UnescapeDataString(elem.Attributes[i].Value));
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				if (j == 0)
				{
					statementRaw.classname = elem.Attributes[j].Value;
				}
				else
				{
					statementRaw.parameters.Set(j - 1, elem.Attributes[j].Name, Uri.UnescapeDataString(elem.Attributes[j].Value));
				}
			}
		}
		return statementRaw;
	}

	public override string ToString()
	{
		return string.Format("StatementRaw " + classname);
	}
}
