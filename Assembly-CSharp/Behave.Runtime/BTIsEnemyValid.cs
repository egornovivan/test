using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTIsEnemyValid), "IsEnemyValid")]
public class BTIsEnemyValid : BTNormal
{
	private class Data
	{
	}

	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy) || base.selectattackEnemy == null)
		{
			return BehaveResult.Success;
		}
		if (base.selectattackEnemy.entityTarget.target != null)
		{
			Enemy enemy = base.selectattackEnemy.entityTarget.target.GetAttackEnemy();
			if (enemy == null)
			{
				Debug.Log("IsEnemyValid    Success");
				return BehaveResult.Success;
			}
			int playerID = (int)enemy.entityTarget.GetAttribute(AttribType.DefaultPlayerID);
			if (GameConfig.IsMultiClient)
			{
				if (global::Singleton<ForceSetting>.Instance.GetForceType(playerID) == EPlayerType.Human)
				{
					return BehaveResult.Failure;
				}
			}
			else if (global::Singleton<ForceSetting>.Instance.GetForceID(playerID) == 1)
			{
				return BehaveResult.Failure;
			}
			Debug.Log("IsEnemyValid    Success");
		}
		return BehaveResult.Success;
	}
}
