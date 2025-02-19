using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class CarryBoxDemo : MonoBehaviour
{
	public FullBodyBipedIK ik;

	public Transform leftHandTarget;

	public Transform rightHandTarget;

	private void LateUpdate()
	{
		ik.solver.leftHandEffector.position = leftHandTarget.position;
		ik.solver.leftHandEffector.rotation = leftHandTarget.rotation;
		ik.solver.rightHandEffector.position = rightHandTarget.position;
		ik.solver.rightHandEffector.rotation = rightHandTarget.rotation;
	}
}
