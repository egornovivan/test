using System.Collections.Generic;
using UnityEngine;

public class HistoryPosTracker
{
	private List<Vector3> historyPos;

	private List<float> historyTime;

	public int maxRecord = 16;

	public float breakDistance = 3f;

	public Vector3 aveVelocity
	{
		get
		{
			if (historyPos.Count < maxRecord)
			{
				return Vector3.zero;
			}
			return (historyPos[0] - historyPos[maxRecord - 1]) / (historyTime[0] - historyTime[maxRecord - 1] + 0.001f);
		}
	}

	public HistoryPosTracker()
	{
		historyPos = new List<Vector3>(maxRecord);
		historyTime = new List<float>(maxRecord);
	}

	public void Record(Vector3 pos, float time)
	{
		if (historyPos.Count > 0 && Vector3.Distance(pos, historyPos[0]) > breakDistance)
		{
			historyPos.Clear();
			historyTime.Clear();
		}
		historyPos.Insert(0, pos);
		historyTime.Insert(0, time);
		if (historyPos.Count > maxRecord)
		{
			historyPos.RemoveAt(maxRecord);
		}
		if (historyTime.Count > maxRecord)
		{
			historyTime.RemoveAt(maxRecord);
		}
	}
}
