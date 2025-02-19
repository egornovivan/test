using System.Collections.Generic;
using SkillSystem;

namespace Pathea;

public class AttackLine : TeamLine
{
	private List<PeEntity> mAtkTargets;

	internal List<Cooperation> tempLists;

	public override ELineType Type => ELineType.TeamAtk;

	public override int Priority => 0;

	public AttackLine()
	{
		mAtkTargets = new List<PeEntity>();
		tempLists = new List<Cooperation>();
	}

	private void CooperationEnd(Cooperation self)
	{
		self.DissolveCooper();
		mCooperationLists.Remove(self);
	}

	public void DissolveTheline(PeEntity target)
	{
		AtkCooperation atkCooperByTarget = GetAtkCooperByTarget(target);
		if (target != null && mAtkTargets.Contains(target))
		{
			RemoveAtkTarget(target);
		}
		if (atkCooperByTarget != null)
		{
			List<PeEntity> cooperMembers = atkCooperByTarget.GetCooperMembers();
			RemoveOut(cooperMembers);
			mCooperationLists.Remove(atkCooperByTarget);
			atkCooperByTarget.DissolveCooper();
		}
	}

	public void OnAtkTargetDeath(SkEntity skSelf, SkEntity skCaster)
	{
		PeEntity component = skSelf.GetComponent<PeEntity>();
		AtkCooperation atkCooperByTarget = GetAtkCooperByTarget(component);
		if (component != null && mAtkTargets.Contains(component))
		{
			RemoveAtkTarget(component);
		}
		if (atkCooperByTarget != null)
		{
			List<PeEntity> cooperMembers = atkCooperByTarget.GetCooperMembers();
			RemoveOut(cooperMembers);
			mCooperationLists.Remove(atkCooperByTarget);
			atkCooperByTarget.DissolveCooper();
		}
	}

