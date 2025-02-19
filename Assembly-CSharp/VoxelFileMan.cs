using System.Collections.Generic;

public class VoxelFileMan
{
	private VoxelFileBA fileBA;

	private VoxelFileUV fileUV;

	public VoxelFileMan(string _fileName)
	{
		fileBA = new VoxelFileBA(_fileName);
		fileUV = new VoxelFileUV(_fileName);
		fileBA.InitHeader();
		fileUV.InitHeader();
	}

	public List<B45ChunkDataBase> ReadAllChunkData(out int out_svn_key, int read_ref_svnkey)
	{
		int num = -1;
		List<B45ChunkDataBase> list = new List<B45ChunkDataBase>();
		fileBA.ReadHeader();
		fileUV.ReadHeader();
		int chunkCount = fileBA.GetChunkCount();
		for (int i = 0; i < chunkCount; i++)
		{
			B45ChunkDataBase b45ChunkDataBase = fileBA.ReadChunkData(i);
			if (b45ChunkDataBase.svn_key_ba > read_ref_svnkey)
			{
				fileUV.AttachUVData(i, b45ChunkDataBase);
				list.Add(b45ChunkDataBase);
				if (num < b45ChunkDataBase.svn_key_ba)
				{
					num = b45ChunkDataBase.svn_key_ba;
				}
			}
		}
		out_svn_key = num;
		return list;
	}

	public List<B45ChunkDataHeader> ReadAllChunkHeader(out int out_svn_key, int read_ref_svnkey)
	{
		int num = -1;
		List<B45ChunkDataHeader> list = new List<B45ChunkDataHeader>();
		fileBA.ReadHeader();
		fileUV.ReadHeader();
		int chunkCount = fileBA.GetChunkCount();
		for (int i = 0; i < chunkCount; i++)
		{
			B45ChunkDataHeader b45ChunkDataHeader = new B45ChunkDataHeader();
			b45ChunkDataHeader._chunkPos = fileBA.chunkCoords[i];
			b45ChunkDataHeader.svn_key_ba = fileBA.svn_keys_ba[i];
			b45ChunkDataHeader.svn_key = fileUV.svn_keys_uv[i];
			if (b45ChunkDataHeader.svn_key_ba > read_ref_svnkey)
			{
				list.Add(b45ChunkDataHeader);
				if (num < b45ChunkDataHeader.svn_key_ba)
				{
					num = b45ChunkDataHeader.svn_key_ba;
				}
			}
		}
		out_svn_key = num;
		return list;
	}

	public bool MergeChunkData(List<B45ChunkDataBase> cdl)
	{
		if (!fileBA.ReadHeader())
		{
			for (int i = 0; i < cdl.Count; i++)
			{
				cdl[i].InitUpdateVectors();
				cdl[i].svn_key_ba = 1;
			}
			fileBA.WriteBAHeader(compressed: false, cdl);
			fileUV.WriteUVHeader(compressed: false, cdl);
			return true;
		}
		int out_svn_key = 0;
		List<B45ChunkDataBase> list = ReadAllChunkData(out out_svn_key, 0);
		out_svn_key++;
		bool flag = false;
		for (int j = 0; j < cdl.Count; j++)
		{
			B45ChunkDataBase b45ChunkDataBase = cdl[j];
			bool flag2 = false;
			for (int k = 0; k < list.Count; k++)
			{
				B45ChunkDataBase b45ChunkDataBase2 = list[k];
				if (b45ChunkDataBase._chunkPos.Equals(b45ChunkDataBase2._chunkPos))
				{
					List<UpdateVector> uvs = computeDifferences(b45ChunkDataBase2, b45ChunkDataBase);
					if (b45ChunkDataBase2.updateVectors == null)
					{
						b45ChunkDataBase2.updateVectors = new List<UpdateVector>();
					}
					if (b45ChunkDataBase2.uvVersionKeys == null)
					{
						b45ChunkDataBase2.uvVersionKeys = new List<UVKeyCount>();
						UVKeyCount uVKeyCount = new UVKeyCount();
						uVKeyCount.svn_key = 0;
						uVKeyCount.count = 0;
						b45ChunkDataBase2.uvVersionKeys.Add(uVKeyCount);
					}
					int svn_key = MergeUV(uvs, b45ChunkDataBase2.updateVectors, b45ChunkDataBase2.uvVersionKeys);
					b45ChunkDataBase2.svn_key = svn_key;
					flag2 = true;
				}
			}
			if (!flag2)
			{
				flag = true;
				b45ChunkDataBase.InitUpdateVectors();
				b45ChunkDataBase.svn_key = out_svn_key;
				list.Add(b45ChunkDataBase);
			}
		}
		if (flag)
		{
			fileBA.WriteBAHeader(compressed: false, list);
		}
		fileUV.WriteUVHeader(compressed: false, list);
		return true;
	}

