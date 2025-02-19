using System;
using UnityEngine;

namespace NovaEnv;

[Serializable]
public class BiomoTheme : Theme
{
	public float FadeInSpeed = 0.004f;

	[Header("Sky & Fog")]
	public Gradient SkyColorChange;

	public Gradient FogBrightColorChange;

	public Gradient FogDarkColorChange;

	public AnimationCurve FogSkyInterpolatorChange;

	public AnimationCurve FogHeightChange;

	public AnimationCurve FogIntensityChange;

	public float FogStartDistance;

	public float FogEndDistance;

	[Header("Sun")]
	public Gradient SunGlowColorChange;

	public AnimationCurve SunGlowPowerChange;

	public Gradient SunBodyColorChange;

	public AnimationCurve SunBodySizeChange;

	public Gradient SunLightColorChange;

	public AnimationCurve SunLightIntensityChange;

	[Header("Cloud")]
	public Gradient CloudBrightColor1Change;

	public Gradient CloudBrightColor2Change;

	public Gradient CloudDarkColor1Change;

	public Gradient CloudDarkColor2Change;

	[Header("Water")]
	public Gradient WaterDepthColorChange;

	public Gradient WaterReflectionColorChange;

	public Gradient WaterFresnelColorChange;

	public Gradient WaterSpecularColorChange;

	public Gradient WaterFoamColorChange;

	public AnimationCurve WaterDepthDensityChange;

	public AnimationCurve WaterReflectionBlendChange;

	[Header("Ambient & Shadow")]
	public AnimationCurve ShadowStrengthChange;

	public Gradient AmbientSkyColorChange;

	public Gradient AmbientEquatorColorChange;

	public Gradient AmbientGroundColorChange;

	public AnimationCurve AmbientIntensityChange;

	[Header("Post-Effect")]
	public Gradient SunShaftColorChange;

	public AnimationCurve SunShaftIntensityChange;

	public AnimationCurve BloomThresholdChange;

	public AnimationCurve BloomIntensityChange;

	[Header("Underwater")]
	public Gradient UnderwaterWaterDepthColorChange;

	public float UnderwaterDensity;

