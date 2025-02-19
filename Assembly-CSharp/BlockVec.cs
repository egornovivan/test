public struct BlockVec
{
	public int _xyz;

	public byte _byte0;

	public byte _byte1;

	public int x => _xyz & 0xFF;

	public int y => (_xyz >> 8) & 0xFF;

	public int z => (_xyz >> 16) & 0xFF;

	public BlockVec(int x, int y, int z, byte b0, byte b1)
	{
		_xyz = ToXYZ(x, y, z);
		_byte0 = b0;
		_byte1 = b1;
	}

	public BlockVec(int xyz, byte b0, byte b1)
	{
		_xyz = xyz;
		_byte0 = b0;
		_byte1 = b1;
	}

	public static int ToXYZ(int x, int y, int z)
	{
		return (x & 0xFF) + ((y & 0xFF) << 8) + ((z & 0xFF) << 16);
	}
}
