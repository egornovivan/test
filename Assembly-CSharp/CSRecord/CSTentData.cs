using System.Collections.Generic;

namespace CSRecord;

public class CSTentData : CSObjectData
{
	public List<int> npcIds = new List<int>();

	public Sickbed[] allSickbeds;

	public CSTentData()
	{
		dType = 14;
		allSickbeds = new Sickbed[8];
		for (int i = 0; i < 8; i++)
		{
			allSickbeds[i] = new Sickbed(i);
		}
	}
}
