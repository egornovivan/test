using System.Collections.Generic;
using UnityEngine;

public class B45LODCompute
{
	public class IntPair
	{
		public int key;

		public int val;

		public IntPair(int _k, int _v)
		{
			key = _k;
			val = _v;
		}
	}

	private const int NumBlockTypes = 7;

	private byte[] TypeToOccupancy = new byte[64]
	{
		0, 4, 2, 2, 1, 3, 1, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0
	};

	private int[] proximityIndices;

	private List<IntPair> intPairList;

	private byte[] OccupancyRates = new byte[56]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 2, 2, 4,
		2, 0, 0, 2, 4, 2, 0, 2, 4, 2,
		0, 2, 3, 1, 0, 1, 1, 0, 0, 0,
		4, 4, 3, 4, 4, 3, 1, 3, 1, 1,
		1, 1, 0, 0, 0, 0
	};

	public byte[] Compute(List<byte[]> srcChunkData)
	{
		byte[] array = new byte[2000];
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					int cid = k + j * 2 + i * 4;
					ComputeOne(array, srcChunkData, cid, new IntVector3(k, j, i));
				}
			}
		}
		return array;
	}

	private void ComputeOne(byte[] LODChunkData, List<byte[]> srcChunkData, int cid, IntVector3 destChunkOfs)
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		byte[] array3 = new byte[8];
		B45Block b45Block = default(B45Block);
		for (int i = 0; i < 8; i += 2)
		{
			for (int j = 0; j < 8; j += 2)
			{
				for (int k = 0; k < 8; k += 2)
				{
					array[0] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k, j, i)];
					array[1] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k, j, i + 1)];
					array[2] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k + 1, j, i + 1)];
					array[3] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k + 1, j, i)];
					array[4] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k, j + 1, i)];
					array[5] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k, j + 1, i + 1)];
					array[6] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k + 1, j + 1, i + 1)];
					array[7] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k + 1, j + 1, i)];
					array2[0] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k, j, i) + 1];
					array2[1] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k, j, i + 1) + 1];
					array2[2] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k + 1, j, i + 1) + 1];
					array2[3] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k + 1, j, i) + 1];
					array2[4] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k, j + 1, i) + 1];
					array2[5] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k, j + 1, i + 1) + 1];
					array2[6] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k + 1, j + 1, i + 1) + 1];
					array2[7] = srcChunkData[cid][2 * B45ChunkData.OneIndex(k + 1, j + 1, i) + 1];
					array3[0] = TypeToOccupancy[array[0] >> 2];
					array3[1] = TypeToOccupancy[array[1] >> 2];
					array3[2] = TypeToOccupancy[array[2] >> 2];
					array3[3] = TypeToOccupancy[array[3] >> 2];
					array3[4] = TypeToOccupancy[array[4] >> 2];
					array3[5] = TypeToOccupancy[array[5] >> 2];
					array3[6] = TypeToOccupancy[array[6] >> 2];
					array3[7] = TypeToOccupancy[array[7] >> 2];
					int key = array3[0] + array3[1] + array3[2] + array3[3] + array3[4] + array3[5] + array3[6] + array3[7];
					List<int> similarIndices = GetSimilarIndices(key, 3);
					int num = 255;
					int primitiveType = -1;
					int rotation = -1;
					for (int l = 0; l < similarIndices.Count; l++)
					{
						int num2 = CalculateError(array3, similarIndices[l], 0);
						if (num > num2)
						{
							num = num2;
							primitiveType = similarIndices[l];
							rotation = 3;
						}
						num2 = CalculateError(array3, similarIndices[l], 1);
						if (num > num2)
						{
							num = num2;
							primitiveType = similarIndices[l];
							rotation = 2;
						}
						num2 = CalculateError(array3, similarIndices[l], 2);
						if (num > num2)
						{
							num = num2;
							primitiveType = similarIndices[l];
							rotation = 1;
						}
						num2 = CalculateError(array3, similarIndices[l], 3);
						if (num > num2)
						{
							num = num2;
							primitiveType = similarIndices[l];
							rotation = 0;
						}
					}
					b45Block.blockType = B45Block.MakeBlockType(primitiveType, rotation);
					b45Block.materialType = 0;
					byte b = 0;
					for (int m = 0; m < 8; m++)
					{
						if (array3[m] > b)
						{
							b = array3[m];
							b45Block.materialType = array2[m];
						}
					}
					int num3 = 2 * B45ChunkData.OneIndex(destChunkOfs.x * 8 / 2 + k / 2, destChunkOfs.y * 8 / 2 + j / 2, destChunkOfs.z * 8 / 2 + i / 2);
					LODChunkData[num3] = b45Block.blockType;
					LODChunkData[num3 + 1] = b45Block.materialType;
				}
			}
		}
	}

	public void Init()
	{
		intPairList = new List<IntPair>();
		for (int i = 0; i < 7; i++)
		{
			int num = 0;
			int num2 = i * 8;
			for (int j = 0; j < 8; j++)
			{
				num += OccupancyRates[num2 + j];
			}
			intPairList.Add(new IntPair(num, i));
		}
		intPairList.Sort((IntPair p1, IntPair p2) => (p1.key < p2.key) ? (-1) : ((p1.key > p2.key) ? 1 : 0));
		initProximityIndices();
	}

	private int CalculateError(byte[] eightOccu, int type, int rot)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			int num2 = (i + rot) % 4;
			num += Mathf.Abs(eightOccu[i] - OccupancyRates[type * 8 + num2]);
			num += Mathf.Abs(eightOccu[i + 4] - OccupancyRates[type * 8 + 4 + num2]);
		}
		return num;
	}

	private List<int> GetSimilarIndices(int key, int count)
	{
		int num = proximityIndices[key];
		List<int> list = new List<int>();
		list.Add(intPairList[num].val);
		int num2 = num - 1;
		int num3 = num + 1;
		for (int i = 0; i < count - 1; i++)
		{
			int num4 = ((num2 >= 0) ? Mathf.Abs(intPairList[num2].key - key) : 255);
			int num5 = ((num3 < 7) ? Mathf.Abs(intPairList[num3].key - key) : 255);
			if (num4 < num5)
			{
				list.Add(intPairList[num2].val);
				num2--;
			}
			else
			{
				list.Add(intPairList[num3].val);
				num3++;
			}
		}
		return list;
	}

	private void initProximityIndices()
	{
		proximityIndices = new int[41];
		for (int i = 0; i <= 40; i++)
		{
			int num = 256;
			int num2 = -1;
			for (int j = 0; j < intPairList.Count; j++)
			{
				if (Mathf.Abs(i - intPairList[j].key) < num)
				{
					num = Mathf.Abs(i - intPairList[j].key);
					num2 = j;
				}
			}
			proximityIndices[i] = num2;
		}
	}
}
