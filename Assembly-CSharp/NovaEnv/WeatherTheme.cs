using System;
using UnityEngine;

namespace NovaEnv;

[Serializable]
public class WeatherTheme : Theme
{
	public AnimationCurve SkySaturateChange;

	public AnimationCurve FogIntensityChange;

	public AnimationCurve FogDensityChange;

	public AnimationCurve LightIntensityChange;

	public AnimationCurve CloudThresholdChange;

	public AnimationCurve CloudDensityChange;

	public AnimationCurve CloudOvercastChange;

	public AnimationCurve SkyOvercastChange;

	public AnimationCurve WaterSaturateChange;

	public AnimationCurve WaterIntensityChange;

	public AnimationCurve WaterSpecularIntensityChange;

	public AnimationCurve AmbientIntensityChange;

	public AnimationCurve ShadowStrengthChange;

	public AnimationCurve SunShaftIntensityChange;

	public override void Execute(Executor executor)
	{
		Output output = executor.Output;
		output.RainCloudDensity = CloudDensityChange.Evaluate(Weight);
		output.RainCloudThreshold = CloudThresholdChange.Evaluate(Weight);
		output.RainCloudOvercast = CloudOvercastChange.Evaluate(Weight);
		output.SkyOvercast = SkyOvercastChange.Evaluate(Weight);
		if (!(Weight < 0.001f))
		{
			output.SkyColor = Utils.ColorSaturate(output.SkyColor, SkySaturateChange.Evaluate(Weight));
			output.FogBrightColor = Utils.ColorSaturate(output.FogBrightColor, SkySaturateChange.Evaluate(Weight)) * FogIntensityChange.Evaluate(Weight);
			output.FogDarkColor = Utils.ColorSaturate(output.FogDarkColor, SkySaturateChange.Evaluate(Weight)) * FogIntensityChange.Evaluate(Weight);
			output.FogEndDistance /= FogDensityChange.Evaluate(Weight);
			output.SunGlowColor = Utils.ColorSaturate(output.SunGlowColor, SkySaturateChange.Evaluate(Weight)) * FogIntensityChange.Evaluate(Weight);
			output.SunLightIntensity *= LightIntensityChange.Evaluate(Weight);
			output.WaterDepthColor = Utils.ColorSaturate(output.WaterDepthColor, WaterSaturateChange.Evaluate(Weight)) * WaterIntensityChange.Evaluate(Weight);
			output.WaterReflectionColor = Utils.ColorSaturate(output.WaterReflectionColor, WaterSaturateChange.Evaluate(Weight)) * WaterIntensityChange.Evaluate(Weight);
			output.WaterFresnelColor = Utils.ColorSaturate(output.WaterFresnelColor, WaterSaturateChange.Evaluate(Weight)) * WaterIntensityChange.Evaluate(Weight);
			output.WaterFoamColor *= WaterIntensityChange.Evaluate(Weight);
			output.WaterSpecularColor *= WaterSpecularIntensityChange.Evaluate(Weight);
			output.AmbientIntensity *= AmbientIntensityChange.Evaluate(Weight);
			output.ShadowStrength *= ShadowStrengthChange.Evaluate(Weight);
			output.SunShaftIntensity *= SunShaftIntensityChange.Evaluate(Weight);
		}
	}
}
