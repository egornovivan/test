using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class FPSCharacter : MonoBehaviour
{
	[SerializeField]
	private AnimationClip aimAnim;

	[SerializeField]
	private Transform mixingTransformRecursive;

	[SerializeField]
	private FPSAiming FPSAiming;

	private float sVel;

	private void Start()
	{
		GetComponent<Animation>()[aimAnim.name].AddMixingTransform(mixingTransformRecursive);
		GetComponent<Animation>()[aimAnim.name].layer = 1;
		GetComponent<Animation>().Play(aimAnim.name);
	}

	private void Update()
	{
		FPSAiming.sightWeight = Mathf.SmoothDamp(FPSAiming.sightWeight, (!Input.GetMouseButton(1)) ? 0f : 1f, ref sVel, 0.1f);
		if (FPSAiming.sightWeight < 0.001f)
		{
			FPSAiming.sightWeight = 0f;
		}
		if (FPSAiming.sightWeight > 0.999f)
		{
			FPSAiming.sightWeight = 1f;
		}
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(Screen.width - 210, 10f, 200f, 25f), "Hold RMB to aim down the sight");
	}
}
