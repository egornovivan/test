namespace Pathea;

public class SleepLine : TeamLine
{
	public override ELineType Type => ELineType.TeamSleep;

	public override int Priority => 2;

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
		if (eLineMsg2 == ELineMsg.Add_Sleep)
		{
			int num = (int)objs[1];
			double startHour = (double)objs[2];
			CreatSleepCooper(num, startHour);
		}
	}

	private void CreatSleepCooper(int num, double startHour)
	{
		SleepCooperation item = new SleepCooperation(num, startHour);
		mCooperationLists.Add(item);
	}

	public SleepCooperation GetUpCooper()
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			SleepCooperation sleepCooperation = mCooperationLists[i] as SleepCooperation;
			if (sleepCooperation.GetCurMemberNum() > 0)
			{
				return sleepCooperation;
			}
		}
		return null;
	}
}
