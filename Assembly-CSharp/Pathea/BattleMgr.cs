using System.Collections.Generic;
using ItemAsset;
using PETools;

namespace Pathea;

public class BattleMgr
{
	private static float MaxDis = 128f;

	private PeEntity mSelf;

	private Enemy mChoicedEnemy;

	public Enemy choicedEnemy
	{
		get
		{
			if (!Enemy.IsNullOrInvalid(mChoicedEnemy))
			{
				mChoicedEnemy.Update();
			}
			return mChoicedEnemy;
		}
	}

	public BattleMgr(PeEntity entity)
	{
		mSelf = entity;
	}

	private float CalculateWeight(float _hp, float _dis)
	{
		float num = ((!(_dis > MaxDis)) ? _dis : MaxDis);
		float num2 = 1f - num / MaxDis;
		return 0.3f * _hp + 0.7f * num2;
	}

	public Enemy CompareEnemy(Enemy one, Enemy other)
	{
		float num = CalculateWeight(one.entity.HPPercent, PEUtil.MagnitudeH(mSelf.position, one.position));
		float num2 = CalculateWeight(other.entity.HPPercent, PEUtil.MagnitudeH(mSelf.position, other.position));
		if (num <= num2)
		{
			return one;
		}
		return other;
	}

	public Enemy ChoiceEnemy(List<Enemy> enemies)
	{
		if (enemies == null || enemies.Count <= 0)
		{
			return null;
		}
		Enemy enemy = enemies[0];
		for (int i = 0; i < enemies.Count; i++)
		{
			if (!Enemy.IsNullOrInvalid(enemies[i]) && enemies[i].canAttacked && SelectItem.MatchEnemyAttack(mSelf, enemies[i].entityTarget) && enemies[i].Distance <= MaxDis && EnmeyTargetIsAlliance(enemies[i]))
			{
				enemy = CompareEnemy(enemy, enemies[i]);
			}
		}
		return (Enemy.IsNullOrInvalid(enemy) || enemy.entityTarget.isRagdoll || !(enemy.Distance <= MaxDis)) ? null : enemy;
	}

	public void SetSelectEnemy(Enemy one)
	{
		mChoicedEnemy = one;
	}

	public bool ChoiceTheEnmey(PeEntity self, PeEntity target)
	{
		List<Enemy> enemies = self.target.GetEnemies();
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].entityTarget == target)
			{
				mChoicedEnemy = enemies[i];
				return true;
			}
		}
		self.target.AddSharedHatred(target, 5f);
		return false;
	}

	private bool EnmeyTargetIsAlliance(Enemy enemy)
	{
		if (enemy.entityTarget == null || enemy.entityTarget.target == null)
		{
			return true;
		}
		Enemy attackEnemy = enemy.entityTarget.target.GetAttackEnemy();
		if (attackEnemy == null)
		{
			return true;
		}
		int playerID = (int)attackEnemy.entityTarget.GetAttribute(AttribType.DefaultPlayerID);
		if (GameConfig.IsMultiClient)
		{
			if (Singleton<ForceSetting>.Instance.GetForceType(playerID) == EPlayerType.Human)
			{
				return true;
			}
		}
		else if (Singleton<ForceSetting>.Instance.GetForceID(playerID) == 1)
		{
			return true;
		}
		return false;
	}

	public bool CanChoiceEnemy(List<Enemy> enemies)
	{
		mChoicedEnemy = ChoiceEnemy(enemies);
		return mChoicedEnemy != null;
	}
}
