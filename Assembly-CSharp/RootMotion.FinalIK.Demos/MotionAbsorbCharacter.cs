using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class MotionAbsorbCharacter : MonoBehaviour
{
	public Animator animator;

	public MotionAbsorb motionAbsorb;

	public Transform cube;

	public float cubeRandomPosition = 0.1f;

	public AnimationCurve motionAbsorbWeight;

	private Vector3 cubeDefaultPosition;

	private AnimatorStateInfo info;

	private void Start()
	{
		cubeDefaultPosition = cube.position;
	}

	private void Update()
	{
		info = animator.GetCurrentAnimatorStateInfo(0);
		motionAbsorb.weight = motionAbsorbWeight.Evaluate(info.normalizedTime - (float)(int)info.normalizedTime);
	}

	private void SwingStart()
	{
		cube.GetComponent<Rigidbody>().MovePosition(cubeDefaultPosition + Random.insideUnitSphere * cubeRandomPosition);
		cube.GetComponent<Rigidbody>().MoveRotation(Quaternion.identity);
		cube.GetComponent<Rigidbody>().velocity = Vector3.zero;
		cube.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	}
}
