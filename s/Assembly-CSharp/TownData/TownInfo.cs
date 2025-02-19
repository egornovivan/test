using System.Collections.Generic;
using uLink;
using UnityEngine;

namespace TownData;

public class TownInfo
{
	private int id;

	private int height = -1;

	private IntVector2 posStart;

	private IntVector2 posEnd;

	private IntVector2 posCenter;

	private int cellNumX;

	private int cellNumZ;

	private int cellSizeX;

	private int cellSizeZ;

	private int sizeX;

	private int sizeZ;

	private int tid;

	private List<uint> buildingIdList = new List<uint>();

	private List<IntVector2> townNpcPosList = new List<IntVector2>();

	private List<IntVector2> buildingCellList = new List<IntVector2>();

	private List<IntVector2> streetCellList = new List<IntVector2>();

	public float radius;

	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public int Height
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
		}
	}

	public IntVector2 PosStart
	{
		get
		{
			return posStart;
		}
		set
		{
			posStart = value;
		}
	}

	public IntVector2 PosEnd
	{
		get
		{
			return posEnd;
		}
		set
		{
			posEnd = value;
		}
	}

	public IntVector2 PosCenter
	{
		get
		{
			return posCenter;
		}
		set
		{
			posCenter = value;
		}
	}

	public int CellNumX
	{
		get
		{
			return cellNumX;
		}
		set
		{
			cellNumX = value;
		}
	}

	public int CellNumZ
	{
		get
		{
			return cellNumZ;
		}
		set
		{
			cellNumZ = value;
		}
	}

	public int CellSizeX
	{
		get
		{
			return cellSizeX;
		}
		set
		{
			cellSizeX = value;
		}
	}

	public int CellSizeZ
	{
		get
		{
			return cellSizeZ;
		}
		set
		{
			cellSizeZ = value;
		}
	}

	public int SizeX
	{
		get
		{
			return sizeX;
		}
		set
		{
			sizeX = value;
		}
	}

	public int SizeZ
	{
		get
		{
			return sizeZ;
		}
		set
		{
			sizeZ = value;
		}
	}

	public int Tid
	{
		get
		{
			return tid;
		}
		set
		{
			tid = value;
		}
	}

	public List<uint> BuildingIdList
	{
		get
		{
			return buildingIdList;
		}
		set
		{
			buildingIdList = value;
		}
	}

	public List<IntVector2> TownNpcPosList
	{
		get
		{
			return townNpcPosList;
		}
		set
		{
			townNpcPosList = value;
		}
	}

	public List<IntVector2> BuildingCellList
	{
		get
		{
			return buildingCellList;
		}
		set
		{
			buildingCellList = value;
		}
	}

	public List<IntVector2> StreetCellList
	{
		get
		{
			return streetCellList;
		}
		set
		{
			streetCellList = value;
		}
	}

	public TownInfo()
	{
	}

	public TownInfo(IntVector2 pos, int cellNumX, int cellNumZ, int cellSizeX, int cellSizeZ, int tid)
	{
		posStart = pos;
		this.cellNumX = cellNumX;
		this.cellNumZ = cellNumZ;
		this.cellSizeX = cellSizeX;
		this.cellSizeZ = cellSizeZ;
		sizeX = cellNumX * cellSizeX;
		sizeZ = cellNumZ * cellSizeZ;
		posEnd = new IntVector2(pos.x + sizeX, pos.y + sizeZ);
		posCenter = new IntVector2(pos.x + sizeX / 2, pos.y + sizeZ / 2);
		this.tid = tid;
		radius = TrianglesSide(sizeX / 2, sizeZ / 2) * 2f;
	}

	public TownInfo(IntVector2 pos, int cellNumX, int cellNumZ, int cellSizeX, int cellSizeZ, int tid, uint[] buildingIdList, IntVector2[] townNpcPosList)
	{
		posStart = pos;
		this.cellNumX = cellNumX;
		this.cellNumZ = cellNumZ;
		this.cellSizeX = cellSizeX;
		this.cellSizeZ = cellSizeZ;
		sizeX = cellNumX * cellSizeX;
		sizeZ = cellNumZ * cellSizeZ;
		posEnd = new IntVector2(pos.x + sizeX, pos.y + sizeZ);
		posCenter = new IntVector2(pos.x + sizeX / 2, pos.y + sizeZ / 2);
		this.tid = tid;
		radius = TrianglesSide(sizeX / 2, sizeZ / 2) * 2f;
		foreach (uint item in buildingIdList)
		{
			BuildingIdList.Add(item);
		}
		foreach (IntVector2 item2 in townNpcPosList)
		{
			TownNpcPosList.Add(item2);
		}
	}

	public void AddStreetCell()
	{
		for (int i = 0; i < cellNumX; i++)
		{
			for (int j = 0; j < cellNumZ; j++)
			{
				IntVector2 item = new IntVector2(i, j);
				if (!buildingCellList.Contains(item))
				{
					streetCellList.Add(item);
				}
			}
		}
	}

	public void AddBuildingCell(int x, int z)
	{
		buildingCellList.Add(new IntVector2(x, z));
	}

	public float TrianglesSide(float x, float y)
	{
		return Mathf.Sqrt(Mathf.Pow(x, 2f) + Mathf.Pow(y, 2f));
	}

	internal static object DeserializeInfo(uLink.BitStream stream, params object[] codecOptions)
	{
		int num = stream.Read<int>(new object[0]);
		IntVector2 pos = stream.Read<IntVector2>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		int num4 = stream.Read<int>(new object[0]);
		int num5 = stream.Read<int>(new object[0]);
		int num6 = stream.Read<int>(new object[0]);
		uint[] array = stream.Read<uint[]>(new object[0]);
		IntVector2[] array2 = stream.Read<IntVector2[]>(new object[0]);
		TownInfo townInfo = new TownInfo(pos, num2, num3, num4, num5, num6, array, array2);
		townInfo.id = num;
		return townInfo;
	}

	internal static void SerializeInfo(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		TownInfo townInfo = value as TownInfo;
		stream.Write(townInfo.id);
		stream.Write(townInfo.posStart);
		stream.Write(townInfo.cellNumX);
		stream.Write(townInfo.cellNumZ);
		stream.Write(townInfo.cellSizeX);
		stream.Write(townInfo.cellSizeZ);
		stream.Write(townInfo.tid);
		stream.Write(townInfo.buildingIdList.ToArray());
		stream.Write(townInfo.TownNpcPosList.ToArray());
	}
}
