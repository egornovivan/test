using System.Collections.Generic;

namespace Pathea;

public abstract class TeamLine
{
	internal List<PeEntity> mLinemembers;

	internal List<Cooperation> mCooperationLists;

	public virtual List<PeEntity> Linemembers => mLinemembers;

	public abstract int Priority { get; }

	public abstract ELineType Type { get; }

	public TeamLine()
	{
		if (mLinemembers == null)
		{
			mLinemembers = new List<PeEntity>();
		}
		if (mCooperationLists == null)
		{
			mCooperationLists = new List<Cooperation>();
		}
	}

	public virtual int GetNeedMemNumber()
	{
		int num = 0;
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			num += mCooperationLists[i].GetCooperNeedNum();
		}
		return num;
	}

	public virtual int GetCooperCurMember()
	{
		int num = 0;
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			num += mCooperationLists[i].GetCurMemberNum();
		}
		return num;
	}

	public virtual int GetInLineMember()
	{
		return mLinemembers.Count;
	}

	public virtual bool CanAddCooperMember(PeEntity member, params object[] objs)
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i].ContainMember(member))
			{
				return false;
			}
		}
		for (int j = 0; j < mCooperationLists.Count; j++)
		{
			if (mCooperationLists[j].CanAddMember(objs))
			{
				mCooperationLists[j].AddMember(member);
				return true;
			}
		}
		return false;
	}

	public virtual bool memberCanAddIn(PeEntity member, params object[] objs)
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i].ContainMember(member))
			{
				return false;
			}
		}
		for (int j = 0; j < mCooperationLists.Count; j++)
		{
			if (mCooperationLists[j].CanAddMember(objs))
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveFromCooper(PeEntity member)
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i].ContainMember(member))
			{
				mCooperationLists[i].RemoveMember(member);
			}
		}
	}

	public virtual bool needAddMemeber()
	{
		return GetNeedMemNumber() > GetCooperCurMember();
	}

	public virtual Cooperation IsNeedMember()
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i].IsneedMember())
			{
				return mCooperationLists[i];
			}
		}
		return null;
	}

	public virtual bool AddIn(PeEntity member, params object[] objs)
	{
		if (mLinemembers == null)
		{
			mLinemembers = new List<PeEntity>();
		}
		if (!mLinemembers.Contains(member))
		{
			if (member.NpcCmpt != null)
			{
				member.NpcCmpt.SetLineType(Type);
			}
			mLinemembers.Add(member);
			return true;
		}
		return false;
	}

	public virtual bool AddIn(List<PeEntity> members, params object[] objs)
	{
		if (members == null)
		{
			members = new List<PeEntity>();
		}
		for (int i = 0; i < members.Count; i++)
		{
			AddIn(members[i], objs);
		}
		return true;
	}

	public virtual bool RemoveOut(PeEntity member)
	{
		if (mLinemembers.Contains(member) && mLinemembers.Remove(member) && member.NpcCmpt != null)
		{
			member.NpcCmpt.SetLineType(ELineType.IDLE);
			return true;
		}
		return false;
	}

	public virtual bool RemoveOut(List<PeEntity> members)
	{
		for (int i = 0; i < members.Count; i++)
		{
			RemoveOut(members[i]);
		}
		return true;
	}

	public virtual void ClearMembers()
	{
		for (int i = 0; i < mLinemembers.Count; i++)
		{
			RemoveOut(mLinemembers[i]);
		}
	}

	public virtual bool ContainEntity(PeEntity member)
	{
		for (int i = 0; i < mLinemembers.Count; i++)
		{
			if (mLinemembers[i] == member)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool Suitline(PeEntity member, params object[] objs)
	{
		return true;
	}

	public abstract void OnMsgLine(params object[] objs);

	public abstract bool Go();
}
