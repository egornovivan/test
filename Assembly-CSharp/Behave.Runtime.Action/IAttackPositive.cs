using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

public interface IAttackPositive : IAttack
{
	float MinRange { get; }

	float MaxRange { get; }

	float MinHeight { get; }

	float MaxHeight { get; }

	Vector3 GetAttackPosition(Enemy enemy);

	bool IsInRange(Enemy enemy);

	bool IsInAngle(Enemy enemy);
}
