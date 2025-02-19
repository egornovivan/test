using System;
using System.Xml.Serialization;
using UnityEngine;

public class AssetPRS
{
	public static AssetPRS Zero = new AssetPRS(Vector3.zero, Quaternion.identity, Vector3.one);

	[XmlAttribute("X")]
	public float x { get; set; }

	[XmlAttribute("Y")]
	public float y { get; set; }

	[XmlAttribute("Z")]
	public float z { get; set; }

	[XmlAttribute("RotX")]
	public float rx { get; set; }

	[XmlAttribute("RotY")]
	public float ry { get; set; }

	[XmlAttribute("RotZ")]
	public float rz { get; set; }

	[XmlAttribute("RotW")]
	public float rw { get; set; }

	[XmlAttribute("ScaleX")]
	public float sx { get; set; }

	[XmlAttribute("ScaleY")]
	public float sy { get; set; }

	[XmlAttribute("ScaleZ")]
	public float sz { get; set; }

	public AssetPRS()
	{
	}

	public AssetPRS(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		x = position.x;
		y = position.y;
		z = position.z;
		sx = scale.x;
		sy = scale.y;
		sz = scale.z;
		rx = rotation.x;
		ry = rotation.y;
		rz = rotation.z;
		rw = rotation.w;
	}

	public Vector3 Position()
	{
		return new Vector3(x, y, z);
	}

	public Quaternion Rotation()
	{
		return new Quaternion(rx, ry, rz, rw);
	}

	public Vector3 Scale()
	{
		return new Vector3(sx, sy, sz);
	}

	public bool Equals(AssetPRS obj)
	{
		return Math.Abs(x - obj.x) < float.Epsilon && Math.Abs(y - obj.y) < float.Epsilon && Math.Abs(z - obj.z) < float.Epsilon && Math.Abs(sx - obj.sx) < float.Epsilon && Math.Abs(sy - obj.sy) < float.Epsilon && Math.Abs(sz - obj.sz) < float.Epsilon && Math.Abs(rx - obj.rx) < float.Epsilon && Math.Abs(rz - obj.rz) < float.Epsilon && Math.Abs(ry - obj.ry) < float.Epsilon && Math.Abs(rw - obj.rw) < float.Epsilon;
	}
}
