using System.Collections.Generic;
using UnityEngine;

public class LightMgr
{
	private static LightMgr _instance;

	private bool _isFastLightingMode;

	public List<LightUnit> lights = new List<LightUnit>();

	public static LightMgr Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new LightMgr();
			}
			return _instance;
		}
	}

	public void Registerlight(LightUnit light)
	{
		if (!lights.Contains(light))
		{
			lights.Add(light);
			if (_isFastLightingMode)
			{
				light.lamp.shadows = LightShadows.None;
				light.lamp.renderMode = LightRenderMode.ForceVertex;
			}
		}
	}

	public void RemoveLight(LightUnit light)
	{
		if (lights.Contains(light))
		{
			lights.Remove(light);
		}
	}

	public LightUnit GetLight(Vector3 point)
	{
		int count = lights.Count;
		for (int i = 0; i < count; i++)
		{
			if (lights[i] != null && lights[i].IsInLight(point))
			{
				return lights[i];
			}
		}
		return null;
	}

	public LightUnit GetLight(Transform tr)
	{
		int count = lights.Count;
		for (int i = 0; i < count; i++)
		{
			if (lights[i] != null && lights[i].IsInLight(tr))
			{
				return lights[i];
			}
		}
		return null;
	}

	public LightUnit GetLight(Transform tr, Bounds bounds)
	{
		int count = lights.Count;
		for (int i = 0; i < count; i++)
		{
			if (lights[i] != null && lights[i].IsInLight(tr, bounds))
			{
				return lights[i];
			}
		}
		return null;
	}

	public void SetLightMode(bool fastMode)
	{
		_isFastLightingMode = fastMode;
		int count = lights.Count;
		if (_isFastLightingMode)
		{
			for (int i = 0; i < count; i++)
			{
				lights[i].lamp.shadows = LightShadows.None;
				lights[i].lamp.renderMode = LightRenderMode.ForceVertex;
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				lights[j].lamp.shadows = lights[j].shadowsBak;
				lights[j].lamp.renderMode = lights[j].renderModeBak;
			}
		}
	}
}
