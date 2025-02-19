using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKMappingLimb : IKMapping
{
	[Serializable]
	public enum BoneMapType
	{
		Parent,
		Bone1,
		Bone2,
		Bone3
	}

	public Transform parentBone;

	public Transform bone1;

	public Transform bone2;

	public Transform bone3;

	[Range(0f, 1f)]
	public float maintainRotationWeight;

	[Range(0f, 1f)]
	public float weight = 1f;

	private BoneMap boneMapParent = new BoneMap();

	private BoneMap boneMap1 = new BoneMap();

	private BoneMap boneMap2 = new BoneMap();

	private BoneMap boneMap3 = new BoneMap();

	public IKMappingLimb()
	{
	}

	public IKMappingLimb(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null)
	{
		SetBones(bone1, bone2, bone3, parentBone);
	}

	public override bool IsValid(IKSolver solver, Warning.Logger logger = null)
	{
		if (!base.IsValid(solver, logger))
		{
			return false;
		}
		if (!BoneIsValid(bone1, solver, logger))
		{
			return false;
		}
		if (!BoneIsValid(bone2, solver, logger))
		{
			return false;
		}
		if (!BoneIsValid(bone3, solver, logger))
		{
			return false;
		}
		return true;
	}

	public BoneMap GetBoneMap(BoneMapType boneMap)
	{
		switch (boneMap)
		{
		case BoneMapType.Parent:
			if (parentBone == null)
			{
				Warning.Log("This limb does not have a parent (shoulder) bone", bone1);
			}
			return boneMapParent;
		case BoneMapType.Bone1:
			return boneMap1;
		case BoneMapType.Bone2:
			return boneMap2;
		default:
			return boneMap3;
		}
	}

	public void SetLimbOrientation(Vector3 upper, Vector3 lower)
	{
		boneMap1.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone1.rotation) * Quaternion.LookRotation(bone2.position - bone1.position, bone1.rotation * -upper));
		boneMap2.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone2.rotation) * Quaternion.LookRotation(bone3.position - bone2.position, bone2.rotation * -lower));
	}

	public void SetBones(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null)
	{
		this.bone1 = bone1;
		this.bone2 = bone2;
		this.bone3 = bone3;
		this.parentBone = parentBone;
	}

	public void StoreDefaultLocalState()
	{
		if (parentBone != null)
		{
			boneMapParent.StoreDefaultLocalState();
		}
		boneMap1.StoreDefaultLocalState();
		boneMap2.StoreDefaultLocalState();
		boneMap3.StoreDefaultLocalState();
	}

	public void FixTransforms()
	{
		if (parentBone != null)
		{
			boneMapParent.FixTransform(position: false);
		}
		boneMap1.FixTransform(position: true);
		boneMap2.FixTransform(position: false);
		boneMap3.FixTransform(position: false);
	}

	protected override void OnInitiate()
	{
		if (boneMapParent == null)
		{
			boneMapParent = new BoneMap();
		}
		if (boneMap1 == null)
		{
			boneMap1 = new BoneMap();
		}
		if (boneMap2 == null)
		{
			boneMap2 = new BoneMap();
		}
		if (boneMap3 == null)
		{
			boneMap3 = new BoneMap();
		}
		if (parentBone != null)
		{
			boneMapParent.Initiate(parentBone, solver);
		}
		boneMap1.Initiate(bone1, solver);
		boneMap2.Initiate(bone2, solver);
		boneMap3.Initiate(bone3, solver);
		boneMap1.SetPlane(boneMap1.node, boneMap2.node, boneMap3.node);
		boneMap2.SetPlane(boneMap2.node, boneMap3.node, boneMap1.node);
		if (parentBone != null)
		{
			boneMapParent.SetLocalSwingAxis(boneMap1);
		}
	}

	public void ReadPose()
	{
		boneMap1.UpdatePlane(rotation: true, position: true);
		boneMap2.UpdatePlane(rotation: true, position: false);
		weight = Mathf.Clamp(weight, 0f, 1f);
		boneMap3.MaintainRotation();
	}

	public void WritePose(bool fullBody)
	{
		if (weight <= 0f)
		{
			return;
		}
		if (fullBody)
		{
			if (parentBone != null)
			{
				boneMapParent.Swing(boneMap1.node.solverPosition, weight);
			}
			boneMap1.FixToNode(weight);
		}
		boneMap1.RotateToPlane(weight);
		boneMap2.RotateToPlane(weight);
		boneMap3.RotateToMaintain(maintainRotationWeight * weight * solver.GetIKPositionWeight());
		boneMap3.RotateToEffector(weight);
	}
}
