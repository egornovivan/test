using System.Collections.Generic;

public class Room
{
	private List<Player> _PlayerList = new List<Player>();

	public void AddPlayer(Player player)
	{
		if (!_PlayerList.Contains(player))
		{
			_PlayerList.Add(player);
		}
	}

	public void RemovePlayer(Player player)
	{
		_PlayerList.Remove(player);
	}
}
