using System.Collections.Generic;

public class TeamData
{
	private int _teamId = -1;

	private int _leaderId = -1;

	public List<PlayerNetwork> Members = new List<PlayerNetwork>();

	public int LeaderId => _leaderId;

	public int TeamId => _teamId;

	public TeamData(int teamId)
	{
		_teamId = teamId;
	}

	public void SetLeader(int leaderId)
	{
		_leaderId = leaderId;
	}

	public void AddMember(PlayerNetwork player)
	{
		if (!Members.Contains(player))
		{
			Members.Add(player);
		}
	}

	public void RemoveMember(PlayerNetwork player)
	{
		if (Members.Contains(player))
		{
			Members.Remove(player);
		}
	}

	public void Reset()
	{
		_teamId = -1;
		_leaderId = -1;
		Members.Clear();
	}
}
