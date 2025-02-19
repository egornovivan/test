using System.Xml;

namespace PatheaScript;

public class ConditionAnd : ConditionGroup
{
	protected int mIndex;

	public override bool Do()
	{
		foreach (Condition m in mList)
		{
			if (!m.Do())
			{
				return false;
			}
		}
		return true;
	}

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		mIndex = Util.GetInt(mInfo, "index");
		foreach (XmlNode childNode in mInfo.ChildNodes)
		{
			Condition condition = mFactory.CreateCondition(Util.GetStmtName(childNode));
			if (condition != null)
			{
				condition.SetInfo(mFactory, childNode);
				condition.SetTrigger(mTrigger);
				if (condition.Parse())
				{
					Add(condition);
				}
			}
		}
		return true;
	}
}
