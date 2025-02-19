using System.Collections.Generic;
using UnityEngine;

namespace Pathea;

public class FindHidePos
{
	private Transform _player;

	private float _radius;

	private static float PLAYER_R = 3f;

	private static float ENEMY_R = 10f;

	private static float K_R = 0.1f;

	private static float K_R1 = 30f;

	private bool _bNeedHide;

	private float _mEnemyR;

	public bool bNeedHide => _bNeedHide;

	public FindHidePos(float radius, bool needHide, float enemyR = 10f)
	{
		_radius = radius;
		_bNeedHide = needHide;
		_mEnemyR = enemyR;
	}

	public Vector3 GetHideDir(Vector3 _playerPos, Vector3 npcPos, List<Enemy> hideEntities)
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = _playerPos - npcPos;
		_bNeedHide = false;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < hideEntities.Count; i++)
		{
			Vector3 position = hideEntities[i].position;
			position.y = npcPos.y;
			Vector3 vector2 = npcPos - position;
			if (vector2.magnitude < _mEnemyR)
			{
				flag = true;
				zero += _mEnemyR / vector2.magnitude * vector2.normalized;
			}
		}
		if (vector.magnitude < PLAYER_R)
		{
			flag2 = true;
		}
		if (flag2)
		{
			_bNeedHide = true;
			return zero + PLAYER_R / vector.magnitude * (npcPos - _playerPos).normalized;
		}
		if (!flag)
		{
			_bNeedHide = false;
			return Vector3.zero;
		}
		_bNeedHide = true;
		if (vector.magnitude > PLAYER_R + K_R1)
		{
			zero += K_R * vector.magnitude * vector.normalized;
		}
		return zero;
	}
}
