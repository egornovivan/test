using System.Collections.Generic;

namespace SkillSystem;

public class AttActionCtrl
{
	private List<AttAction> mActions;

	public AttActionCtrl(AttRuleCtrl ctrl, SkEntity skEntity, string para)
	{
		mActions = new List<AttAction>();
		string[] array = para.Split(';');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			switch (array3[0].ToLower())
			{
			case "skill":
			{
				AttAction_Skill item4 = new AttAction_Skill(skEntity, array3);
				mActions.Add(item4);
				break;
			}
			case "camp":
			{
				AttAction_Att item3 = new AttAction_Att(skEntity, array3);
				mActions.Add(item3);
				break;
			}
			case "func":
			{
				AttAction_Func item2 = new AttAction_Func(skEntity, array3);
				mActions.Add(item2);
				break;
			}
			case "Action":
			{
				AttAction_Action item = new AttAction_Action(skEntity, array3, ctrl);
				mActions.Add(item);
				break;
			}
			}
		}
	}

	public void DoAction()
	{
		foreach (AttAction mAction in mActions)
		{
			mAction.Do();
		}
	}
}
