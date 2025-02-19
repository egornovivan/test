using System;
using UnityEngine;

namespace NovaEnv;

[Serializable]
public abstract class Theme
{
	public string Name;

	public float Weight;

	public abstract void Execute(Executor executor);

	public static Color Evaluate(Gradient g, float sunHeight, float ap)
	{
		if (ap <= 0.001f)
		{
			return g.Evaluate(sunHeight);
		}
		if (ap >= 0.999f)
		{
			return g.Evaluate(1f - sunHeight);
		}
		return Color.Lerp(g.Evaluate(sunHeight), g.Evaluate(1f - sunHeight), ap);
	}

	public static float Evaluate(AnimationCurve c, float sunHeight, float ap)
	{
		if (ap <= 0.001f)
		{
			return c.Evaluate(sunHeight);
		}
		if (ap >= 0.999f)
		{
			return c.Evaluate(1f - sunHeight);
		}
		return Mathf.Lerp(c.Evaluate(sunHeight), c.Evaluate(1f - sunHeight), ap);
	}
}
