using System;
using System.Collections.Generic;
using System.Xml;

namespace ScenarioRTL.IO;

public class TriggerRaw
{
	public string name;

	public int repeat;

	public bool multiThreaded;

	public string owner;

	public StatementRaw[] events;

	public List<StatementRaw[]> conditions;

	public List<StatementRaw[]> actions;

	public static TriggerRaw Create(XmlElement elem)
	{
		TriggerRaw triggerRaw = new TriggerRaw();
		triggerRaw.name = Uri.UnescapeDataString(elem.Attributes["name"].Value);
		triggerRaw.repeat = XmlConvert.ToInt32(elem.Attributes["repeat"].Value);
		if (elem.HasAttribute("multi_threaded"))
		{
			triggerRaw.multiThreaded = elem.Attributes["multi_threaded"].Value.ToLower() == "true";
		}
		else
		{
			triggerRaw.multiThreaded = false;
		}
		if (elem.HasAttribute("owner"))
		{
			triggerRaw.owner = Uri.UnescapeDataString(elem.Attributes["owner"].Value);
		}
		else
		{
			triggerRaw.owner = "-";
		}
		XmlElement xmlElement = elem["EVENTS"];
		XmlElement xmlElement2 = elem["CONDITIONS"];
		XmlElement xmlElement3 = elem["ACTIONS"];
		int count = xmlElement.ChildNodes.Count;
		triggerRaw.events = new StatementRaw[count];
		for (int i = 0; i < count; i++)
		{
			triggerRaw.events[i] = StatementRaw.Create(xmlElement.ChildNodes[i] as XmlElement);
		}
		int count2 = xmlElement2.ChildNodes.Count;
		triggerRaw.conditions = new List<StatementRaw[]>();
		for (int j = 0; j < count2; j++)
		{
			XmlElement xmlElement4 = xmlElement2.ChildNodes[j] as XmlElement;
			int count3 = xmlElement4.ChildNodes.Count;
			StatementRaw[] array = new StatementRaw[count3];
			for (int k = 0; k < count3; k++)
			{
				array[k] = StatementRaw.Create(xmlElement4.ChildNodes[k] as XmlElement);
			}
			triggerRaw.conditions.Add(array);
		}
		count2 = xmlElement3.ChildNodes.Count;
		triggerRaw.actions = new List<StatementRaw[]>();
		for (int l = 0; l < count2; l++)
		{
			XmlElement xmlElement5 = xmlElement3.ChildNodes[l] as XmlElement;
			int count4 = xmlElement5.ChildNodes.Count;
			StatementRaw[] array2 = new StatementRaw[count4];
			for (int m = 0; m < count4; m++)
			{
				array2[m] = StatementRaw.Create(xmlElement5.ChildNodes[m] as XmlElement);
			}
			triggerRaw.actions.Add(array2);
		}
		return triggerRaw;
	}

	public override string ToString()
	{
		return string.Format("TriggerRaw " + name + " " + repeat + "x");
	}
}
