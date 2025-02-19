using System.Collections.Generic;
using ItemAsset;

public class GroupNetwork
{
	internal const int minTeamID = 10000;

	internal const int maxTeamID = 19999;

	private static ItemPackage _itemPackage = new ItemPackage(200);

	private static List<TeamData> _teamInfo = new List<TeamData>();

	private static List<PlayerNetwork> joinReqeust = new List<PlayerNetwork>();

	public static SlotList GetSlotList(ItemPackage.ESlotType type)
	{
		return _itemPackage.GetSlotList(type);
	}

	public static bool TeamExists(int teamId)
	{
		return _teamInfo.Exists((TeamData iter) => iter.TeamId == teamId);
	}

	private static TeamData NewTeam(int teamId)
	{
		TeamData teamData = new TeamData(teamId);
		_teamInfo.Add(teamData);
		return teamData;
	}

	public static void AddJoinRequest(int teamId, PlayerNetwork player)
	{
		if (!joinReqeust.Contains(player))
		{
			joinReqeust.Add(player);
		}
	}

	public static void GetJoinRequest(List<PlayerNetwork> players)
	{
		for (int i = 0; i < joinReqeust.Count; i++)
		{
			if (!players.Contains(joinReqeust[i]))
			{
				players.Add(joinReqeust[i]);
			}
		}
	}

	public static bool IsJoinRequest(PlayerNetwork player)
	{
		return !(null == player) && joinReqeust.Contains(player);
	}

	public static void DelJoinRequest(PlayerNetwork player)
	{
		if (joinReqeust.Contains(player))
		{
			joinReqeust.Remove(player);
		}
	}

	public static void ClearJoinRequest()
	{
		joinReqeust.Clear();
	}

	public static TeamData AddToTeam(int teamId, PlayerNetwork player)
	{
		if (teamId == -1 || null == player)
		{
			return null;
		}
		TeamData teamData = _teamInfo.Find((TeamData iter) => iter.TeamId == teamId);
		if (teamData == null)
		{
			teamData = NewTeam(teamId);
		}
		teamData.AddMember(player);
		return teamData;
	}

	public static void RemoveFromTeam(int teamId, PlayerNetwork player)
	{
		_teamInfo.Find((TeamData iter) => iter.TeamId == teamId)?.RemoveMember(player);
	}

	public static void GetMembers(int teamId, ref List<PlayerNetwork> members)
	{
		TeamData teamData = _teamInfo.Find((TeamData iter) => iter.TeamId == teamId);
		if (teamData != null)
		{
			members.AddRange(teamData.Members);
		}
	}

	public static int GetLeaderId(int teamId)
	{
		return _teamInfo.Find((TeamData iter) => iter.TeamId == teamId)?.LeaderId ?? (-1);
	}

	public static bool IsEmpty(int teamId)
	{
		int num = _teamInfo.FindIndex((TeamData iter) => iter.TeamId == teamId);
		return num != -1 && _teamInfo[num].Members.Count <= 1;
	}

	public static TeamData GetTeamInfo(int teamId)
	{
		return _teamInfo.Find((TeamData iter) => iter.TeamId == teamId);
	}

	public static TeamData[] GetTeamInfos()
	{
		return _teamInfo.ToArray();
	}
}
