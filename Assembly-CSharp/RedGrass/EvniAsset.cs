using UnityEngine;

namespace RedGrass;

public class EvniAsset : ScriptableObject
{
	public int Tile = 32;

	public int XStart = -3072;

	public int ZStart = -3072;

	public int XTileCount = 192;

	public int ZTileCount = 192;

	public int XZTileCount;

	public int XEnd;

	public int ZEnd;

	public int FileXCount = 3;

	public int FlieZCount = 3;

	[HideInInspector]
	public int FileXZcount;

	[SerializeField]
	public EChunkSize ChunkSize;

	[SerializeField]
	private int _chunkShift = 5;

	[SerializeField]
	private int _chunkMask = 31;

	[SerializeField]
	private int _chunkSize;

	public ELodType LODType;

	[SerializeField]
	private int _maxLOD;

	[SerializeField]
	private int[] _LODExpandNums;

	[SerializeField]
	private int _dataExpandNums;

	public float[] LODDensities = new float[0];

	public int MeshQuadMaxCount = 6000;

	public float Density = 1f;

	public int SHIFT => _chunkShift;

	public int MASK => _chunkMask;

	public int CHUNKSIZE => _chunkSize;

	public int MaxLOD => _maxLOD;

	public int[] LODExpandNum => _LODExpandNums;

	public int DataExpandNum => _dataExpandNums;

	public void SetLODType(ELodType type)
	{
		LODType = type;
		CalcLODExtraVars();
	}

	public void SetDensity(float density)
	{
		Density = density;
	}

	public void SetChunkSize(EChunkSize chunk_size)
	{
		ChunkSize = chunk_size;
		CalcExtraChunkSizeVars();
	}

	public void CalcExtraTileVars()
	{
		XZTileCount = XTileCount * ZTileCount;
		XEnd = XStart + XTileCount * Tile;
		ZEnd = ZStart + ZTileCount * Tile;
	}

	public void CalcExtraFileVars()
	{
		FileXZcount = FileXCount * FlieZCount;
	}

	public void CalcExtraChunkSizeVars()
	{
		_chunkSize = 1 << (int)(ChunkSize + 5);
		_chunkShift = (int)(ChunkSize + 5);
		_chunkMask = (1 << _chunkShift) - 1;
	}

	public string GetLodTypeDesc(ELodType lod)
	{
		string empty = string.Empty;
		empty += "Max Lod : 1  ";
		for (int i = 0; i <= MaxLOD; i++)
		{
			if (i % 3 == 0)
			{
				string text = empty;
				empty = text + "\r\n Lod " + i + " expand num : " + _LODExpandNums[i] + " ;     ";
			}
			else
			{
				string text = empty;
				empty = text + "Lod " + i + " expand num : " + _LODExpandNums[i] + " ;     ";
			}
		}
		return empty;
	}

	public void CalcLODExtraVars()
	{
		_maxLOD = GetMaxLod();
		_LODExpandNums = new int[_maxLOD + 1];
		for (int i = 0; i <= _maxLOD; i++)
		{
			_LODExpandNums[i] = GetLODNumExpands(i);
		}
		if (MaxLOD != LODDensities.Length - 1)
		{
			LODDensities = new float[MaxLOD + 1];
			for (int j = 0; j <= MaxLOD; j++)
			{
				LODDensities[j] = 1f / (float)(1 << j);
			}
		}
		_dataExpandNums = ((CHUNKSIZE << MaxLOD) * _LODExpandNums[MaxLOD] + (CHUNKSIZE << MaxLOD) >> SHIFT) - 1;
	}

	private int GetMaxLod()
	{
		return LODType switch
		{
			ELodType.LOD_1_TYPE_1 => 0, 
			ELodType.LOD_1_TYPE_2 => 1, 
			ELodType.LOD_2_TYPE_1 => 2, 
			ELodType.LOD_2_TYPE_2 => 2, 
			ELodType.LOD_3_TYPE_1 => 3, 
			ELodType.LOD_3_TYPE_2 => 3, 
			ELodType.LOD_4_TYPE_1 => 4, 
			_ => -1, 
		};
	}

	private int GetLODNumExpands(int LOD)
	{
		switch (LODType)
		{
		case ELodType.LOD_1_TYPE_1:
			switch (LOD)
			{
			case 0:
				return 3;
			case 1:
				return 2;
			}
			break;
		case ELodType.LOD_1_TYPE_2:
			switch (LOD)
			{
			case 0:
				return 4;
			case 1:
				return 4;
			}
			break;
		case ELodType.LOD_2_TYPE_1:
			switch (LOD)
			{
			case 0:
				return 2;
			case 1:
				return 2;
			case 2:
				return 2;
			}
			break;
		case ELodType.LOD_2_TYPE_2:
			switch (LOD)
			{
			case 0:
				return 3;
			case 1:
				return 3;
			case 2:
				return 3;
			}
			break;
		case ELodType.LOD_3_TYPE_1:
			switch (LOD)
			{
			case 0:
				return 2;
			case 1:
				return 2;
			case 2:
				return 2;
			case 3:
				return 2;
			}
			break;
		case ELodType.LOD_3_TYPE_2:
			switch (LOD)
			{
			case 0:
				return 3;
			case 1:
				return 3;
			case 2:
				return 3;
			case 3:
				return 3;
			}
			break;
		case ELodType.LOD_4_TYPE_1:
			switch (LOD)
			{
			case 0:
				return 2;
			case 1:
				return 2;
			case 2:
				return 2;
			case 3:
				return 2;
			case 4:
				return 2;
			}
			break;
		}
		return -1;
	}
}
