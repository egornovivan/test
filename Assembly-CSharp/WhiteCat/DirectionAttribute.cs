using System;
using UnityEngine;

namespace WhiteCat;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class DirectionAttribute : PropertyAttribute
{
	public readonly float length;

	public bool initialized;

	public Vector3 eulerAngles;

	public DirectionAttribute(float length = 1f)
	{
		this.length = length;
	}
}
