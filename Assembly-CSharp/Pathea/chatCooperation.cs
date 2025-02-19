using System.Collections.Generic;
using PETools;
using UnityEngine;

namespace Pathea;

public class chatCooperation : Cooperation
{
	public class DataV
	{
		private int _useid;

		private Vector3 _Pos;

		private bool _Isused;

		public bool IsUsed => _Isused;

		public Vector3 Pos => _Pos;

		public int UseID => _useid;

		public DataV(Vector3 v3, bool isused)
		{
			_Pos = v3;
			_Isused = isused;
		}

		public void UsePos(bool use, int protoId)
		{
			_Isused = use;
			_useid = protoId;
		}
	}

	private List<DataV> mchatPoses;

	private PeEntity mcentor;

	private EQuadrant mEQuadrant;

	private Vector3 mCenterPos;

	public EQuadrant quadrant => mEQuadrant;

	public chatCooperation(int memberNum)
		: base(memberNum)
	{
		mchatPoses = new List<DataV>();
	}

	public void setCentor(PeEntity _centor, EQuadrant q, Vector3 centorPos)
	{
		mcentor = _centor;
		mEQuadrant = q;
		mCenterPos = centorPos;
		calculateRestPos();
	}

	private void calculateRestPos()
	{
		Vector3 vector = mcentor.position - mCenterPos;
		float num = 360f / (float)mCooperMemNum;
		for (int i = 0; i < mCooperMemNum; i++)
		{
			float num2 = num * (float)i;
			mchatPoses.Add(new DataV(PEUtil.GetRandomPosition(mCenterPos, -vector, 0.8f * CSNpcTeam.Chat_Radiu, CSNpcTeam.Chat_Radiu, num2, num2), isused: false));
		}
	}

	public override bool AddMember(PeEntity entity)
	{
		for (int i = 0; i < mchatPoses.Count; i++)
		{
			if (!mchatPoses[i].IsUsed && entity.NpcCmpt != null && base.AddMember(entity))
			{
				entity.NpcCmpt.setTeamData(mchatPoses[i].Pos, mCenterPos);
				entity.NpcCmpt.SetLineType(ELineType.TeamChat);
				mchatPoses[i].UsePos(use: true, entity.ProtoID);
				return true;
			}
		}
		return false;
	}

	public override bool RemoveMember(PeEntity entity)
	{
		for (int i = 0; i < mchatPoses.Count; i++)
		{
			if (entity.NpcCmpt != null && mchatPoses[i].UseID == entity.ProtoID)
			{
				mchatPoses[i].UsePos(use: false, 0);
				entity.NpcCmpt.SetLineType(ELineType.IDLE);
				return base.RemoveMember(entity);
			}
		}
		return false;
	}

	public override void DissolveCooper()
	{
		mCooperMembers.Clear();
	}
}
