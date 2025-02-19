using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LODDataUpdate
{
	public class LODUpdateCmd
	{
		public VFVoxelChunkData chunk;

		public IntVector3 posToRead;

		public IntVector3 posToWrite;

		public LODUpdateCmd(VFVoxelChunkData _chunk)
		{
			chunk = _chunk;
			posToRead = null;
			posToWrite = null;
		}

		public static void PreReadOps(IntVector3 pos1)
		{
			if (pos1.x >= 0)
			{
				pos1.x = pos1.x >> 1 << 1;
			}
			if (pos1.y >= 0)
			{
				pos1.y = pos1.y >> 1 << 1;
			}
			if (pos1.z >= 0)
			{
				pos1.z = pos1.z >> 1 << 1;
			}
		}

		public static void PostReadOps(IntVector3 pos1, IntVector3 pos2)
		{
		}

		public override int GetHashCode()
		{
			return posToRead.x + (posToRead.z << 10) + (posToRead.y << 20) + (chunk.LOD << 28);
		}

		public override bool Equals(object obj)
		{
			LODUpdateCmd lODUpdateCmd = (LODUpdateCmd)obj;
			return posToRead == lODUpdateCmd.posToRead && posToWrite == lODUpdateCmd.posToWrite && chunk.ChunkPosLod.Equals(lODUpdateCmd.chunk.ChunkPosLod);
		}
	}

	public static LODDataUpdate self;

	private Thread workerThread;

	private bool _bThreadOn = true;

	private Mutex chunkDataMutex;

	private List<IntVector3> inpFlaggedPosList;

	private List<List<VFVoxelChunkData>> inpModifiedChunkMulList;

	private int[] written = new int[35];

	private Dictionary<IntVector4, LODUpdateCmd> cmdCollision = new Dictionary<IntVector4, LODUpdateCmd>();

	private List<IntVector3> UpdateChunkCoordList;

	private List<IntVector3> UpdateLocalCoordList;

	private static int[] voxelIndexInverted = new int[70]
	{
		0, 1, -1, 2, -1, 3, -1, 4, -1, 5,
		-1, 6, -1, 7, -1, 8, -1, 9, -1, 10,
		-1, 11, -1, 12, -1, 13, -1, 14, -1, 15,
		-1, 16, -1, -1, -1, -1, 17, -1, 18, -1,
		19, -1, 20, -1, 21, -1, 22, -1, 23, -1,
		24, -1, 25, -1, 26, -1, 27, -1, 28, -1,
		29, -1, 30, -1, 31, -1, -1, 32, 33, 34
	};

	public void init()
	{
		for (int i = 0; i < 35; i++)
		{
			written[i] = 0;
		}
		workerThread = new Thread(threadProc);
		workerThread.Start();
		self = this;
	}

	public void Stop()
	{
		_bThreadOn = false;
		try
		{
			workerThread.Join();
			MonoBehaviour.print("[LODDataUpdate]Thread stopped");
		}
		catch
		{
			MonoBehaviour.print("[LODDataUpdate]Thread stopped with exception");
		}
	}

	public void threadProc()
	{
		_bThreadOn = true;
		while (_bThreadOn)
		{
			if (UpdateChunkCoordList == null || UpdateChunkCoordList.Count == 0)
			{
				Thread.Sleep(100);
				continue;
			}
			List<IntVector3> list = null;
			List<IntVector3> list2 = null;
			lock (this)
			{
				list = UpdateChunkCoordList;
				UpdateChunkCoordList = null;
				list2 = UpdateLocalCoordList;
				UpdateLocalCoordList = null;
			}
			cmdCollision.Clear();
			List<LODUpdateCmd> list3 = new List<LODUpdateCmd>();
			for (int i = 0; i < list.Count; i++)
			{
				GenUpdateCmd(list[i], list2[i], list3);
			}
			VFVoxel voxel = new VFVoxel(0, 0);
			for (int j = 0; j < list3.Count; j++)
			{
				LODUpdateCmd lODUpdateCmd = list3[j];
				lODUpdateCmd.chunk = PrepareChunkData(lODUpdateCmd.chunk);
				if (lODUpdateCmd.chunk.LOD > 0)
				{
					lODUpdateCmd.chunk.WriteVoxelAtIdx4LodUpdate(lODUpdateCmd.posToWrite.x, lODUpdateCmd.posToWrite.y, lODUpdateCmd.posToWrite.z, voxel);
					VFVoxelTerrain.self.SaveLoad.SaveChunkToTmpFile(lODUpdateCmd.chunk);
				}
				voxel = ReadVoxel(lODUpdateCmd.chunk, lODUpdateCmd.posToRead);
			}
		}
	}

	private VFVoxelChunkData PrepareChunkData(VFVoxelChunkData chunk)
	{
		VFVoxelChunkData vFVoxelChunkData = null;
		if (chunk.LOD <= LODOctreeMan._maxLod)
		{
			vFVoxelChunkData = VFVoxelTerrain.self.Voxels.readChunk(chunk.ChunkPosLod.x, chunk.ChunkPosLod.y, chunk.ChunkPosLod.z, chunk.LOD);
		}
		if (vFVoxelChunkData == null || vFVoxelChunkData.DataVT == VFVoxelChunkData.S_ChunkDataNull)
		{
			byte[] array = VFVoxelTerrain.self.SaveLoad.TryGetChunkData(chunk.ChunkPosLod, useChunkDataPool: false);
			if (array != null)
			{
				chunk.SetDataVT(array);
			}
			else
			{
				decompressData(chunk);
			}
		}
		else
		{
			chunk = null;
			chunk = vFVoxelChunkData;
		}
		return chunk;
	}

	private VFVoxel ReadVoxel(VFVoxelChunkData inputChunk, IntVector3 InChunkPos)
	{
		int num = ((!inputChunk.IsHollow) ? VFVoxelChunkData.OneIndexNoPrefix(InChunkPos.x, InChunkPos.y, InChunkPos.z) : 0);
		int num2 = num * 2;
		return new VFVoxel(inputChunk.DataVT[num2], inputChunk.DataVT[num2 + 1]);
	}

	private VFVoxel GetAverageVoxel2(VFVoxelChunkData inputChunk, IntVector3 InChunkPos)
	{
		int num = 0;
		byte type = 0;
		VFVoxel[] array = new VFVoxel[8];
		for (int i = 0; i < 8; i++)
		{
			int num2 = (((i & 1) > 0) ? 1 : 0);
			int num3 = (((i & 2) > 0) ? 1 : 0);
			int num4 = (((i & 4) > 0) ? 1 : 0);
			try
			{
				ref VFVoxel reference = ref array[i];
				reference = inputChunk.ReadVoxelAtIdx(InChunkPos.x + num2, InChunkPos.y + num3, InChunkPos.z + num4);
			}
			catch
			{
			}
			num += array[i].Volume;
			if (array[i].Type > 0)
			{
				type = array[i].Type;
			}
		}
		return new VFVoxel((byte)(num >> 3), type);
	}

	private void decompressData(VFVoxelChunkData chunkData)
	{
		int lOD = chunkData.LOD;
		VFFileUtil.WorldChunkPosToPiecePos(chunkData.ChunkPosLod, out var px, out var py, out var pz);
		VFFileUtil.PiecePos2FilePos(px, py, pz, lOD, out var fx, out var fz);
		VFPieceDataClone pieceDataSub = VFDataReaderClone.GetPieceDataSub(new IntVector4(px, py, pz, lOD), VFDataReaderClone.GetFileSetSub(new IntVector4(fx, 0, fz, lOD)));
		pieceDataSub.Decompress();
		pieceDataSub.SetChunkData(chunkData);
		pieceDataSub._data = null;
		pieceDataSub = null;
	}

	public void InsertUpdateCoord(int x, int y, int z, IntVector4 chunkPos)
	{
		lock (this)
		{
			if (UpdateChunkCoordList == null)
			{
				UpdateChunkCoordList = new List<IntVector3>();
			}
			if (UpdateLocalCoordList == null)
			{
				UpdateLocalCoordList = new List<IntVector3>();
			}
			UpdateChunkCoordList.Add(chunkPos.XYZ);
			UpdateLocalCoordList.Add(new IntVector3(x, y, z));
		}
	}

	public void Stats(int x, int y, int z, IntVector3 chunkPos)
	{
		try
		{
			written[y]++;
		}
		catch
		{
		}
	}

	public void GenUpdateCmd(IntVector3 chunkPos, IntVector3 localPos, List<LODUpdateCmd> updateCmdList)
	{
		IntVector3 intVector = null;
		localPos.x++;
		localPos.y++;
		localPos.z++;
		IntVector3 intVector2 = new IntVector3(localPos);
		for (int i = 0; i < 4; i++)
		{
			IntVector3 intVector3 = new IntVector3(chunkPos);
			intVector3.x = intVector3.x >> i << i;
			intVector3.y = intVector3.y >> i << i;
			intVector3.z = intVector3.z >> i << i;
			VFVoxelChunkData vFVoxelChunkData = ForgeChunk(intVector3, i);
			if (intVector == null)
			{
				intVector = vFVoxelChunkData.ChunkPosLod.XYZ;
			}
			LODUpdateCmd lODUpdateCmd = new LODUpdateCmd(vFVoxelChunkData);
			if (i > 0)
			{
				if (intVector2.x < 0 || intVector2.y < 0 || intVector2.z < 0)
				{
					break;
				}
				lODUpdateCmd.posToWrite = new IntVector3(intVector2);
			}
			lODUpdateCmd.posToRead = new IntVector3(intVector2);
			intVector2.x += intVector.x % 2 * 35;
			intVector2.y += intVector.y % 2 * 35;
			intVector2.z += intVector.z % 2 * 35;
			int x = voxelIndexInverted[intVector2.x];
			int y = voxelIndexInverted[intVector2.y];
			int z = voxelIndexInverted[intVector2.z];
			intVector2.x = x;
			intVector2.y = y;
			intVector2.z = z;
			intVector.x >>= 1;
			intVector.y >>= 1;
			intVector.z >>= 1;
			IntVector4 key = new IntVector4(lODUpdateCmd.posToRead.x, lODUpdateCmd.posToRead.y, lODUpdateCmd.posToRead.z, i);
			if (cmdCollision.TryGetValue(key, out var value))
			{
				if (!value.Equals(lODUpdateCmd))
				{
					updateCmdList.Add(lODUpdateCmd);
				}
			}
			else
			{
				cmdCollision.Add(key, lODUpdateCmd);
				updateCmdList.Add(lODUpdateCmd);
			}
		}
	}

	private static List<IntVector3> NeighbourCoords(IntVector3 pos, int lod)
	{
		int num = LODOctreeMan._xChunkCount >> lod;
		int num2 = LODOctreeMan._yChunkCount >> lod;
		int num3 = LODOctreeMan._zChunkCount >> lod;
		int num4 = 5 + lod;
		int num5 = pos.x >> num4;
		int num6 = pos.y >> num4;
		int num7 = pos.z >> num4;
		int num8 = (pos.x >> lod) & 0x1F;
		int num9 = (pos.y >> lod) & 0x1F;
		int num10 = (pos.z >> lod) & 0x1F;
		int num11 = 2;
		int num12 = 31;
		int num13 = 0;
		int num14 = 0;
		int num15 = 0;
		int num16 = 128;
		if (num8 < num11 && num5 > 0)
		{
			num13 = -1;
			num16 |= 0x11;
		}
		else if (num8 >= num12 && num5 < 575)
		{
			num13 = 1;
			num16 |= 1;
		}
		if (num9 < num11 && num6 > 0)
		{
			num14 = -1;
			num16 |= 0x22;
		}
		else if (num9 >= num12 && num6 < 91)
		{
			num14 = 1;
			num16 |= 2;
		}
		if (num10 < num11 && num7 > 0)
		{
			num15 = -1;
			num16 |= 0x44;
		}
		else if (num10 >= num12 && num7 < 575)
		{
			num15 = 1;
			num16 |= 4;
		}
		List<IntVector3> list = new List<IntVector3>();
		if (num16 != 128)
		{
			for (int i = 1; i < 8; i++)
			{
				if ((num16 & i) == i)
				{
					int num17 = num13 * VFVoxelChunkData.S_NearChunkOfs[i, 0];
					int num18 = num14 * VFVoxelChunkData.S_NearChunkOfs[i, 1];
					int num19 = num15 * VFVoxelChunkData.S_NearChunkOfs[i, 2];
					list.Add(new IntVector3((num5 + num17) % num, (num6 + num18) % num2, (num7 + num19) % num3));
					list.Add(new IntVector3(num8 - num17 * 32, num9 - num18 * 32, num10 - num19 * 32));
				}
			}
		}
		return list;
	}

	private VFVoxelChunkData ForgeChunk(IntVector3 pos, int lod)
	{
		LODOctreeNode node = new LODOctreeNode(null, lod, pos.x << 5, pos.y << 5, pos.z << 5);
		VFVoxelChunkData vFVoxelChunkData = new VFVoxelChunkData(node);
		vFVoxelChunkData.ChunkPosLod_w = new IntVector4(pos, lod);
		return vFVoxelChunkData;
	}

	private IntVector3 genNeighbourChunkPos(IntVector3 pos, int dim)
	{
		int num = Mathf.FloorToInt(pos.x / dim);
		int num2 = Mathf.FloorToInt(pos.y / dim);
		int num3 = Mathf.FloorToInt(pos.z / dim);
		return new IntVector3(num * dim, num2 * dim, num3 * dim);
	}
}
