using UnityEngine;

namespace NovaEnv;

public class Sky : MonoBehaviour
{
	[HideInInspector]
	public Executor Executor;

	public Color SkyColor;

	public Color FogColorA;

	public Color FogColorB;

	public Color UnityFogColor;

	public float FogHeight = 0.7f;

	public Color SunBloomColor;

	public Color SunColor;

	public float SunSize = 0.5f;

	public float SunPower = 100f;

	public float Overcast = 1f;

	public void Apply()
	{
		Executor.SkySphereMat.SetColor("_SkyColor", SkyColor);
		Executor.SkySphereMat.SetColor("_FogColorA", FogColorA);
		Executor.SkySphereMat.SetColor("_FogColorB", FogColorB);
		Executor.FogSphereMat.SetColor("_FogColor", UnityFogColor);
		Executor.FogSphereMat.SetFloat("_FogHeight", FogHeight * 0.2f);
		Executor.SkySphereMat.SetFloat("_FogHeight", FogHeight);
		Executor.SkySphereMat.SetColor("_SunBloomColor", SunBloomColor);
		Executor.SkySphereMat.SetColor("_SunColor", SunColor);
		Executor.SkySphereMat.SetFloat("_SunSize", SunSize);
		Executor.SkySphereMat.SetFloat("_SunPower", SunPower);
		Executor.SkySphereMat.SetFloat("_Overcast", Overcast);
	}

	public Color SkyColorAtPoint(Vector3 point, Vector3 sunpos)
	{
		Vector3 normalized = sunpos.normalized;
		Vector3 normalized2 = point.normalized;
		float num = Vector3.Dot(normalized, normalized2);
		num = ((!(num > 0f)) ? 0f : Mathf.Pow(num, SunPower));
		num *= 0.4f;
		float num2 = Vector3.Dot(normalized, normalized2) + 0.7f;
		Color b = Color.Lerp(FogColorB, FogColorA, Mathf.Clamp01(num2 * 0.7f));
		num2 *= 0.3f;
		float num3 = Mathf.Clamp01(normalized2.y);
		float value = Mathf.Pow(1f - Mathf.Clamp01(num3 / FogHeight), 1.5f) * b.a;
		Color result = Color.Lerp(SkyColor, b, Mathf.Clamp01(value)) + SunBloomColor * num + SunBloomColor * num2;
		result.a = SkyColor.a;
		return result;
	}

	public Color FogColorAtPoint(Vector3 point, Vector3 sunpos, float interpolator)
	{
		Vector3 normalized = sunpos.normalized;
		Vector3 normalized2 = point.normalized;
		float num = Vector3.Dot(normalized, normalized2);
		num = ((!(num > 0f)) ? 0f : Mathf.Pow(num, SunPower));
		num *= 0.4f;
		float num2 = Vector3.Dot(normalized, normalized2) + 0.7f;
		Color color = Color.Lerp(FogColorB, FogColorA, Mathf.Clamp01(num2 * 0.7f));
		num2 *= 0.3f;
		float num3 = Mathf.Clamp01(normalized2.y);
		float value = Mathf.Pow(1f - Mathf.Clamp01(num3 / FogHeight), 1.5f) * color.a;
		Color b = Color.Lerp(SkyColor, color, Mathf.Clamp01(value)) + SunBloomColor * num + SunBloomColor * num2;
		b.a = SkyColor.a;
		return Color.Lerp(color, b, interpolator);
	}
}
