using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using uLink;
using UnityEngine;

public class GroupNetwork : UnityEngine.MonoBehaviour
{
	internal const int minTeamID = 10000;

	internal const int maxTeamID = 19999;

	private static int _newTeamID = 10000;

	private static List<TeamData> TeamList = new List<TeamData>();

	private static List<uLink.NetworkPlayer> Members = new List<uLink.NetworkPlayer>();

	internal static int CurNewTeamId => _newTeamID;

	internal static int NewTeamID => ++_newTeamID;

	internal static void SetCurTeamId(int teamId)
	{
		_newTeamID = teamId;
	}

	public static void RemoveFromTeam(int teamId, int id)
	{
		TeamData teamData = TeamList.Find((TeamData iter) => iter.TeamID == teamId);
		if (teamData != null)
		{
			teamData.RemoveMember(id);
			if (id == teamData.LeaderID)
			{
				DissolveTeam(teamId);
			}
		}
	}

	public static void InitTeam(int maxNum)
	{
		if (!ServerConfig.IsSurvive)
		{
			int num = (ServerConfig.IsCooperation ? 1 : 10001);
			for (int i = 0; i < maxNum; i++)
			{
				if (!IsTeamExisted(num))
				{
					TeamData teamData = new TeamData(num, ServerConfig.NumPerTeam);
					teamData.InitTeamData();
					TeamList.Add(teamData);
				}
				num++;
			}
		}
		else
		{
			for (int j = 10001; j <= CurNewTeamId; j++)
			{
				TeamData teamData2 = new TeamData(j, ServerConfig.NumPerTeam);
				teamData2.InitTeamData();
				TeamList.Add(teamData2);
			}
		}
	}

	public static int InitTeam()
	{
		ForceSetting.InitCustomHumanForces();
		int num = 0;
		List<PlayerDesc> humanPlayers = ForceSetting.humanPlayers;
		List<ForceDesc> humanForces = ForceSetting.humanForces;
		ForceDesc force;
		foreach (ForceDesc item in humanForces)
		{
			force = item;
			int num2 = humanPlayers.Count((PlayerDesc iter) => iter.Force == force.ID);
			num2 += force.JoinablePlayerCount;
			num += num2;
			if (!IsTeamExisted(force.ID))
			{
				TeamData teamData = new TeamData(force.ID, num2);
				teamData.InitTeamData();
				TeamList.Add(teamData);
			}
		}
		return num;
	}

