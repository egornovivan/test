using System;
using System.Reflection;
using UnityEngine;

namespace WhiteCat;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class GetSetAttribute : PropertyAttribute
{
	public readonly string propertyName;

	public PropertyInfo propertyInfo;

	public string undoString;

	public GetSetAttribute(string propertyName)
	{
		this.propertyName = propertyName;
	}
}
