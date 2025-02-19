namespace PeCustom;

public class MissionGoal_Bool : MissionGoal
{
	private bool _achieved;

	public override bool achieved
	{
		get
		{
			return _achieved;
		}
		set
		{
			_achieved = value;
		}
	}
}
