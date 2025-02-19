using System;
using UnityEngine;

namespace WhiteCat;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class EulerAnglesAttribute : PropertyAttribute
{
	public bool initialized;

	public Vector3 eulerAngles;
}
