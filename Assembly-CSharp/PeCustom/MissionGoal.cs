using System;

namespace PeCustom;

public abstract class MissionGoal
{
	public int id;

	public int missionId;

	public string text;

	public Action<int, int> onAchieve;

	private bool _last_achieved;

	public abstract bool achieved { get; set; }

	public void Update()
	{
		if (!_last_achieved && achieved && onAchieve != null)
		{
			onAchieve(id, missionId);
		}
		_last_achieved = achieved;
	}

	public virtual void Init()
	{
	}

	public virtual void Free()
	{
	}
}
