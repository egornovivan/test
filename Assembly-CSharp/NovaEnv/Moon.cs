using System;
using UnityEngine;

namespace NovaEnv;

public class Moon : MonoBehaviour
{
	private Executor executor;

	public int Index;

	[SerializeField]
	private float Size;

	[HideInInspector]
	public Light MoonLight;

	[SerializeField]
	private float LightIntensity;

	[SerializeField]
	private Color LightColor;

	[SerializeField]
	private double Period;

	[SerializeField]
	private double Phi;

	[SerializeField]
	private float Obliquity;

	[SerializeField]
	private float UCA;

	[SerializeField]
	private float LCA;

	[SerializeField]
	private float DayPhase;

	private Material MoonMat;

	private GameObject MoonBody;

	public Executor Executor
	{
		get
		{
			return executor;
		}
		set
		{
			executor = value;
			Utils.DestroyChild(base.gameObject);
			MoonParam moonParam = executor.MoonsSettings[Index];
			base.gameObject.name = moonParam.Name;
			Size = moonParam.Size;
			LightIntensity = moonParam.LightIntensity;
			LightColor = moonParam.LightColor;
			Period = moonParam.Period;
			Phi = moonParam.Phi;
			Obliquity = moonParam.Obliquity;
			MoonMat = new Material(executor.Settings.MoonBodyShader);
			MoonMat.SetTexture("_MainTexture", moonParam.MainTex);
			MoonMat.SetTexture("_NormalTexture", moonParam.BumpTex);
			MoonMat.SetVector("_MoonRect", new Vector4(moonParam.MoonTexRect.x, moonParam.MoonTexRect.y, moonParam.MoonTexRect.xMax, moonParam.MoonTexRect.yMax));
			MoonMat.SetColor("_TintColor", moonParam.TintColor);
			MoonMat.renderQueue = 1000 + executor.MoonsSettings.Length - Index + 1;
			GameObject gameObject = Utils.CreateGameObject(PrimitiveType.Quad, "MoonBody", base.transform);
			gameObject.transform.localPosition = Vector3.forward * executor.Settings.SkySize * (0.94f + 0.05f * (float)Index / (float)executor.MoonsSettings.Length);
			gameObject.transform.localScale = Vector3.one * Size * 200f;
			gameObject.GetComponent<Renderer>().material = MoonMat;
			MoonBody = gameObject;
			MoonLight = Utils.CreateGameObject(null, "MoonLight", base.transform).AddComponent<Light>();
			MoonLight.type = LightType.Directional;
			MoonLight.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			MoonLight.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z - 1f);
			MoonLight.color = LightColor;
			MoonLight.intensity = LightIntensity;
			MoonLight.shadows = LightShadows.None;
			MoonLight.cullingMask = executor.Settings.LightCullingMask;
			Tick();
		}
	}

	public void OnDestroy()
	{
		UnityEngine.Object.Destroy(MoonMat);
	}

	public void Tick()
	{
		MoonLight.enabled = Executor.Settings.MoonLightEnable;
		Vector3 vector = _getDir(0f);
		Vector3 normalized = (_getDir(720f) - vector).normalized;
		Vector3 normalized2 = Vector3.Cross(vector, normalized).normalized;
		Debug.DrawLine(MoonBody.transform.position, MoonBody.transform.position + normalized * Size * 300f, Color.red);
		Debug.DrawLine(MoonBody.transform.position, MoonBody.transform.position + normalized2 * Size * 300f, Color.green);
		Quaternion identity = Quaternion.identity;
		identity.SetLookRotation(vector, normalized2);
		if (Executor.Settings.MainCamera != null)
		{
			Quaternion identity2 = Quaternion.identity;
			identity2.SetLookRotation(Executor.Settings.MainCamera.transform.forward, normalized2);
			MoonBody.transform.rotation = identity2;
		}
		base.transform.localRotation = identity;
	}

	private Vector3 _getDir(float timeofs = 0f)
	{
		double period = Executor.SunSettings.Period;
		float num = Utils.NormalizeDegree(Executor.Settings.LocalLatitude);
		float num2 = Utils.NormalizeDegree(180f - Executor.Settings.LocalLatitude);
		double num3 = period / (period / Period - 1.0);
		float num4 = (float)((Executor.UTC + (double)timeofs - Phi) / num3);
		num4 -= Mathf.Floor(num4);
		float num5 = Obliquity - Executor.SunSettings.Obliquity;
		float b = 0f - num5;
		float t = (1f - Mathf.Cos((float)((double)num4 + Executor.SunYear / 18.6 + 0.25) * 2f * (float)Math.PI)) * 0.5f;
		float num6 = Mathf.Lerp(num5, b, t);
		UCA = Utils.NormalizeDegree(90f - (num6 - num));
		LCA = Utils.NormalizeDegree(num6 - num2 - 270f);
		Vector3 vector = new Vector3(0f, Mathf.Sin(UCA * ((float)Math.PI / 180f)), Mathf.Cos(UCA * ((float)Math.PI / 180f)));
		Vector3 vector2 = new Vector3(0f, Mathf.Sin(LCA * ((float)Math.PI / 180f)), Mathf.Cos(LCA * ((float)Math.PI / 180f)));
		float num7 = Mathf.Sqrt(Mathf.Clamp01(1f - ((vector + vector2) * 0.5f).sqrMagnitude));
		float f = (float)(Executor.FracSunDay + (double)timeofs / Executor.Settings.SecondsPerDay - (double)num4) * 2f * (float)Math.PI;
		Vector3 vector3 = Vector3.Lerp(vector, vector2, Mathf.Cos(f) * 0.5f + 0.5f);
		vector3.x = Mathf.Lerp(0f - num7, num7, Mathf.Sin(f) * 0.5f + 0.5f);
		vector3.Normalize();
		if (Mathf.Abs(timeofs) < 0.01f)
		{
			DayPhase = (float)((double)num4 * num3 / executor.Settings.SecondsPerDay);
			MoonMat.SetVector("_SunDir", base.transform.InverseTransformDirection(Executor.SunDirection));
			MoonMat.SetColor("_CurrSkyColor", Executor.Sky.SkyColorAtPoint(vector3, Executor.SunDirection));
			MoonMat.SetFloat("_Overcast", Executor.Sky.Overcast);
			num4 = DayPhase;
		}
		return vector3;
	}
}
