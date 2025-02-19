using System;
using System.IO;
using UnityEngine;

namespace PETools;

public static class Serialize
{
	public class PeExporter
	{
		public delegate void Export(BinaryWriter w);

		public byte[] Do(Export export, int suggestCapacity = 100)
		{
			if (export == null)
			{
				return null;
			}
			try
			{
				using MemoryStream memoryStream = new MemoryStream(suggestCapacity);
				using (BinaryWriter w = new BinaryWriter(memoryStream))
				{
					export(w);
				}
				return memoryStream.ToArray();
			}
			catch (Exception ex)
			{
				Debug.LogError("catched exception:" + ex);
				return null;
			}
		}
	}

	public class PeImporter
	{
		public delegate void Import(BinaryReader r);

		public void Do(byte[] buff, Import import)
		{
			if (buff == null || import == null)
			{
				return;
			}
			try
			{
				using MemoryStream input = new MemoryStream(buff, writable: false);
				using BinaryReader r = new BinaryReader(input);
				import(r);
			}
			catch (Exception ex)
			{
				Debug.LogError("catched exception:" + ex);
			}
		}
	}

	private static PeExporter exporter;

	private static PeImporter importer;

	private static PeExporter Exporter
	{
		get
		{
			if (exporter == null)
			{
				exporter = new PeExporter();
			}
			return exporter;
		}
	}

	private static PeImporter Importer
	{
		get
		{
			if (importer == null)
			{
				importer = new PeImporter();
			}
			return importer;
		}
	}

	public static void WriteData(Action<BinaryWriter> writer, BinaryWriter bw)
	{
		long position = bw.BaseStream.Position;
		bw.Write(0);
		if (writer != null)
		{
			long position2 = bw.BaseStream.Position;
			writer(bw);
			long position3 = bw.BaseStream.Position;
			if (position3 != position2)
			{
				bw.BaseStream.Position = position;
				bw.Write((int)(position3 - position2));
				bw.BaseStream.Position = position3;
			}
		}
	}

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
		int count;
		if (r.BaseStream.Position > r.BaseStream.Length - 4 || (count = r.ReadInt32()) <= 0)
		{
			return null;
		}
		return r.ReadBytes(count);
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

	public static Vector3 ReadVector3(BinaryReader r)
	{
		float x = r.ReadSingle();
		float y = r.ReadSingle();
		float z = r.ReadSingle();
		return new Vector3(x, y, z);
	}

	public static void WriteVector3(BinaryWriter w, Vector3 v)
	{
		w.Write(v.x);
		w.Write(v.y);
		w.Write(v.z);
	}

	public static void WriteVector4(BinaryWriter w, IntVector4 v)
	{
		w.Write(v.x);
		w.Write(v.y);
		w.Write(v.z);
		w.Write(v.w);
	}

	public static IntVector4 ReadVector4(BinaryReader r)
	{
		int x_ = r.ReadInt32();
		int y_ = r.ReadInt32();
		int z_ = r.ReadInt32();
		int w_ = r.ReadInt32();
		return new IntVector4(x_, y_, z_, w_);
	}

	public static Quaternion ReadQuaternion(BinaryReader r)
	{
		float x = r.ReadSingle();
		float y = r.ReadSingle();
		float z = r.ReadSingle();
		float w = r.ReadSingle();
		return new Quaternion(x, y, z, w);
	}

	public static void WriteQuaternion(BinaryWriter w, Quaternion v)
	{
		w.Write(v.x);
		w.Write(v.y);
		w.Write(v.z);
		w.Write(v.w);
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

	public static byte[] Export(PeExporter.Export export, int suggestCapacity = 100)
	{
		return Exporter.Do(export, suggestCapacity);
	}

	public static void Import(byte[] buff, PeImporter.Import import)
	{
		Importer.Do(buff, import);
	}
}
