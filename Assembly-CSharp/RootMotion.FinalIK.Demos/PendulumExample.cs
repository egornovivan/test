using UnityEngine;

namespace RootMotion.FinalIK.Demos;

[RequireComponent(typeof(FullBodyBipedIK))]
public class PendulumExample : MonoBehaviour
{
	[SerializeField]
	private Transform target;

	[SerializeField]
	private Transform leftHandTarget;

	[SerializeField]
	private Transform rightHandTarget;

	[SerializeField]
	private Transform leftFootTarget;

	[SerializeField]
	private Transform rightFootTarget;

	[SerializeField]
	private Transform pelvisTarget;

	[SerializeField]
	private Transform bodyTarget;

	[SerializeField]
	private Transform headTarget;

	[SerializeField]
	private Vector3 pelvisDownAxis = Vector3.right;

	public float hangingDistanceMlp = 1.3f;

	private FullBodyBipedIK ik;

	private Quaternion rootRelativeToPelvis;

	private Vector3 pelvisToRoot;

	private void Start()
	{
		ik = GetComponent<FullBodyBipedIK>();
		Quaternion rotation = target.rotation;
		target.rotation = leftHandTarget.rotation;
		FixedJoint fixedJoint = target.gameObject.AddComponent<FixedJoint>();
		fixedJoint.connectedBody = leftHandTarget.GetComponent<Rigidbody>();
		target.rotation = rotation;
		rootRelativeToPelvis = Quaternion.Inverse(pelvisTarget.rotation) * base.transform.rotation;
		pelvisToRoot = Quaternion.Inverse(ik.references.pelvis.rotation) * (base.transform.position - ik.references.pelvis.position);
		ik.solver.leftHandEffector.positionWeight = 1f;
		ik.solver.leftHandEffector.rotationWeight = 1f;
	}

	private void LateUpdate()
	{
		base.transform.rotation = pelvisTarget.rotation * rootRelativeToPelvis;
		base.transform.position = pelvisTarget.position + pelvisTarget.rotation * pelvisToRoot * hangingDistanceMlp;
		ik.solver.leftHandEffector.position = leftHandTarget.position;
		ik.solver.leftHandEffector.rotation = leftHandTarget.rotation;
		Vector3 fromDirection = ik.references.pelvis.rotation * pelvisDownAxis;
		Quaternion quaternion = Quaternion.FromToRotation(fromDirection, rightHandTarget.position - headTarget.position);
		ik.references.rightUpperArm.rotation = quaternion * ik.references.rightUpperArm.rotation;
		Quaternion quaternion2 = Quaternion.FromToRotation(fromDirection, leftFootTarget.position - bodyTarget.position);
		ik.references.leftThigh.rotation = quaternion2 * ik.references.leftThigh.rotation;
		Quaternion quaternion3 = Quaternion.FromToRotation(fromDirection, rightFootTarget.position - bodyTarget.position);
		ik.references.rightThigh.rotation = quaternion3 * ik.references.rightThigh.rotation;
	}
}
