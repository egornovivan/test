public class GroupNetInterface : ObjNetInterface
{
	protected int _teamId;

	protected int _worldId;

	public int TeamId => _teamId;

	public int WorldId => _worldId;

	public virtual void SetTeamId(int teamId)
	{
		_teamId = teamId;
	}

	protected virtual void OnPlayerDisconnect(Player player)
	{
	}
}