	public void OnAtkTargetDestroy(SkEntity entity)
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i] is AtkCooperation atkCooperation)
			{
				atkCooperation.OnAtkTargetDestroy(entity);
			}
		}
		PeEntity component = entity.GetComponent<PeEntity>();
		if (component != null && mAtkTargets.Contains(component))
		{
			RemoveAtkTarget(component);
		}
	}

	public void OnAtkTargetLost(PeEntity entity)
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i] is AtkCooperation atkCooperation)
			{
				atkCooperation.OnAtkTargetLost(entity);
			}
		}
		if (entity != null && mAtkTargets.Contains(entity))
		{
			RemoveAtkTarget(entity);
		}
	}

	public List<PeEntity> GetAtkMemberByTarget(PeEntity target)
	{
		return GetAtkCooperByTarget(target)?.GetAtkCooperMembers();
	}

	public AtkCooperation GetAtkCooperByTarget(PeEntity target)
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i] is AtkCooperation atkCooperation && atkCooperation.HasBeTarget(target))
			{
				return atkCooperation;
			}
		}
		return null;
	}

	public AtkCooperation GetAtkCooperByMember(PeEntity member)
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i] is AtkCooperation atkCooperation && atkCooperation.ContainMember(member))
			{
				return atkCooperation;
			}
		}
		return null;
	}

	public void UpdateAtkTarget()
	{
		for (int i = 0; i < mAtkTargets.Count; i++)
		{
			if (!BeCooperationTarget(mAtkTargets[i]))
			{
				AddNewTargetAtkCooperation(mAtkTargets[i]);
			}
		}
	}

	public void AddAktTarget(PeEntity target)
	{
		mAtkTargets.Add(target);
	}

	public bool RemoveAtkTarget(PeEntity target)
	{
		return mAtkTargets.Remove(target);
	}

	public void AddNewTargetAtkCooperation(PeEntity target)
	{
		if (target != null && target.monsterProtoDb != null)
		{
			int memberNum = ((target.monsterProtoDb.AtkDb.mNumber == 0) ? CSNpcTeam.CsNpcNumber : target.monsterProtoDb.AtkDb.mNumber);
			AtkCooperation atkCooperation = new AtkCooperation(memberNum, 1);
			atkCooperation.AddAktTarget(target);
			mCooperationLists.Add(atkCooperation);
		}
	}

	public AtkCooperation NewTargetAtkCooperation(PeEntity target)
	{
		if (target != null && target.monsterProtoDb != null)
		{
			int memberNum = ((target.monsterProtoDb.AtkDb.mNumber == 0) ? CSNpcTeam.CsNpcNumber : target.monsterProtoDb.AtkDb.mNumber);
			AtkCooperation atkCooperation = new AtkCooperation(memberNum, 1);
			atkCooperation.AddAktTarget(target);
			mCooperationLists.Add(atkCooperation);
			return atkCooperation;
		}
		return null;
	}

	public bool ChangeCooperTarget(AtkCooperation cooper, PeEntity target)
	{
		if (cooper == null)
		{
			return false;
		}
		cooper.AddAktTarget(target);
		return true;
	}

	public bool BeCooperationTarget(PeEntity target)
	{
		for (int i = 0; i < mCooperationLists.Count; i++)
		{
			if (mCooperationLists[i] is AtkCooperation atkCooperation && atkCooperation.HasBeTarget(target))
			{
				atkCooperation.SetAtkTarget(target);
				return true;
			}
		}
		for (int j = 0; j < mCooperationLists.Count; j++)
		{
			if (mCooperationLists[j] is AtkCooperation atkCooperation2 && atkCooperation2.CanBeTarget(target))
			{
				atkCooperation2.AddAktTarget(target);
				return true;
			}
		}
		return false;
	}

	public void AddNewMemberAtkCooperation(PeEntity member)
	{
		AtkCooperation atkCooperation = new AtkCooperation(2, 1);
		member.target.SetEnityCanAttack(canAttackOrNot: true);
		atkCooperation.AddMember(member);
		mCooperationLists.Add(atkCooperation);
	}

	private bool IsInEnems(PeEntity member, PeEntity target)
	{
		if (member == null || target == null || member.target == null)
		{
			return false;
		}
		List<Enemy> enemies = member.target.GetEnemies();
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].entityTarget == target)
			{
				return true;
			}
		}
		return false;
	}

	public override bool Go()
	{
		if (mLinemembers == null)
		{
			return false;
		}
		for (int i = 0; i < mLinemembers.Count; i++)
		{
			if (mLinemembers[i].NpcCmpt != null && mLinemembers[i].target != null)
			{
				mLinemembers[i].NpcCmpt.SetLineType(ELineType.TeamAtk);
				mLinemembers[i].target.SetEnityCanAttack(canAttackOrNot: true);
				mLinemembers[i].target.SetCanAtiveWeapon(value: true);
			}
		}
		UpdateAtkTarget();
		return true;
	}

	public override bool AddIn(PeEntity member, params object[] objs)
	{
		if (objs == null || objs.Length <= 0)
		{
			return base.AddIn(member);
		}
		if (CanAddCooperMember(member, (PeEntity)objs[0]))
		{
			if (member.NpcCmpt != null)
			{
				member.NpcCmpt.SetLineType(ELineType.TeamAtk);
			}
			if (member.target != null)
			{
				member.target.SetEnityCanAttack(canAttackOrNot: true);
			}
			return base.AddIn(member);
		}
		return false;
	}

	public override bool RemoveOut(PeEntity member)
	{
		RemoveFromCooper(member);
		member.target.SetEnityCanAttack(canAttackOrNot: true);
		member.NpcCmpt.BattleMgr.SetSelectEnemy(null);
		return base.RemoveOut(member);
	}

	public override void OnMsgLine(params object[] objs)
	{
		switch ((ELineMsg)(int)objs[0])
		{
		case ELineMsg.ADD_Target:
		{
			PeEntity peEntity = (PeEntity)objs[1];
			if (!mAtkTargets.Contains(peEntity))
			{
				AddAktTarget(peEntity);
				UpdateAtkTarget();
			}
			break;
		}
		}
	}
}
