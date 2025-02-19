using System.Collections.Generic;
using UnityEngine;

public class BuildingMap
{
	public List<BuildingInfo> s_Data;

	public BuildingMap()
	{
		s_Data = new List<BuildingInfo>();
	}

	public Transform OccupyTran(int entityId)
	{
		for (int i = 0; i < s_Data.Count; i++)
		{
			if (s_Data[i].IsOccupy && s_Data[i].occupyId == entityId)
			{
				return s_Data[i].tran;
			}
		}
		List<int> emptyTran = GetEmptyTran();
		if (emptyTran.Count > 0)
		{
			int index = Random.Range(0, emptyTran.Count);
			index = emptyTran[index];
			s_Data[index].occupyId = entityId;
			s_Data[index].IsOccupy = true;
			return s_Data[index].tran;
		}
		return null;
	}

	private List<int> GetEmptyTran()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < s_Data.Count; i++)
		{
			if (!s_Data[i].IsOccupy)
			{
				list.Add(i);
			}
		}
		return list;
	}

	public void ReleaseTran(int entityId)
	{
		for (int i = 0; i < s_Data.Count; i++)
		{
			if (s_Data[i].IsOccupy && s_Data[i].occupyId == entityId)
			{
				s_Data[i].IsOccupy = false;
				s_Data[i].occupyId = 0;
			}
		}
	}

	public void LoadIn(Transform[] trans)
	{
		for (int i = 0; i < trans.Length; i++)
		{
			s_Data.Add(new BuildingInfo(0, trans[i]));
		}
	}
}
