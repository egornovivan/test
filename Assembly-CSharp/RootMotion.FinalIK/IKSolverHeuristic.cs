using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverHeuristic : IKSolver
{
	public Transform target;

	public float tolerance;

	public int maxIterations = 4;

	public bool useRotationLimits = true;

	public Bone[] bones = new Bone[0];

	protected Vector3 lastLocalDirection;

	protected float chainLength;

	protected virtual int minBones => 2;

	protected virtual bool boneLengthCanBeZero => true;

	protected virtual bool allowCommonParent => false;

	protected virtual Vector3 localDirection => bones[0].transform.InverseTransformDirection(bones[bones.Length - 1].transform.position - bones[0].transform.position);

	protected float positionOffset => Vector3.SqrMagnitude(localDirection - lastLocalDirection);

	public bool SetChain(Transform[] hierarchy, Transform root)
	{
		if (bones == null || bones.Length != hierarchy.Length)
		{
			bones = new Bone[hierarchy.Length];
		}
		for (int i = 0; i < hierarchy.Length; i++)
		{
			if (bones[i] == null)
			{
				bones[i] = new Bone();
			}
			bones[i].transform = hierarchy[i];
		}
		Initiate(root);
		return base.initiated;
	}

	public override void StoreDefaultLocalState()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].StoreDefaultLocalState();
		}
	}

	public override void FixTransforms()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].FixTransform();
		}
	}

	public override bool IsValid(bool log)
	{
		if (bones.Length == 0)
		{
			if (log)
			{
				LogWarning("IK chain has no bones. Can not initiate solver.");
			}
			return false;
		}
		if (bones.Length < minBones)
		{
			if (log)
			{
				LogWarning("IK chain has less than " + minBones + " bones. Can not initiate solver.");
			}
			return false;
		}
		Bone[] array = bones;
		foreach (Bone bone in array)
		{
			if (bone.transform == null)
			{
				if (log)
				{
					LogWarning("Bone transform is null in IK chain. Can not initiate solver.");
				}
				return false;
			}
		}
		if (!allowCommonParent && !IKSolver.HierarchyIsValid(bones))
		{
			if (log)
			{
				LogWarning("IK requires for it's bones to be parented to each other. Invalid bone hierarchy detected.");
			}
			return false;
		}
		Transform transform = IKSolver.ContainsDuplicateBone(bones);
		if (transform != null)
		{
			if (log)
			{
				LogWarning(transform.name + " is represented multiple times in a single IK chain. Can nott initiate solver.");
			}
			return false;
		}
		if (!boneLengthCanBeZero)
		{
			for (int j = 0; j < bones.Length - 1; j++)
			{
				float magnitude = (bones[j].transform.position - bones[j + 1].transform.position).magnitude;
				if (magnitude == 0f)
				{
					if (log)
					{
						LogWarning("Bone " + j + " length is zero. Can nott initiate solver.");
					}
					return false;
				}
			}
		}
		return true;
	}

	public override Point[] GetPoints()
	{
		return bones;
	}

	public override Point GetPoint(Transform transform)
	{
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i].transform == transform)
			{
				return bones[i];
			}
		}
		return null;
	}

	protected override void OnInitiate()
	{
	}

	protected override void OnUpdate()
	{
	}

	protected void InitiateBones()
	{
		chainLength = 0f;
		for (int i = 0; i < bones.Length; i++)
		{
			if (i < bones.Length - 1)
			{
				bones[i].length = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
				chainLength += bones[i].length;
				Vector3 position = bones[i + 1].transform.position;
				bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (position - bones[i].transform.position);
				if (bones[i].rotationLimit != null)
				{
					bones[i].rotationLimit.Disable();
				}
			}
			else
			{
				bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (bones[bones.Length - 1].transform.position - bones[0].transform.position);
			}
		}
	}

	protected Vector3 GetSingularityOffset()
	{
		if (!SingularityDetected())
		{
			return Vector3.zero;
		}
		Vector3 normalized = (IKPosition - bones[0].transform.position).normalized;
		Vector3 rhs = new Vector3(normalized.y, normalized.z, normalized.x);
		if (useRotationLimits && bones[bones.Length - 2].rotationLimit != null && bones[bones.Length - 2].rotationLimit is RotationLimitHinge)
		{
			rhs = bones[bones.Length - 2].transform.rotation * bones[bones.Length - 2].rotationLimit.axis;
		}
		return Vector3.Cross(normalized, rhs) * bones[bones.Length - 2].length * 0.5f;
	}

	private bool SingularityDetected()
	{
		if (!base.initiated)
		{
			return false;
		}
		Vector3 vector = bones[bones.Length - 1].transform.position - bones[0].transform.position;
		Vector3 vector2 = IKPosition - bones[0].transform.position;
		float magnitude = vector.magnitude;
		float magnitude2 = vector2.magnitude;
		if (magnitude < magnitude2)
		{
			return false;
		}
		if (magnitude < chainLength - bones[bones.Length - 2].length * 0.1f)
		{
			return false;
		}
		if (magnitude == 0f)
		{
			return false;
		}
		if (magnitude2 == 0f)
		{
			return false;
		}
		if (magnitude2 > magnitude)
		{
			return false;
		}
		float num = Vector3.Dot(vector / magnitude, vector2 / magnitude2);
		if (num < 0.999f)
		{
			return false;
		}
		return true;
	}
}
