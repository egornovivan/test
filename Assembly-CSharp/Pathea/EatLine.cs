namespace Pathea;

public class EatLine : TeamLine
{
	public override ELineType Type => ELineType.TeamEat;

	public override int Priority => 1;

	public override bool Go()
	{
		return false;
	}

	public override bool AddIn(PeEntity member, params object[] objs)
	{
		if (CanAddCooperMember(member, objs))
		{
			return base.AddIn(member, objs);
		}
		return false;
	}

	public override bool RemoveOut(PeEntity member)
	{
		RemoveFromCooper(member);
		return base.RemoveOut(member);
	}

	public override void OnMsgLine(params object[] objs)
	{
		ELineMsg eLineMsg = (ELineMsg)(int)objs[0];
		ELineMsg eLineMsg2 = eLineMsg;
		if (eLineMsg2 == ELineMsg.Add_Eat)
		{
			int num = (int)objs[1];
			CreatEatCooper(num);
		}
	}

	private void CreatEatCooper(int num)
	{
		EatCooperation item = new EatCooperation(num);
		mCooperationLists.Add(item);
	}
}
