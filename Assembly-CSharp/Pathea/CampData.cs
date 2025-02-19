using System.Collections.Generic;

namespace Pathea;

public class CampData
{
	private int m_ID;

	private string m_Name;

	private int[] m_Data;

	private static Dictionary<int, CampData> s_CampData;

	public static void LoadData()
	{
		s_CampData = new Dictionary<int, CampData>();
	}

	public static int GetValue(int src, int dst)
	{
		if (s_CampData.ContainsKey(src))
		{
			return s_CampData[src].m_Data[dst];
		}
		return 0;
	}
}
