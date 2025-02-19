using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

namespace NovaEnv;

public class Output : MonoBehaviour
{
	[HideInInspector]
	public Executor Executor;

	public float WeightTotal;

	public Color SkyColor;

	public Color FogBrightColor;

	public Color FogDarkColor;

	public float FogSkyInterpolator;

	public float FogHeight;

	public float FogIntensity;

	public float FogStartDistance;

	public float FogEndDistance;

	public Color SunGlowColor;

	public float SunGlowPower;

	public Color SunBodyColor;

	public float SunBodySize;

	public Color SunLightColor;

	public float SunLightIntensity;

	public float SunlightIntensityBase = -1f;

	public float SkyOvercast;

	public Color CloudBrightColor1;

	public Color CloudBrightColor2;

	public Color CloudDarkColor1;

	public Color CloudDarkColor2;

	public float RainCloudDensity;

	public float RainCloudThreshold;

	public float RainCloudOvercast;

	public Color WaterDepthColor;

	public Color WaterReflectionColor;

	public Color WaterFresnelColor;

	public Color WaterSpecularColor;

	public Color WaterFoamColor;

	public float WaterDepthDensity;

	public float WaterReflectionBlend;

	public float ShadowStrength;

	public Color AmbientSkyColor;

	public Color AmbientEquatorColor;

	public Color AmbientGroundColor;

	public float AmbientIntensity;

	public Color SunShaftColor;

	public float SunShaftIntensity;

	public float BloomThreshold;

	public float BloomIntensity;

	public void Normalize(float multiplier = 1f)
	{
		multiplier = ((!(WeightTotal <= 0.0001f)) ? (multiplier / WeightTotal) : 0f);
		WeightTotal *= multiplier;
		SkyColor *= multiplier;
		FogBrightColor *= multiplier;
		FogDarkColor *= multiplier;
		FogSkyInterpolator *= multiplier;
		FogHeight *= multiplier;
		FogIntensity *= multiplier;
		FogStartDistance *= multiplier;
		FogEndDistance *= multiplier;
		SunGlowColor *= multiplier;
		SunGlowPower *= multiplier;
		SunBodyColor *= multiplier;
		SunBodySize *= multiplier;
		SunLightColor *= multiplier;
		SunLightIntensity *= multiplier;
		CloudBrightColor1 *= multiplier;
		CloudBrightColor2 *= multiplier;
		CloudDarkColor1 *= multiplier;
		CloudDarkColor2 *= multiplier;
		WaterDepthColor *= multiplier;
		WaterReflectionColor *= multiplier;
		WaterFresnelColor *= multiplier;
		WaterSpecularColor *= multiplier;
		WaterFoamColor *= multiplier;
		WaterDepthDensity *= multiplier;
		WaterReflectionBlend *= multiplier;
		ShadowStrength *= multiplier;
		AmbientSkyColor *= multiplier;
		AmbientEquatorColor *= multiplier;
		AmbientGroundColor *= multiplier;
		AmbientIntensity *= multiplier;
		SunShaftColor *= multiplier;
		SunShaftIntensity *= multiplier;
		BloomThreshold *= multiplier;
		BloomIntensity *= multiplier;
	}

