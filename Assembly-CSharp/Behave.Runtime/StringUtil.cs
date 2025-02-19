using System;
using System.Collections.Generic;
using UnityEngine;

namespace Behave.Runtime;

public class StringUtil
{
	public static string[] ToArrayString(string str, char c)
	{
		return str.Split(c);
	}

	public static int[] ToArrayInt32(string str, char c)
	{
		string[] array = ToArrayString(str, c);
		List<int> list = new List<int>();
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToInt32(value));
		}
		return list.ToArray();
	}

	public static byte[] ToArrayByte(string str, char c)
	{
		string[] array = ToArrayString(str, c);
		List<byte> list = new List<byte>();
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToByte(value));
		}
		return list.ToArray();
	}

	public static float[] ToArraySingle(string str, char c)
	{
		string[] array = ToArrayString(str, c);
		List<float> list = new List<float>();
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToSingle(value));
		}
		return list.ToArray();
	}

	public static Vector3 ToVector3(string str, char c)
	{
		string[] array = ToArrayString(str, c);
		if (array.Length != 3)
		{
			return Vector3.zero;
		}
		List<float> list = new List<float>();
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToSingle(value));
		}
		return new Vector3(list[0], list[1], list[2]);
	}

	public static Vector3[] ToArrayVector3(string str, char s, char c)
	{
		string[] array = ToArrayString(str, s);
		List<Vector3> list = new List<Vector3>();
		string[] array2 = array;
		foreach (string str2 in array2)
		{
			list.Add(ToVector3(str2, c));
		}
		return list.ToArray();
	}

	public static Color32 ToColor32(string data, char c)
	{
		byte[] array = ToArrayByte(data, c);
		if (array.Length != 4)
		{
			return new Color32(0, 0, 0, 0);
		}
		return new Color32(array[0], array[1], array[2], array[3]);
	}
}
