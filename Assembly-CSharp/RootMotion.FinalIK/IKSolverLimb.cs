using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverLimb : IKSolverTrigonometric
{
	[Serializable]
	public enum BendModifier
	{
		Animation,
		Target,
		Parent,
		Arm,
		Goal
	}

	[Serializable]
	public struct AxisDirection
	{
		public Vector3 direction;

		public Vector3 axis;

		public float dot;

		public AxisDirection(Vector3 direction, Vector3 axis)
		{
			this.direction = direction.normalized;
			this.axis = axis.normalized;
			dot = 0f;
		}
	}

	public AvatarIKGoal goal;

	public BendModifier bendModifier;

	[Range(0f, 1f)]
	public float maintainRotationWeight;

	[Range(0f, 1f)]
	public float bendModifierWeight = 1f;

	public Transform bendGoal;

	private bool maintainBendFor1Frame;

	private bool maintainRotationFor1Frame;

	private Quaternion defaultRootRotation;

	private Quaternion parentDefaultRotation;

	private Quaternion bone3RotationBeforeSolve;

	private Quaternion maintainRotation;

	private Quaternion bone3DefaultRotation;

	private Vector3 _bendNormal;

	private Vector3 animationNormal;

	private AxisDirection[] axisDirections = new AxisDirection[0];

	public IKSolverLimb()
	{
	}

	public IKSolverLimb(AvatarIKGoal goal)
	{
		this.goal = goal;
	}

	public void MaintainRotation()
	{
		if (base.initiated)
		{
			maintainRotation = bone3.transform.rotation;
			maintainRotationFor1Frame = true;
		}
	}

	public void MaintainBend()
	{
		if (base.initiated)
		{
			animationNormal = bone1.GetBendNormalFromCurrentRotation();
			maintainBendFor1Frame = true;
		}
	}

	protected override void OnInitiateVirtual()
	{
		defaultRootRotation = root.rotation;
		if (bone1.transform.parent != null)
		{
			parentDefaultRotation = Quaternion.Inverse(defaultRootRotation) * bone1.transform.parent.rotation;
		}
		if (bone3.rotationLimit != null)
		{
			bone3.rotationLimit.Disable();
		}
		bone3DefaultRotation = bone3.transform.rotation;
		Vector3 vector = Vector3.Cross(bone2.transform.position - bone1.transform.position, bone3.transform.position - bone2.transform.position);
		if (vector != Vector3.zero)
		{
			bendNormal = vector;
		}
		animationNormal = bendNormal;
		if (goal == AvatarIKGoal.LeftHand || goal == AvatarIKGoal.RightHand)
		{
			StoreAxisDirections();
		}
	}

	protected override void OnUpdateVirtual()
	{
		if (IKPositionWeight > 0f)
		{
			bendModifierWeight = Mathf.Clamp(bendModifierWeight, 0f, 1f);
			maintainRotationWeight = Mathf.Clamp(maintainRotationWeight, 0f, 1f);
			_bendNormal = bendNormal;
			bendNormal = GetModifiedBendNormal();
		}
		if (maintainRotationWeight * IKPositionWeight > 0f)
		{
			bone3RotationBeforeSolve = ((!maintainRotationFor1Frame) ? bone3.transform.rotation : maintainRotation);
			maintainRotationFor1Frame = false;
		}
	}

	protected override void OnPostSolveVirtual()
	{
		if (IKPositionWeight > 0f)
		{
			bendNormal = _bendNormal;
		}
		if (maintainRotationWeight * IKPositionWeight > 0f)
		{
			bone3.transform.rotation = Quaternion.Slerp(bone3.transform.rotation, bone3RotationBeforeSolve, maintainRotationWeight * IKPositionWeight);
		}
	}

	private void StoreAxisDirections()
	{
		AxisDirection axisDirection = new AxisDirection(Vector3.zero, new Vector3(-1f, 0f, 0f));
		AxisDirection axisDirection2 = new AxisDirection(new Vector3(0.5f, 0f, -0.2f), new Vector3(-0.5f, -1f, 1f));
		AxisDirection axisDirection3 = new AxisDirection(new Vector3(-0.5f, -1f, -0.2f), new Vector3(0f, 0.5f, -1f));
		AxisDirection axisDirection4 = new AxisDirection(new Vector3(-0.5f, -0.5f, 1f), new Vector3(-1f, -1f, -1f));
		axisDirections = new AxisDirection[4] { axisDirection, axisDirection2, axisDirection3, axisDirection4 };
	}

	private Vector3 GetModifiedBendNormal()
	{
		float num = bendModifierWeight;
		if (num <= 0f)
		{
			return bendNormal;
		}
		switch (bendModifier)
		{
		case BendModifier.Animation:
			if (!maintainBendFor1Frame)
			{
				MaintainBend();
			}
			maintainBendFor1Frame = false;
			return Vector3.Lerp(bendNormal, animationNormal, num);
		case BendModifier.Parent:
		{
			if (bone1.transform.parent == null)
			{
				return bendNormal;
			}
			Quaternion quaternion = bone1.transform.parent.rotation * Quaternion.Inverse(parentDefaultRotation);
			return Quaternion.Slerp(Quaternion.identity, quaternion * Quaternion.Inverse(defaultRootRotation), num) * bendNormal;
		}
		case BendModifier.Target:
		{
			Quaternion b = IKRotation * Quaternion.Inverse(bone3DefaultRotation);
			return Quaternion.Slerp(Quaternion.identity, b, num) * bendNormal;
		}
		case BendModifier.Arm:
		{
			if (bone1.transform.parent == null)
			{
				return bendNormal;
			}
			if (goal == AvatarIKGoal.LeftFoot || goal == AvatarIKGoal.RightFoot)
			{
				if (!Warning.logged)
				{
					LogWarning("Trying to use the 'Arm' bend modifier on a leg.");
				}
				return bendNormal;
			}
			Vector3 normalized = (IKPosition - bone1.transform.position).normalized;
			normalized = Quaternion.Inverse(bone1.transform.parent.rotation * Quaternion.Inverse(parentDefaultRotation)) * normalized;
			if (goal == AvatarIKGoal.LeftHand)
			{
				normalized.x = 0f - normalized.x;
			}
			for (int i = 1; i < axisDirections.Length; i++)
			{
				axisDirections[i].dot = Mathf.Clamp(Vector3.Dot(axisDirections[i].direction, normalized), 0f, 1f);
				axisDirections[i].dot = Interp.Float(axisDirections[i].dot, InterpolationMode.InOutQuintic);
			}
			Vector3 vector2 = axisDirections[0].axis;
			for (int j = 1; j < axisDirections.Length; j++)
			{
				vector2 = Vector3.Slerp(vector2, axisDirections[j].axis, axisDirections[j].dot);
			}
			if (goal == AvatarIKGoal.LeftHand)
			{
				vector2.x = 0f - vector2.x;
				vector2 = -vector2;
			}
			Vector3 vector3 = bone1.transform.parent.rotation * Quaternion.Inverse(parentDefaultRotation) * vector2;
			if (num >= 1f)
			{
				return vector3;
			}
			return Vector3.Lerp(bendNormal, vector3, num);
		}
		case BendModifier.Goal:
		{
			if (bendGoal == null)
			{
				if (!Warning.logged)
				{
					LogWarning("Trying to use the 'Goal' Bend Modifier, but the Bend Goal is unassigned.");
				}
				return bendNormal;
			}
			Vector3 vector = Vector3.Cross(bendGoal.position - bone1.transform.position, IKPosition - bone1.transform.position);
			if (vector == Vector3.zero)
			{
				return bendNormal;
			}
			if (num >= 1f)
			{
				return vector;
			}
			return Vector3.Lerp(bendNormal, vector, num);
		}
		default:
			return bendNormal;
		}
	}
}
