using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class FBIKDrivingRig : MonoBehaviour
{
	public FullBodyBipedIK ik;

	public Transform leftHandPoseTarget;

	public Transform rightHandPoseTarget;

	private HandPoser[] handPosers;

	private void Start()
	{
		if (leftHandPoseTarget != null)
		{
			handPosers = new HandPoser[2]
			{
				ik.solver.leftHandEffector.bone.gameObject.AddComponent<HandPoser>(),
				ik.solver.rightHandEffector.bone.gameObject.AddComponent<HandPoser>()
			};
			handPosers[0].poseRoot = leftHandPoseTarget;
			handPosers[1].poseRoot = rightHandPoseTarget;
		}
	}

	private void LateUpdate()
	{
		HandPoser[] array = handPosers;
		foreach (HandPoser handPoser in array)
		{
			handPoser.localRotationWeight = ik.solver.IKPositionWeight;
		}
	}
}
