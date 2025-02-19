using System;
using System.Collections.Generic;

public class RandomFecesDataMgr
{
	private static string defaultPath = "AiPrefab/MonsterSpecialItem/Monster_feces01";

	public static int[] GenFecesItemIdCount(out string modelPath)
	{
		List<int> list = new List<int>();
		modelPath = defaultPath;
		List<int> allId = FecesData.GetAllId();
		Random random = new Random();
		int index = random.Next(allId.Count);
		int id = allId[index];
		FecesData fecesData = FecesData.GetFecesData(id);
		modelPath = fecesData.path;
		foreach (ProbableItem item in fecesData.fixItem)
		{
			if (!(random.NextDouble() > (double)item.probability))
			{
				list.Add(item.protoId);
				list.Add(random.Next(item.numMin, item.numMax));
			}
		}
		foreach (ProbableItem probableItem in fecesData.probableItems)
		{
			if (!(random.NextDouble() > (double)probableItem.probability))
			{
				list.Add(probableItem.protoId);
				list.Add(random.Next(probableItem.numMin, probableItem.numMax));
			}
		}
		return list.ToArray();
	}
}
