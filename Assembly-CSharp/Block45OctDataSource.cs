using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Block45OctDataSource
{
	private Block45OctNode _octRoot;

	private Action<Block45OctNode> _onCreateNode;

	public Block45OctNode RootNode => _octRoot;

	public Block45OctDataSource(Action<Block45OctNode> onCreateNode = null)
	{
		_onCreateNode = onCreateNode;
	}

	private static int AppendToWrite(Block45OctNode node, BinaryWriter bw, int lod)
	{
		int num = 0;
		if (node._pos.w == lod)
		{
			List<BlockVec> vecData = node.VecData;
			if (vecData != null)
			{
				IntVector4 intVector = Block45Constants.ToBlockUnitPos(node._pos.x, node._pos.y, node._pos.z, node._pos.w);
				int count = vecData.Count;
				for (int i = 0; i < count; i++)
				{
					BlockVec blockVec = vecData[i];
					int num2 = blockVec.x - 1;
					int num3 = blockVec.y - 1;
					int num4 = blockVec.z - 1;
					if (num2 >= 0 && num2 < 8 && num3 >= 0 && num3 < 8 && num4 >= 0 && num4 < 8)
					{
						bw.Write(intVector.x + num2);
						bw.Write(intVector.y + num3);
						bw.Write(intVector.z + num4);
						bw.Write(blockVec._byte0);
						bw.Write(blockVec._byte1);
						num++;
					}
				}
			}
		}
		else if (node._pos.w > lod && node._children != null)
		{
			for (int j = 0; j < 8; j++)
			{
				num += AppendToWrite(node._children[j], bw, lod);
			}
		}
		return num;
	}

	public void Import(BinaryReader br)
	{
		int num = br.ReadInt32();
		int num2 = num;
		if (num2 == 2)
		{
			int num3 = br.ReadInt32();
			B45Block voxel = default(B45Block);
			for (int i = 0; i < num3; i++)
			{
				int x = br.ReadInt32();
				int y = br.ReadInt32();
				int z = br.ReadInt32();
				voxel.blockType = br.ReadByte();
				voxel.materialType = br.ReadByte();
				Write(voxel, x, y, z);
			}
		}
	}

	public void Export(BinaryWriter bw)
	{
		int num = 2;
		bw.Write(num);
		long position = bw.BaseStream.Position;
		bw.Write(0);
		if (_octRoot != null)
		{
			int num2 = num;
			if (num2 == 2)
			{
				int value = AppendToWrite(_octRoot, bw, 0);
				bw.Seek((int)position, SeekOrigin.Begin);
				bw.Write(value);
			}
		}
	}

	public void ImportData(byte[] b45Data)
	{
		MemoryStream memoryStream = new MemoryStream(b45Data);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		Import(binaryReader);
		binaryReader.Close();
		memoryStream.Close();
	}

	public byte[] ExportData()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		Export(binaryWriter);
		binaryWriter.Close();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	public B45Block Read(int x, int y, int z, int lod = 0)
	{
		IntVector4 poslod = Block45Constants.ToWorldUnitPos(x, y, z, lod);
		Block45OctNode nodeRO = Block45OctNode.GetNodeRO(poslod, _octRoot);
		if (nodeRO == null)
		{
			return new B45Block(0, 0);
		}
		int x2 = (x >> lod) & 7;
		int y2 = (y >> lod) & 7;
		int z2 = (z >> lod) & 7;
		return nodeRO.Read(x2, y2, z2);
	}

	public int Write(B45Block voxel, int x, int y, int z, int lod = 0)
	{
		int num = (x >> lod) & 7;
		int num2 = (y >> lod) & 7;
		int num3 = (z >> lod) & 7;
		IntVector4 intVector;
		Block45OctNode block45OctNode;
		if (_octRoot == null)
		{
			intVector = Block45Constants.ToWorldUnitPos(x & -8, y & -8, z & -8, lod);
			IntVector4 atpos = new IntVector4(intVector);
			block45OctNode = (_octRoot = Block45OctNode.CreateNode(atpos, _onCreateNode));
			_octRoot = _octRoot.RerootToLOD(LODOctreeMan._maxLod);
		}
		else
		{
			intVector = Block45Constants.ToWorldUnitPos(x, y, z, lod);
			block45OctNode = Block45OctNode.GetNodeRW(intVector, ref _octRoot);
		}
		block45OctNode.Write(num, num2, num3, voxel.blockType, voxel.materialType);
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 128;
		if (num < Block45OctNode.S_MinNoDirtyIdx)
		{
			num4 = -1;
			num7 |= 0x11;
		}
		else if (num >= Block45OctNode.S_MaxNoDirtyIdx)
		{
			num4 = 1;
			num7 |= 1;
		}
		if (num2 < Block45OctNode.S_MinNoDirtyIdx)
		{
			num5 = -1;
			num7 |= 0x22;
		}
		else if (num2 >= Block45OctNode.S_MaxNoDirtyIdx)
		{
			num5 = 1;
			num7 |= 2;
		}
		if (num3 < Block45OctNode.S_MinNoDirtyIdx)
		{
			num6 = -1;
			num7 |= 0x44;
		}
		else if (num3 >= Block45OctNode.S_MaxNoDirtyIdx)
		{
			num6 = 1;
			num7 |= 4;
		}
		if (num7 != 128)
		{
			int num8 = 3;
			int num9 = x >> lod + num8;
			int num10 = y >> lod + num8;
			int num11 = z >> lod + num8;
			for (int i = 1; i < 8; i++)
			{
				if ((num7 & i) == i)
				{
					int num12 = num4 * Block45OctNode.S_NearNodeOfs[i, 0];
					int num13 = num5 * Block45OctNode.S_NearNodeOfs[i, 1];
					int num14 = num6 * Block45OctNode.S_NearNodeOfs[i, 2];
					int num15 = num9 + num12;
					int num16 = num10 + num13;
					int num17 = num11 + num14;
					intVector.x = num15 << 2 + lod;
					intVector.y = num16 << 2 + lod;
					intVector.z = num17 << 2 + lod;
					Block45OctNode nodeRW = Block45OctNode.GetNodeRW(intVector, ref _octRoot);
					nodeRW.Write(num - num12 * 8, num2 - num13 * 8, num3 - num14 * 8, voxel.blockType, voxel.materialType);
				}
			}
		}
		return num7;
	}

	public B45Block SafeRead(int x, int y, int z, int lod = 0)
	{
		return Read(x, y, z, lod);
	}

	public bool SafeWrite(B45Block voxel, int x, int y, int z, int lod = 0)
	{
		Write(voxel, x, y, z, lod);
		return true;
	}

	public bool ReadExtendableBlock(IntVector4 pos, out List<IntVector4> posList, out List<B45Block> blocks)
	{
		B45Block b45Block = Read(pos.x, pos.y, pos.z, pos.w);
		if (!b45Block.IsExtendable())
		{
			posList = null;
			blocks = null;
			return false;
		}
		int num = b45Block.materialType & 3;
		int num2 = Block45Kernel._2BitToExDir[num * 3];
		int num3 = Block45Kernel._2BitToExDir[num * 3 + 1];
		int num4 = Block45Kernel._2BitToExDir[num * 3 + 2];
		int num5 = pos.x;
		int num6 = pos.y;
		int num7 = pos.z;
		int w = pos.w;
		if (!b45Block.IsExtendableRoot())
		{
			do
			{
				num5 -= num2;
				num6 -= num3;
				num7 -= num4;
				b45Block = Read(num5, num6, num7, w);
				if (!b45Block.IsExtendable())
				{
					Debug.LogError("[Block]Get root data error in ReadExtendableBlock:" + pos);
					posList = null;
					blocks = null;
					return false;
				}
			}
			while (!b45Block.IsExtendableRoot());
		}
		int num8 = (b45Block.materialType >> 2) + 2;
		posList = new List<IntVector4>(num8);
		blocks = new List<B45Block>(num8);
		for (int i = 0; i < num8; i++)
		{
			IntVector4 intVector = new IntVector4(num5 + i * num2, num6 + i * num3, num7 + i * num4, w);
			posList.Add(intVector);
			blocks.Add(Read(intVector.x, intVector.y, intVector.z, intVector.w));
		}
		return true;
	}

	public void Clear()
	{
		if (_octRoot != null)
		{
			Block45OctNode.Clear(_octRoot);
			Block45OctNode.Merge(_octRoot);
		}
	}
}
