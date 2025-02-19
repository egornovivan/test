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

	private Vector3 leftFootPos = Vector3.zero;

	private Vector3 rightFootPos = Vector3.zero;

	private Vector3 bodyPos = Vector3.zero;

	[HideInInspector]
	public Vector3 FootDir;
}
