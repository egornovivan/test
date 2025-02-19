using UnityEngine;

public interface IAimWeapon
{
	bool Aimed { get; }

	void SetAimState(bool aimState);

	void SetTarget(Vector3 aimPos);

	void SetTarget(Transform trans);
}
