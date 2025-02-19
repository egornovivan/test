public class PETimer : UTimer
{
	public PETimer()
	{
		c_Day2Hour = 26L;
	}

	public override TimeStruct DayToCalendar(int day)
	{
		TimeStruct result = default(TimeStruct);
		result.Date = day;
		return result;
	}

	public override int CalendarToDay(TimeStruct ts)
	{
		return ts.Date;
	}
}
