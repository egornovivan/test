using System;
using UnityEngine;

namespace AnimFollow;

[ExecuteInEditMode]
public class ResizeProfiles_AF : MonoBehaviour
{
	public readonly int version = 4;

	private AnimFollow_AF animFollow_S;

	private void Start()
	{
		if ((bool)(animFollow_S = GetComponent<AnimFollow_AF>()))
		{
			if (animFollow_S.version != version)
			{
				Debug.LogWarning("AnimFollow on " + base.transform.name + " is not version " + version + " but the ResizeProfiles script is");
			}
			int num = GetComponentsInChildren<Rigidbody>().Length;
			int num2 = animFollow_S.maxTorqueProfile.Length;
			Array.Resize(ref animFollow_S.maxTorqueProfile, num);
			for (int i = 1; i <= num - num2; i++)
			{
				animFollow_S.maxTorqueProfile[num - i] = animFollow_S.maxTorqueProfile[num2 - 1];
			}
			num2 = animFollow_S.maxForceProfile.Length;
			Array.Resize(ref animFollow_S.maxForceProfile, num);
			for (int j = 1; j <= num - num2; j++)
			{
				animFollow_S.maxForceProfile[num - j] = animFollow_S.maxForceProfile[num2 - 1];
			}
			num2 = animFollow_S.maxJointTorqueProfile.Length;
			Array.Resize(ref animFollow_S.maxJointTorqueProfile, num);
			for (int k = 1; k <= num - num2; k++)
			{
				animFollow_S.maxJointTorqueProfile[num - k] = animFollow_S.maxJointTorqueProfile[num2 - 1];
			}
			num2 = animFollow_S.jointDampingProfile.Length;
			Array.Resize(ref animFollow_S.jointDampingProfile, num);
			for (int l = 1; l <= num - num2; l++)
			{
				animFollow_S.jointDampingProfile[num - l] = animFollow_S.jointDampingProfile[num2 - 1];
			}
			num2 = animFollow_S.PTorqueProfile.Length;
			Array.Resize(ref animFollow_S.PTorqueProfile, num);
			for (int m = 1; m <= num - num2; m++)
			{
				animFollow_S.PTorqueProfile[num - m] = animFollow_S.PTorqueProfile[num2 - 1];
			}
			num2 = animFollow_S.PForceProfile.Length;
			Array.Resize(ref animFollow_S.PForceProfile, num);
			for (int n = 1; n <= num - num2; n++)
			{
				animFollow_S.PForceProfile[num - n] = animFollow_S.PForceProfile[num2 - 1];
			}
			num2 = animFollow_S.forceErrorWeightProfile.Length;
			Array.Resize(ref animFollow_S.forceErrorWeightProfile, num);
			for (int num3 = 1; num3 <= num - num2; num3++)
			{
				animFollow_S.forceErrorWeightProfile[num - num3] = animFollow_S.forceErrorWeightProfile[num2 - 1];
			}
		}
		else
		{
			Debug.LogWarning("There is no AnimFollow script on this game object. \nUnable to resize profiles");
		}
		UnityEngine.Object.DestroyImmediate(this);
	}
}
