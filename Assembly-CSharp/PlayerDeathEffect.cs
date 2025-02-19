using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PlayerDeathEffect : MonoBehaviour
{
	private float GrayScale;

	private float GrayScaleWanted;

	private float BlurThreshold = 0.5f;

	private float BlurThresholdWanted = 0.5f;

	public float _Dumper = 0.025f;

	private void Start()
	{
		GrayScale = 0f;
		GrayScaleWanted = 0f;
	}

	private void Update()
	{
		if (Mathf.Abs(GrayScale - GrayScaleWanted) < 0.001f)
		{
			GrayScale = GrayScaleWanted;
		}
		else
		{
			GrayScale += (GrayScaleWanted - GrayScale) * _Dumper;
		}
		if (Mathf.Abs(BlurThreshold - BlurThresholdWanted) < 0.001f)
		{
			BlurThreshold = BlurThresholdWanted;
		}
		else
		{
			BlurThreshold += (BlurThresholdWanted - BlurThreshold) * _Dumper;
		}
		Grayscale component = base.gameObject.GetComponent<Grayscale>();
		BloomAndFlares component2 = base.gameObject.GetComponent<BloomAndFlares>();
		if (!(component == null))
		{
			if (GrayScale < 0.001f)
			{
				component.rampOffset = 0f;
				component.enabled = false;
			}
			else
			{
				component.enabled = true;
				component.rampOffset = GrayScale;
			}
			if (!(component2 == null) && component2.enabled)
			{
				component2.bloomThreshold = BlurThreshold;
			}
		}
	}

	public void DisplayEffect(float grayscalewanted, float blurwanted)
	{
		GrayScaleWanted = grayscalewanted;
		BlurThresholdWanted = blurwanted;
	}
}
