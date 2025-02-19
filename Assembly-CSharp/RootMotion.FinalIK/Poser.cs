using UnityEngine;

namespace RootMotion.FinalIK;

public abstract class Poser : MonoBehaviour
{
	public Transform poseRoot;

	public float weight = 1f;

	public float localRotationWeight = 1f;

	public float localPositionWeight;

	public abstract void AutoMapping();
}
