using System;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

public class BTUtil : MonoBehaviour
{
	public static string GetString(string data, int index)
	{
		string result = string.Empty;
		string[] array = PEUtil.ToArrayString(data, ' ');
		if (index >= 0 && index < array.Length)
		{
			result = array[index];
		}
		else
		{
			Debug.LogError("[" + data + "][index : " + index + "]");
		}
		return result;
	}

	public static bool GetBool(string data, int index)
	{
		bool result = false;
		string[] array = PEUtil.ToArrayString(data, ' ');
		if (index >= 0 && index < array.Length)
		{
			try
			{
				result = Convert.ToBoolean(array[index]);
			}
			catch (Exception)
			{
				Debug.LogError("[" + data + "][" + array[index] + "] is not boolean!");
			}
		}
		else
		{
			Debug.LogError("[" + data + "][index : " + index + "]");
		}
		return result;
	}

	public static float GetFloat(string data, int index)
	{
		float result = 0f;
		string[] array = PEUtil.ToArrayString(data, ' ');
		if (index >= 0 && index < array.Length)
		{
			try
			{
				result = Convert.ToSingle(array[index]);
			}
			catch (Exception)
			{
				Debug.LogError("[" + data + "][" + array[index] + "] is not float!");
			}
		}
		else
		{
			Debug.LogError("[" + data + "][index : " + index + "]");
		}
		return result;
	}

	public static int GetInt32(string data, int index)
	{
		int result = 0;
		string[] array = PEUtil.ToArrayString(data, ' ');
		if (index >= 0 && index < array.Length)
		{
			try
			{
				result = Convert.ToInt32(array[index]);
			}
			catch (Exception)
			{
				Debug.LogError("[" + data + "][" + array[index] + "] is not int!");
			}
		}
		else
		{
			Debug.LogError("[" + data + "][index : " + index + "]");
		}
		return result;
	}

	public static Vector3 GetVector3(string data, int index)
	{
		Vector3 result = Vector3.zero;
		string[] array = PEUtil.ToArrayString(data, ' ');
		if (index >= 0 && index < array.Length)
		{
			try
			{
				result = PEUtil.ToVector3(array[index], ',');
			}
			catch (Exception)
			{
				Debug.LogError("[" + data + "][" + array[index] + "] is not vector3!");
			}
		}
		else
		{
			Debug.LogError("[" + data + "][index : " + index + "]");
		}
		return result;
	}
}