	public void Apply()
	{
		Executor.Sky.SkyColor = SkyColor;
		Executor.Sky.FogColorA = FogBrightColor;
		Executor.Sky.FogColorB = FogDarkColor;
		Executor.Sky.FogHeight = FogHeight;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogEndDistance = Mathf.Min(FogEndDistance, Executor.Settings.MaxFogEndDistance);
		float num = RenderSettings.fogEndDistance / FogEndDistance;
		RenderSettings.fogStartDistance = FogStartDistance * num;
		if (Executor.Underwater <= 0f)
		{
			if (Executor.Settings.MainCamera != null)
			{
				Vector3 forward = Executor.Settings.MainCamera.transform.forward;
				forward.y = 0f;
				RenderSettings.fogColor = Executor.Sky.FogColorAtPoint(forward, Executor.SunDirection, FogSkyInterpolator) * FogIntensity;
			}
			else
			{
				RenderSettings.fogColor = (FogDarkColor * 0.7f + FogBrightColor * 0.3f) * FogIntensity;
			}
		}
		else
		{
			RenderSettings.fogColor = WaterDepthColor;
		}
		Executor.Sky.UnityFogColor = RenderSettings.fogColor;
		Executor.Sky.SunBloomColor = SunGlowColor;
		Executor.Sky.SunPower = SunGlowPower;
		Executor.Sky.SunColor = SunBodyColor;
		Executor.Sky.SunSize = SunBodySize;
		Executor.Sky.Overcast = SkyOvercast;
		Executor.Sky.Apply();
		Executor.Sun.SunLight.color = SunLightColor;
		Executor.Sun.SunLight.intensity = ((!(SunLightIntensity < SunlightIntensityBase)) ? SunLightIntensity : SunlightIntensityBase);
		Executor.SunCloudLayer.Color1 = CloudBrightColor1;
		Executor.SunCloudLayer.Color2 = CloudBrightColor2;
		Executor.SunCloudLayer.Color3 = CloudDarkColor1;
		Executor.SunCloudLayer.Color4 = CloudDarkColor2;
		Executor.RainCloudLayer.Color1 = CloudBrightColor1;
		Executor.RainCloudLayer.Color2 = CloudBrightColor2;
		Executor.RainCloudLayer.Color3 = CloudDarkColor1;
		Executor.RainCloudLayer.Color4 = CloudDarkColor2;
		Executor.RainCloudLayer.LayerMat.SetFloat("_CloudDensity", RainCloudDensity);
		Executor.SunCloudLayer.LayerMat.SetFloat("_CloudThreshold", Mathf.Max(RainCloudThreshold, 1f));
		Executor.RainCloudLayer.LayerMat.SetFloat("_CloudThreshold", RainCloudThreshold);
		Executor.SunCloudLayer.LayerMat.SetFloat("_Overcast", RainCloudOvercast * 0.7f + 0.3f);
		Executor.RainCloudLayer.LayerMat.SetFloat("_Overcast", RainCloudOvercast);
		Executor.RainCloudLayer.LayerMat.SetFloat("_CloudTile", 7.5f);
		Material waterMaterial = Executor.Settings.WaterMaterial;
		if (waterMaterial != null)
		{
			if (!string.IsNullOrEmpty(Executor.Settings.WaterDepthColorProp))
			{
				waterMaterial.SetColor(Executor.Settings.WaterDepthColorProp, WaterDepthColor);
			}
			if (!string.IsNullOrEmpty(Executor.Settings.WaterReflectionColorProp))
			{
				waterMaterial.SetColor(Executor.Settings.WaterReflectionColorProp, WaterReflectionColor);
			}
			if (!string.IsNullOrEmpty(Executor.Settings.WaterFresnelColorProp))
			{
				waterMaterial.SetColor(Executor.Settings.WaterFresnelColorProp, WaterFresnelColor);
			}
			if (!string.IsNullOrEmpty(Executor.Settings.WaterSpecularColorProp))
			{
				waterMaterial.SetColor(Executor.Settings.WaterSpecularColorProp, WaterSpecularColor);
			}
			if (!string.IsNullOrEmpty(Executor.Settings.WaterFoamColorProp))
			{
				waterMaterial.SetColor(Executor.Settings.WaterFoamColorProp, WaterFoamColor);
			}
			if (!string.IsNullOrEmpty(Executor.Settings.WaterDepthDensityProp))
			{
				waterMaterial.SetVector(Executor.Settings.WaterDepthDensityProp, new Vector4(WaterDepthDensity, 0.1f, Mathf.Max(0.5f, Executor.UnderwaterDensity - 1f), 0f));
			}
			if (!string.IsNullOrEmpty(Executor.Settings.WaterReflectionBlendProp))
			{
				waterMaterial.SetFloat(Executor.Settings.WaterReflectionBlendProp, Mathf.Lerp(1f, WaterReflectionBlend, Executor.WaterReflectionMasterBlend));
			}
			if (!string.IsNullOrEmpty(Executor.Settings.WaterSunLightDirProp))
			{
				waterMaterial.SetVector(Executor.Settings.WaterSunLightDirProp, -Executor.SunDirection);
			}
		}
		Executor.Sun.SunLight.shadowStrength = ShadowStrength;
		RenderSettings.ambientMode = AmbientMode.Trilight;
		RenderSettings.ambientSkyColor = AmbientSkyColor;
		RenderSettings.ambientEquatorColor = AmbientEquatorColor;
		RenderSettings.ambientGroundColor = AmbientGroundColor;
		RenderSettings.ambientIntensity = AmbientIntensity;
		if (Executor.Settings.MainCamera != null)
		{
			SunShafts component = Executor.Settings.MainCamera.GetComponent<SunShafts>();
			if (component != null)
			{
				component.sunTransform = Executor.Sun.SunLight.transform;
				component.sunColor = SunShaftColor;
				component.sunShaftIntensity = SunShaftIntensity;
			}
			Bloom component2 = Executor.Settings.MainCamera.GetComponent<Bloom>();
			if (component2 != null)
			{
				component2.bloomIntensity = BloomIntensity;
				component2.bloomThreshold = BloomThreshold;
			}
			BlurOptimized component3 = Executor.Settings.MainCamera.GetComponent<BlurOptimized>();
			GlobalFog component4 = Executor.Settings.MainCamera.GetComponent<GlobalFog>();
			if (component3 != null)
			{
				component3.enabled = Executor.Underwater > 0f;
			}
			if (component4 != null)
			{
				component4.enabled = Executor.Underwater > 0f;
			}
		}
		Executor.UpdateCameraEffects();
	}
}
