using System;
using System.Collections.Generic;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace Pathfinding.Serialization;

public class MatrixConverter : JsonConverter
{
	private float[] values = new float[16];

	public override bool CanConvert(Type type)
	{
		return object.Equals(type, typeof(Matrix4x4));
	}

	public override object ReadJson(Type objectType, Dictionary<string, object> values)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		Array array = (Array)values["values"];
		if (array.Length != 16)
		{
			Debug.LogError("Number of elements in matrix was not 16 (got " + array.Length + ")");
			return matrix4x;
		}
		for (int i = 0; i < 16; i++)
		{
			matrix4x[i] = Convert.ToSingle(array.GetValue(new int[1] { i }));
		}
		return matrix4x;
	}

	public override Dictionary<string, object> WriteJson(Type type, object value)
	{
		Matrix4x4 matrix4x = (Matrix4x4)value;
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = matrix4x[i];
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("values", values);
		return dictionary;
	}
}
