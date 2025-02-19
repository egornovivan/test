using System;
using System.IO;

public class VFTerTileCacheDesc
{
	public const int DataOfs = 24;

	public const int c_dataAxisLen = 35;

	public const int c_bitMaskBTown = 1;

	public const int c_bitMaskVData = 2;

	public const int c_bitMaskNData = 4;

	public const int c_bitMaskHData = 8;

	public const int c_bitMaskGData = 16;

	public const int c_bitMaskTData = 32;

	public const int c_bitMaskAllData = 62;

	public const int c_bitMaskVoxData = 2;

	public const int c_nDataLen = 9800;

	public const int c_hDataLen = 4900;

	public const int c_gDataLen = 4900;

	public const int c_tDataLen = 1225;

	public IntVector4 xzlh;

	public int bitMask;

	public int dataLen;

	public long pos;

	public int vDataLen => dataLen - (((bitMask & 4) != 0) ? 9800 : 0) - (((bitMask & 8) != 0) ? 4900 : 0) - (((bitMask & 0x10) != 0) ? 4900 : 0) - (((bitMask & 0x20) != 0) ? 1225 : 0);

	public static VFTerTileCacheDesc ReadDescFromCache(BinaryReader br)
	{
		int x_ = br.ReadInt32();
		int y_ = br.ReadInt32();
		int z_ = br.ReadInt32();
		int w_ = br.ReadInt32();
		int num = br.ReadInt32();
		int num2 = br.ReadInt32();
		VFTerTileCacheDesc vFTerTileCacheDesc = new VFTerTileCacheDesc();
		vFTerTileCacheDesc.xzlh = new IntVector4(x_, y_, z_, w_);
		vFTerTileCacheDesc.bitMask = num;
		vFTerTileCacheDesc.dataLen = num2;
		vFTerTileCacheDesc.pos = br.BaseStream.Position;
		return vFTerTileCacheDesc;
	}

	public void WriteDescToCache(BinaryWriter bw)
	{
		bw.Write(xzlh.x);
		bw.Write(xzlh.y);
		bw.Write(xzlh.z);
		bw.Write(xzlh.w);
		bw.Write(bitMask);
		bw.Write(dataLen);
	}

	public void ReadDataFromCache(BinaryReader br, VFTile tile, double[][] nData, float[][] hData, float[][] gData, RandomMapType[][] tData)
	{
		br.BaseStream.Seek(pos, SeekOrigin.Begin);
		if ((bitMask & 2) != 0)
		{
			tile.tileX = xzlh.x;
			tile.tileZ = xzlh.y;
			tile.tileL = xzlh.z;
			tile.tileH = xzlh.w;
			int maxDataLenY = tile.MaxDataLenY;
			int dataLenX = VFTile.DataLenX;
			int dataLenZ = VFTile.DataLenZ;
			for (int i = 0; i < dataLenZ; i++)
			{
				int[] array = tile.nTerraYLens[i];
				int[] array2 = tile.nWaterYLens[i];
				byte[][] array3 = tile.terraVoxels[i];
				byte[][] array4 = tile.waterVoxels[i];
				for (int j = 0; j < dataLenX; j++)
				{
					int count = (array[j] = br.ReadUInt16());
					Array.Clear(array3[j], 0, maxDataLenY);
					br.Read(array3[j], 0, count);
					count = (array2[j] = br.ReadUInt16());
					Array.Clear(array4[j], 0, maxDataLenY);
					br.Read(array4[j], 0, count);
				}
			}
		}
		if ((bitMask & 4) != 0)
		{
			for (int k = 0; k < 35; k++)
			{
				for (int l = 0; l < 35; l++)
				{
					nData[k][l] = br.ReadDouble();
				}
			}
		}
		if ((bitMask & 8) != 0)
		{
			for (int m = 0; m < 35; m++)
			{
				for (int n = 0; n < 35; n++)
				{
					hData[m][n] = br.ReadSingle();
				}
			}
		}
		if ((bitMask & 0x10) != 0)
		{
			for (int num = 0; num < 35; num++)
			{
				for (int num2 = 0; num2 < 35; num2++)
				{
					gData[num][num2] = br.ReadSingle();
				}
			}
		}
		if ((bitMask & 0x20) == 0)
		{
			return;
		}
		for (int num3 = 0; num3 < 35; num3++)
		{
			for (int num4 = 0; num4 < 35; num4++)
			{
				tData[num3][num4] = (RandomMapType)br.ReadByte();
			}
		}
	}

	public static VFTerTileCacheDesc WriteDataToCache(BinaryWriter bw, int bitMask, VFTile tile, double[][] nData, float[][] hData, float[][] gData, RandomMapType[][] tData)
	{
		VFTerTileCacheDesc vFTerTileCacheDesc = new VFTerTileCacheDesc();
		vFTerTileCacheDesc.xzlh = new IntVector4(tile.tileX, tile.tileZ, tile.tileL, tile.tileH);
		vFTerTileCacheDesc.bitMask = bitMask;
		vFTerTileCacheDesc.dataLen = 0;
		vFTerTileCacheDesc.WriteDescToCache(bw);
		vFTerTileCacheDesc.pos = bw.BaseStream.Position;
		if ((bitMask & 2) != 0)
		{
			int dataLenX = VFTile.DataLenX;
			int dataLenZ = VFTile.DataLenZ;
			for (int i = 0; i < dataLenZ; i++)
			{
				int[] array = tile.nTerraYLens[i];
				int[] array2 = tile.nWaterYLens[i];
				byte[][] array3 = tile.terraVoxels[i];
				byte[][] array4 = tile.waterVoxels[i];
				for (int j = 0; j < dataLenX; j++)
				{
					int num = array[j];
					bw.Write((ushort)num);
					bw.Write(array3[j], 0, num);
					num = array2[j];
					bw.Write((ushort)num);
					bw.Write(array4[j], 0, num);
				}
			}
		}
		if ((bitMask & 4) != 0)
		{
			for (int k = 0; k < 35; k++)
			{
				for (int l = 0; l < 35; l++)
				{
					bw.Write(nData[k][l]);
				}
			}
		}
		if ((bitMask & 8) != 0)
		{
			for (int m = 0; m < 35; m++)
			{
				for (int n = 0; n < 35; n++)
				{
					bw.Write(hData[m][n]);
				}
			}
		}
		if ((bitMask & 0x10) != 0)
		{
			for (int num2 = 0; num2 < 35; num2++)
			{
				for (int num3 = 0; num3 < 35; num3++)
				{
					bw.Write(gData[num2][num3]);
				}
			}
		}
		if ((bitMask & 0x20) != 0)
		{
			for (int num4 = 0; num4 < 35; num4++)
			{
				for (int num5 = 0; num5 < 35; num5++)
				{
					bw.Write((byte)tData[num4][num5]);
				}
			}
		}
		vFTerTileCacheDesc.dataLen = (int)(bw.BaseStream.Position - vFTerTileCacheDesc.pos);
		bw.BaseStream.Seek(vFTerTileCacheDesc.pos - 24, SeekOrigin.Begin);
		vFTerTileCacheDesc.WriteDescToCache(bw);
		return vFTerTileCacheDesc;
	}
}
