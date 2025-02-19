public class FollowData
{
	public Player _player;

	public float _x;

	public float _y;

	public int _targetid;

	public int _missionId;

	public FollowData(Player p, float x, float y, int tar, int mission)
	{
		_player = p;
		_x = x;
		_y = y;
		_targetid = tar;
		_missionId = mission;
	}
}
