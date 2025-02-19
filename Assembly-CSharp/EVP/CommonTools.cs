using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace EVP;

public static class CommonTools
{
	public class BiasLerpContext
	{
		public float lastBias = -1f;

		public float lastExponent;
	}

	private static Dictionary<string, Color> m_colors;

	static CommonTools()
	{
		m_colors = new Dictionary<string, Color>();
		m_colors["red"] = Color.red;
		m_colors["green"] = Color.green;
		m_colors["blue"] = Color.blue;
		m_colors["white"] = Color.white;
		m_colors["black"] = Color.black;
		m_colors["yellow"] = Color.yellow;
		m_colors["cyan"] = Color.cyan;
		m_colors["magenta"] = Color.magenta;
		m_colors["gray"] = Color.gray;
		m_colors["grey"] = Color.grey;
		m_colors["clear"] = Color.clear;
	}

	public static int HexToDecimal(char ch)
	{
		switch (ch)
		{
		case '0':
			return 0;
		case '1':
			return 1;
		case '2':
			return 2;
		case '3':
			return 3;
		case '4':
			return 4;
		case '5':
			return 5;
		case '6':
			return 6;
		case '7':
			return 7;
		case '8':
			return 8;
		case '9':
			return 9;
		case 'A':
		case 'a':
			return 10;
		case 'B':
		case 'b':
			return 11;
		case 'C':
		case 'c':
			return 12;
		case 'D':
		case 'd':
			return 13;
		case 'E':
		case 'e':
			return 14;
		case 'F':
		case 'f':
			return 15;
		default:
			return 0;
		}
	}

	public static Color ParseColor(string col)
	{
		if (m_colors.ContainsKey(col))
		{
			return m_colors[col];
		}
		Color black = Color.black;
		int length = col.Length;
		if (length > 0 && col[0] == "#"[0])
		{
			switch (length)
			{
			case 4:
			case 5:
			{
				float num = 1f / 15f;
				black.r = (float)HexToDecimal(col[1]) * num;
				black.g = (float)HexToDecimal(col[2]) * num;
				black.b = (float)HexToDecimal(col[3]) * num;
				if (length == 5)
				{
					black.a = (float)HexToDecimal(col[4]) * num;
				}
				break;
			}
			case 7:
			case 9:
			{
				float num = 0.003921569f;
				black.r = (float)((HexToDecimal(col[1]) << 4) | HexToDecimal(col[2])) * num;
				black.g = (float)((HexToDecimal(col[3]) << 4) | HexToDecimal(col[4])) * num;
				black.b = (float)((HexToDecimal(col[5]) << 4) | HexToDecimal(col[6])) * num;
				if (length == 9)
				{
					black.a = (float)((HexToDecimal(col[7]) << 4) | HexToDecimal(col[8])) * num;
				}
				break;
			}
			}
		}
		return black;
	}

	public static float ClampAngle(float angle)
	{
		angle %= 360f;
		if (angle > 180f)
		{
			angle -= 360f;
		}
		return angle;
	}

	public static float ClampAngle360(float angle)
	{
		angle %= 360f;
		if (angle < 0f)
		{
			angle += 360f;
		}
		return angle;
	}

	public static void DrawCrossMark(Vector3 pos, Transform trans, Color col, float length = 0.1f)
	{
		length *= 0.5f;
		Vector3 vector = trans.forward * length;
		Vector3 vector2 = trans.up * length;
		Vector3 vector3 = trans.right * length;
		Debug.DrawLine(pos - vector, pos + vector, col);
		Debug.DrawLine(pos - vector2, pos + vector2, col);
		Debug.DrawLine(pos - vector3, pos + vector3, col);
	}

	public static float Lin2Log(float val)
	{
		return Mathf.Log(Mathf.Abs(val) + 1f) * Mathf.Sign(val);
	}

	public static Vector3 Lin2Log(Vector3 val)
	{
		return Vector3.ClampMagnitude(val, Lin2Log(val.magnitude));
	}

