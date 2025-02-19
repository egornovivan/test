public struct VFVoxel
{
	public enum EType : byte
	{
		MUD = 3,
		TERRAIN_TYPE_MAX = 64,
		WaterSourceBeg = 128,
		WaterSourceEnd = 249,
		Reserved0 = 250,
		Reserved1 = 251,
		Reserved2 = 252,
		Reserved3 = 253,
		Reserved4 = 254,
		Reserved5 = byte.MaxValue
	}

	public const int c_shift = 1;

	public const int c_VTSize = 2;

	public const float RECIPROCAL_255 = 0.003921569f;

	public byte Volume;

	public byte Type;

	public bool IsBuilding => Type >= 64;

	public VFVoxel(byte volume)
	{
		Volume = volume;
		Type = 0;
	}

	public VFVoxel(byte volume, byte type)
	{
		Volume = volume;
		Type = type;
	}

	public VFVoxel(byte volume, byte type, byte owner)
	{
		Volume = volume;
		Type = type;
	}

	public static byte ToNormByte(float vol)
	{
		return (byte)(vol * 255f);
	}
}
