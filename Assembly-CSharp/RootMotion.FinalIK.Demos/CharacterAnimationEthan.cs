using System;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

[RequireComponent(typeof(Animator))]
public class CharacterAnimationEthan : CharacterAnimationThirdPerson
{
	[SerializeField]
	private FullBodyBipedIK ik;

	[SerializeField]
	private float turnSensitivity = 0.2f;

	[SerializeField]
	private float turnSpeed = 5f;

	[SerializeField]
	private float runCycleLegOffset = 0.2f;

	[Range(0.1f, 3f)]
	[SerializeField]
	private float animSpeedMultiplier = 1f;

	private Animator animator;

	private Vector3 lastForward;

	protected override void Start()
	{
		base.Start();
		animator = GetComponent<Animator>();
		lastForward = base.transform.forward;
		IKSolverFullBodyBiped solver = ik.solver;
		solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
	}

	public override Vector3 GetPivotPoint()
	{
		return animator.pivotPosition;
	}

	public override void UpdateState(CharacterThirdPerson.AnimState state)
	{
		if (Time.deltaTime != 0f)
		{
			base.animationGrounded = animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded Directional") || animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded Strafe");
			if (state.jump)
			{
				float num = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1f);
				float value = (float)((num < 0f) ? 1 : (-1)) * state.moveDirection.z;
				animator.SetFloat("JumpLeg", value);
			}
			float num2 = 0f - GetAngleFromForward(lastForward);
			lastForward = base.transform.forward;
			num2 *= turnSensitivity * 0.01f;
			num2 = Mathf.Clamp(num2 / Time.deltaTime, -1f, 1f);
			animator.SetFloat("Turn", Mathf.Lerp(animator.GetFloat("Turn"), num2, Time.deltaTime * turnSpeed));
			animator.SetFloat("Forward", state.moveDirection.z);
			animator.SetFloat("Right", state.moveDirection.x);
			animator.SetBool("Crouch", state.crouch);
			animator.SetBool("OnGround", state.onGround);
			animator.SetBool("IsStrafing", state.isStrafing);
			if (!state.onGround)
			{
				animator.SetFloat("Jump", state.yVelocity);
			}
			if (state.onGround && state.moveDirection.z > 0f)
			{
				animator.speed = animSpeedMultiplier;
			}
			else
			{
				animator.speed = 1f;
			}
		}
	}

	private void AfterFBBIK()
	{
		UpdateCamera();
	}

	protected override void LateUpdate()
	{
		Follow();
		if (!(Vector3.Angle(base.transform.up, Vector3.up) <= 0.01f))
		{
			Quaternion rotation = Quaternion.FromToRotation(base.transform.up, Vector3.up);
			RotateEffector(ik.solver.bodyEffector, rotation, 0.1f);
			RotateEffector(ik.solver.leftShoulderEffector, rotation, 0.2f);
			RotateEffector(ik.solver.rightShoulderEffector, rotation, 0.2f);
			RotateEffector(ik.solver.leftHandEffector, rotation, 0.1f);
			RotateEffector(ik.solver.rightHandEffector, rotation, 0.1f);
		}
	}

	private void RotateEffector(IKEffector effector, Quaternion rotation, float mlp)
	{
		Vector3 vector = effector.bone.position - base.transform.position;
		Vector3 vector2 = rotation * vector;
		Vector3 vector3 = vector2 - vector;
		effector.positionOffset += vector3 * mlp;
	}

	private void OnAnimatorMove()
	{
		character.Move(animator.deltaPosition);
	}
}
