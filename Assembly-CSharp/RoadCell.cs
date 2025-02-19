using System.IO;
using UnityEngine;

public struct RoadCell
{
	public Color32 color_type;

	public int type
	{
		get
		{
			return color_type.a;
		}
		set
		{
			color_type.a = (byte)value;
		}
	}

	public Color32 color
	{
		get
		{
			Color32 result = color_type;
			result.a = byte.MaxValue;
			return result;
		}
		set
		{
			color_type.r = value.r;
			color_type.g = value.g;
			color_type.b = value.b;
			color_type.a = value.a;
		}
	}

	public RoadCell(int _type)
	{
		color_type = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		type = _type;
	}

	public RoadCell(int _type, Color32 _color)
	{
		color_type = _color;
		type = _type;
	}

	public void WriteToStream(BinaryWriter w)
	{
		w.Write(color_type.r);
		w.Write(color_type.g);
		w.Write(color_type.b);
		w.Write(color_type.a);
	}

	public void ReadFromStream(BinaryReader r)
	{
		color_type.r = r.ReadByte();
		color_type.g = r.ReadByte();
		color_type.b = r.ReadByte();
		color_type.a = r.ReadByte();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is RoadCell roadCell)
		{
			return color_type.r == roadCell.color_type.r && color_type.g == roadCell.color_type.g && color_type.b == roadCell.color_type.b && color_type.a == roadCell.color_type.a;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
