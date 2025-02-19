using System;
using System.Collections.Generic;
using UnityEngine;

namespace CameraForge;

public static class InputModule
{
	public delegate object DAxisFunc();

	private static Dictionary<string, DAxisFunc> axisFuncMap = new Dictionary<string, DAxisFunc>();

	public static Var Axis(string axis)
	{
		if (axisFuncMap.ContainsKey(axis))
		{
			DAxisFunc dAxisFunc = axisFuncMap[axis];
			if (dAxisFunc != null)
			{
				object obj = dAxisFunc();
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (obj is int)
				{
					return (int)obj;
				}
				if (obj is float)
				{
					return (float)obj;
				}
				if (obj is Vector2)
				{
					return (Vector2)obj;
				}
				if (obj is Vector3)
				{
					return (Vector3)obj;
				}
				if (obj is Vector4)
				{
					return (Vector4)obj;
				}
				return Var.Null;
			}
		}
		switch (axis)
		{
		case "Mouse Left Button":
			return Input.GetMouseButton(0);
		case "Mouse Right Button":
			return Input.GetMouseButton(1);
		case "Mouse Middle Button":
			return Input.GetMouseButton(2);
		default:
			if (axis.Substring(axis.Length - 4, 4) == " Key")
			{
				string value = axis.Substring(0, axis.Length - 4);
				KeyCode key = (KeyCode)(int)Enum.Parse(typeof(KeyCode), value);
				return Input.GetKey(key);
			}
			return Input.GetAxis(axis);
		}
	}

	public static void SetAxis(string axis, DAxisFunc func)
	{
		axis = axis.Trim();
		if (!string.IsNullOrEmpty(axis))
		{
			if (func == null)
			{
				axisFuncMap.Remove(axis);
			}
			axisFuncMap[axis] = func;
		}
	}

	public static void ResetAxes()
	{
		axisFuncMap.Clear();
	}
}