	public override void Execute(Executor executor)
	{
		if (!(Weight < 0.001f))
		{
			Output output = executor.Output;
			float num = (float)executor.FracSunDay;
			float sunHeight = (Mathf.Asin(executor.SunDirection.y) / (float)Math.PI + 0.5f) * 0.5f;
			float num2 = 0.5f;
			num2 = ((0f <= num && num <= 0.1f) ? Mathf.Lerp(0.5f, 0f, num / 0.1f) : ((0.1f <= num && num <= 0.4f) ? 0f : ((0.4f <= num && num <= 0.6f) ? ((num - 0.5f) / 0.1f * 0.5f + 0.5f) : ((0.6f <= num && num <= 0.9f) ? 1f : ((!(0.9f <= num) || !(num <= 1f)) ? 0.5f : Mathf.Lerp(0.5f, 1f, (1f - num) / 0.1f))))));
			output.WeightTotal += Weight;
			output.SkyColor += Theme.Evaluate(SkyColorChange, sunHeight, num2) * Weight;
			output.FogBrightColor += Theme.Evaluate(FogBrightColorChange, sunHeight, num2) * Weight;
			output.FogDarkColor += Theme.Evaluate(FogDarkColorChange, sunHeight, num2) * Weight;
			output.FogSkyInterpolator += Theme.Evaluate(FogSkyInterpolatorChange, sunHeight, num2) * Weight;
			float num3 = Mathf.Min(1f, 2f / executor.UnderwaterDensity);
			if (executor.Underwater <= 0f)
			{
				output.FogHeight += Theme.Evaluate(FogHeightChange, sunHeight, num2) * Weight;
				output.FogIntensity += Theme.Evaluate(FogIntensityChange, sunHeight, num2) * Weight;
				output.FogStartDistance += FogStartDistance * Weight;
				output.FogEndDistance += FogEndDistance * Weight;
			}
			else
			{
				output.FogHeight += 1f * Weight;
				output.FogIntensity += Theme.Evaluate(FogIntensityChange, sunHeight, num2) * Weight;
				output.FogStartDistance += FogStartDistance * Weight;
				output.FogEndDistance += FogEndDistance / Mathf.Max(1f, UnderwaterDensity) * Weight;
			}
			output.SunGlowColor += Theme.Evaluate(SunGlowColorChange, sunHeight, num2) * Weight;
			output.SunGlowPower += Theme.Evaluate(SunGlowPowerChange, sunHeight, num2) * Weight;
			output.SunBodyColor += Theme.Evaluate(SunBodyColorChange, sunHeight, num2) * Weight;
			output.SunBodySize += Theme.Evaluate(SunBodySizeChange, sunHeight, num2) * Weight;
			output.SunLightColor += Theme.Evaluate(SunLightColorChange, sunHeight, num2) * Weight;
			output.SunLightIntensity += Theme.Evaluate(SunLightIntensityChange, sunHeight, num2) * Weight;
			output.CloudBrightColor1 += Theme.Evaluate(CloudBrightColor1Change, sunHeight, num2) * Weight;
			output.CloudBrightColor2 += Theme.Evaluate(CloudBrightColor2Change, sunHeight, num2) * Weight;
			output.CloudDarkColor1 += Theme.Evaluate(CloudDarkColor1Change, sunHeight, num2) * Weight;
			output.CloudDarkColor2 += Theme.Evaluate(CloudDarkColor2Change, sunHeight, num2) * Weight;
			if (executor.Underwater <= 0f)
			{
				output.WaterDepthColor += Theme.Evaluate(WaterDepthColorChange, sunHeight, num2) * Weight;
			}
			else
			{
				output.WaterDepthColor += Theme.Evaluate(UnderwaterWaterDepthColorChange, sunHeight, num2) * num3 * Weight;
			}
			output.WaterReflectionColor += Theme.Evaluate(WaterReflectionColorChange, sunHeight, num2) * Weight;
			output.WaterFresnelColor += Theme.Evaluate(WaterFresnelColorChange, sunHeight, num2) * Weight;
			output.WaterSpecularColor += Theme.Evaluate(WaterSpecularColorChange, sunHeight, num2) * Weight;
			output.WaterFoamColor += Theme.Evaluate(WaterFoamColorChange, sunHeight, num2) * Weight;
			output.WaterDepthDensity += Theme.Evaluate(WaterDepthDensityChange, sunHeight, num2) * Weight;
			output.WaterReflectionBlend += Theme.Evaluate(WaterReflectionBlendChange, sunHeight, num2) * Weight;
			output.ShadowStrength += Theme.Evaluate(ShadowStrengthChange, sunHeight, num2) * Weight;
			output.AmbientSkyColor += Theme.Evaluate(AmbientSkyColorChange, sunHeight, num2) * Weight;
			output.AmbientEquatorColor += Theme.Evaluate(AmbientEquatorColorChange, sunHeight, num2) * Weight;
			output.AmbientGroundColor += Theme.Evaluate(AmbientGroundColorChange, sunHeight, num2) * Weight;
			output.AmbientIntensity += Theme.Evaluate(AmbientIntensityChange, sunHeight, num2) * num3 * Weight;
			output.SunShaftColor += Theme.Evaluate(SunShaftColorChange, sunHeight, num2) * Weight;
			output.SunShaftIntensity += Theme.Evaluate(SunShaftIntensityChange, sunHeight, num2) * Weight;
			output.BloomThreshold += Theme.Evaluate(BloomThresholdChange, sunHeight, num2) * Weight;
			output.BloomIntensity += Theme.Evaluate(BloomIntensityChange, sunHeight, num2) * Weight;
		}
	}
}