	public static bool CanJoin(int teamID)
	{
		int num = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamID);
		return num != -1 && !TeamList[num].IsFull;
	}

	public static bool IsEmpty(int teamId)
	{
		int num = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamId);
		return num != -1 && TeamList[num].MemberNum <= 1;
	}

	public static int CanJoin()
	{
		foreach (TeamData team in TeamList)
		{
			if (!team.IsFull)
			{
				return team.TeamID;
			}
		}
		return -1;
	}

	public static TeamData GetTeamData(int teamId)
	{
		return TeamList.Find((TeamData iter) => iter.TeamID == teamId);
	}

	public static int NewTeam(int leaderId, uLink.NetworkPlayer peer)
	{
		TeamData teamData = new TeamData(NewTeamID, ServerConfig.NumPerTeam);
		teamData.InitTeamData();
		TeamList.Add(teamData);
		teamData.SetLeader(leaderId);
		teamData.AddMember(leaderId, peer);
		return teamData.TeamID;
	}

	public static void NewSurviveTeam(int teamId, int leaderId, uLink.NetworkPlayer peer)
	{
		TeamData teamData = TeamList.Find((TeamData iter) => iter.TeamID == teamId);
		if (teamData == null)
		{
			teamData = new TeamData(teamId, ServerConfig.NumPerTeam);
			teamData.InitTeamData();
			TeamList.Add(teamData);
		}
		teamData.SetLeader(leaderId);
		teamData.AddMember(leaderId, peer);
	}

	public static void DissolveTeam(int teamId)
	{
		TeamData td = TeamList.Find((TeamData iter) => iter.TeamID == teamId);
		if (td == null || td.MemberNum == 0)
		{
			return;
		}
		td.MemberAction(delegate(MemberData md)
		{
			if (md != null && md.Id != td.LeaderID)
			{
				Player player = Player.GetPlayer(md.Id);
				if (null != player)
				{
					player.ResetTeamId();
					player.SyncTeamInfo();
				}
				td.RemoveMember(md.Id);
			}
		});
	}

	public static void LeaderDeliver(int teamId, int id)
	{
		TeamData td = TeamList.Find((TeamData iter) => iter.TeamID == teamId);
		if (td == null)
		{
			return;
		}
		Player player = Player.GetPlayer(id);
		if (null == player)
		{
			return;
		}
		int leaderTeamId = player.originTeamId;
		td.MemberAction(delegate(MemberData md)
		{
			if (md != null)
			{
				int num = AddToTeam(md.Id, md.Peer, leaderTeamId);
				if (num != -1)
				{
					Player player2 = Player.GetPlayer(md.Id);
					if (null != player2)
					{
						player2.SetTeamId(leaderTeamId);
						player2.SyncTeamInfo();
					}
					td.RemoveMember(md.Id);
				}
			}
		});
	}

	public static int GetLeaderID(int teamId)
	{
		return TeamList.Find((TeamData iter) => iter.TeamID == teamId)?.LeaderID ?? (-1);
	}

	public static bool IsLeaderID(int teamId, int id)
	{
		TeamData teamData = TeamList.Find((TeamData iter) => iter.TeamID == teamId);
		if (teamData == null)
		{
			return false;
		}
		return teamData.LeaderID == id;
	}

	public static bool IsMember(int teamId, int id)
	{
		return TeamList.Find((TeamData iter) => iter.TeamID == teamId)?.IsMember(id) ?? false;
	}

	public static bool IsTeamExisted(int teamID)
	{
		return TeamList.Exists((TeamData iter) => iter.TeamID == teamID);
	}

	public static int AddToTeam(int id, uLink.NetworkPlayer peer)
	{
		foreach (TeamData team in TeamList)
		{
			if (team.AddMember(id, peer))
			{
				return team.TeamID;
			}
		}
		return -1;
	}

	public static int AddToTeam(int id, uLink.NetworkPlayer peer, int destTeam)
	{
		if (destTeam == -1)
		{
			return -1;
		}
		TeamData teamData = TeamList.Find((TeamData iter) => iter.TeamID == destTeam);
		if (teamData == null)
		{
			return -1;
		}
		if (!teamData.AddMember(id, peer))
		{
			return -1;
		}
		return destTeam;
	}

	public static void SyncGroup(NetInterface net, params object[] args)
	{
		GroupNetInterface groupNetInterface = net as GroupNetInterface;
		if (null != groupNetInterface)
		{
			List<uLink.NetworkPlayer> members = GetMembers(groupNetInterface.TeamId);
			groupNetInterface.RPCPeers(members, args);
		}
	}

	public static void SyncGroup(NetInterface net, int teamId, params object[] args)
	{
		if (!(null == net))
		{
			List<uLink.NetworkPlayer> members = GetMembers(teamId);
			net.RPCPeers(members, args);
		}
	}

	public static List<uLink.NetworkPlayer> GetMembers(int teamId)
	{
		Members.Clear();
		int num = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamId);
		if (num != -1)
		{
			TeamList[num].GetMembers(Members);
		}
		return Members;
	}

	public static bool GetPackageIDs(int teamId, int tab, out int[] ids)
	{
		int num = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamId);
		if (num == -1)
		{
			ids = null;
			return false;
		}
		ids = TeamList[num].GetItemIDs(tab).ToArray();
		return true;
	}

	public static void Store(Player player, ItemObject itemObj, int storageIndex)
	{
		int teamId = player.TeamId;
		int num = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamId);
		if (num != -1)
		{
			TeamList[num].Store(player, itemObj, storageIndex);
		}
	}

	public static void Change(Player player, ItemObject itemSrc, int srcIndex, int destIndex)
	{
		int teamId = player.TeamId;
		int num = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamId);
		if (num != -1)
		{
			TeamList[num].Change(player, itemSrc, srcIndex, destIndex);
		}
	}

	public static void Fetch(Player player, ItemObject itemObj, int packageIndex)
	{
		int teamId = player.TeamId;
		int num = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamId);
		if (num != -1)
		{
			TeamList[num].Fetch(player, itemObj, packageIndex);
		}
	}

	public static void Delete(Player player, ItemObject itemObj)
	{
		int teamId = player.TeamId;
		int num = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamId);
		if (num != -1)
		{
			TeamList[num].Delete(player, itemObj);
		}
	}

	public static void Split(Player player, ItemObject itemObj, int num)
	{
		int teamId = player.TeamId;
		int num2 = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamId);
		if (num2 != -1)
		{
			TeamList[num2].Split(player, itemObj, num);
		}
	}

	public static void Sort(Player player, int tab)
	{
		int teamId = player.TeamId;
		int num = TeamList.FindIndex((TeamData iter) => iter.TeamID == teamId);
		if (num != -1)
		{
			TeamList[num].Sort(player, tab);
		}
	}
}
