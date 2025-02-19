using System.Collections.Generic;

namespace SkillSystem;

public class AttCondCtrl
{
	private List<AttCond> mConds;

	public AttCondCtrl(SkEntity skEntity, string para)
	{
		mConds = new List<AttCond>();
		string[] array = para.Split(';');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			switch (array3[0].ToLower())
			{
			case "compare":
			{
				AttCond_Compare item3 = new AttCond_Compare(skEntity, array3);
				mConds.Add(item3);
				break;
			}
			case "camp":
			{
				AttCond_Camp item2 = new AttCond_Camp(skEntity, array3);
				mConds.Add(item2);
				break;
			}
			case "RunSkill":
			{
				AttCond_RunSkill item = new AttCond_RunSkill(skEntity, array3);
				mConds.Add(item);
				break;
			}
			}
		}
	}

	public virtual bool Check()
	{
		foreach (AttCond mCond in mConds)
		{
			if (!mCond.Check())
			{
				return false;
			}
		}
		return true;
	}
}
