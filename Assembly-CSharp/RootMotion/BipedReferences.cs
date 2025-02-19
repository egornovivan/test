using System;
using UnityEngine;

namespace RootMotion;

[Serializable]
public class BipedReferences
{
	public struct AutoDetectParams
	{
		public bool legsParentInSpine;

		public bool includeEyes;

		public static AutoDetectParams Default => new AutoDetectParams(legsParentInSpine: true, includeEyes: true);

		public AutoDetectParams(bool legsParentInSpine, bool includeEyes)
		{
			this.legsParentInSpine = legsParentInSpine;
			this.includeEyes = includeEyes;
		}
	}

	public Transform root;

	public Transform pelvis;

	public Transform leftThigh;

	public Transform leftCalf;

	public Transform leftFoot;

	public Transform rightThigh;

	public Transform rightCalf;

	public Transform rightFoot;

	public Transform leftUpperArm;

	public Transform leftForearm;

	public Transform leftHand;

	public Transform rightUpperArm;

	public Transform rightForearm;

	public Transform rightHand;

	public Transform head;

	public Transform[] spine = new Transform[0];

	public Transform[] eyes = new Transform[0];

	public bool isValid
	{
		get
		{
			if (root == null)
			{
				return false;
			}
			if (pelvis == null)
			{
				return false;
			}
			if (leftThigh == null || leftCalf == null || leftFoot == null)
			{
				return false;
			}
			if (rightThigh == null || rightCalf == null || rightFoot == null)
			{
				return false;
			}
			if (leftUpperArm == null || leftForearm == null || leftHand == null)
			{
				return false;
			}
			if (rightUpperArm == null || rightForearm == null || rightHand == null)
			{
				return false;
			}
			Transform[] array = spine;
			foreach (Transform transform in array)
			{
				if (transform == null)
				{
					return false;
				}
			}
			Transform[] array2 = eyes;
			foreach (Transform transform2 in array2)
			{
				if (transform2 == null)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool isEmpty => IsEmpty(includeRoot: true);

	public bool IsEmpty(bool includeRoot)
	{
		if (includeRoot && root != null)
		{
			return false;
		}
		if (pelvis != null || head != null)
		{
			return false;
		}
		if (leftThigh != null || leftCalf != null || leftFoot != null)
		{
			return false;
		}
		if (rightThigh != null || rightCalf != null || rightFoot != null)
		{
			return false;
		}
		if (leftUpperArm != null || leftForearm != null || leftHand != null)
		{
			return false;
		}
		if (rightUpperArm != null || rightForearm != null || rightHand != null)
		{
			return false;
		}
		Transform[] array = spine;
		foreach (Transform transform in array)
		{
			if (transform != null)
			{
				return false;
			}
		}
		Transform[] array2 = eyes;
		foreach (Transform transform2 in array2)
		{
			if (transform2 != null)
			{
				return false;
			}
		}
		return true;
	}

	public static bool AutoDetectReferences(ref BipedReferences references, Transform root, AutoDetectParams autoDetectParams)
	{
		if (references == null)
		{
			references = new BipedReferences();
		}
		references.root = root;
		DetectReferencesByNaming(ref references, root, autoDetectParams);
		if (references.isValid && CheckSetupError(references, log: false) && CheckSetupWarning(references, log: false))
		{
			return true;
		}
		AssignHumanoidReferences(ref references, root.GetComponent<Animator>(), autoDetectParams);
		bool flag = references.isValid;
		if (!flag)
		{
			Warning.Log("BipedReferences contains one or more missing Transforms.", root, logInEditMode: true);
		}
		return flag;
	}

	public static void DetectReferencesByNaming(ref BipedReferences references, Transform root, AutoDetectParams autoDetectParams)
	{
		if (references == null)
		{
			references = new BipedReferences();
		}
		Transform[] componentsInChildren = root.GetComponentsInChildren<Transform>();
		DetectLimb(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Left, ref references.leftUpperArm, ref references.leftForearm, ref references.leftHand, componentsInChildren);
		DetectLimb(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Right, ref references.rightUpperArm, ref references.rightForearm, ref references.rightHand, componentsInChildren);
		DetectLimb(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Left, ref references.leftThigh, ref references.leftCalf, ref references.leftFoot, componentsInChildren);
		DetectLimb(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Right, ref references.rightThigh, ref references.rightCalf, ref references.rightFoot, componentsInChildren);
		references.head = BipedNaming.GetBone(componentsInChildren, BipedNaming.BoneType.Head, BipedNaming.BoneSide.Center);
		references.pelvis = BipedNaming.GetNamingMatch(componentsInChildren, BipedNaming.pelvis);
		if ((references.pelvis == null || !Hierarchy.IsAncestor(references.leftThigh, references.pelvis)) && references.leftThigh != null)
		{
			references.pelvis = references.leftThigh.parent;
		}
		if (references.leftUpperArm != null && references.rightUpperArm != null && references.pelvis != null && references.leftThigh != null)
		{
			Transform firstCommonAncestor = Hierarchy.GetFirstCommonAncestor(references.leftUpperArm, references.rightUpperArm);
			if (firstCommonAncestor != null)
			{
				Transform[] array = new Transform[1] { firstCommonAncestor };
				Hierarchy.AddAncestors(array[0], references.pelvis, ref array);
				references.spine = new Transform[0];
				for (int num = array.Length - 1; num > -1; num--)
				{
					if (AddBoneToSpine(array[num], ref references, autoDetectParams))
					{
						Array.Resize(ref references.spine, references.spine.Length + 1);
						references.spine[references.spine.Length - 1] = array[num];
					}
				}
				if (references.head == null)
				{
					for (int i = 0; i < firstCommonAncestor.childCount; i++)
					{
						Transform child = firstCommonAncestor.GetChild(i);
						if (!Hierarchy.ContainsChild(child, references.leftUpperArm) && !Hierarchy.ContainsChild(child, references.rightUpperArm))
						{
							references.head = child;
							break;
						}
					}
				}
			}
		}
		Transform[] bonesOfType = BipedNaming.GetBonesOfType(BipedNaming.BoneType.Eye, componentsInChildren);
		references.eyes = new Transform[0];
		if (!autoDetectParams.includeEyes)
		{
			return;
		}
		for (int j = 0; j < bonesOfType.Length; j++)
		{
			if (AddBoneToEyes(bonesOfType[j], ref references, autoDetectParams))
			{
				Array.Resize(ref references.eyes, references.eyes.Length + 1);
				references.eyes[references.eyes.Length - 1] = bonesOfType[j];
			}
		}
	}

	public static void AssignHumanoidReferences(ref BipedReferences references, Animator animator, AutoDetectParams autoDetectParams)
	{
		if (references == null)
		{
			references = new BipedReferences();
		}
		if (!(animator == null) && animator.isHuman)
		{
			references.spine = new Transform[0];
			references.eyes = new Transform[0];
			references.head = animator.GetBoneTransform(HumanBodyBones.Head);
			references.leftThigh = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
			references.leftCalf = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			references.leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			references.rightThigh = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
			references.rightCalf = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			references.rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
			references.leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			references.leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			references.leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
			references.rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			references.rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			references.rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
			references.pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
			AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Spine));
			AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Chest));
			if (references.leftUpperArm != null && !IsNeckBone(animator.GetBoneTransform(HumanBodyBones.Neck), references.leftUpperArm))
			{
				AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Neck));
			}
			if (autoDetectParams.includeEyes)
			{
				AddBoneToHierarchy(ref references.eyes, animator.GetBoneTransform(HumanBodyBones.LeftEye));
				AddBoneToHierarchy(ref references.eyes, animator.GetBoneTransform(HumanBodyBones.RightEye));
			}
		}
	}

	public static bool CheckSetupError(BipedReferences references, bool log)
	{
		if (!references.isValid)
		{
			if (log)
			{
				Warning.Log("BipedReferences contains one or more missing Transforms.", references.root, logInEditMode: true);
			}
			return false;
		}
		if (!CheckLimbError(references.leftThigh, references.leftCalf, references.leftFoot, log))
		{
			return false;
		}
		if (!CheckLimbError(references.rightThigh, references.rightCalf, references.rightFoot, log))
		{
			return false;
		}
		if (!CheckLimbError(references.leftUpperArm, references.leftForearm, references.leftHand, log))
		{
			return false;
		}
		if (!CheckLimbError(references.rightUpperArm, references.rightForearm, references.rightHand, log))
		{
			return false;
		}
		if (!CheckSpineError(references, log))
		{
			return false;
		}
		if (!CheckEyesError(references, log))
		{
			return false;
		}
		return true;
	}

	public static bool CheckSetupWarning(BipedReferences references, bool log)
	{
		if (!CheckLimbWarning(references.leftThigh, references.leftCalf, references.leftFoot, log))
		{
			return false;
		}
		if (!CheckLimbWarning(references.rightThigh, references.rightCalf, references.rightFoot, log))
		{
			return false;
		}
		if (!CheckLimbWarning(references.leftUpperArm, references.leftForearm, references.leftHand, log))
		{
			return false;
		}
		if (!CheckLimbWarning(references.rightUpperArm, references.rightForearm, references.rightHand, log))
		{
			return false;
		}
		if (!CheckSpineWarning(references, log))
		{
			return false;
		}
		if (!CheckEyesWarning(references, log))
		{
			return false;
		}
		if (!CheckRootHeightWarning(references, log))
		{
			return false;
		}
		if (!CheckFacingAxisWarning(references, log))
		{
			return false;
		}
		return true;
	}

	private static bool IsNeckBone(Transform bone, Transform leftUpperArm)
	{
		if (leftUpperArm.parent != null && leftUpperArm.parent == bone)
		{
			return false;
		}
		if (Hierarchy.IsAncestor(leftUpperArm, bone))
		{
			return false;
		}
		return true;
	}

	private static bool AddBoneToEyes(Transform bone, ref BipedReferences references, AutoDetectParams autoDetectParams)
	{
		if (references.head != null && !Hierarchy.IsAncestor(bone, references.head))
		{
			return false;
		}
		if (bone.GetComponent<SkinnedMeshRenderer>() != null)
		{
			return false;
		}
		return true;
	}

	private static bool AddBoneToSpine(Transform bone, ref BipedReferences references, AutoDetectParams autoDetectParams)
	{
		if (bone == references.root)
		{
			return false;
		}
		if (bone == references.leftThigh.parent && !autoDetectParams.legsParentInSpine)
		{
			return false;
		}
		if (references.pelvis != null)
		{
			if (bone == references.pelvis)
			{
				return false;
			}
			if (Hierarchy.IsAncestor(references.pelvis, bone))
			{
				return false;
			}
		}
		return true;
	}

	private static void DetectLimb(BipedNaming.BoneType boneType, BipedNaming.BoneSide boneSide, ref Transform firstBone, ref Transform secondBone, ref Transform lastBone, Transform[] transforms)
	{
		Transform[] bonesOfTypeAndSide = BipedNaming.GetBonesOfTypeAndSide(boneType, boneSide, transforms);
		if (bonesOfTypeAndSide.Length >= 3)
		{
			if (bonesOfTypeAndSide.Length == 3)
			{
				firstBone = bonesOfTypeAndSide[0];
				secondBone = bonesOfTypeAndSide[1];
				lastBone = bonesOfTypeAndSide[2];
			}
			if (bonesOfTypeAndSide.Length > 3)
			{
				firstBone = bonesOfTypeAndSide[0];
				secondBone = bonesOfTypeAndSide[2];
				lastBone = bonesOfTypeAndSide[bonesOfTypeAndSide.Length - 1];
			}
		}
	}

	private static void AddBoneToHierarchy(ref Transform[] bones, Transform transform)
	{
		if (!(transform == null))
		{
			Array.Resize(ref bones, bones.Length + 1);
			bones[bones.Length - 1] = transform;
		}
	}

	private static bool CheckLimbError(Transform bone1, Transform bone2, Transform bone3, bool log)
	{
		if (bone1 == null)
		{
			if (log)
			{
				Warning.Log("Bone 1 of a BipedReferences limb is null.", bone2, logInEditMode: true);
			}
			return false;
		}
		if (bone2 == null)
		{
			if (log)
			{
				Warning.Log("Bone 2 of a BipedReferences limb is null.", bone3, logInEditMode: true);
			}
			return false;
		}
		if (bone3 == null)
		{
			if (log)
			{
				Warning.Log("Bone 3 of a BipedReferences limb is null.", bone1, logInEditMode: true);
			}
			return false;
		}
		if (bone2.position == bone1.position)
		{
			if (log)
			{
				Warning.Log("Second bone's position equals first bone's position in the biped's limb.", bone2, logInEditMode: true);
			}
			return false;
		}
		if (bone3.position == bone2.position)
		{
			if (log)
			{
				Warning.Log("Third bone's position equals second bone's position in the biped's limb.", bone3, logInEditMode: true);
			}
			return false;
		}
		Transform transform = (Transform)Hierarchy.ContainsDuplicate(new Transform[3] { bone1, bone2, bone3 });
		if (transform != null)
		{
			if (log)
			{
				Warning.Log(transform.name + " is represented multiple times in the same BipedReferences limb.", bone1, logInEditMode: true);
			}
			return false;
		}
		if (!Hierarchy.HierarchyIsValid(new Transform[3] { bone1, bone2, bone3 }))
		{
			if (log)
			{
				Warning.Log("BipedReferences limb hierarchy is invalid. Bone transforms in a limb do not belong to the same ancestry. Please make sure the bones are parented to each other. Bones: " + bone1.name + ", " + bone2.name + ", " + bone3.name, bone1, logInEditMode: true);
			}
			return false;
		}
		return true;
	}

	private static bool CheckLimbWarning(Transform bone1, Transform bone2, Transform bone3, bool log)
	{
		Vector3 vector = Vector3.Cross(bone2.position - bone1.position, bone3.position - bone1.position);
		if (vector == Vector3.zero)
		{
			if (log)
			{
				Warning.Log("BipedReferences limb is completely stretched out in the initial pose. IK solver can not calculate the default bend plane for the limb. Please make sure you character's limbs are at least slightly bent in the initial pose. First bone: " + bone1.name + ", second bone: " + bone2.name + ".", bone1, logInEditMode: true);
			}
			return false;
		}
		return true;
	}

	private static bool CheckSpineError(BipedReferences references, bool log)
	{
		if (references.spine.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < references.spine.Length; i++)
		{
			if (references.spine[i] == null)
			{
				if (log)
				{
					Warning.Log("BipedReferences spine bone at index " + i + " is null.", references.root, logInEditMode: true);
				}
				return false;
			}
		}
		Transform transform = (Transform)Hierarchy.ContainsDuplicate(references.spine);
		if (transform != null)
		{
			if (log)
			{
				Warning.Log(transform.name + " is represented multiple times in BipedReferences spine.", references.spine[0], logInEditMode: true);
			}
			return false;
		}
		if (!Hierarchy.HierarchyIsValid(references.spine))
		{
			if (log)
			{
				Warning.Log("BipedReferences spine hierarchy is invalid. Bone transforms in the spine do not belong to the same ancestry. Please make sure the bones are parented to each other.", references.spine[0], logInEditMode: true);
			}
			return false;
		}
		for (int j = 0; j < references.spine.Length; j++)
		{
			bool flag = false;
			if (j == 0 && references.spine[j].position == references.pelvis.position)
			{
				flag = true;
			}
			if (j != 0 && references.spine.Length > 1 && references.spine[j].position == references.spine[j - 1].position)
			{
				flag = true;
			}
			if (flag)
			{
				if (log)
				{
					Warning.Log("Biped's spine bone nr " + j + " position is the same as it's parent spine/pelvis bone's position. Please remove this bone from the spine.", references.spine[j], logInEditMode: true);
				}
				return false;
			}
		}
		return true;
	}

	private static bool CheckSpineWarning(BipedReferences references, bool log)
	{
		return true;
	}

	private static bool CheckEyesError(BipedReferences references, bool log)
	{
		if (references.eyes.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < references.eyes.Length; i++)
		{
			if (references.eyes[i] == null)
			{
				if (log)
				{
					Warning.Log("BipedReferences eye bone at index " + i + " is null.", references.root, logInEditMode: true);
				}
				return false;
			}
		}
		Transform transform = (Transform)Hierarchy.ContainsDuplicate(references.eyes);
		if (transform != null)
		{
			if (log)
			{
				Warning.Log(transform.name + " is represented multiple times in BipedReferences eyes.", references.eyes[0], logInEditMode: true);
			}
			return false;
		}
		return true;
	}

	private static bool CheckEyesWarning(BipedReferences references, bool log)
	{
		return true;
	}

	private static bool CheckRootHeightWarning(BipedReferences references, bool log)
	{
		if (references.head == null)
		{
			return true;
		}
		float verticalOffset = GetVerticalOffset(references.head.position, references.leftFoot.position, references.root.rotation);
		float verticalOffset2 = GetVerticalOffset(references.root.position, references.leftFoot.position, references.root.rotation);
		if (verticalOffset2 / verticalOffset > 0.2f)
		{
			if (log)
			{
				Warning.Log("Biped's root Transform's position should be at ground level relative to the character (at the character's feet not at it's pelvis).", references.root, logInEditMode: true);
			}
			return false;
		}
		return true;
	}

	private static bool CheckFacingAxisWarning(BipedReferences references, bool log)
	{
		Vector3 vector = references.rightHand.position - references.leftHand.position;
		Vector3 vector2 = references.rightFoot.position - references.leftFoot.position;
		float num = Vector3.Dot(vector.normalized, references.root.right);
		float num2 = Vector3.Dot(vector2.normalized, references.root.right);
		if (num < 0f || num2 < 0f)
		{
			if (log)
			{
				Warning.Log("Biped does not seem to be facing it's forward axis. Please make sure that in the initial pose the character is facing towards the positive Z axis of the Biped root gameobject.", references.root, logInEditMode: true);
			}
			return false;
		}
		return true;
	}

	private static float GetVerticalOffset(Vector3 p1, Vector3 p2, Quaternion rotation)
	{
		return (Quaternion.Inverse(rotation) * (p1 - p2)).y;
	}
}
