using System.Collections.Generic;
using TownData;

internal class TownNpcManager
{
	private static TownNpcManager mInstance;

	public Dictionary<IntVector2, TownNpcInfo> npcInfoMap;

	public static TownNpcManager Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new TownNpcManager();
			}
			return mInstance;
		}
	}

	public TownNpcManager()
	{
		npcInfoMap = new Dictionary<IntVector2, TownNpcInfo>();
	}

	public void Clear()
	{
		npcInfoMap = new Dictionary<IntVector2, TownNpcInfo>();
	}

	public bool AddNpc(IntVector2 posXZ, int id)
	{
		if (npcInfoMap.ContainsKey(posXZ))
		{
			return false;
		}
		TownNpcInfo value = new TownNpcInfo(posXZ, id);
		npcInfoMap.Add(posXZ, value);
		return true;
	}

	public int GetRNpcIdByPos(IntVector2 posXZ)
	{
		if (!npcInfoMap.ContainsKey(posXZ))
		{
			return -1;
		}
		return npcInfoMap[posXZ].getId();
	}

	public TownNpcInfo GetNpcInfoByPos(IntVector2 posXZ)
	{
		if (!npcInfoMap.ContainsKey(posXZ))
		{
			return null;
		}
		return npcInfoMap[posXZ];
	}
}
