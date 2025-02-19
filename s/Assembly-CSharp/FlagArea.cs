public class FlagArea
{
	public const int FlagLv1 = 376;

	public const int FlagLv2 = 377;

	public const int FlagLv3 = 378;

	protected int _teamId;

	protected int _index;

	protected int _flagLv;

	public int TeamId => _teamId;

	public int Index => _index;

	public int FlagLv => _flagLv;

	public FlagArea(int index, int teamId)
	{
		_index = index;
		_teamId = teamId;
	}

	public void SetLevel(int lvl)
	{
		_flagLv = lvl;
	}
}
