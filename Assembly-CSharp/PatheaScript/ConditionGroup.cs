using System.Collections.Generic;
using System.Text;

namespace PatheaScript;

public abstract class ConditionGroup : Condition
{
	protected List<Condition> mList;

	public void Add(Condition c)
	{
		mList.Add(c);
	}

	public override bool Parse()
	{
		mList = new List<Condition>(mInfo.ChildNodes.Count);
		return true;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(GetType().ToString());
		foreach (Condition m in mList)
		{
			stringBuilder.AppendLine(m.ToString());
		}
		return stringBuilder.ToString();
	}
}
