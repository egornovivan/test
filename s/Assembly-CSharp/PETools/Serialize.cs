using System;
using System.IO;
using UnityEngine;

namespace PETools;

public static class Serialize
{
	public static void WriteBytes(byte[] buff, BinaryWriter w)
	{
		if (buff != null && buff.Length > 0)
		{
			w.Write(buff.Length);
			w.Write(buff);
		}
		else
		{
			w.Write(0);
		}
	}

	public static byte[] ReadBytes(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num <= 0)
		{
			return null;
		}
		return r.ReadBytes(num);
	}

	public static void WriteColor(BinaryWriter bw, Color c)
	{
		bw.Write(c.r);
		bw.Write(c.g);
		bw.Write(c.b);
		bw.Write(c.a);
	}

	public static Color ReadColor(BinaryReader br)
	{
		Color result = default(Color);
		result.r = br.ReadSingle();
		result.g = br.ReadSingle();
		result.b = br.ReadSingle();
		result.a = br.ReadSingle();
		return result;
	}

	public static void WriteVector(BinaryWriter bw, Vector3 v)
	{
		bw.Write(v.x);
		bw.Write(v.y);
		bw.Write(v.z);
	}

	public static Vector3 ReadVector(BinaryReader br)
	{
		Vector3 result = default(Vector3);
		result.x = br.ReadSingle();
		result.y = br.ReadSingle();
		result.z = br.ReadSingle();
		return result;
	}

	public static void WriteNullableString(BinaryWriter w, string v)
	{
		if (string.IsNullOrEmpty(v))
		{
			w.Write(value: true);
			return;
		}
		w.Write(value: false);
		w.Write(v);
	}

	public static string ReadNullableString(BinaryReader r)
	{
		if (r.ReadBoolean())
		{
			return null;
		}
		return r.ReadString();
	}

	public static byte[] Export(Action<BinaryWriter> export, int suggestCapacity = 100)
	{
		if (export == null)
		{
			return null;
		}
		MemoryStream memoryStream = null;
		BinaryWriter binaryWriter = null;
		try
		{
			memoryStream = new MemoryStream(suggestCapacity);
			binaryWriter = new BinaryWriter(memoryStream);
			export(binaryWriter);
			return memoryStream.ToArray();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
		finally
		{
			if (binaryWriter != null)
			{
				binaryWriter.Close();
				binaryWriter = null;
			}
			if (memoryStream != null)
			{
				memoryStream.Close();
				memoryStream = null;
			}
		}
	}

	public static void Import(byte[] buff, Action<BinaryReader> import)
	{
		if (buff == null || import == null)
		{
			return;
		}
		MemoryStream memoryStream = null;
		BinaryReader binaryReader = null;
		try
		{
			memoryStream = new MemoryStream(buff, writable: false);
			binaryReader = new BinaryReader(memoryStream);
			import(binaryReader);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		finally
		{
			if (binaryReader != null)
			{
				binaryReader.Close();
				binaryReader = null;
			}
			if (memoryStream != null)
			{
				memoryStream.Close();
				memoryStream = null;
			}
		}
	}
}
