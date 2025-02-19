using System;
using System.Collections.Generic;
using UnityEngine;

public class CircleTrans
{
	private List<CircleTran> _mCircleTransList = new List<CircleTran>();

	private Vector3 _center = default(Vector3);

	private List<Vector3> Postions;

	public List<CircleTran> mCircleTransList => _mCircleTransList;

	public CircleTrans(Vector3 center, float radius = 3f)
	{
		_center = center;
	}

	public bool DisCircletarns(int Num, float radius = 3f)
	{
		if (Postions == null)
		{
			Postions = new List<Vector3>();
		}
		Postions = GetCirclePosition(_center, Num, radius);
		foreach (Vector3 postion in Postions)
		{
			Quaternion rotation = default(Quaternion);
			rotation.SetFromToRotation(_center, postion);
			CircleTran item = new CircleTran(_center, postion, rotation);
			mCircleTransList.Add(item);
		}
		return Postions.Count > 0;
	}

	private Vector3 GetHorizontalDir()
	{
		float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		return new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
	}

	private List<Vector3> GetCirclePosition(Vector3 center, int num, float radius)
	{
		List<Vector3> list = new List<Vector3>();
		float num2 = Mathf.Sin((float)Math.PI / (float)num) * radius * 2f;
		int num3 = 0;
		while (list.Count < num)
		{
			num3++;
			Vector3 horizontalDir = GetHorizontalDir();
			bool flag = false;
			foreach (Vector3 item in list)
			{
				if (Vector3.Distance(item, center + Vector3.up + horizontalDir * radius) < num2 * 0.5f)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (!Physics.Raycast(center + Vector3.up, horizontalDir + Vector3.up * 0.5f, radius) && Physics.Raycast(center + Vector3.up * radius / 2f + horizontalDir * radius, Vector3.down, 3f))
				{
					list.Add(center + Vector3.up + horizontalDir * radius);
				}
				if (num3 >= 100)
				{
					break;
				}
			}
		}
		if (list.Count == 0)
		{
			return list;
		}
		while (list.Count < num)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list.Add((center + Vector3.up + list[i]) / 2f);
				if (list.Count >= num)
				{
					break;
				}
			}
		}
		return list;
	}

	public bool OccupyPostion(int enityId)
	{
		foreach (CircleTran mCircleTrans in mCircleTransList)
		{
			if (!mCircleTrans.IsOccpied)
			{
				mCircleTrans.Occpy(enityId);
				return true;
			}
		}
		return false;
	}

	public bool ReleasePostion(int enityid)
	{
		foreach (CircleTran mCircleTrans in mCircleTransList)
		{
			if (mCircleTrans.CantainId(enityid))
			{
				mCircleTrans.Release();
				return true;
			}
		}
		return false;
	}

	public bool CantainEnityID(int enityID)
	{
		foreach (CircleTran mCircleTrans in mCircleTransList)
		{
			if (mCircleTrans.CantainId(enityID))
			{
				return true;
			}
		}
		return false;
	}

	public Vector3 CalculateEmptyPostion(int enityId)
	{
		foreach (CircleTran mCircleTrans in mCircleTransList)
		{
			if (!mCircleTrans.IsOccpied)
			{
				mCircleTrans.Occpy(enityId);
				return mCircleTrans.Postion;
			}
		}
		return Vector3.zero;
	}
}
