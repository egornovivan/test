using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BlockVectorNode : B45LODNode
{
	public List<BlockVector> blockVectors;

	public B45ChunkData chunk;

	public bool isByteArrayMode;

	public BlockVectorNode(IntVector4 _pos, B45LODNode _parent, int _octant)
		: base(_pos, _parent, _octant)
	{
		isByteArrayMode = false;
	}

	public static List<BlockVector> ChunkData2BlockVectors(byte[] chunkData)
	{
		List<BlockVector> list = new List<BlockVector>();
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				for (int k = 0; k < 10; k++)
				{
					int num = Block45Kernel.OneIndexNoPrefix(k, j, i);
					if (chunkData[num] != 0)
					{
						list.Add(new BlockVector(k, j, i, chunkData[num], chunkData[num + 1]));
					}
				}
			}
		}
		return list;
	}

	public static void Clear(BlockVectorNode node)
	{
		if (node.blockVectors != null)
		{
			node.blockVectors.Clear();
			node.blockVectors = null;
		}
		if (node.chunk != null)
		{
			node.chunk.DestroyGameObject();
			node.chunk = null;
		}
		if (!node.isLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				Clear(node.children[i] as BlockVectorNode);
			}
		}
	}

	public static void BlockVectors2ChunkData(List<BlockVector> list, byte[] chunkData)
	{
		Array.Clear(chunkData, 0, chunkData.Length);
		for (int i = 0; i < list.Count; i++)
		{
			BlockVector blockVector = list[i];
			int num = Block45Kernel.OneIndexNoPrefix(blockVector.x, blockVector.y, blockVector.z);
			chunkData[num] = blockVector.byte0;
			chunkData[num + 1] = blockVector.byte1;
		}
	}

	public static void BlockVectors2ChunkData(List<BlockVector> list, B45ChunkData chunkData)
	{
		for (int i = 0; i < list.Count; i++)
		{
			BlockVector blockVector = list[i];
			chunkData.WriteVoxelAtIdx(blockVector.x, blockVector.y, blockVector.z, new B45Block(blockVector.byte0, blockVector.byte1));
		}
	}

	public static int rec_count(BlockVectorNode node)
	{
		int num = 0;
		if (node.blockVectors != null)
		{
			num = node.blockVectors.Count;
		}
		if (!node.isLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				num += rec_count(node.children[i] as BlockVectorNode);
			}
		}
		return num;
	}

	public static int rec_count_dbg(BlockVectorNode node)
	{
		int num = 0;
		if (node.chunk != null)
		{
			num = node.chunk.getFillRate();
		}
		if (!node.isLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				num += rec_count_dbg(node.children[i] as BlockVectorNode);
			}
		}
		return num;
	}

	public static void rec_append(BlockVectorNode node, BinaryWriter bw)
	{
		if (node.isLeaf)
		{
			if (node.blockVectors != null)
			{
				for (int i = 0; i < node.blockVectors.Count; i++)
				{
					IntVector3 intVector = new IntVector3(node.pos.XYZ);
					BlockVector blockVector = node.blockVectors[i];
					intVector.x += blockVector.x - 1;
					intVector.y += blockVector.y - 1;
					intVector.z += blockVector.z - 1;
					bw.Write(intVector.x);
					bw.Write(intVector.y);
					bw.Write(intVector.z);
					bw.Write(blockVector.byte0);
					bw.Write(blockVector.byte1);
				}
			}
		}
		else
		{
			for (int j = 0; j < 8; j++)
			{
				rec_append(node.children[j] as BlockVectorNode, bw);
			}
		}
	}

	public bool isCloseTo(IntVector3 atpos)
	{
		int num = base.logicalSize / 2;
		int num2 = pos.x + num;
		int num3 = pos.y + num;
		int num4 = pos.z + num;
		if (Mathf.Abs(num2 - atpos.x) > 256 - num || Mathf.Abs(num3 - atpos.y) > 256 - num || Mathf.Abs(num4 - atpos.z) > 256 - num)
		{
			return false;
		}
		return true;
	}

	public bool isInLodRange(IntVector3 atpos)
	{
		int num = base.logicalSize / 2;
		int num2 = pos.x + num;
		int num3 = pos.y + num;
		int num4 = pos.z + num;
		if ((Mathf.Abs(num2 - atpos.x) > 256 - num || Mathf.Abs(num3 - atpos.y) > 256 - num || Mathf.Abs(num4 - atpos.z) > 256 - num) && (Mathf.Abs(num2 - atpos.x) < 256 - num || Mathf.Abs(num3 - atpos.y) < 256 - num || Mathf.Abs(num4 - atpos.z) < 256 - num))
		{
			return false;
		}
		return false;
	}

	public static bool isCloseTo_static(IntVector3 nodePos, IntVector3 atpos)
	{
		int num = 4;
		int num2 = nodePos.x + num;
		int num3 = nodePos.y + num;
		int num4 = nodePos.z + num;
		if (Mathf.Abs(num2 - atpos.x) > 256 - num || Mathf.Abs(num3 - atpos.y) > 256 - num || Mathf.Abs(num4 - atpos.z) > 256 - num)
		{
			return false;
		}
		return true;
	}

	public static void rec_find(IntVector3 atpos, BlockVectorNode node, List<BlockVectorNode> ret)
	{
		if (node.isLeaf && node.pos.w == 0 && node.isCloseTo(atpos))
		{
			ret.Add(node);
		}
		if (!node.isLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				rec_find(atpos, node.children[i] as BlockVectorNode, ret);
			}
		}
	}

	public static void rec_findLod(IntVector3 atpos, BlockVectorNode node, List<BlockVectorNode> ret)
	{
		if (!node.isLeaf && node.isInLodRange(atpos))
		{
			ret.Add(node);
		}
		if (!node.isLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				rec_findLod(atpos, node.children[i] as BlockVectorNode, ret);
			}
		}
	}

	public void Split()
	{
		children = new BlockVectorNode[8];
		int w = pos.w;
		int num = base.logicalSize >> 1;
		for (int i = 0; i < 8; i++)
		{
			IntVector4 intVector = new IntVector4(pos);
			intVector.w = w - 1;
			intVector.x += (i & 1) * num;
			intVector.y += ((i >> 1) & 1) * num;
			intVector.z += ((i >> 2) & 1) * num;
			children[i] = new BlockVectorNode(intVector, this, i);
		}
	}

	public bool covers(IntVector3 atpos)
	{
		if (atpos.x >= pos.x && atpos.x < pos.x + base.logicalSize && atpos.y >= pos.y && atpos.y < pos.y + base.logicalSize && atpos.z >= pos.z && atpos.z < pos.z + base.logicalSize)
		{
			return true;
		}
		return false;
	}

	public BlockVectorNode reroot(IntVector3 atpos)
	{
		if (!covers(atpos))
		{
			IntVector4 intVector = new IntVector4(pos.XYZ, pos.w + 1);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (atpos.x < pos.x)
			{
				num = 1;
			}
			if (atpos.y < pos.y)
			{
				num2 = 1;
			}
			if (atpos.z < pos.z)
			{
				num3 = 1;
			}
			intVector.x -= num * base.logicalSize;
			intVector.y -= num2 * base.logicalSize;
			intVector.z -= num3 * base.logicalSize;
			BlockVectorNode blockVectorNode = (BlockVectorNode)(parent = new BlockVectorNode(intVector, null, 0));
			blockVectorNode.Split();
			int num4 = num + (num2 << 1) + (num3 << 2);
			blockVectorNode.children[num4] = null;
			blockVectorNode.children[num4] = this;
			octant = num4;
			return blockVectorNode.reroot(atpos);
		}
		return this;
	}

	public static BlockVectorNode readNode(IntVector3 atpos, BlockVectorNode root)
	{
		int num = 0;
		BlockVectorNode blockVectorNode = root;
		IntVector3 zero = IntVector3.Zero;
		while (blockVectorNode.pos.w != 0)
		{
			zero.x = blockVectorNode.pos.x + blockVectorNode.logicalSize / 2;
			zero.y = blockVectorNode.pos.y + blockVectorNode.logicalSize / 2;
			zero.z = blockVectorNode.pos.z + blockVectorNode.logicalSize / 2;
			num = ((atpos.x >= zero.x) ? 1 : 0) | ((atpos.y >= zero.y) ? 2 : 0) | ((atpos.z >= zero.z) ? 4 : 0);
			if (blockVectorNode.isLeaf)
			{
				blockVectorNode.Split();
			}
			blockVectorNode = blockVectorNode.children[num] as BlockVectorNode;
		}
		return blockVectorNode;
	}

	public void write(int x, int y, int z, byte b0, byte b1)
	{
		if (blockVectors == null)
		{
			blockVectors = new List<BlockVector>();
		}
		bool flag = false;
		for (int i = 0; i < blockVectors.Count; i++)
		{
			BlockVector blockVector = blockVectors[i];
			if (x == blockVector.x && y == blockVector.y && z == blockVector.z)
			{
				blockVector.byte0 = b0;
				blockVector.byte1 = b1;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			blockVectors.Add(new BlockVector(x, y, z, b0, b1));
		}
	}
}
