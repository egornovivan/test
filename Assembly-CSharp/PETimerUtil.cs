public static class PETimerUtil
{
	private static PETimer _timer = new PETimer();

	public static PETimer GetTmpTimer()
	{
		_timer.Reset();
		return _timer;
	}
}
