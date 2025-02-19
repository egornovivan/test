public struct VFVoxel
{
	public enum EType
	{
		HOLLOW = 0,
		WATER = 1,
		WATERDEEP = 2,
		MUD = 3,
		ROCK = 4,
		SANDLAND = 5,
		IRON = 6,
		COPPER = 7,
		SILVER_GOLD = 8,
		VOLCANO = 9,
		SULFER = 10,
		MINE_TYPE_MAX = 49,
		TERRAIN_TYPE_MAX = 49
	}

	public const int c_VTSize = 2;

	public const int c_OtherSize = 1;

	public const float RECIPROCAL_255 = 0.003921569f;

	public byte Volume;

	public byte Type;

	public bool IsBuilding => Type >= 49;

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
