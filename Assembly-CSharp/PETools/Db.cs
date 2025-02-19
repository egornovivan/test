using System;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace PETools;

public static class Db
{
	public static int[] ReadIntArray(string text)
	{
		if (string.IsNullOrEmpty(text) || text == "0")
		{
			return null;
		}
		string[] array = text.Split(',');
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = Convert.ToInt32(array[i]);
		}
		return array2;
	}

	public static float[] ReadFloatArray(string text)
	{
		if (string.IsNullOrEmpty(text) || text == "0")
		{
			return null;
		}
		string[] array = text.Split(',');
		float[] array2 = new float[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = Convert.ToSingle(array[i]);
		}
		return array2;
	}

	private static Vector3[] ReadVector3Array(string text)
	{
		if (string.IsNullOrEmpty(text) || text == "0")
		{
			return null;
		}
		string[] array = text.Split(';');
		Vector3[] array2 = new Vector3[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			float[] array3 = ReadFloatArray(array[i]);
			if (array3 != null && array3.Length >= 3)
			{
				ref Vector3 reference = ref array2[i];
				reference = new Vector3(array3[0], array3[1], array3[2]);
			}
		}
		return array2;
	}

	private static Quaternion[] ReadQuaternionArray(string text)
	{
		if (string.IsNullOrEmpty(text) || text == "0")
		{
			return null;
		}
		string[] array = text.Split(';');
		Quaternion[] array2 = new Quaternion[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			float[] array3 = ReadFloatArray(array[i]);
			if (array3 != null && array3.Length >= 4)
			{
				ref Quaternion reference = ref array2[i];
				reference = new Quaternion(array3[0], array3[1], array3[2], array3[3]);
			}
		}
		return array2;
	}

	private static Color ReadColor(string text)
	{
		if (string.IsNullOrEmpty(text) || text == "0")
		{
			return Color.black;
		}
		string[] array = text.Split(',');
		return new Color(Convert.ToSingle(array[0]) / 255f, Convert.ToSingle(array[1]) / 255f, Convert.ToSingle(array[2]) / 255f, Convert.ToSingle(array[3]) / 255f);
	}

	public static Vector3 GetVector3(SqliteDataReader reader, string fieldName)
	{
		float[] array = ReadFloatArray(reader.GetString(reader.GetOrdinal(fieldName)));
		if (array != null && array.Length >= 3)
		{
			return new Vector3(array[0], array[1], array[2]);
		}
		return Vector3.zero;
	}

	public static Vector3[] GetVector3Array(SqliteDataReader reader, string fieldName)
	{
		return ReadVector3Array(reader.GetString(reader.GetOrdinal(fieldName)));
	}

	public static Quaternion GetQuaternion(SqliteDataReader reader, string fieldName)
	{
		float[] array = ReadFloatArray(reader.GetString(reader.GetOrdinal(fieldName)));
		if (array != null && array.Length >= 4)
		{
			return new Quaternion(array[0], array[1], array[2], array[3]);
		}
		return Quaternion.identity;
	}

	public static Quaternion[] GetQuaternionArray(SqliteDataReader reader, string fieldName)
	{
		return ReadQuaternionArray(reader.GetString(reader.GetOrdinal(fieldName)));
	}

	public static int GetInt(SqliteDataReader reader, string fieldName)
	{
		return Convert.ToInt32(reader.GetString(reader.GetOrdinal(fieldName)));
	}

	public static int[] GetIntArray(SqliteDataReader reader, string fieldName)
	{
		return ReadIntArray(reader.GetString(reader.GetOrdinal(fieldName)));
	}

	public static float GetFloat(SqliteDataReader reader, string fieldName)
	{
		return Convert.ToSingle(reader.GetString(reader.GetOrdinal(fieldName)));
	}

	public static float[] GetFloatArray(SqliteDataReader reader, string fieldName)
	{
		return ReadFloatArray(reader.GetString(reader.GetOrdinal(fieldName)));
	}

	public static string GetString(SqliteDataReader reader, string fieldName)
	{
		return reader.GetString(reader.GetOrdinal(fieldName));
	}

	public static bool GetBool(SqliteDataReader reader, string fieldName)
	{
		return (GetInt(reader, fieldName) != 0) ? true : false;
	}

	public static Color GetColor(SqliteDataReader reader, string fieldName)
	{
		return ReadColor(reader.GetString(reader.GetOrdinal(fieldName)));
	}

	public static object TraverseHierarchySerial(this Transform root, Func<Transform, int, object> operate, int depthLimit = -1)
	{
		if (operate != null)
		{
			object obj = operate(root, depthLimit);
			if (obj != null || depthLimit == 0)
			{
				return obj;
			}
			for (int num = root.childCount - 1; num >= 0; num--)
			{
				obj = root.GetChild(num).TraverseHierarchySerial(operate, depthLimit - 1);
				if (obj != null)
				{
					return obj;
				}
			}
		}
		return null;
	}

	public static void TraverseHierarchySerial(this Transform root, Action<Transform, int> operate, int depthLimit = -1)
	{
		if (operate == null)
		{
			return;
		}
		operate(root, depthLimit);
		if (depthLimit != 0)
		{
			for (int i = 0; i < root.childCount; i++)
			{
				root.GetChild(i).TraverseHierarchySerial(operate, depthLimit - 1);
			}
		}
	}
}
