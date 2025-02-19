using UnityEngine;

namespace RootMotion.FinalIK.Demos;

[RequireComponent(typeof(AimIK))]
[RequireComponent(typeof(FullBodyBipedIK))]
public class AnimatorController3rdPersonIK : AnimatorController3rdPerson
{
	[SerializeField]
	private bool useIK = true;

	[SerializeField]
	private Transform rightHandTarget;

	[SerializeField]
	private Transform leftHandTarget;

	[SerializeField]
	private Transform head;

	[SerializeField]
	private Vector3 headLookAxis = Vector3.forward;

	[SerializeField]
	private float headLookWeight = 1f;

	[SerializeField]
	private Camera firstPersonCam;

	private AimIK aim;

	private FullBodyBipedIK ik;

	private Quaternion rightHandRotation;

	private Quaternion fpsCamDefaultRot;

	private IKEffector leftHand => ik.solver.leftHandEffector;

	private IKEffector rightHand => ik.solver.rightHandEffector;

	private void OnGUI()
	{
		GUILayout.Label("Press F to switch Final IK on/off");
		GUILayout.Label("Press C to toggle between 3rd person/1st person camera");
	}

	protected override void Start()
	{
		base.Start();
		aim = GetComponent<AimIK>();
		ik = GetComponent<FullBodyBipedIK>();
		aim.Disable();
		ik.Disable();
		fpsCamDefaultRot = firstPersonCam.transform.localRotation;
	}

	public override void Move(Vector3 moveInput, bool isMoving, Vector3 faceDirection, Vector3 aimTarget)
	{
		base.Move(moveInput, isMoving, faceDirection, aimTarget);
		if (Input.GetKeyDown(KeyCode.C))
		{
			firstPersonCam.enabled = !firstPersonCam.enabled;
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			useIK = !useIK;
		}
		if (useIK)
		{
			aim.solver.IKPosition = aimTarget;
			FBBIKPass1();
			aim.solver.Update();
			FBBIKPass2();
			HeadLookAt(aimTarget);
			if (firstPersonCam.enabled)
			{
				StabilizeFPSCamera();
			}
		}
	}

	private void FBBIKPass1()
	{
		rightHandRotation = rightHandTarget.rotation;
		rightHand.position = rightHandTarget.position;
		rightHand.positionWeight = 1f;
		leftHand.positionWeight = 0f;
		ik.solver.Update();
		rightHand.bone.rotation = rightHandRotation;
	}

	private void FBBIKPass2()
	{
		rightHand.position = rightHand.bone.position;
		rightHandRotation = rightHand.bone.rotation;
		leftHand.position = leftHandTarget.position;
		leftHand.positionWeight = 1f;
		ik.solver.Update();
		rightHand.bone.rotation = rightHandRotation;
		leftHand.bone.rotation = leftHandTarget.rotation;
	}

	private void HeadLookAt(Vector3 lookAtTarget)
	{
		if (!(head == null))
		{
			Quaternion b = Quaternion.FromToRotation(head.rotation * headLookAxis, lookAtTarget - head.position);
			head.rotation = Quaternion.Lerp(Quaternion.identity, b, headLookWeight) * head.rotation;
		}
	}

	private void StabilizeFPSCamera()
	{
		firstPersonCam.transform.localRotation = fpsCamDefaultRot;
		Vector3 normal = firstPersonCam.transform.forward;
		Vector3 tangent = firstPersonCam.transform.up;
		Vector3.OrthoNormalize(ref normal, ref tangent);
		normal = firstPersonCam.transform.forward;
		Vector3 tangent2 = Vector3.up;
		Vector3.OrthoNormalize(ref normal, ref tangent2);
		Quaternion b = Quaternion.FromToRotation(tangent, tangent2);
		float t = Vector3.Dot(base.transform.forward, firstPersonCam.transform.forward);
		firstPersonCam.transform.rotation = Quaternion.Lerp(Quaternion.identity, b, t) * firstPersonCam.transform.rotation;
	}
}
