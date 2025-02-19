using System.Collections.Generic;
using UnityEngine;

namespace TownData;

public class BuildingInfo
{
	public uint buildingNo;

	public Vector3 pos;

	public int rotation;

	public int id;

	public Vector3 size;

	public IntVector2 center;

	public bool isRendered;

	public int cellSizeX;

	public int cellSizeZ;

	public Vector3 endPos;

	public List<CreatItemInfo> itemInfoList;

	public List<Vector3> npcPos;

	public Dictionary<int, int> npcIdNum;

	public Vector3 root;

	public Vector3 rootSize;

	public BuildingInfo()
	{
	}

	public BuildingInfo(IntVector2 pos, int id, int rot, int cellSizeX, int cellSizeZ)
	{
		this.pos.x = pos.x;
		this.pos.y = -1f;
		this.pos.z = pos.y;
		this.id = id;
		rotation = rot;
		this.cellSizeX = cellSizeX;
		this.cellSizeZ = cellSizeZ;
		switch (rot)
		{
		case 0:
			root = this.pos;
			break;
		case 1:
			root = new Vector3(this.pos.x, this.pos.y, this.pos.z + (float)cellSizeZ);
			break;
		case 2:
			root = new Vector3(this.pos.x + (float)cellSizeX, this.pos.y, this.pos.z + (float)cellSizeZ);
			break;
		case 3:
			root = new Vector3(this.pos.x + (float)cellSizeX, this.pos.y, this.pos.z);
			break;
		}
		center = new IntVector2();
		center.x = Mathf.FloorToInt(this.pos.x + (float)(cellSizeX / 2));
		center.y = Mathf.FloorToInt(this.pos.z + (float)(cellSizeZ / 2));
	}

	public BuildingInfo(IntVector2 pos, int id, int rot, int cellSizeX, int cellSizeZ, uint no)
	{
		this.pos.x = pos.x;
		this.pos.y = -1f;
		this.pos.z = pos.y;
		this.id = id;
		rotation = rot;
		this.cellSizeX = cellSizeX;
		this.cellSizeZ = cellSizeZ;
		switch (rot)
		{
		case 0:
			root = this.pos;
			break;
		case 1:
			root = new Vector3(this.pos.x, this.pos.y, this.pos.z + (float)cellSizeZ);
			break;
		case 2:
			root = new Vector3(this.pos.x + (float)cellSizeX, this.pos.y, this.pos.z + (float)cellSizeZ);
			break;
		case 3:
			root = new Vector3(this.pos.x + (float)cellSizeX, this.pos.y, this.pos.z);
			break;
		}
		center = new IntVector2();
		center.x = Mathf.FloorToInt(this.pos.x + (float)(cellSizeX / 2));
		center.y = Mathf.FloorToInt(this.pos.z + (float)(cellSizeZ / 2));
		buildingNo = no;
	}

	public void setEndPos()
	{
		endPos = new Vector3(pos.x + (float)cellSizeX, pos.y, pos.z + (float)cellSizeZ);
	}

	public bool isBulidingArea(IntVector2 posXZ)
	{
		bool result = false;
		if ((float)posXZ.x >= pos.x && (float)posXZ.y >= pos.z && (float)posXZ.x <= endPos.x && (float)posXZ.y <= endPos.z)
		{
			result = true;
		}
		return result;
	}

	public void setHeight(float top)
	{
		pos.y = top;
		endPos.y = top;
		root.y = top;
	}

	public float getHeight()
	{
		return pos.y;
	}
}
