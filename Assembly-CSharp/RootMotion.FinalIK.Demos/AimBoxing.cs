using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class AimBoxing : MonoBehaviour
{
	public AimIK aimIK;

	public Transform pin;

	private void LateUpdate()
	{
		aimIK.solver.transform.LookAt(pin.position);
		aimIK.solver.IKPosition = base.transform.position;
	}
}
