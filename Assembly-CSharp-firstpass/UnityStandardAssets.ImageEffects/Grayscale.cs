using UnityEngine;

namespace UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Color Adjustments/Grayscale")]
public class Grayscale : ImageEffectBase
{
	public Texture textureRamp;

	public float rampOffset;

	public float saturate = 1f;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetTexture("_RampTex", textureRamp);
		base.material.SetFloat("_RampOffset", rampOffset);
		base.material.SetFloat("_Saturate", saturate);
		Graphics.Blit(source, destination, base.material);
	}
}
