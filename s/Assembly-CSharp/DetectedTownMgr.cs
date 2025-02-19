using System;
using System.Collections.Generic;

public class DetectedTownMgr
{
	public delegate void AddDetectedTownResult(DetectedTownInfo town, int teamId);

	private static DetectedTownMgr mInstance;

	public Dictionary<int, List<IntVector2>> allDetectedTowns = new Dictionary<int, List<IntVector2>>();

	public static DetectedTownMgr Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new DetectedTownMgr();
				DetectedTownMgr.AddDetectedTownListener = (AddDetectedTownResult)Delegate.Combine(DetectedTownMgr.AddDetectedTownListener, new AddDetectedTownResult(DetectedTownInfo.AddDetectedTown));
			}
			return mInstance;
		}
	}

	public static event AddDetectedTownResult AddDetectedTownListener;

	public void AddDetectedTown(DetectedTownInfo town, int teamId)
	{
		if (!allDetectedTowns.ContainsKey(teamId))
		{
			allDetectedTowns[teamId] = new List<IntVector2>();
		}
		if (!allDetectedTowns[teamId].Contains(town.PosCenter))
		{
			allDetectedTowns[teamId].Add(town.PosCenter);
			DetectedTownMgr.AddDetectedTownListener(town, teamId);
		}
	}
}