	public static T CloneObject<T>(T source)
	{
		if (!typeof(T).IsSerializable)
		{
			throw new ArgumentException("The type must be serializable.", "source");
		}
		if (object.ReferenceEquals(source, null))
		{
			return default(T);
		}
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		Stream stream = new MemoryStream();
		using (stream)
		{
			xmlSerializer.Serialize(stream, source);
			stream.Seek(0L, SeekOrigin.Begin);
			return (T)xmlSerializer.Deserialize(stream);
		}
	}

	public static float FastLerp(float from, float to, float t)
	{
		return from + (to - from) * t;
	}

	public static float LinearLerp(float x0, float y0, float x1, float y1, float x)
	{
		return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
	}

	public static float LinearLerp(Vector2 from, Vector2 to, float t)
	{
		return LinearLerp(from.x, from.y, to.x, to.y, t);
	}

	public static float CubicLerp(float x0, float y0, float x1, float y1, float x)
	{
		float num = (x - x0) / (x1 - x0);
		float num2 = num * num;
		float num3 = num * num2;
		return y0 * (2f * num3 - 3f * num2 + 1f) + y1 * (-2f * num3 + 3f * num2);
	}

	public static float CubicLerp(Vector2 from, Vector2 to, float t)
	{
		return CubicLerp(from.x, from.y, to.x, to.y, t);
	}

	public static float TangentLerp(float x0, float y0, float x1, float y1, float a, float b, float x)
	{
		float num = y1 - y0;
		float num2 = 3f * num * a;
		float num3 = 3f * num * b;
		float num4 = (x - x0) / (x1 - x0);
		float num5 = num4 * num4;
		float num6 = num4 * num5;
		return y0 * (2f * num6 - 3f * num5 + 1f) + y1 * (-2f * num6 + 3f * num5) + num2 * (num6 - 2f * num5 + num4) + num3 * (num6 - num5);
	}

	public static float TangentLerp(Vector2 from, Vector2 to, float a, float b, float t)
	{
		return TangentLerp(from.x, from.y, to.x, to.y, a, b, t);
	}

	public static float HermiteLerp(float x0, float y0, float x1, float y1, float outTangent, float inTangent, float x)
	{
		float num = (x - x0) / (x1 - x0);
		float num2 = num * num;
		float num3 = num * num2;
		return y0 * (2f * num3 - 3f * num2 + 1f) + y1 * (-2f * num3 + 3f * num2) + outTangent * (num3 - 2f * num2 + num) + inTangent * (num3 - num2);
	}

	private static float BiasWithContext(float x, float bias, BiasLerpContext context)
	{
		if (x <= 0f)
		{
			return 0f;
		}
		if (x >= 1f)
		{
			return 1f;
		}
		if (bias != context.lastBias)
		{
			if (bias <= 0f)
			{
				return (!(x >= 1f)) ? 0f : 1f;
			}
			if (bias >= 1f)
			{
				return (!(x > 0f)) ? 0f : 1f;
			}
			if (bias == 0.5f)
			{
				return x;
			}
			context.lastExponent = Mathf.Log(bias) * -1.4427f;
			context.lastBias = bias;
		}
		return Mathf.Pow(x, context.lastExponent);
	}

	private static float BiasRaw(float x, float bias)
	{
		if (x <= 0f)
		{
			return 0f;
		}
		if (x >= 1f)
		{
			return 1f;
		}
		if (bias <= 0f)
		{
			return (!(x >= 1f)) ? 0f : 1f;
		}
		if (bias >= 1f)
		{
			return (!(x > 0f)) ? 0f : 1f;
		}
		if (bias == 0.5f)
		{
			return x;
		}
		float p = Mathf.Log(bias) * -1.4427f;
		return Mathf.Pow(x, p);
	}

	public static float BiasedLerp(float x, float bias)
	{
		float num = ((!(bias <= 0.5f)) ? (1f - BiasRaw(1f - Mathf.Abs(x), 1f - bias)) : BiasRaw(Mathf.Abs(x), bias));
		return (!(x < 0f)) ? num : (0f - num);
	}

	public static float BiasedLerp(float x, float bias, BiasLerpContext context)
	{
		float num = ((!(bias <= 0.5f)) ? (1f - BiasWithContext(1f - Mathf.Abs(x), 1f - bias, context)) : BiasWithContext(Mathf.Abs(x), bias, context));
		return (!(x < 0f)) ? num : (0f - num);
	}
}
