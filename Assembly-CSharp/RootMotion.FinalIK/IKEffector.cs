using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKEffector
{
	public Transform bone;

	public Transform target;

	[Range(0f, 1f)]
	public float positionWeight;

	[Range(0f, 1f)]
	public float rotationWeight;

	public Vector3 position = Vector3.zero;

	public Quaternion rotation = Quaternion.identity;

	public Vector3 positionOffset;

	public bool effectChildNodes = true;

	[Range(0f, 1f)]
	public float maintainRelativePositionWeight;

	public Transform[] childBones;

	public Transform planeBone1;

	public Transform planeBone2;

	public Transform planeBone3;

	public Quaternion planeRotationOffset = Quaternion.identity;

	private IKSolver.Node node = new IKSolver.Node();

	private IKSolver.Node planeNode1 = new IKSolver.Node();

	private IKSolver.Node planeNode2 = new IKSolver.Node();

	private IKSolver.Node planeNode3 = new IKSolver.Node();

	private IKSolver.Node[] childNodes;

	private IKSolver solver;

	private float posW;

	private float rotW;

	private Vector3[] localPositions;

	private bool usePlaneNodes;

	private Quaternion animatedPlaneRotation = Quaternion.identity;

	private Vector3 animatedPosition;

	private bool firstUpdate;

	public bool isEndEffector { get; private set; }

	private Quaternion planeRotation
	{
		get
		{
			Vector3 vector = planeNode2.solverPosition - planeNode1.solverPosition;
			Vector3 upwards = planeNode3.solverPosition - planeNode1.solverPosition;
			if (vector == Vector3.zero)
			{
				Warning.Log("Make sure you are not placing 2 or more FBBIK effectors of the same chain to exactly the same position.", bone);
				return Quaternion.identity;
			}
			return Quaternion.LookRotation(vector, upwards);
		}
	}

	public IKEffector()
	{
	}

	public IKEffector(Transform bone, Transform[] childBones)
	{
		this.bone = bone;
		this.childBones = childBones;
	}

	public IKSolver.Node GetNode()
	{
		return node;
	}

	public void PinToBone(float positionWeight, float rotationWeight)
	{
		position = bone.position;
		this.positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
		rotation = bone.rotation;
		this.rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
	}

	public bool IsValid(IKSolver solver, Warning.Logger logger)
	{
		if (bone == null)
		{
			logger?.Invoke("IK Effector bone is null.");
			return false;
		}
		if (solver.GetPoint(bone) == null)
		{
			logger?.Invoke("IK Effector is referencing to a bone '" + bone.name + "' that does not excist in the Node Chain.");
			return false;
		}
		Transform[] array = childBones;
		foreach (Transform transform in array)
		{
			if (transform == null)
			{
				logger?.Invoke("IK Effector contains a null reference.");
				return false;
			}
		}
		Transform[] array2 = childBones;
		foreach (Transform transform2 in array2)
		{
			if (solver.GetPoint(transform2) == null)
			{
				logger?.Invoke("IK Effector is referencing to a bone '" + transform2.name + "' that does not excist in the Node Chain.");
				return false;
			}
		}
		if (planeBone1 != null && solver.GetPoint(planeBone1) == null)
		{
			logger?.Invoke("IK Effector is referencing to a bone '" + planeBone1.name + "' that does not excist in the Node Chain.");
			return false;
		}
		if (planeBone2 != null && solver.GetPoint(planeBone2) == null)
		{
			logger?.Invoke("IK Effector is referencing to a bone '" + planeBone2.name + "' that does not excist in the Node Chain.");
			return false;
		}
		if (planeBone3 != null && solver.GetPoint(planeBone3) == null)
		{
			logger?.Invoke("IK Effector is referencing to a bone '" + planeBone3.name + "' that does not excist in the Node Chain.");
			return false;
		}
		return true;
	}

	public void Initiate(IKSolver solver)
	{
		this.solver = solver;
		position = bone.position;
		rotation = bone.rotation;
		animatedPlaneRotation = Quaternion.identity;
		node = solver.GetPoint(bone) as IKSolver.Node;
		if (childNodes == null || childNodes.Length != childBones.Length)
		{
			childNodes = new IKSolver.Node[childBones.Length];
		}
		for (int i = 0; i < childBones.Length; i++)
		{
			childNodes[i] = solver.GetPoint(childBones[i]) as IKSolver.Node;
		}
		if (localPositions == null || localPositions.Length != childBones.Length)
		{
			localPositions = new Vector3[childBones.Length];
		}
		usePlaneNodes = false;
		if (planeBone1 != null)
		{
			planeNode1 = solver.GetPoint(planeBone1) as IKSolver.Node;
			if (planeBone2 != null)
			{
				planeNode2 = solver.GetPoint(planeBone2) as IKSolver.Node;
				if (planeBone3 != null)
				{
					planeNode3 = solver.GetPoint(planeBone3) as IKSolver.Node;
					usePlaneNodes = true;
				}
			}
			isEndEffector = true;
		}
		else
		{
			isEndEffector = false;
		}
	}

	public void ResetOffset()
	{
		node.offset = Vector3.zero;
		if (childNodes != null)
		{
			for (int i = 0; i < childNodes.Length; i++)
			{
				childNodes[i].offset = Vector3.zero;
			}
		}
	}

	public void SetToTarget()
	{
		if (!(target == null))
		{
			position = target.position;
			rotation = target.rotation;
		}
	}

	public void OnPreSolve(bool fullBody)
	{
		positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
		rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		maintainRelativePositionWeight = Mathf.Clamp(maintainRelativePositionWeight, 0f, 1f);
		posW = positionWeight * solver.GetIKPositionWeight();
		rotW = rotationWeight * solver.GetIKPositionWeight();
		node.effectorPositionWeight = posW;
		node.effectorRotationWeight = rotW;
		node.solverRotation = rotation;
		if (float.IsInfinity(positionOffset.x) || float.IsInfinity(positionOffset.y) || float.IsInfinity(positionOffset.z))
		{
			Debug.LogError("Invalid IKEffector.positionOffset (contains Infinity)! Please make sure not to set IKEffector.positionOffset to infinite values.", bone);
		}
		if (float.IsNaN(positionOffset.x) || float.IsNaN(positionOffset.y) || float.IsNaN(positionOffset.z))
		{
			Debug.LogError("Invalid IKEffector.positionOffset (contains NaN)! Please make sure not to set IKEffector.positionOffset to NaN values.", bone);
		}
		if (positionOffset.sqrMagnitude > 1E+10f)
		{
			Debug.LogError("Additive effector positionOffset detected in Full Body IK (extremely large value). Make sure you are not circularily adding to effector positionOffset each frame.", bone);
		}
		if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
		{
			Debug.LogError("Invalid IKEffector.position (contains Infinity)!");
		}
		node.offset += positionOffset * solver.GetIKPositionWeight();
		if (effectChildNodes && fullBody)
		{
			for (int i = 0; i < childNodes.Length; i++)
			{
				ref Vector3 reference = ref localPositions[i];
				reference = childNodes[i].transform.position - node.transform.position;
				childNodes[i].offset += positionOffset * solver.GetIKPositionWeight();
			}
		}
		if (usePlaneNodes && maintainRelativePositionWeight > 0f)
		{
			animatedPlaneRotation = Quaternion.LookRotation(planeNode2.transform.position - planeNode1.transform.position, planeNode3.transform.position - planeNode1.transform.position);
		}
		firstUpdate = true;
	}

	public void OnPostWrite()
	{
		positionOffset = Vector3.zero;
	}

	public void Update()
	{
		if (firstUpdate)
		{
			animatedPosition = node.transform.position + node.offset;
			firstUpdate = false;
		}
		node.solverPosition = Vector3.Lerp(GetPosition(out planeRotationOffset), position, posW);
		if (effectChildNodes)
		{
			for (int i = 0; i < childNodes.Length; i++)
			{
				childNodes[i].solverPosition = Vector3.Lerp(childNodes[i].solverPosition, node.solverPosition + localPositions[i], posW);
			}
		}
	}

	private Vector3 GetPosition(out Quaternion planeRotationOffset)
	{
		planeRotationOffset = Quaternion.identity;
		if (!isEndEffector)
		{
			return node.solverPosition;
		}
		if (maintainRelativePositionWeight <= 0f)
		{
			return animatedPosition;
		}
		Vector3 vector = node.transform.position;
		Vector3 vector2 = vector - planeNode1.transform.position;
		planeRotationOffset = planeRotation * Quaternion.Inverse(animatedPlaneRotation);
		vector = planeNode1.solverPosition + planeRotationOffset * vector2;
		planeRotationOffset = Quaternion.Lerp(Quaternion.identity, planeRotationOffset, maintainRelativePositionWeight);
		return Vector3.Lerp(animatedPosition, vector + node.offset, maintainRelativePositionWeight);
	}
}
