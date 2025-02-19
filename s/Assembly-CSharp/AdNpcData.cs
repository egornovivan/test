using System.Collections.Generic;
using UnityEngine;

public class AdNpcData
{
	public int mID;

	public int mRnpc_ID;

	public int mRecruitQC_ID;

	public int mArea;

	public List<GroupInfo> mQC_IDList = new List<GroupInfo>();

	public List<int> m_CSRecruitMissionList = new List<int>();

	public int mWild;

	public int mQC_ID
	{
		get
		{
			if (mQC_IDList.Count == 0)
			{
				return -1;
			}
			int radius = mQC_IDList[mQC_IDList.Count - 1].radius;
			int num = Random.Range(0, radius);
			for (int i = 0; i < mQC_IDList.Count; i++)
			{
				GroupInfo groupInfo = mQC_IDList[i];
				if (num < groupInfo.radius)
				{
					return groupInfo.id;
				}
			}
			return -1;
		}
	}
}
