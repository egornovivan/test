using System;

namespace Pathea;

public class SleepCooperation : Cooperation
{
	private double mStartHour;

	public SleepCooperation(int memNum, double _startHour)
		: base(memNum)
	{
		mStartHour = _startHour;
	}

	public override void DissolveCooper()
	{
		throw new NotImplementedException();
	}

	public bool IsTimeout()
	{
		return GameTime.Timer.Hour - mStartHour > (double)CSNpcTeam.Sleep_ALl_Time;
	}
}
