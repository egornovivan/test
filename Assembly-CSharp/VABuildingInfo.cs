using System;
using System.Collections.Generic;
using UnityEngine;

public class VABuildingInfo
{
	public VArtifactUnit vau;

	public BuildingID buildingId;

	public Vector3 pos;

	public float rotation;

	public int id;

	public Vector2 size;

	public IntVector2 center;

	public bool isRendered;

	public int cellSizeX;

	public int cellSizeZ;

	public Vector3 endPos;

	public VABuildingType buildingType;

	public Vector3 frontDoorPos;

	public List<CreatItemInfo> itemInfoList;

	public List<Vector3> npcPos;

	public Dictionary<int, int> npcIdNum;

	public List<Vector3> nativePos;

	public Dictionary<int, int> nativeIdNum;

	public Vector3 root;

	public Vector3 rootSize;

	public int pathID;

	public int campID;

	public int damageID;

	public VABuildingInfo()
	{
	}

	public VABuildingInfo(Vector3 pos, float rot, int id, BuildingID buildingId, VABuildingType type, VArtifactUnit vau, Vector2 size)
	{
		this.pos = pos;
		this.id = id;
		rotation = rot;
		buildingType = type;
		center = new IntVector2();
		center.x = Mathf.FloorToInt(this.pos.x);
		center.y = Mathf.FloorToInt(this.pos.z);
		root = pos;
		this.size = size;
		int num = Mathf.CeilToInt(size.y);
		int num2 = num / 2 + 3;
		frontDoorPos = root + new Vector3((float)num2 * Mathf.Sin(rot * (float)Math.PI / 180f), 0f, (float)num2 * Mathf.Cos(rot * (float)Math.PI / 180f));
		this.buildingId = buildingId;
		this.vau = vau;
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

	public void OnSpawned(GameObject obj)
	{
	}
}
