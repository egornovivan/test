using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class VCUtils
{
	public static Vector3 ClampInBound(Vector3 vec, Bounds bound)
	{
		if (vec.x < bound.min.x)
		{
			vec.x = bound.min.x;
		}
		else if (vec.x > bound.max.x)
		{
			vec.x = bound.max.x;
		}
		if (vec.y < bound.min.y)
		{
			vec.y = bound.min.y;
		}
		else if (vec.y > bound.max.y)
		{
			vec.y = bound.max.y;
		}
		if (vec.z < bound.min.z)
		{
			vec.z = bound.min.z;
		}
		else if (vec.z > bound.max.z)
		{
			vec.z = bound.max.z;
		}
		return vec;
	}

	public static List<string> ExplodeString(string s, char delimiter)
	{
		List<string> list = new List<string>();
		string text = string.Empty;
		foreach (char c in s)
		{
			if (c == delimiter)
			{
				list.Add(text);
				text = string.Empty;
			}
			else
			{
				text += c;
			}
		}
		list.Add(text);
		return list;
	}

	public static string Capital(string s, bool everyword = false)
	{
		string text = string.Empty;
		bool flag = false;
		foreach (char c in s)
		{
			if (everyword && c == ' ')
			{
				flag = false;
			}
			if (c >= 'A' && c <= 'Z')
			{
				flag = true;
			}
			if (!flag && c >= 'a' && c <= 'z')
			{
				text += (char)(c - 32);
				flag = true;
			}
			else
			{
				text += c;
			}
		}
		return text;
	}

	public static Color HSB2RGB(float h, float s, float b)
	{
		h %= 360f;
		Color result = new Color(0f, 0f, 0f, 1f);
		int num = (int)(h / 60f % 6f);
		float num2 = h / 60f - (float)num;
		float num3 = b * (1f - s);
		float num4 = b * (1f - num2 * s);
		float num5 = b * (1f - (1f - num2) * s);
		switch (num)
		{
		case 0:
			result.r = b;
			result.g = num5;
			result.b = num3;
			break;
		case 1:
			result.r = num4;
			result.g = b;
			result.b = num3;
			break;
		case 2:
			result.r = num3;
			result.g = b;
			result.b = num5;
			break;
		case 3:
			result.r = num3;
			result.g = num4;
			result.b = b;
			break;
		case 4:
			result.r = num5;
			result.g = num3;
			result.b = b;
			break;
		case 5:
			result.r = b;
			result.g = num3;
			result.b = num4;
			break;
		}
		return result;
	}

	public static Vector3 RGB2HSB(Color rgb, float nanH = 0f, float nanS = 0f)
	{
		rgb.r = Mathf.Clamp01(rgb.r);
		rgb.g = Mathf.Clamp01(rgb.g);
		rgb.b = Mathf.Clamp01(rgb.b);
		float num = Mathf.Max(rgb.r, rgb.g, rgb.b);
		float num2 = Mathf.Min(rgb.r, rgb.g, rgb.b);
		float z = num;
		float y = ((!(num < 0.01f)) ? ((num - num2) / num) : nanS);
		float x = 0f;
		if (Mathf.Abs(num - num2) < 0.005f)
		{
			x = nanH;
		}
		else if (num == rgb.r && rgb.g >= rgb.b)
		{
			x = (rgb.g - rgb.b) * 60f / (num - num2);
		}
		else if (num == rgb.r && rgb.g < rgb.b)
		{
			x = (rgb.g - rgb.b) * 60f / (num - num2) + 360f;
		}
		else if (num == rgb.g)
		{
			x = (rgb.b - rgb.r) * 60f / (num - num2) + 120f;
		}
		else if (num == rgb.b)
		{
			x = (rgb.r - rgb.g) * 60f / (num - num2) + 240f;
		}
		return new Vector3(x, y, z);
	}

	public static Texture2D LoadTextureFromFile(string filename)
	{
		if (!File.Exists(filename))
		{
			return null;
		}
		byte[] array = null;
		try
		{
			FileStream fileStream = new FileStream(filename, FileMode.Open);
			if (fileStream.Length > 4194304 || fileStream.Length < 8)
			{
				fileStream.Close();
				return null;
			}
			array = new byte[(int)fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
		}
		catch (Exception)
		{
			return null;
		}
		Texture2D texture2D = new Texture2D(2, 2);
		if (!texture2D.LoadImage(array))
		{
			UnityEngine.Object.Destroy(texture2D);
			return null;
		}
		return texture2D;
	}

	public static string MakeFileName(string name)
	{
		string text = string.Empty;
		for (int i = 0; i < name.Length; i++)
		{
			text = ((name[i] != '/' && name[i] != '\\' && name[i] != ':' && name[i] != '*' && name[i] != '\r' && name[i] != '\n' && name[i] != '?' && name[i] != '"' && name[i] != '<' && name[i] != '>' && name[i] != '|' && name[i] != '\b') ? (text + name[i]) : (text + " "));
		}
		return text;
	}

	public static bool MakeSingleLine(ref string s)
	{
		string text = string.Empty;
		bool result = true;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '\r' || s[i] == '\n')
			{
				result = false;
			}
			else
			{
				text += s[i];
			}
		}
		s = text;
		return result;
	}

	public static bool IsInteger(float x)
	{
		return Mathf.Abs(Mathf.Round(x) - x) < 1E-05f;
	}

	public static string LengthToString(float l)
	{
		if (IsInteger(l))
		{
			return l.ToString("0") + " m";
		}
		if (IsInteger(l * 100f))
		{
			return (l * 100f).ToString("0") + " cm";
		}
		if (IsInteger(l * 1000f))
		{
			return (l * 1000f).ToString("0") + " mm";
		}
		if (IsInteger(l * 3f))
		{
			return (l * 3f).ToString("0") + "/3 m";
		}
		if (IsInteger(l * 30f))
		{
			return (l * 30f).ToString("0") + "/30 m";
		}
		return (l * 100f).ToString("0.00") + " cm";
	}

	public static string VolumeToString(float v)
	{
		if (v == 0f)
		{
			return "0";
		}
		if ((double)v < 0.001)
		{
			return (v * 1000000f).ToString("#,##0.0") + " cm^3";
		}
		if (v < 1f)
		{
			return (v * 1000f).ToString("#,##0.00") + " L";
		}
		if (v < 100f)
		{
			return v.ToString("0.00") + " m^3";
		}
		if (v < 10000f)
		{
			return v.ToString("#,##0.0") + " m^3";
		}
		return v.ToString("#,##0") + " m^3";
	}

	public static string WeightToString(float w)
	{
		if (w == 0f)
		{
			return "0";
		}
		if ((double)w < 0.001)
		{
			return (w * 1000000f).ToString("#,##0.0") + " mg";
		}
		if (w < 1f)
		{
			return (w * 1000f).ToString("#,##0.0") + " g";
		}
		if (w < 1000f)
		{
			return w.ToString("0.00") + " kg";
		}
		if (w < 100000f)
		{
			return ((double)w * 0.001).ToString("#,##0.00") + " T";
		}
		return ((double)w * 0.001).ToString("#,##0.0") + " T";
	}

	public static bool VectorApproximate(Vector3 a, Vector3 b, string format)
	{
		string text = a.x.ToString(format) + " " + a.y.ToString(format) + " " + a.z.ToString(format);
		string text2 = b.x.ToString(format) + " " + b.y.ToString(format) + " " + b.z.ToString(format);
		return text == text2;
	}

	public static T GetComponentOrOnParent<T>(GameObject go) where T : Component
	{
		Transform transform = go.transform;
		T result = (T)null;
		while (transform != null)
		{
			T component = transform.GetComponent<T>();
			if (component != null)
			{
				result = component;
			}
			transform = transform.parent;
		}
		return result;
	}

	public static Transform GetChildByName(Transform parent, string child_name)
	{
		if (child_name == string.Empty)
		{
			return null;
		}
		foreach (Transform item in parent)
		{
			if (item.name.Equals(child_name))
			{
				return item;
			}
			Transform childByName = GetChildByName(item, child_name);
			if (childByName != null)
			{
				return childByName;
			}
		}
		return null;
	}

	public static Vector3 RandPosInBoundingBox(Bounds bbox)
	{
		return new Vector3(bbox.min.x + UnityEngine.Random.value * bbox.size.x, bbox.min.y + UnityEngine.Random.value * bbox.size.y, bbox.min.z + UnityEngine.Random.value * bbox.size.z);
	}

	public static void ISOCut(VCIsoData iso, VCEAction action)
	{
		List<VCEAlterVoxel> list = new List<VCEAlterVoxel>();
		foreach (KeyValuePair<int, VCVoxel> voxel in iso.m_Voxels)
		{
			int pos = voxel.Key + 1;
			int pos2 = voxel.Key - 1;
			int pos3 = voxel.Key + 1024;
			int pos4 = voxel.Key - 1024;
			int pos5 = voxel.Key + 1048576;
			int pos6 = voxel.Key - 1048576;
			if (voxel.Value.Volume < 128 && iso.GetVoxel(pos).Volume < 128 && iso.GetVoxel(pos2).Volume < 128 && iso.GetVoxel(pos3).Volume < 128 && iso.GetVoxel(pos4).Volume < 128 && iso.GetVoxel(pos5).Volume < 128 && iso.GetVoxel(pos6).Volume < 128)
			{
				VCEAlterVoxel item = new VCEAlterVoxel(voxel.Key, voxel.Value, new VCVoxel(0, 0));
				list.Add(item);
				action.Modifies.Add(item);
			}
		}
		foreach (VCEAlterVoxel item2 in list)
		{
			item2.Redo();
		}
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

	public static short CompressSmallFloat(float f)
	{
		return (short)Mathf.RoundToInt(Mathf.Clamp(f * 400f, -32768f, 32767f));
	}

	public static float UncompressSmallFloat(short s)
	{
		return (float)((double)s * 0.0025);
	}

	public static bool IsSeat(EVCComponent type)
	{
		return type == EVCComponent.cpVehicleCockpit || type == EVCComponent.cpVtolCockpit || type == EVCComponent.cpShipCockpit || type == EVCComponent.cpSideSeat;
	}
}
