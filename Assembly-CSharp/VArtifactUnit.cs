using System.Collections.Generic;
using TownData;
using UnityEngine;
using VArtifactTownXML;

public class VArtifactUnit
{
	public int unitIndex;

	public int isoId;

	public string isoName;

	public ulong isoGuId;

	public VArtifactTown vat;

	public Vector3 worldPos;

	public float rot;

	public Dictionary<IntVector3, VFVoxel> townVoxel = new Dictionary<IntVector3, VFVoxel>();

	public Dictionary<Vector3, BuildingID> buildingPosID = new Dictionary<Vector3, BuildingID>();

	public Dictionary<Vector3, VATownNpcInfo> npcPosInfo = new Dictionary<Vector3, VATownNpcInfo>();

	public Dictionary<Vector3, NativePointInfo> nativePointInfo = new Dictionary<Vector3, NativePointInfo>();

	public IntVector2 isoStartPos;

	public IntVector2 isoEndPos;

	public IntVector2 PosStart;

	public IntVector2 PosEnd;

	public IntVector2 PosCenter;

	public int level;

	public bool isRandomed;

	public bool isAddedToRender;

	public bool isDoodadNpcRendered;

	public VArtifactType type;

	public List<BuildingIdNum> buildingIdNum;

	public List<NpcIdNum> npcIdNum;

	public List<BuildingCell> buildingCell;

	public List<Vector3> npcPos;

	public IntVector3 vaSize;

	public Vector3 towerPos;

	public IntVector2 PosEntrance => new IntVector2(PosCenter.x, PosStart.y - 5);

	public int SmallRadius => Mathf.Max(Mathf.Abs(PosEnd.x - PosStart.x), Mathf.Abs(PosEnd.y - PosStart.y)) / 2 - 10;

	public float GetCenterDistance(IntVector2 pointPos)
	{
		return pointPos.Distance(PosCenter);
	}

	public bool NeedCut(int x, int z)
	{
		return new IntVector2(x, z).Distance(PosCenter) < (float)SmallRadius;
	}

	public void Clear()
	{
		townVoxel.Clear();
	}

	public void SetHeight(float y)
	{
		worldPos.y = y;
	}

	public bool IsInTown(int x, int z)
	{
		if (x >= PosStart.x && z >= PosStart.y && x <= PosEnd.x && z <= PosEnd.y)
		{
			return true;
		}
		return false;
	}
}
