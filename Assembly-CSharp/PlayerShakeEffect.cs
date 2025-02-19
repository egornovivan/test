using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PlayerShakeEffect : MonoBehaviour
{
	public static bool bShaking;

	public float ShakeEffectLife = 0.25f;

	private void Start()
	{
		bShaking = false;
	}

	private void Update()
	{
	}

	private void Shake()
	{
		bShaking = true;
		MotionBlur component = base.gameObject.GetComponent<MotionBlur>();
		if (!(component == null))
		{
			component.enabled = true;
		}
	}
}
