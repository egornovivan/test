using System.Collections.Generic;
using UnityEngine;

public class GlobalTreeInfo
{
	public int _terrainIndex;

	public TreeInfo _treeInfo;

	private Vector3 _treeCapCenterPos;

	private List<PickPersonInfo> mPickInfo = new List<PickPersonInfo>();

	public Vector3 WorldPos
	{
		get
		{
			if (_terrainIndex < 0)
			{
				return _treeInfo.m_pos;
			}
			return LSubTerrUtils.TreeTerrainPosToWorldPos(_terrainIndex, _treeInfo.m_pos);
		}
	}

	public Vector3 TreeCapCenterPos
	{
		get
		{
			return _treeCapCenterPos;
		}
		set
		{
			_treeCapCenterPos = value;
		}
	}

	public bool HasCreatPickPos => (mPickInfo != null && mPickInfo.Count > 0) ? true : false;

	public GlobalTreeInfo(int index, TreeInfo treeinfo)
	{
		_terrainIndex = index;
		_treeInfo = treeinfo;
	}

	public GlobalTreeInfo(int xindex, int zindex, TreeInfo treeinfo)
	{
		_terrainIndex = LSubTerrUtils.PosToIndex(xindex, zindex);
		_treeInfo = treeinfo;
	}

	public bool CreatCutPos(Vector3 center, Vector3 dir, float radiu, float pointNum = 3f)
	{
		Quaternion quaternion = Quaternion.LookRotation(dir);
		float num = 360f / pointNum;
		Quaternion quaternion2 = Quaternion.Euler(quaternion.eulerAngles.x, quaternion.eulerAngles.y + num, quaternion.eulerAngles.z);
		Quaternion quaternion3 = Quaternion.Euler(quaternion.eulerAngles.x, quaternion.eulerAngles.y - num, quaternion.eulerAngles.z);
		Vector3 pos = center + quaternion2 * dir * (radiu + 0.2f);
		Vector3 pos2 = center + quaternion3 * dir * (radiu + 0.2f);
		mPickInfo.Add(new PickPersonInfo(0, pos));
		mPickInfo.Add(new PickPersonInfo(0, pos2));
		return (float)mPickInfo.Count == pointNum - 1f;
	}

	public bool AddCutter(int entityid, out Vector3 cutPos)
	{
		cutPos = Vector3.zero;
		for (int i = 0; i < mPickInfo.Count; i++)
		{
			if (mPickInfo[i]._entityId == entityid)
			{
				cutPos = mPickInfo[i]._PickPos;
				mPickInfo[i]._entityId = entityid;
				return true;
			}
		}
		for (int j = 0; j < mPickInfo.Count; j++)
		{
			if (mPickInfo[j]._entityId == 0)
			{
				cutPos = mPickInfo[j]._PickPos;
				mPickInfo[j]._entityId = entityid;
				return true;
			}
		}
		return false;
	}
}
