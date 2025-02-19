public struct BlockVector
{
	private int xyz;

	public byte byte0;

	public byte byte1;

	public int x => xyz & 0xFF;

	public int y => (xyz >> 8) & 0xFF;

	public int z => (xyz >> 16) & 0xFF;

	public BlockVector(int x, int y, int z, byte b0, byte b1)
	{
		xyz = (x & 0xFF) + ((y & 0xFF) << 8) + ((z & 0xFF) << 16);
		byte0 = b0;
		byte1 = b1;
	}
}
