public struct BSVoxel
{
	public byte value0;

	public byte value1;

	public byte volmue
	{
		get
		{
			return value0;
		}
		set
		{
			value0 = value;
		}
	}

	public byte type
	{
		get
		{
			return value1;
		}
		set
		{
			value1 = value;
		}
	}

	public byte blockType
	{
		get
		{
			return value0;
		}
		set
		{
			value0 = value;
		}
	}

	public byte materialType
	{
		get
		{
			return value1;
		}
		set
		{
			value1 = value;
		}
	}

	public int RotId => blockType & 3;

	public int PrimId => blockType >> 2;

	public BSVoxel(VFVoxel voxel)
	{
		value0 = voxel.Volume;
		value1 = voxel.Type;
	}

	public BSVoxel(byte v0, byte v1)
	{
		value0 = v0;
		value1 = v1;
	}

	public BSVoxel(B45Block block)
	{
		value0 = block.blockType;
		value1 = block.materialType;
	}

	public VFVoxel ToVoxel()
	{
		return new VFVoxel(value0, value1);
	}

	public B45Block ToBlock()
	{
		return new B45Block(value0, value1);
	}

	public bool IsExtendable()
	{
		return blockType >= 128;
	}

	public bool IsExtendableRoot()
	{
		return blockType >> 2 == 63;
	}

	public static byte MakeBlockType(int primitiveType, int rotation)
	{
		return B45Block.MakeBlockType(primitiveType, rotation);
	}
}
