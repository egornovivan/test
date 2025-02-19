using System.Collections.Generic;
using PETools;
using UnityEngine;

namespace Pathea;

public class chatLine : TeamLine
{
	private int centorNum;

	private List<PeEntity> mchatCentors;

	public override ELineType Type => ELineType.TeamChat;

	public override int Priority => 3;

	public int CenterNum => mchatCentors.Count;

	public chatLine(int _cNum)
	{
		centorNum = _cNum;
		mchatCentors = new List<PeEntity>(centorNum);
	}

	public override void OnMsgLine(params object[] objs)
	{
		switch ((ELineMsg)(int)objs[0])
		{
		case ELineMsg.Add_chatCentor:
		{
			EQuadrant q = (EQuadrant)(int)objs[1];
			PeEntity peEntity = (PeEntity)objs[2];
			if (peEntity != null)
			{
				AddrestCenter(peEntity, q);
			}
			break;
		}
		case ELineMsg.Clear_chat:
			ClearCenter();
			break;
		}
	}

	public override bool Go()
	{
		if (mLinemembers == null)
		{
			return false;
		}
		UpdataChat();
		return false;
	}

	public override bool CanAddCooperMember(PeEntity member, params object[] objs)
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
			if (mCooperationLists[j].CanAddMember(member.position))
			{
				mCooperationLists[j].AddMember(member);
				return true;
			}
		}
		return false;
	}

	public override bool memberCanAddIn(PeEntity member, params object[] objs)
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
			if (mCooperationLists[j].CanAddMember(member.position))
			{
				return true;
			}
		}
		return false;
	}

	public override bool AddIn(PeEntity member, params object[] objs)
	{
		if (CanAddCooperMember(member, objs))
		{
			if (member.NpcCmpt != null)
			{
				member.NpcCmpt.SetLineType(ELineType.TeamChat);
			}
			return base.AddIn(member, objs);
		}
		return false;
	}

	public override bool RemoveOut(PeEntity member)
	{
		RemoveFromCooper(member);
		return base.RemoveOut(member);
	}

	public override bool Suitline(PeEntity member, params object[] objs)
	{
		if (objs == null)
		{
			return true;
		}
		return member.NpcCmpt != null && !member.NpcCmpt.IsNeedMedicine;
	}

	private void CreatchatCooper(PeEntity centor, EQuadrant q, Vector3 centorPos)
	{
		chatCooperation chatCooperation2 = new chatCooperation(Random.Range(2, CSNpcTeam.Chat_member_Num + 1));
		chatCooperation2.setCentor(centor, q, centorPos);
		mCooperationLists.Add(chatCooperation2);
	}

	private void UpdataChat()
	{
		for (int i = 0; i < mLinemembers.Count; i++)
		{
			if (mLinemembers[i].NpcCmpt != null)
			{
				mLinemembers[i].NpcCmpt.SetLineType(ELineType.TeamChat);
			}
		}
	}

	private bool QuadrantHasCentor(EQuadrant q)
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i] is chatCooperation chatCooperation2 && chatCooperation2.quadrant == q)
			{
				return true;
			}
		}
		return false;
	}

	private void AddrestCenter(PeEntity centor, EQuadrant q)
	{
		Vector3 emptyPositionOnGround = PEUtil.GetEmptyPositionOnGround(centor.position, 2f, 5f);
		if (emptyPositionOnGround != Vector3.zero && !mchatCentors.Contains(centor) && mchatCentors.Count < centorNum && !QuadrantHasCentor(q) && JudgeCenter(centor))
		{
			mchatCentors.Add(centor);
			CreatchatCooper(centor, q, emptyPositionOnGround);
		}
	}

	private bool JudgeCenter(PeEntity centor, float radiu = 20f)
	{
		for (int i = 0; i < mchatCentors.Count; i++)
		{
			if (PEUtil.MagnitudeH(centor.position, mchatCentors[i].position) < radiu)
			{
				return false;
			}
		}
		return true;
	}

	private void RemoveCentor(PeEntity centor)
	{
		if (mchatCentors.Contains(centor))
		{
			mchatCentors.Remove(centor);
		}
	}

	private void ClearCenter()
	{
		mchatCentors.Clear();
	}

	public bool CanAddcenter()
	{
		return mchatCentors.Count <= 0;
	}
}
