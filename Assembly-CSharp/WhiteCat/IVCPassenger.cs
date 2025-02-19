using UnityEngine;

namespace WhiteCat;

public interface IVCPassenger
{
	void GetOn(string sitAnimName, VCPBaseSeat seat);

	void GetOff();

	void Sync(Vector3 position, Quaternion rotation);

	void SetHands(Transform left, Transform right);
}
