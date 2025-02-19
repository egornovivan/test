using System.Collections.Generic;

namespace SkillSystem;

public class SkBeModified
{
	public List<int> indexList = new List<int>();

	public List<float> valueList = new List<float>();

	public void Clear()
	{
		indexList.Clear();
		valueList.Clear();
	}

	public bool HaveModifyData()
	{
		if (indexList.Count > 0 || valueList.Count > 0)
		{
			return true;
		}
		return false;
	}
}
