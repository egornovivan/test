using UnityEngine;

namespace Railway;

public interface IPassenger
{
	void GetOn(string pose);

	void GetOff(Vector3 pos);

	void UpdateTrans(Transform trans);
}
