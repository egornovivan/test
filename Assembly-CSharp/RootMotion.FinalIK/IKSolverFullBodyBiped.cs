using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverFullBodyBiped : IKSolverFullBody
{
	public Transform rootNode;

	[Range(0f, 1f)]
	public float spineStiffness = 0.5f;

	[Range(-1f, 1f)]
	public float pullBodyVertical = 0.5f;

	[Range(-1f, 1f)]
	public float pullBodyHorizontal;

	private Vector3 offset;

	public IKEffector bodyEffector => GetEffector(FullBodyBipedEffector.Body);

	public IKEffector leftShoulderEffector => GetEffector(FullBodyBipedEffector.LeftShoulder);

	public IKEffector rightShoulderEffector => GetEffector(FullBodyBipedEffector.RightShoulder);

	public IKEffector leftThighEffector => GetEffector(FullBodyBipedEffector.LeftThigh);

	public IKEffector rightThighEffector => GetEffector(FullBodyBipedEffector.RightThigh);

	public IKEffector leftHandEffector => GetEffector(FullBodyBipedEffector.LeftHand);

	public IKEffector rightHandEffector => GetEffector(FullBodyBipedEffector.RightHand);

	public IKEffector leftFootEffector => GetEffector(FullBodyBipedEffector.LeftFoot);

	public IKEffector rightFootEffector => GetEffector(FullBodyBipedEffector.RightFoot);

	public FBIKChain leftArmChain => chain[1];

	public FBIKChain rightArmChain => chain[2];

	public FBIKChain leftLegChain => chain[3];

	public FBIKChain rightLegChain => chain[4];

	public IKMappingLimb leftArmMapping => limbMappings[0];

	public IKMappingLimb rightArmMapping => limbMappings[1];

	public IKMappingLimb leftLegMapping => limbMappings[2];

	public IKMappingLimb rightLegMapping => limbMappings[3];

	public IKMappingBone headMapping => boneMappings[0];

	public void SetChainWeights(FullBodyBipedChain c, float pull, float reach = 0f)
	{
		GetChain(c).pull = pull;
		GetChain(c).reach = reach;
	}

	public void SetEffectorWeights(FullBodyBipedEffector effector, float positionWeight, float rotationWeight)
	{
		GetEffector(effector).positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
		GetEffector(effector).rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
	}

	public FBIKChain GetChain(FullBodyBipedChain c)
	{
		return c switch
		{
			FullBodyBipedChain.LeftArm => chain[1], 
			FullBodyBipedChain.RightArm => chain[2], 
			FullBodyBipedChain.LeftLeg => chain[3], 
			FullBodyBipedChain.RightLeg => chain[4], 
			_ => null, 
		};
	}

	public FBIKChain GetChain(FullBodyBipedEffector effector)
	{
		return effector switch
		{
			FullBodyBipedEffector.Body => chain[0], 
			FullBodyBipedEffector.LeftShoulder => chain[1], 
			FullBodyBipedEffector.RightShoulder => chain[2], 
			FullBodyBipedEffector.LeftThigh => chain[3], 
			FullBodyBipedEffector.RightThigh => chain[4], 
			FullBodyBipedEffector.LeftHand => chain[1], 
			FullBodyBipedEffector.RightHand => chain[2], 
			FullBodyBipedEffector.LeftFoot => chain[3], 
			FullBodyBipedEffector.RightFoot => chain[4], 
			_ => null, 
		};
	}

	public IKEffector GetEffector(FullBodyBipedEffector effector)
	{
		return effector switch
		{
			FullBodyBipedEffector.Body => effectors[0], 
			FullBodyBipedEffector.LeftShoulder => effectors[1], 
			FullBodyBipedEffector.RightShoulder => effectors[2], 
			FullBodyBipedEffector.LeftThigh => effectors[3], 
			FullBodyBipedEffector.RightThigh => effectors[4], 
			FullBodyBipedEffector.LeftHand => effectors[5], 
			FullBodyBipedEffector.RightHand => effectors[6], 
			FullBodyBipedEffector.LeftFoot => effectors[7], 
			FullBodyBipedEffector.RightFoot => effectors[8], 
			_ => null, 
		};
	}

	public IKEffector GetEndEffector(FullBodyBipedChain c)
	{
		return c switch
		{
			FullBodyBipedChain.LeftArm => effectors[5], 
			FullBodyBipedChain.RightArm => effectors[6], 
			FullBodyBipedChain.LeftLeg => effectors[7], 
			FullBodyBipedChain.RightLeg => effectors[8], 
			_ => null, 
		};
	}

	public IKMappingLimb GetLimbMapping(FullBodyBipedChain chain)
	{
		return chain switch
		{
			FullBodyBipedChain.LeftArm => limbMappings[0], 
			FullBodyBipedChain.RightArm => limbMappings[1], 
			FullBodyBipedChain.LeftLeg => limbMappings[2], 
			FullBodyBipedChain.RightLeg => limbMappings[3], 
			_ => null, 
		};
	}

	public IKMappingLimb GetLimbMapping(FullBodyBipedEffector effector)
	{
		return effector switch
		{
			FullBodyBipedEffector.LeftShoulder => limbMappings[0], 
			FullBodyBipedEffector.RightShoulder => limbMappings[1], 
			FullBodyBipedEffector.LeftThigh => limbMappings[2], 
			FullBodyBipedEffector.RightThigh => limbMappings[3], 
			FullBodyBipedEffector.LeftHand => limbMappings[0], 
			FullBodyBipedEffector.RightHand => limbMappings[1], 
			FullBodyBipedEffector.LeftFoot => limbMappings[2], 
			FullBodyBipedEffector.RightFoot => limbMappings[3], 
			_ => null, 
		};
	}

	public IKMappingSpine GetSpineMapping()
	{
		return spineMapping;
	}

	public IKMappingBone GetHeadMapping()
	{
		return boneMappings[0];
	}

	public IKConstraintBend GetBendConstraint(FullBodyBipedChain limb)
	{
		return limb switch
		{
			FullBodyBipedChain.LeftArm => chain[1].bendConstraint, 
			FullBodyBipedChain.RightArm => chain[2].bendConstraint, 
			FullBodyBipedChain.LeftLeg => chain[3].bendConstraint, 
			FullBodyBipedChain.RightLeg => chain[4].bendConstraint, 
			_ => null, 
		};
	}

	public override bool IsValid(bool log)
	{
		if (!base.IsValid(log))
		{
			return false;
		}
		if (rootNode == null)
		{
			if (log)
			{
				LogWarning("Root Node bone is null. FBBIK will not initiate.");
			}
			return false;
		}
		if (chain.Length != 5 || chain[0].nodes.Length != 1 || chain[1].nodes.Length != 3 || chain[2].nodes.Length != 3 || chain[3].nodes.Length != 3 || chain[4].nodes.Length != 3 || effectors.Length != 9 || limbMappings.Length != 4)
		{
			if (log)
			{
				LogWarning("Invalid FBIK Biped setup. FBBIK will not initiate.");
			}
			return false;
		}
		return true;
	}

	public void SetToReferences(BipedReferences references, Transform rootNode = null)
	{
		root = references.root;
		if (rootNode == null)
		{
			rootNode = DetectRootNodeBone(references);
		}
		this.rootNode = rootNode;
		if (chain == null || chain.Length != 5)
		{
			chain = new FBIKChain[5];
		}
		for (int i = 0; i < chain.Length; i++)
		{
			if (chain[i] == null)
			{
				chain[i] = new FBIKChain();
			}
		}
		chain[0].pin = 0f;
		chain[0].SetNodes(rootNode);
		chain[0].children = new int[4] { 1, 2, 3, 4 };
		chain[1].SetNodes(references.leftUpperArm, references.leftForearm, references.leftHand);
		chain[2].SetNodes(references.rightUpperArm, references.rightForearm, references.rightHand);
		chain[3].SetNodes(references.leftThigh, references.leftCalf, references.leftFoot);
		chain[4].SetNodes(references.rightThigh, references.rightCalf, references.rightFoot);
		if (effectors.Length != 9)
		{
			effectors = new IKEffector[9]
			{
				new IKEffector(),
				new IKEffector(),
				new IKEffector(),
				new IKEffector(),
				new IKEffector(),
				new IKEffector(),
				new IKEffector(),
				new IKEffector(),
				new IKEffector()
			};
		}
		effectors[0].bone = rootNode;
		effectors[0].childBones = new Transform[2] { references.leftThigh, references.rightThigh };
		effectors[1].bone = references.leftUpperArm;
		effectors[2].bone = references.rightUpperArm;
		effectors[3].bone = references.leftThigh;
		effectors[4].bone = references.rightThigh;
		effectors[5].bone = references.leftHand;
		effectors[6].bone = references.rightHand;
		effectors[7].bone = references.leftFoot;
		effectors[8].bone = references.rightFoot;
		effectors[5].planeBone1 = references.leftUpperArm;
		effectors[5].planeBone2 = references.rightUpperArm;
		effectors[5].planeBone3 = rootNode;
		effectors[6].planeBone1 = references.rightUpperArm;
		effectors[6].planeBone2 = references.leftUpperArm;
		effectors[6].planeBone3 = rootNode;
		effectors[7].planeBone1 = references.leftThigh;
		effectors[7].planeBone2 = references.rightThigh;
		effectors[7].planeBone3 = rootNode;
		effectors[8].planeBone1 = references.rightThigh;
		effectors[8].planeBone2 = references.leftThigh;
		effectors[8].planeBone3 = rootNode;
		chain[0].childConstraints = new FBIKChain.ChildConstraint[4]
		{
			new FBIKChain.ChildConstraint(references.leftUpperArm, references.rightThigh, 0f, 1f),
			new FBIKChain.ChildConstraint(references.rightUpperArm, references.leftThigh, 0f, 1f),
			new FBIKChain.ChildConstraint(references.leftUpperArm, references.rightUpperArm),
			new FBIKChain.ChildConstraint(references.leftThigh, references.rightThigh)
		};
		Transform[] array = new Transform[references.spine.Length + 1];
		array[0] = references.pelvis;
		for (int j = 0; j < references.spine.Length; j++)
		{
			array[j + 1] = references.spine[j];
		}
		if (spineMapping == null)
		{
			spineMapping = new IKMappingSpine();
			spineMapping.iterations = 3;
		}
		spineMapping.SetBones(array, references.leftUpperArm, references.rightUpperArm, references.leftThigh, references.rightThigh);
		int num = ((references.head != null) ? 1 : 0);
		if (boneMappings.Length != num)
		{
			boneMappings = new IKMappingBone[num];
			for (int k = 0; k < boneMappings.Length; k++)
			{
				boneMappings[k] = new IKMappingBone();
			}
			if (num == 1)
			{
				boneMappings[0].maintainRotationWeight = 0f;
			}
		}
		if (boneMappings.Length > 0)
		{
			boneMappings[0].bone = references.head;
		}
		if (limbMappings.Length != 4)
		{
			limbMappings = new IKMappingLimb[4]
			{
				new IKMappingLimb(),
				new IKMappingLimb(),
				new IKMappingLimb(),
				new IKMappingLimb()
			};
			limbMappings[2].maintainRotationWeight = 1f;
			limbMappings[3].maintainRotationWeight = 1f;
		}
		limbMappings[0].SetBones(references.leftUpperArm, references.leftForearm, references.leftHand, GetLeftClavicle(references));
		limbMappings[1].SetBones(references.rightUpperArm, references.rightForearm, references.rightHand, GetRightClavicle(references));
		limbMappings[2].SetBones(references.leftThigh, references.leftCalf, references.leftFoot);
		limbMappings[3].SetBones(references.rightThigh, references.rightCalf, references.rightFoot);
		if (Application.isPlaying)
		{
			Initiate(references.root);
		}
	}

	public static Transform DetectRootNodeBone(BipedReferences references)
	{
		if (!references.isValid)
		{
			return null;
		}
		if (references.spine.Length < 1)
		{
			return null;
		}
		int num = references.spine.Length;
		if (num == 1)
		{
			return references.spine[0];
		}
		Vector3 vector = Vector3.Lerp(references.leftThigh.position, references.rightThigh.position, 0.5f);
		Vector3 vector2 = Vector3.Lerp(references.leftUpperArm.position, references.rightUpperArm.position, 0.5f);
		Vector3 onNormal = vector2 - vector;
		float magnitude = onNormal.magnitude;
		if (references.spine.Length < 2)
		{
			return references.spine[0];
		}
		int num2 = 0;
		for (int i = 1; i < num; i++)
		{
			Vector3 vector3 = references.spine[i].position - vector;
			Vector3 vector4 = Vector3.Project(vector3, onNormal);
			float num3 = Vector3.Dot(vector4.normalized, onNormal.normalized);
			if (num3 > 0f)
			{
				float num4 = vector4.magnitude / magnitude;
				if (num4 < 0.5f)
				{
					num2 = i;
				}
			}
		}
		return references.spine[num2];
	}

	public void SetLimbOrientations(BipedLimbOrientations o)
	{
		SetLimbOrientation(FullBodyBipedChain.LeftArm, o.leftArm);
		SetLimbOrientation(FullBodyBipedChain.RightArm, o.rightArm);
		SetLimbOrientation(FullBodyBipedChain.LeftLeg, o.leftLeg);
		SetLimbOrientation(FullBodyBipedChain.RightLeg, o.rightLeg);
	}

	private void SetLimbOrientation(FullBodyBipedChain chain, BipedLimbOrientations.LimbOrientation limbOrientation)
	{
		if (chain == FullBodyBipedChain.LeftArm || chain == FullBodyBipedChain.RightArm)
		{
			GetBendConstraint(chain).SetLimbOrientation(-limbOrientation.upperBoneForwardAxis, -limbOrientation.lowerBoneForwardAxis, -limbOrientation.lastBoneLeftAxis);
			GetLimbMapping(chain).SetLimbOrientation(-limbOrientation.upperBoneForwardAxis, -limbOrientation.lowerBoneForwardAxis);
		}
		else
		{
			GetBendConstraint(chain).SetLimbOrientation(limbOrientation.upperBoneForwardAxis, limbOrientation.lowerBoneForwardAxis, limbOrientation.lastBoneLeftAxis);
			GetLimbMapping(chain).SetLimbOrientation(limbOrientation.upperBoneForwardAxis, limbOrientation.lowerBoneForwardAxis);
		}
	}

	private static Transform GetLeftClavicle(BipedReferences references)
	{
		if (references.leftUpperArm == null)
		{
			return null;
		}
		if (!Contains(references.spine, references.leftUpperArm.parent))
		{
			return references.leftUpperArm.parent;
		}
		return null;
	}

	private static Transform GetRightClavicle(BipedReferences references)
	{
		if (references.rightUpperArm == null)
		{
			return null;
		}
		if (!Contains(references.spine, references.rightUpperArm.parent))
		{
			return references.rightUpperArm.parent;
		}
		return null;
	}

	private static bool Contains(Transform[] array, Transform transform)
	{
		foreach (Transform transform2 in array)
		{
			if (transform2 == transform)
			{
				return true;
			}
		}
		return false;
	}

	protected override void ReadPose()
	{
		for (int i = 0; i < effectors.Length; i++)
		{
			effectors[i].SetToTarget();
		}
		PullBody();
		float pushElasticity = Mathf.Clamp(1f - spineStiffness, 0f, 1f);
		chain[0].childConstraints[0].pushElasticity = pushElasticity;
		chain[0].childConstraints[1].pushElasticity = pushElasticity;
		base.ReadPose();
	}

	private void PullBody()
	{
		if (iterations >= 1 && (pullBodyVertical != 0f || pullBodyHorizontal != 0f))
		{
			Vector3 bodyOffset = GetBodyOffset();
			bodyEffector.positionOffset += V3Tools.ExtractVertical(bodyOffset, root.up, pullBodyVertical) + V3Tools.ExtractHorizontal(bodyOffset, root.up, pullBodyHorizontal);
		}
	}

	private Vector3 GetBodyOffset()
	{
		Vector3 vector = Vector3.zero + GetHandBodyPull(leftHandEffector, leftArmChain, Vector3.zero) * Mathf.Clamp(leftHandEffector.positionWeight, 0f, 1f);
		return vector + GetHandBodyPull(rightHandEffector, rightArmChain, vector) * Mathf.Clamp(rightHandEffector.positionWeight, 0f, 1f);
	}

	private Vector3 GetHandBodyPull(IKEffector effector, FBIKChain arm, Vector3 offset)
	{
		Vector3 vector = effector.position - (arm.nodes[0].transform.position + offset);
		float num = arm.nodes[0].length + arm.nodes[1].length;
		float magnitude = vector.magnitude;
		if (magnitude < num)
		{
			return Vector3.zero;
		}
		float num2 = magnitude - num;
		return vector / magnitude * num2;
	}

	protected override void ApplyBendConstraints()
	{
		if (iterations > 0)
		{
			chain[1].bendConstraint.rotationOffset = leftHandEffector.planeRotationOffset;
			chain[2].bendConstraint.rotationOffset = rightHandEffector.planeRotationOffset;
			chain[3].bendConstraint.rotationOffset = leftFootEffector.planeRotationOffset;
			chain[4].bendConstraint.rotationOffset = rightFootEffector.planeRotationOffset;
		}
		else
		{
			offset = Vector3.Lerp(effectors[0].positionOffset, effectors[0].position - (effectors[0].bone.position + effectors[0].positionOffset), effectors[0].positionWeight);
			for (int i = 0; i < 5; i++)
			{
				effectors[i].GetNode().solverPosition += offset;
			}
		}
		base.ApplyBendConstraints();
	}

	protected override void WritePose()
	{
		if (iterations == 0)
		{
			spineMapping.spineBones[0].position += offset;
		}
		base.WritePose();
	}
}
