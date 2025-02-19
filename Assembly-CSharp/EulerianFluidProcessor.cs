using System;
using System.Collections.Generic;
using UnityEngine;

public class EulerianFluidProcessor
{
	private byte _tmpTerVolume;

	private byte[] _tmpTerDataVT = new byte[85750];

	private VFVoxelChunkData[] _tmpNeibourChunks = new VFVoxelChunkData[27];

	private VFVoxelChunkData[] _tmpDirtyNeibourChunks = new VFVoxelChunkData[27];

	private List<IntVector4> _tmpDirtyChunkPosList = new List<IntVector4>();

	private List<IntVector4> _dirtyChunkPosList = new List<IntVector4>();

	private List<VFVoxelChunkData> _tmpDirtyChunks = new List<VFVoxelChunkData>();

	private HashSet<IntVector4> _allDirtyChunkPosList = new HashSet<IntVector4>();

	public int Viscosity = 4;

	public List<IntVector4> DirtyChunkPosList => _dirtyChunkPosList;

	public List<VFVoxelChunkData> DirtyChunkList => _tmpDirtyChunks;

	public bool UpdateFluid(bool bRebuild)
	{
		int count = _dirtyChunkPosList.Count;
		if (count == 0)
		{
			return false;
		}
		_tmpDirtyChunks.Clear();
		_tmpDirtyChunkPosList.Clear();
		for (int i = 0; i < count; i++)
		{
			IntVector4 intVector = _dirtyChunkPosList[i];
			VFVoxelChunkData vFVoxelChunkData = VFVoxelWater.self.Voxels.readChunk(intVector.x, intVector.y, intVector.z);
			if (vFVoxelChunkData == null || vFVoxelChunkData.DataVT.Length <= 2)
			{
				if (!_tmpDirtyChunkPosList.Contains(intVector))
				{
					_tmpDirtyChunkPosList.Add(intVector);
				}
				continue;
			}
			int num = 0;
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					for (int l = -1; l <= 1; l++)
					{
						VFVoxelChunkData vFVoxelChunkData2 = ((num != 13) ? VFVoxelWater.self.Voxels.readChunk(intVector.x + l, intVector.y + k, intVector.z + j) : vFVoxelChunkData);
						if (vFVoxelChunkData2 == null || vFVoxelChunkData2.DataVT.Length < 2)
						{
							l = (k = (j = 2));
						}
						else
						{
							_tmpNeibourChunks[num++] = vFVoxelChunkData2;
						}
					}
				}
			}
			if (num < 27)
			{
				if (!_tmpDirtyChunkPosList.Contains(intVector))
				{
					_tmpDirtyChunkPosList.Add(intVector);
				}
				continue;
			}
			Array.Clear(_tmpDirtyNeibourChunks, 0, _tmpDirtyNeibourChunks.Length);
			GenerateFluid(vFVoxelChunkData, _tmpNeibourChunks, _tmpDirtyNeibourChunks, bRebuildNow: false);
			for (int m = 0; m < 27; m++)
			{
				if (_tmpDirtyNeibourChunks[m] != null)
				{
					IntVector4 chunkPosLod = _tmpDirtyNeibourChunks[m].ChunkPosLod;
					if (!_tmpDirtyChunkPosList.Contains(chunkPosLod))
					{
						_tmpDirtyChunkPosList.Add(chunkPosLod);
						_tmpDirtyChunks.Add(_tmpDirtyNeibourChunks[m]);
					}
				}
			}
		}
		List<IntVector4> dirtyChunkPosList = _dirtyChunkPosList;
		_dirtyChunkPosList = _tmpDirtyChunkPosList;
		_tmpDirtyChunkPosList = dirtyChunkPosList;
		if (bRebuild)
		{
			int count2 = _tmpDirtyChunks.Count;
			for (int n = 0; n < count2; n++)
			{
				_tmpDirtyChunks[n].EndBatchWriteVoxels();
			}
		}
		return true;
	}

	private byte[] GetTerraDataForWater(VFVoxelChunkData curWaterChunk)
	{
		VFVoxelChunkData nodeData = curWaterChunk.GetNodeData(VFVoxelTerrain.self.IdxInLODNodeData);
		byte b = 0;
		if (nodeData != null)
		{
			switch (nodeData.DataVT.Length)
			{
			case 85750:
				return nodeData.DataVT;
			case 2:
				b = nodeData.DataVT[0];
				break;
			}
		}
		if (b != _tmpTerVolume)
		{
			if (b == 0)
			{
				Array.Clear(_tmpTerDataVT, 0, 85750);
			}
			else
			{
				int num;
				for (num = 0; num < 85750; num++)
				{
					_tmpTerDataVT[num++] = b;
					num++;
					_tmpTerDataVT[num++] = b;
					num++;
					_tmpTerDataVT[num++] = b;
					num++;
					_tmpTerDataVT[num++] = b;
					num++;
					_tmpTerDataVT[num++] = b;
				}
			}
			_tmpTerVolume = b;
		}
		return _tmpTerDataVT;
	}

	private VFVoxelChunkData[] GenerateFluid(VFVoxelChunkData curChunk, VFVoxelChunkData[] neibourChunks, VFVoxelChunkData[] dirtyNeibourChunks, bool bRebuildNow)
	{
		byte[] dataVT = curChunk.DataVT;
		if (dataVT.Length < 85750)
		{
			return dirtyNeibourChunks;
		}
		byte[] terraDataForWater = GetTerraDataForWater(curChunk);
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < 32; j++)
			{
				for (int k = 0; k < 32; k++)
				{
					int num = VFVoxelChunkData.OneIndex(j, i, k);
					int num2 = num * 2;
					int cell = dataVT[num2];
					int num3 = terraDataForWater[num2];
					if (cell <= num3)
					{
						continue;
					}
					byte b = dataVT[num2 + 1];
					bool flag = b >= 128;
					int num4 = num2 - 70;
					int bottom = dataVT[num4];
					if (bottom < 255 && (terraDataForWater[num4] < byte.MaxValue || terraDataForWater[num2] == 0))
					{
						if (flag)
						{
							bottom = 255;
							VFVoxelChunkData.ModVolumeType(curChunk, num4, (byte)bottom, b, neibourChunks, dirtyNeibourChunks);
							if (dataVT[num2 + 70] != 0)
							{
								cell = 255;
							}
							if (dataVT[num2] != cell)
							{
								VFVoxelChunkData.ModVolumeType(curChunk, num2, (byte)cell, b, neibourChunks, dirtyNeibourChunks);
								continue;
							}
						}
						else
						{
							FlowBottom(ref cell, ref bottom);
							VFVoxelChunkData.ModVolume(curChunk, num4, (byte)bottom, neibourChunks, dirtyNeibourChunks);
						}
					}
					if (cell > num3)
					{
						EulerianMatrix eulerianMatrix = EulerianMatrix.None;
						int num5 = num2 - 2;
						int num6 = num2 + 2;
						int num7 = num2 + 2450;
						int num8 = num2 - 2450;
						int other = dataVT[num5];
						int other2 = dataVT[num6];
						int other3 = dataVT[num7];
						int other4 = dataVT[num8];
						int num9 = cell - Viscosity;
						if (other < num9 && terraDataForWater[num5] < cell)
						{
							eulerianMatrix |= EulerianMatrix.Left;
						}
						if (other2 < num9 && terraDataForWater[num6] < cell)
						{
							eulerianMatrix |= EulerianMatrix.Right;
						}
						if (other3 < num9 && terraDataForWater[num7] < cell)
						{
							eulerianMatrix |= EulerianMatrix.Forward;
						}
						if (other4 < num9 && terraDataForWater[num8] < cell)
						{
							eulerianMatrix |= EulerianMatrix.Backward;
						}
						switch (eulerianMatrix)
						{
						case EulerianMatrix.Left:
							Flow1(ref cell, ref other);
							goto default;
						case EulerianMatrix.Right:
							Flow1(ref cell, ref other2);
							goto default;
						case (EulerianMatrix)6:
							Flow2(ref cell, ref other, ref other2);
							goto default;
						case EulerianMatrix.Forward:
							Flow1(ref cell, ref other3);
							goto default;
						case (EulerianMatrix)10:
							Flow2(ref cell, ref other3, ref other);
							goto default;
						case (EulerianMatrix)12:
							Flow2(ref cell, ref other3, ref other2);
							goto default;
						case (EulerianMatrix)14:
							Flow3(ref cell, ref other, ref other2, ref other3);
							goto default;
						case EulerianMatrix.Backward:
							Flow1(ref cell, ref other4);
							goto default;
						case (EulerianMatrix)18:
							Flow2(ref cell, ref other4, ref other);
							goto default;
						case (EulerianMatrix)20:
							Flow2(ref cell, ref other4, ref other2);
							goto default;
						case (EulerianMatrix)22:
							Flow3(ref cell, ref other, ref other2, ref other4);
							goto default;
						case (EulerianMatrix)24:
							Flow2(ref cell, ref other3, ref other4);
							goto default;
						case (EulerianMatrix)26:
							Flow3(ref cell, ref other3, ref other4, ref other);
							goto default;
						case (EulerianMatrix)28:
							Flow3(ref cell, ref other3, ref other4, ref other2);
							goto default;
						case (EulerianMatrix)30:
							Flow4(ref cell, ref other3, ref other4, ref other, ref other2);
							goto default;
						default:
							if ((eulerianMatrix & EulerianMatrix.Left) != 0 && dataVT[num5 + 1] < 128 && dataVT[num5] != other && dataVT[num5 + 1 - 70] < 128)
							{
								VFVoxelChunkData.ModVolume(curChunk, num5, (byte)other, neibourChunks, dirtyNeibourChunks);
							}
							if ((eulerianMatrix & EulerianMatrix.Right) != 0 && dataVT[num6 + 1] < 128 && dataVT[num6] != other2 && dataVT[num6 + 1 - 70] < 128)
							{
								VFVoxelChunkData.ModVolume(curChunk, num6, (byte)other2, neibourChunks, dirtyNeibourChunks);
							}
							if ((eulerianMatrix & EulerianMatrix.Forward) != 0 && dataVT[num7 + 1] < 128 && dataVT[num7] != other3 && dataVT[num7 + 1 - 70] < 128)
							{
								VFVoxelChunkData.ModVolume(curChunk, num7, (byte)other3, neibourChunks, dirtyNeibourChunks);
							}
							if ((eulerianMatrix & EulerianMatrix.Backward) != 0 && dataVT[num8 + 1] < 128 && dataVT[num8] != other4 && dataVT[num8 + 1 - 70] < 128)
							{
								VFVoxelChunkData.ModVolume(curChunk, num8, (byte)other4, neibourChunks, dirtyNeibourChunks);
							}
							break;
						case EulerianMatrix.None:
							break;
						}
					}
					if (dataVT[num2] != cell && !flag)
					{
						VFVoxelChunkData.ModVolume(curChunk, num2, (byte)cell, neibourChunks, dirtyNeibourChunks);
					}
				}
			}
		}
		if (bRebuildNow)
		{
			for (int l = 0; l < 27; l++)
			{
				if (dirtyNeibourChunks[l] != null)
				{
					dirtyNeibourChunks[l].EndBatchWriteVoxels();
				}
			}
		}
		return dirtyNeibourChunks;
	}

	private void FlowBottom(ref int cell, ref int bottom)
	{
		int a = 255 - bottom;
		int num = Mathf.Min(a, cell);
		bottom += num;
		cell -= num;
	}

	private void Flow1(ref int cell, ref int other)
	{
		int num = other + cell >> 1;
		int num2 = (other + cell) & 1;
		other = num + num2;
		cell = num;
	}

	private void Flow2(ref int cell, ref int other1, ref int other2)
	{
		int num = (other1 + other2 + cell) / 3;
		int num2 = (other1 + other2 + cell) % 3;
		cell = num + num2;
		other1 = num;
		other2 = num;
	}

	private void Flow3(ref int cell, ref int other1, ref int other2, ref int other3)
	{
		int num = other1 + other2 + other3 + cell >> 2;
		int num2 = (other1 + other2 + other3 + cell) & 3;
		cell = num + num2;
		other1 = num;
		other2 = num;
		other3 = num;
	}

	private void Flow4(ref int cell, ref int other1, ref int other2, ref int other3, ref int other4)
	{
		int num = (other1 + other2 + other3 + other4 + cell) / 5;
		int num2 = (other1 + other2 + other3 + other4 + cell) % 5;
		cell = num + num2;
		other1 = num;
		other2 = num;
		other3 = num;
		other4 = num;
	}

	public bool UpdateFluidConstWater(bool bRebuild)
	{
		int count = _dirtyChunkPosList.Count;
		if (count == 0)
		{
			return false;
		}
		_tmpDirtyChunks.Clear();
		_tmpDirtyChunkPosList.Clear();
		for (int i = 0; i < count; i++)
		{
			IntVector4 intVector = _dirtyChunkPosList[i];
			VFVoxelChunkData vFVoxelChunkData = VFVoxelWater.self.Voxels.readChunk(intVector.x, intVector.y, intVector.z);
			if (vFVoxelChunkData == null || vFVoxelChunkData.DataVT.Length <= 2)
			{
				if (!_tmpDirtyChunkPosList.Contains(intVector))
				{
					_tmpDirtyChunkPosList.Add(intVector);
				}
				continue;
			}
			int num = 0;
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					for (int l = -1; l <= 1; l++)
					{
						VFVoxelChunkData vFVoxelChunkData2 = ((num != 13) ? VFVoxelWater.self.Voxels.readChunk(intVector.x + l, intVector.y + k, intVector.z + j) : vFVoxelChunkData);
						if (vFVoxelChunkData2 == null || vFVoxelChunkData2.DataVT.Length < 2)
						{
							l = (k = (j = 2));
						}
						else
						{
							_tmpNeibourChunks[num++] = vFVoxelChunkData2;
						}
					}
				}
			}
			if (num < 27)
			{
				if (!_tmpDirtyChunkPosList.Contains(intVector))
				{
					_tmpDirtyChunkPosList.Add(intVector);
				}
				continue;
			}
			Array.Clear(_tmpDirtyNeibourChunks, 0, _tmpDirtyNeibourChunks.Length);
			GenerateFluidConstWater(vFVoxelChunkData, _tmpNeibourChunks, _tmpDirtyNeibourChunks, bRebuildNow: false);
			for (int m = 0; m < 27; m++)
			{
				if (_tmpDirtyNeibourChunks[m] != null)
				{
					IntVector4 chunkPosLod = _tmpDirtyNeibourChunks[m].ChunkPosLod;
					if (!_tmpDirtyChunkPosList.Contains(chunkPosLod))
					{
						_tmpDirtyChunkPosList.Add(chunkPosLod);
						_tmpDirtyChunks.Add(_tmpDirtyNeibourChunks[m]);
					}
				}
			}
		}
		List<IntVector4> dirtyChunkPosList = _dirtyChunkPosList;
		_dirtyChunkPosList = _tmpDirtyChunkPosList;
		_tmpDirtyChunkPosList = dirtyChunkPosList;
		if (bRebuild)
		{
			int count2 = _tmpDirtyChunks.Count;
			for (int n = 0; n < count2; n++)
			{
				_tmpDirtyChunks[n].EndBatchWriteVoxels();
			}
		}
		return true;
	}

	private VFVoxelChunkData[] GenerateFluidConstWater(VFVoxelChunkData curChunk, VFVoxelChunkData[] neibourChunks, VFVoxelChunkData[] dirtyNeibourChunks, bool bRebuildNow)
	{
		byte[] dataVT = curChunk.DataVT;
		byte[] terraDataForWater = GetTerraDataForWater(curChunk);
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < 32; j++)
			{
				for (int k = 0; k < 32; k++)
				{
					int num = VFVoxelChunkData.OneIndex(j, i, k);
					int num2 = num * 2;
					int num3 = dataVT[num2];
					int num4 = terraDataForWater[num2];
					if (num3 <= num4)
					{
						continue;
					}
					byte type = dataVT[num2 + 1];
					int num5 = num2 - 70;
					int num6 = dataVT[num5];
					if (num6 < 255 && (terraDataForWater[num5] < byte.MaxValue || num4 == 0))
					{
						num6 = 255;
						VFVoxelChunkData.ModVolumeType(curChunk, num5, (byte)num6, type, neibourChunks, dirtyNeibourChunks);
					}
					EulerianMatrix eulerianMatrix = EulerianMatrix.None;
					int num7 = num2 - 2;
					int num8 = num2 + 2;
					int num9 = num2 + 2450;
					int num10 = num2 - 2450;
					int num11 = dataVT[num7];
					int num12 = dataVT[num8];
					int num13 = dataVT[num9];
					int num14 = dataVT[num10];
					int num15 = num3 - Viscosity;
					if (num11 < num15 && terraDataForWater[num7] < num3)
					{
						eulerianMatrix |= EulerianMatrix.Left;
					}
					if (num12 < num15 && terraDataForWater[num8] < num3)
					{
						eulerianMatrix |= EulerianMatrix.Right;
					}
					if (num13 < num15 && terraDataForWater[num9] < num3)
					{
						eulerianMatrix |= EulerianMatrix.Forward;
					}
					if (num14 < num15 && terraDataForWater[num10] < num3)
					{
						eulerianMatrix |= EulerianMatrix.Backward;
					}
					if (eulerianMatrix != 0)
					{
						if ((eulerianMatrix & EulerianMatrix.Left) != 0 && dataVT[num7] != num15)
						{
							VFVoxelChunkData.ModVolumeType(curChunk, num7, (byte)num15, type, neibourChunks, dirtyNeibourChunks);
						}
						if ((eulerianMatrix & EulerianMatrix.Right) != 0 && dataVT[num8] != num15)
						{
							VFVoxelChunkData.ModVolumeType(curChunk, num8, (byte)num15, type, neibourChunks, dirtyNeibourChunks);
						}
						if ((eulerianMatrix & EulerianMatrix.Forward) != 0 && dataVT[num9] != num15)
						{
							VFVoxelChunkData.ModVolumeType(curChunk, num9, (byte)num15, type, neibourChunks, dirtyNeibourChunks);
						}
						if ((eulerianMatrix & EulerianMatrix.Backward) != 0 && dataVT[num10] != num15)
						{
							VFVoxelChunkData.ModVolumeType(curChunk, num10, (byte)num15, type, neibourChunks, dirtyNeibourChunks);
						}
					}
				}
			}
		}
		if (bRebuildNow)
		{
			for (int l = 0; l < 27; l++)
			{
				if (dirtyNeibourChunks[l] != null)
				{
					dirtyNeibourChunks[l].EndBatchWriteVoxels();
				}
			}
		}
		return dirtyNeibourChunks;
	}
}
