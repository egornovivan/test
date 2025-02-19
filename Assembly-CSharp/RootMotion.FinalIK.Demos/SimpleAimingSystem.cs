using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class SimpleAimingSystem : MonoBehaviour
{
	public AimPoser aimPoser;

	public AimIK aim;

	public LookAtIK lookAt;

	public Transform recursiveMixingTransform;

	[HideInInspector]
	public Vector3 targetPosition;

	private AimPoser.Pose aimPose;

	private AimPoser.Pose lastPose;

	private void Start()
	{
		AimPoser.Pose[] poses = aimPoser.poses;
		foreach (AimPoser.Pose pose in poses)
		{
			GetComponent<Animation>()[pose.name].AddMixingTransform(recursiveMixingTransform, recursive: true);
		}
		aim.Disable();
		lookAt.Disable();
	}

	private void LateUpdate()
	{
		Pose();
		aim.solver.SetIKPosition(targetPosition);
		lookAt.solver.SetIKPosition(targetPosition);
		aim.solver.Update();
		lookAt.solver.Update();
	}

	private void Pose()
	{
		Vector3 direction = targetPosition - aim.solver.bones[0].transform.position;
		Vector3 localDirection = base.transform.InverseTransformDirection(direction);
		aimPose = aimPoser.GetPose(localDirection);
		if (aimPose != lastPose)
		{
			GetComponent<Animation>().CrossFade(aimPose.name);
			aimPoser.SetPoseActive(aimPose);
			lastPose = aimPose;
		}
	}
}
