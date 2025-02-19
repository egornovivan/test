namespace Pathea.PeEntityExtFollow;

public static class PeEntityExtFollow
{
	public static void ExtFollow(this PeEntity entity, PeEntity target)
	{
		if (!(null == target) && !(entity == null))
		{
			FollowCmpt followCmpt = entity.GetCmpt<FollowCmpt>();
			if (null == followCmpt)
			{
				followCmpt = entity.Add<FollowCmpt>();
			}
			PeTrans peTrans = target.peTrans;
			if (!(null == peTrans))
			{
				followCmpt.Follow(peTrans);
			}
		}
	}

	public static void ExtDefollow(this PeEntity entity)
	{
		FollowCmpt cmpt = entity.GetCmpt<FollowCmpt>();
		if (!(null == cmpt))
		{
			cmpt.Defollow();
		}
	}

	public static void ExtSetLostDis(this PeEntity entity, float dis)
	{
		FollowCmpt cmpt = entity.GetCmpt<FollowCmpt>();
		if (!(null == cmpt))
		{
			cmpt.lostDis = dis;
		}
	}

	public static void ExtSetSetPosDis(this PeEntity entity, float dis)
	{
		FollowCmpt cmpt = entity.GetCmpt<FollowCmpt>();
		if (!(null == cmpt))
		{
			cmpt.setPosDis = dis;
		}
	}
}
