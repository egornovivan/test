using UnityEngine;

public struct VCVoxel
{
	public const int c_Size = 2;

	public const float RECIPROCAL_255 = 0.003921569f;

	public byte Volume;

	public byte Type;

	public float VolumeF
	{
		get
		{
			return (float)(int)Volume / 255f;
		}
		set
		{
			Volume = (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
		}
	}

	public VCVoxel(byte v, byte t)
	{
		Volume = v;
		Type = t;
	}

	public VCVoxel(ushort val)
	{
		Volume = (byte)(val & 0xFF);
		Type = (byte)(val >> 8);
	}

	public void PrintValue()
	{
		Debug.Log("Vol = " + Volume + "  Type = " + Type);
	}

	public static implicit operator VCVoxel(ushort val)
	{
		return new VCVoxel(val);
	}

	public static implicit operator ushort(VCVoxel voxel)
	{
		return (ushort)(voxel.Volume | (voxel.Type << 8));
	}

	public static implicit operator VCVoxel(int val)
	{
		return new VCVoxel((ushort)val);
	}

	public static implicit operator int(VCVoxel voxel)
	{
		return voxel.Volume | (voxel.Type << 8);
	}
}
