using System.Collections.Generic;
using SkillSystem;

namespace Pathea;

public class AtkCooperation : Cooperation
{
	private List<PeEntity> mAtkTarget;

	private int mAtkNum;

	public AtkCooperation(int memberNum, int atkNumber)
		: base(memberNum)
	{
		mAtkNum = atkNumber;
		mAtkTarget = new List<PeEntity>(mAtkNum);
	}

	public override bool AddMember(PeEntity entity)
	{
		if (base.AddMember(entity))
		{
			if (entity.NpcCmpt != null)
			{
				entity.NpcCmpt.SetLineType(ELineType.TeamAtk);
			}
			return true;
		}
		return false;
	}

	public override void DissolveCooper()
	{
		for (int i = 0; i < mCooperMembers.Count; i++)
		{
			mCooperMembers[i].NpcCmpt.BattleMgr.SetSelectEnemy(null);
			mCooperMembers[i].target.SetEnityCanAttack(canAttackOrNot: true);
		}
		mCooperMembers.Clear();
		mAtkTarget.Clear();
	}

	public override bool CanAddMember(params object[] objs)
	{
		if (objs != null && objs.Length > 0)
		{
			PeEntity item = (PeEntity)objs[0];
			return base.CanAddMember() && mAtkTarget.Contains(item);
		}
		return base.CanAddMember();
	}

	public List<PeEntity> GetAtkCooperMembers()
	{
		return mCooperMembers;
	}

	public List<PeEntity> GetAktCooperTarget()
	{
		return mAtkTarget;
	}

	public void SetAtkTarget(PeEntity target)
	{
		for (int i = 0; i < mCooperMembers.Count; i++)
		{
			mCooperMembers[i].NpcCmpt.BattleMgr.ChoiceTheEnmey(mCooperMembers[i], target);
		}
	}

	public void AddAktTarget(PeEntity target)
	{
		if (!HasBeTarget(target))
		{
			mAtkTarget.Clear();
			SetAtkTarget(target);
			mAtkTarget.Add(target);
		}
	}

	public bool RomoveAtkTarget(PeEntity target)
	{
		return mAtkTarget.Remove(target);
	}

	public void OnAtkTargetDeath(SkEntity skSelf, SkEntity skCaster)
	{
		PeEntity component = skSelf.GetComponent<PeEntity>();
		RemoveEnemy(component);
		RomoveAtkTarget(component);
		ClearCooper();
	}

	public void OnAtkTargetDestroy(SkEntity entity)
	{
		PeEntity component = entity.GetComponent<PeEntity>();
		RemoveEnemy(component);
		RomoveAtkTarget(component);
	}

	public void OnAtkTargetLost(PeEntity entity)
	{
		RemoveEnemy(entity);
		RomoveAtkTarget(entity);
	}

	public bool HasBeTarget(PeEntity target)
	{
		return mAtkTarget.Contains(target);
	}

	public bool CanBeTarget(PeEntity target)
	{
		return mAtkTarget.Count < mAtkNum;
	}

	private void RemoveEnemy(PeEntity enmey)
	{
		if (mCooperMembers == null)
		{
			return;
		}
		for (int i = 0; i < mCooperMembers.Count; i++)
		{
			if (mCooperMembers[i] != null && mCooperMembers[i].NpcCmpt != null && !Enemy.IsNullOrInvalid(mCooperMembers[i].NpcCmpt.BattleMgr.choicedEnemy) && mCooperMembers[i].NpcCmpt.BattleMgr.choicedEnemy.entityTarget == enmey)
			{
				mCooperMembers[i].NpcCmpt.BattleMgr.SetSelectEnemy(null);
				mCooperMembers[i].target.SetEnityCanAttack(canAttackOrNot: true);
			}
		}
	}

	private Enemy GetEnemy(PeEntity self, PeEntity target)
	{
		List<Enemy> enemies = self.target.GetEnemies();
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].entityTarget == target)
			{
				return enemies[i];
			}
		}
		return null;
	}
}
