using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Motion_FootIK_Human : MonoBehaviour
{
	public Transform LeftFootBone;

	public Transform RightFootBone;

	public float MaxStepHeight = 0.4f;

	public float AnimChangeSpeed = 10f;

	[HideInInspector]
	public bool mGrounded = true;

	public float BodyOffset = 0.05f;

	public float FootOffset;

	private float LeftFootIKHeight;

	private float RightFootIKHeight;

	private float BodyIKHeight;

	public Animator mAnimator;

	private PhysicsCharacterMotor mPhyMotor;

	private bool UPD;

	[HideInInspector]
	public Vector3 FootDir;
}
