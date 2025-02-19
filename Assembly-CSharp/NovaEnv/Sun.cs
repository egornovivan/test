using System;
using UnityEngine;

namespace NovaEnv;

public class Sun : MonoBehaviour
{
	private Executor executor;

	[HideInInspector]
	public Light SunLight;

	[SerializeField]
	private float Obliquity;

	[SerializeField]
	private float UCA;

	[SerializeField]
	private float LCA;

	private bool _changeTrans;

	public Vector3 Direction;

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
			SunParam sunSettings = executor.SunSettings;
			Obliquity = sunSettings.Obliquity;
			SunLight = Utils.CreateGameObject(null, "SunLight", base.transform).AddComponent<Light>();
			SunLight.type = LightType.Directional;
			SunLight.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			SunLight.transform.localPosition = Vector3.forward * (executor.Settings.SkySize * 0.99f - 1f);
			SunLight.shadows = LightShadows.Soft;
			SunLight.shadowBias = 0.2f;
			SunLight.cullingMask = executor.Settings.LightCullingMask;
			Tick();
		}
	}

	public void Tick()
	{
		Vector3 vector = _getDir(0f);
		Vector3 normalized = (_getDir(720f) - vector).normalized;
		Vector3 normalized2 = Vector3.Cross(vector, normalized).normalized;
		Debug.DrawLine(SunLight.transform.position, SunLight.transform.position + normalized * 200f, Color.red);
		Debug.DrawLine(SunLight.transform.position, SunLight.transform.position + normalized2 * 200f, Color.green);
		Direction = vector;
		Quaternion identity = Quaternion.identity;
		identity.SetLookRotation(vector, normalized2);
		if (Vector3.Angle(base.transform.forward, vector) > 0.3f)
		{
			_changeTrans = true;
		}
		if (_changeTrans && Vector3.Angle(base.transform.forward, vector) < 0.01f)
		{
			_changeTrans = false;
		}
		Quaternion rotation = base.transform.rotation;
		base.transform.rotation = identity;
		Executor.SkySphereMat.SetVector("_StarAxisX", base.transform.right);
		Executor.SkySphereMat.SetVector("_StarAxisY", base.transform.up);
		Executor.SkySphereMat.SetVector("_StarAxisZ", base.transform.forward);
		Executor.SkySphereMat.SetVector("_SunPos", base.transform.forward);
		base.transform.rotation = rotation;
		if (_changeTrans)
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, identity, 0.3f);
		}
	}

	public Vector3 _getDir(float timeofs = 0f)
	{
		float num = (float)Math.Sin(Executor.SunYear * 6.28318530718);
		float num2 = num * Obliquity;
		float num3 = Utils.NormalizeDegree(Executor.Settings.LocalLatitude);
		float num4 = Utils.NormalizeDegree(180f - Executor.Settings.LocalLatitude);
		UCA = Utils.NormalizeDegree(90f - (num2 - num3));
		LCA = Utils.NormalizeDegree(num2 - num4 - 270f);
		Vector3 vector = new Vector3(0f, Mathf.Sin(UCA * ((float)Math.PI / 180f)), Mathf.Cos(UCA * ((float)Math.PI / 180f)));
		Vector3 vector2 = new Vector3(0f, Mathf.Sin(LCA * ((float)Math.PI / 180f)), Mathf.Cos(LCA * ((float)Math.PI / 180f)));
		float num5 = Mathf.Sqrt(Mathf.Clamp01(1f - ((vector + vector2) * 0.5f).sqrMagnitude));
		float f = (float)(Executor.FracSunDay + (double)timeofs / Executor.Settings.SecondsPerDay) * 2f * (float)Math.PI;
		Vector3 result = Vector3.Lerp(vector, vector2, Mathf.Cos(f) * 0.5f + 0.5f);
		result.x = Mathf.Lerp(0f - num5, num5, Mathf.Sin(f) * 0.5f + 0.5f);
		result.Normalize();
		return result;
	}
}
