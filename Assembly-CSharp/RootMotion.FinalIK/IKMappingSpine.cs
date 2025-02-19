using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKMappingSpine : IKMapping
{
	public Transform[] spineBones;

	public Transform leftUpperArmBone;

	public Transform rightUpperArmBone;

	public Transform leftThighBone;

	public Transform rightThighBone;

	[Range(1f, 3f)]
	public int iterations = 3;

	[Range(0f, 1f)]
	public float twistWeight = 1f;

	private int rootNodeIndex;

	private BoneMap[] spine = new BoneMap[0];

	private BoneMap leftUpperArm = new BoneMap();

	private BoneMap rightUpperArm = new BoneMap();

	private BoneMap leftThigh = new BoneMap();

	private BoneMap rightThigh = new BoneMap();

	private bool useFABRIK;

	public IKMappingSpine()
	{
	}

	public IKMappingSpine(Transform[] spineBones, Transform leftUpperArmBone, Transform rightUpperArmBone, Transform leftThighBone, Transform rightThighBone)
	{
		SetBones(spineBones, leftUpperArmBone, rightUpperArmBone, leftThighBone, rightThighBone);
	}

	public override bool IsValid(IKSolver solver, Warning.Logger logger = null)
	{
		if (!base.IsValid(solver, logger))
		{
			return false;
		}
		Transform[] array = spineBones;
		foreach (Transform transform in array)
		{
			if (transform == null)
			{
				logger?.Invoke("Spine bones contains a null reference.");
				return false;
			}
		}
		int num = 0;
		for (int j = 0; j < spineBones.Length; j++)
		{
			if (solver.GetPoint(spineBones[j]) != null)
			{
				num++;
			}
		}
		if (num == 0)
		{
			logger?.Invoke("IKMappingSpine does not contain any nodes.");
			return false;
		}
		if (leftUpperArmBone == null)
		{
			logger?.Invoke("IKMappingSpine is missing the left upper arm bone.");
			return false;
		}
		if (rightUpperArmBone == null)
		{
			logger?.Invoke("IKMappingSpine is missing the right upper arm bone.");
			return false;
		}
		if (leftThighBone == null)
		{
			logger?.Invoke("IKMappingSpine is missing the left thigh bone.");
			return false;
		}
		if (rightThighBone == null)
		{
			logger?.Invoke("IKMappingSpine is missing the right thigh bone.");
			return false;
		}
		if (solver.GetPoint(leftUpperArmBone) == null)
		{
			logger?.Invoke("Full Body IK is missing the left upper arm node.");
			return false;
		}
		if (solver.GetPoint(rightUpperArmBone) == null)
		{
			logger?.Invoke("Full Body IK is missing the right upper arm node.");
			return false;
		}
		if (solver.GetPoint(leftThighBone) == null)
		{
			logger?.Invoke("Full Body IK is missing the left thigh node.");
			return false;
		}
		if (solver.GetPoint(rightThighBone) == null)
		{
			logger?.Invoke("Full Body IK is missing the right thigh node.");
			return false;
		}
		return true;
	}

	public void SetBones(Transform[] spineBones, Transform leftUpperArmBone, Transform rightUpperArmBone, Transform leftThighBone, Transform rightThighBone)
	{
		this.spineBones = spineBones;
		this.leftUpperArmBone = leftUpperArmBone;
		this.rightUpperArmBone = rightUpperArmBone;
		this.leftThighBone = leftThighBone;
		this.rightThighBone = rightThighBone;
	}

	public void StoreDefaultLocalState()
	{
		for (int i = 0; i < spine.Length; i++)
		{
			spine[i].StoreDefaultLocalState();
		}
	}

	public void FixTransforms()
	{
		for (int i = 0; i < spine.Length; i++)
		{
			spine[i].FixTransform(i == 0 || i == spine.Length - 1);
		}
	}

	protected override void OnInitiate()
	{
		if (iterations <= 0)
		{
			iterations = 3;
		}
		if (spine == null || spine.Length != spineBones.Length)
		{
			spine = new BoneMap[spineBones.Length];
		}
		rootNodeIndex = -1;
		for (int i = 0; i < spineBones.Length; i++)
		{
			if (spine[i] == null)
			{
				spine[i] = new BoneMap();
			}
			spine[i].Initiate(spineBones[i], solver);
			if (spine[i].isNodeBone)
			{
				rootNodeIndex = i;
			}
		}
		if (leftUpperArm == null)
		{
			leftUpperArm = new BoneMap();
		}
		if (rightUpperArm == null)
		{
			rightUpperArm = new BoneMap();
		}
		if (leftThigh == null)
		{
			leftThigh = new BoneMap();
		}
		if (rightThigh == null)
		{
			rightThigh = new BoneMap();
		}
		leftUpperArm.Initiate(leftUpperArmBone, solver);
		rightUpperArm.Initiate(rightUpperArmBone, solver);
		leftThigh.Initiate(leftThighBone, solver);
		rightThigh.Initiate(rightThighBone, solver);
		for (int j = 0; j < spine.Length; j++)
		{
			spine[j].SetIKPosition();
		}
		spine[0].SetPlane(spine[rootNodeIndex].node, leftThigh.node, rightThigh.node);
		for (int k = 0; k < spine.Length - 1; k++)
		{
			spine[k].SetLength(spine[k + 1]);
			spine[k].SetLocalSwingAxis(spine[k + 1]);
			spine[k].SetLocalTwistAxis(leftUpperArm.transform.position - rightUpperArm.transform.position, spine[k + 1].transform.position - spine[k].transform.position);
		}
		spine[spine.Length - 1].SetPlane(spine[rootNodeIndex].node, leftUpperArm.node, rightUpperArm.node);
		spine[spine.Length - 1].SetLocalSwingAxis(leftUpperArm, rightUpperArm);
		useFABRIK = UseFABRIK();
	}

	private bool UseFABRIK()
	{
		if (spine.Length > 3)
		{
			return true;
		}
		if (rootNodeIndex != 1)
		{
			return true;
		}
		return false;
	}

	public void ReadPose()
	{
		spine[0].UpdatePlane(rotation: true, position: true);
		for (int i = 0; i < spine.Length - 1; i++)
		{
			spine[i].SetLength(spine[i + 1]);
			spine[i].SetLocalSwingAxis(spine[i + 1]);
			spine[i].SetLocalTwistAxis(leftUpperArm.transform.position - rightUpperArm.transform.position, spine[i + 1].transform.position - spine[i].transform.position);
		}
		spine[spine.Length - 1].UpdatePlane(rotation: true, position: true);
		spine[spine.Length - 1].SetLocalSwingAxis(leftUpperArm, rightUpperArm);
	}

	public void WritePose()
	{
		Vector3 planePosition = spine[0].GetPlanePosition();
		Vector3 solverPosition = spine[rootNodeIndex].node.solverPosition;
		Vector3 planePosition2 = spine[spine.Length - 1].GetPlanePosition();
		if (useFABRIK)
		{
			Vector3 vector = spine[rootNodeIndex].node.solverPosition - spine[rootNodeIndex].transform.position;
			for (int i = 0; i < spine.Length; i++)
			{
				spine[i].ikPosition = spine[i].transform.position + vector;
			}
			for (int j = 0; j < iterations; j++)
			{
				ForwardReach(planePosition2);
				BackwardReach(planePosition);
				spine[rootNodeIndex].ikPosition = solverPosition;
			}
		}
		else
		{
			spine[0].ikPosition = planePosition;
			spine[rootNodeIndex].ikPosition = solverPosition;
		}
		spine[spine.Length - 1].ikPosition = planePosition2;
		MapToSolverPositions();
	}

	public void ForwardReach(Vector3 position)
	{
		spine[spineBones.Length - 1].ikPosition = position;
		for (int num = spine.Length - 2; num > -1; num--)
		{
			spine[num].ikPosition = IKSolverFABRIK.SolveJoint(spine[num].ikPosition, spine[num + 1].ikPosition, spine[num].length);
		}
	}

	private void BackwardReach(Vector3 position)
	{
		spine[0].ikPosition = position;
		for (int i = 1; i < spine.Length; i++)
		{
			spine[i].ikPosition = IKSolverFABRIK.SolveJoint(spine[i].ikPosition, spine[i - 1].ikPosition, spine[i - 1].length);
		}
	}

	private void MapToSolverPositions()
	{
		spine[0].SetToIKPosition();
		spine[0].RotateToPlane(1f);
		for (int i = 1; i < spine.Length - 1; i++)
		{
			spine[i].Swing(spine[i + 1].ikPosition, 1f);
			if (twistWeight > 0f)
			{
				float num = (float)i / ((float)spine.Length - 2f);
				spine[i].Twist(leftUpperArm.node.solverPosition - rightUpperArm.node.solverPosition, spine[i + 1].ikPosition - spine[i].transform.position, num * twistWeight);
			}
		}
		spine[spine.Length - 1].SetToIKPosition();
		spine[spine.Length - 1].RotateToPlane(1f);
	}
}
