public struct B45Block
{
	public const int Block45Size = 2;

	public const int EBX = 0;

	public const int EBY = 2;

	public const int EBZ = 1;

	public byte blockType;

	public byte materialType;

	public B45Block(byte b, byte m)
	{
		blockType = b;
		materialType = m;
	}

	public B45Block(byte b)
	{
		blockType = b;
		materialType = 0;
	}

	internal void Update(B45Block block)
	{
		blockType = block.blockType;
		materialType = block.materialType;
	}

	public static byte MakeBlockType(int primitiveType, int rotation)
	{
		return (byte)((primitiveType << 2) | rotation);
	}

	public static byte[] MakeExtendableBlockType(int primitiveType, int rotation, int extendDir, int length)
	{
		return new byte[3]
		{
			(byte)(0xFC | rotation),
			(byte)((primitiveType << 2) | extendDir),
			(byte)(length - 2)
		};
	}
}
