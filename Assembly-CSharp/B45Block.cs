using System.Collections.Generic;
using UnityEngine;

public struct B45Block
{
	public const int Block45Size = 2;

	public const int EBX = 0;

	public const int EBY = 2;

	public const int EBZ = 1;

	public byte blockType;

	public byte materialType;

	public int RotId => blockType & 3;

	public int PrimId => blockType >> 2;

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

	public bool IsExtendable()
	{
		return blockType >= 128;
	}

	public bool IsExtendableRoot()
	{
		return blockType >> 2 == 63;
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

	public static void MakeExtendableBlock(int primitiveType, int rotation, int extendDir, int length, int materialType, out B45Block block0, out B45Block block1)
	{
		block0 = new B45Block((byte)(0xFC | rotation), (byte)((length - 2 << 2) | extendDir));
		block1 = new B45Block((byte)(0x80 | (primitiveType << 2) | rotation), (byte)((materialType << 2) | extendDir));
	}

	public static void RepositionBlocks(List<IntVector3> posLst, List<B45Block> blockLst, int rot, Vector3 originalPos)
	{
		int count = posLst.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = posLst[i].ToVector3();
			Vector3 vector2 = Quaternion.Euler(0f, 90 * rot, 0f) * new Vector3(vector.x + 0.5f, vector.y + 0.5f, vector.z + 0.5f);
			posLst[i] = new IntVector3(Mathf.FloorToInt(originalPos.x * 2f) + Mathf.FloorToInt(vector2.x), Mathf.FloorToInt(originalPos.y * 2f) + Mathf.FloorToInt(vector2.y), Mathf.FloorToInt(originalPos.z * 2f) + Mathf.FloorToInt(vector2.z));
		}
		if ((rot & 3) == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int j = 0; j < count; j++)
		{
			B45Block b45Block = blockLst[j];
			int num = ((b45Block.blockType & 3) + rot) & 3;
			if (b45Block.blockType >= 128)
			{
				int num2 = b45Block.blockType >> 2;
				int num3 = b45Block.materialType & 3;
				if (num3 != 2)
				{
					if ((rot & 1) != 0)
					{
						num3 = ((num3 == 0) ? 1 : 0);
					}
					if (num2 == 63 && (rot == 1 || rot == 2))
					{
						list.Add(j);
					}
				}
				blockLst[j] = new B45Block((byte)((num2 << 2) | num), (byte)((b45Block.materialType & 0xFC) | num3));
			}
			else
			{
				blockLst[j] = new B45Block((byte)((b45Block.blockType & 0xFC) | num), b45Block.materialType);
			}
		}
		foreach (int item in list)
		{
			IntVector3 intVector = posLst[item];
			B45Block b45Block2 = blockLst[item];
			int num4 = b45Block2.materialType & 3;
			int num5 = Block45Kernel._2BitToExDir[num4 * 3];
			int num6 = Block45Kernel._2BitToExDir[num4 * 3 + 1];
			int num7 = Block45Kernel._2BitToExDir[num4 * 3 + 2];
			int num8 = (b45Block2.materialType >> 2) + 1;
			int endx = intVector.x - num5 * num8;
			int endy = intVector.y - num6 * num8;
			int endz = intVector.z - num7 * num8;
			int num9 = posLst.FindIndex((IntVector3 it) => it.x == endx && it.y == endy && it.z == endz);
			if (num9 < 0)
			{
				Debug.LogError("[BLOCK]Failed to rotate blocks because len_info not found.");
				continue;
			}
			posLst[item] = posLst[num9];
			posLst[num9] = intVector;
		}
	}
}
