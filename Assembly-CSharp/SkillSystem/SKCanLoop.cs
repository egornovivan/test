namespace SkillSystem;

public class SKCanLoop
{
	public int _casterId;

	public int _skillId;

	public bool _bLoop;

	public bool _bFailedRecv;

	public void Reset()
	{
		_casterId = 0;
		_skillId = 0;
		_bLoop = false;
		_bFailedRecv = false;
	}
}