	public static int MergeUV(List<UpdateVector> uvs1, List<UpdateVector> uvs2, List<UVKeyCount> kcs2)
	{
		int svn_key = kcs2[kcs2.Count - 1].svn_key;
		UVKeyCount uVKeyCount = new UVKeyCount();
		uVKeyCount.svn_key = svn_key + 1;
		uVKeyCount.count = 0;
		int num = 0;
		int count = kcs2[num].count;
		int num2 = 0;
		byte[] array = new byte[uvs1.Count];
		int count2 = uvs2.Count;
		for (int i = 0; i < count2; i++)
		{
			int num3 = uvs2[i].xyz0 + (uvs2[i].xyz1 << 8);
			for (int j = 0; j < uvs1.Count; j++)
			{
				int num4 = uvs1[j].xyz0 + (uvs1[j].xyz1 << 8);
				if (num4 == num3)
				{
					array[j] = 1;
					if (uvs1[j].voxelData0 != uvs2[i].voxelData0 || uvs1[j].voxelData1 != uvs2[i].voxelData1)
					{
						uvs2[i].voxelData0 = uvs1[j].voxelData0;
						uvs2[i].voxelData1 = uvs1[j].voxelData1;
						uvs2.Add(uvs2[i]);
						uvs2[i] = null;
						kcs2[num].count--;
						uVKeyCount.count++;
					}
				}
			}
			num2++;
			if (num2 >= count)
			{
				num2 = 0;
				num++;
				if (num < kcs2.Count)
				{
					count = kcs2[num].count;
				}
			}
		}
		int num5 = 0;
		for (int k = 0; k < uvs2.Count; k++)
		{
			if (uvs2[k] != null)
			{
				if (num5 != 0)
				{
					uvs2[k - num5] = uvs2[k];
					uvs2[k] = null;
				}
			}
			else
			{
				num5++;
			}
		}
		int num6 = uvs2.Count - 1;
		while (num6 >= 0 && uvs2[num6] == null)
		{
			uvs2.RemoveAt(num6);
			num6--;
		}
		for (int l = 0; l < uvs1.Count; l++)
		{
			if (array[l] == 0)
			{
				UpdateVector updateVector = new UpdateVector();
				updateVector.xyz0 = uvs1[l].xyz0;
				updateVector.xyz1 = uvs1[l].xyz1;
				updateVector.voxelData0 = uvs1[l].voxelData0;
				updateVector.voxelData1 = uvs1[l].voxelData1;
				uvs2.Add(updateVector);
				uVKeyCount.count++;
			}
		}
		int num7 = kcs2.Count - 1;
		while (num7 >= 0 && kcs2[num7].count == 0)
		{
			kcs2.RemoveAt(num7);
			num7--;
		}
		kcs2.Add(uVKeyCount);
		return svn_key + 1;
	}

	private List<UpdateVector> computeDifferences(B45ChunkDataBase cd1, B45ChunkDataBase cd2)
	{
		List<UpdateVector> list = new List<UpdateVector>();
		for (int i = 0; i < 1000; i++)
		{
			if (cd1._chunkData[i * 2] != cd2._chunkData[i * 2] || cd1._chunkData[i * 2 + 1] != cd2._chunkData[i * 2 + 1])
			{
				UpdateVector updateVector = new UpdateVector();
				updateVector.xyz0 = (byte)(i & 0xFF);
				updateVector.xyz1 = (byte)((i >> 8) & 0xFF);
				updateVector.voxelData0 = cd2._chunkData[i * 2];
				updateVector.voxelData1 = cd2._chunkData[i * 2 + 1];
				list.Add(updateVector);
			}
		}
		return list;
	}
}
