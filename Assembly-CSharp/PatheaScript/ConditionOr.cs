using System.Xml;

namespace PatheaScript;

public class ConditionOr : ConditionGroup
{
	public override bool Do()
	{
		foreach (Condition m in mList)
		{
			if (m.Do())
			{
				return true;
			}
		}
		return false;
	}

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		foreach (XmlNode childNode in mInfo.ChildNodes)
		{
			ConditionAnd conditionAnd = new ConditionAnd();
			conditionAnd.SetInfo(mFactory, childNode);
			conditionAnd.SetTrigger(mTrigger);
			if (conditionAnd.Parse())
			{
				Add(conditionAnd);
			}
		}
		return true;
	}
}
