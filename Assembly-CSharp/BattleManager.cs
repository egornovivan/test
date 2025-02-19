using System.Collections.Generic;
using uLink;

public class BattleManager
{
	internal static Dictionary<int, BattleInfo> CampList = new Dictionary<int, BattleInfo>();

	internal static Dictionary<int, List<PlayerBattleInfo>> BattleInfoDict = new Dictionary<int, List<PlayerBattleInfo>>();

	private static int mTeamNum;

	private static int mNumberTeam;

	public static int teamNum => mTeamNum;

	public static int numberTeam => mNumberTeam;

	internal static void InitBattleInfo(int teamNum = 1, int numberTeam = 4)
	{
		mTeamNum = teamNum;
		mNumberTeam = numberTeam;
		CampList.Clear();
		BattleInfoDict.Clear();
		for (int i = 0; i < teamNum; i++)
		{
			CampList[i] = new BattleInfo();
			BattleInfoDict[i] = new List<PlayerBattleInfo>();
		}
	}

	internal static BattleInfo GetBattleInfo(int camp)
	{
		if (!CampList.ContainsKey(camp))
		{
			return null;
		}
		return CampList[camp];
	}

	public static void RPC_S2C_BattleOver(BitStream stream, NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
	}

	public static void RPC_S2C_BattleInfo(BitStream stream, NetworkMessageInfo info)
	{
		BattleInfo battleInfo = stream.Read<BattleInfo>(new object[0]);
		if (CampList.ContainsKey(battleInfo._group))
		{
			CampList[battleInfo._group].Update(battleInfo);
		}
		else
		{
			CampList[battleInfo._group] = battleInfo;
		}
	}

	public static void RPC_S2C_BattleInfos(BitStream stream, NetworkMessageInfo info)
	{
		BattleInfo[] array = stream.Read<BattleInfo[]>(new object[0]);
		BattleInfo[] array2 = array;
		foreach (BattleInfo battleInfo in array2)
		{
			if (CampList.ContainsKey(battleInfo._group))
			{
				CampList[battleInfo._group].Update(battleInfo);
			}
			else
			{
				CampList[battleInfo._group] = battleInfo;
			}
		}
	}
}
