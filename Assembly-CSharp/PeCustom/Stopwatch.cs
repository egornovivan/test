namespace PeCustom;

public class Stopwatch
{
	public string name;

	public UTimer timer;

	public Stopwatch()
	{
		name = string.Empty;
		timer = new UTimer();
		timer.Reset();
	}
}
