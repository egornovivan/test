using System.Collections.Generic;

public class DetectedTownInfo
{
	public IntVector2 PosCenter;

	public static Dictionary<IntVector2, DetectedTownInfo> AllDetectedTowns = new Dictionary<IntVector2, DetectedTownInfo>();

	public static void AddDetectedTown(DetectedTownInfo dti, int teamId)
	{
		AllDetectedTowns.Add(dti.PosCenter, dti);
	}
}
