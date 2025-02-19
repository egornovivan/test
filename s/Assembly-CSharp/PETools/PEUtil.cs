using System;
using System.Collections.Generic;
using UnityEngine;

namespace PETools;

public class PEUtil
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

	public static Color32 ToColor32(string data, char c)
	{
		byte[] array = ToArrayByte(data, c);
		if (array.Length != 4)
		{
			return new Color32(0, 0, 0, 0);
		}
		return new Color32(array[0], array[1], array[2], array[3]);
	}

	public static int CompressEulerAngle(Vector3 eulerAngle)
	{
		eulerAngle.x %= 360f;
		eulerAngle.y %= 360f;
		eulerAngle.z %= 360f;
		if (eulerAngle.x < 0f)
		{
			eulerAngle.x += 360f;
		}
		if (eulerAngle.y < 0f)
		{
			eulerAngle.y += 360f;
		}
		if (eulerAngle.z < 0f)
		{
			eulerAngle.z += 360f;
		}
		int num = Mathf.RoundToInt(eulerAngle.x / 360f * 1024f);
		int num2 = Mathf.RoundToInt(eulerAngle.y / 360f * 2048f);
		int num3 = Mathf.RoundToInt(eulerAngle.z / 360f * 1024f);
		return (num & 0x3FF) | ((num3 & 0x3FF) << 10) | ((num2 & 0x7FF) << 20);
	}

	public static Vector3 UncompressEulerAngle(int data)
	{
		int num = data & 0x3FF;
		int num2 = (data >> 10) & 0x3FF;
		int num3 = (data >> 20) & 0x7FF;
		return new Vector3((float)((double)num * (45.0 / 128.0)), (float)((double)num3 * (45.0 / 256.0)), (float)((double)num2 * (45.0 / 128.0)));
	}
}
