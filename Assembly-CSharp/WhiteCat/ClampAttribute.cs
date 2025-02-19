using System;
using UnityEngine;

namespace WhiteCat;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class ClampAttribute : PropertyAttribute
{
	public readonly float min;

	public readonly float max;

	public ClampAttribute(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}
