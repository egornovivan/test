using System.Collections.Generic;
using System.Linq;
using uLink;
using UnityEngine;

public class BattleManager : UnityEngine.MonoBehaviour
{
	private static Dictionary<int, BattleInfo> CampList = new Dictionary<int, BattleInfo>();

	private static bool IsBattleOver = false;

	public static void InitBattleInfo(int teamID)
	{
		if (!CampList.ContainsKey(teamID))
		{
			CampList[teamID] = new BattleInfo(teamID);
		}
		CampList[teamID].Reset();
	}

	public static void Reset(int teamID)
	{
		if (CampList.ContainsKey(teamID))
		{
			CampList[teamID].Reset();
		}
	}

	public static void Update()
	{
		foreach (KeyValuePair<int, BattleInfo> camp in CampList)
		{
			camp.Value.Point += (float)camp.Value.SiteCount * BattleConstData.Instance._points_site;
		}
		SyncBattleInfos();
	}

	public static void SyncBattleInfo(int teamNum)
	{
		if (CampList.ContainsKey(teamNum))
		{
			ChannelNetwork.SyncTeamData(teamNum, EPacketType.PT_InGame_BattleInfo, CampList[teamNum]);
		}
	}

	public static void SyncBattleInfo(uLink.NetworkPlayer peer, int teamNum)
	{
		if (CampList.ContainsKey(teamNum))
		{
			ChannelNetwork.SyncChannelPeer(peer, EPacketType.PT_InGame_BattleInfo, CampList[teamNum]);
		}
	}

	public static void SyncBattleInfos()
	{
		ChannelNetwork.SyncPlayerChannel(EPacketType.PT_InGame_BattleInfos, CampList.Values.ToArray(), false);
	}

	public static void IsGameOver(int teamNum)
	{
		if (!CampList.ContainsKey(teamNum) || !CampList[teamNum].IsBattleOver() || IsBattleOver)
		{
			return;
		}
		List<Player> list = ObjNetInterface.Get<Player>();
		foreach (Player item in list)
		{
			ActionEventsMgr._self.ProcessAction(OperatorEnum.Oper_Battle, ActionOpportunity.Opp_OnDeath, item, teamNum);
		}
		IsBattleOver = true;
		ChannelNetwork.SyncPlayerChannel(EPacketType.PT_InGame_BattleOver, teamNum);
	}

	public static void OnDeath(int teamNum, bool bKill)
	{
		if (CampList.ContainsKey(teamNum))
		{
			CampList[teamNum].DeathCount++;
		}
	}

	public static void OnKill(int teamNum)
	{
		if (CampList.ContainsKey(teamNum))
		{
			CampList[teamNum].KillCount++;
		}
		IsGameOver(teamNum);
	}

	public static void OnOccupy(int teamNum)
	{
		if (CampList.ContainsKey(teamNum))
		{
			CampList[teamNum].SiteCount++;
			CampList[teamNum].Point += BattleConstData.Instance._points_capture;
		}
		IsGameOver(teamNum);
	}

	public static void OnBattleMoney(int teamNum, int money)
	{
		if (CampList.ContainsKey(teamNum))
		{
			CampList[teamNum].Money += money;
		}
	}

	public static void OnBattlePoint(int teamNum, float point)
	{
		if (CampList.ContainsKey(teamNum))
		{
			CampList[teamNum].Point += point;
		}
		IsGameOver(teamNum);
	}

	internal static void OnGetPoint(int teamNum, float point)
	{
		if (CampList.ContainsKey(teamNum))
		{
			CampList[teamNum].Point += point;
		}
		IsGameOver(teamNum);
	}
}
