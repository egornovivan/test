using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class ExplosionDemo : MonoBehaviour
{
	public FullBodyBipedIK ik;

	public Rigidbody rigidbody;

	public float forceMlp = 1f;

	public float upForce = 1f;

	public float weightFalloffSpeed = 1f;

	public AnimationCurve weightFalloff;

	public AnimationCurve explosionForceByDistance;

	public AnimationCurve scale;

	private float weight;

	private Vector3 defaultScale = Vector3.one;

	private void Start()
	{
		defaultScale = base.transform.localScale;
	}

	private void Update()
	{
		weight = Mathf.Clamp(weight - Time.deltaTime * weightFalloffSpeed, 0f, 1f);
		if (Input.GetKeyDown(KeyCode.E))
		{
			ik.solver.IKPositionWeight = 1f;
			ik.solver.leftHandEffector.position = ik.solver.leftHandEffector.bone.position;
			ik.solver.rightHandEffector.position = ik.solver.rightHandEffector.bone.position;
			ik.solver.leftFootEffector.position = ik.solver.leftFootEffector.bone.position;
			ik.solver.rightFootEffector.position = ik.solver.rightFootEffector.bone.position;
			weight = 1f;
			Vector3 vector = rigidbody.position - base.transform.position;
			float num = explosionForceByDistance.Evaluate(vector.magnitude);
			rigidbody.AddForce((vector.normalized + Vector3.up * upForce) * num * forceMlp, ForceMode.VelocityChange);
		}
		SetEffectorWeights(weightFalloff.Evaluate(weight));
		base.transform.localScale = scale.Evaluate(weight) * defaultScale;
	}

	private void SetEffectorWeights(float w)
	{
		ik.solver.leftHandEffector.positionWeight = w;
		ik.solver.rightHandEffector.positionWeight = w;
		ik.solver.leftFootEffector.positionWeight = w;
		ik.solver.rightFootEffector.positionWeight = w;
	}
}
