namespace PeCustom;

public class TickMgr
{
	public delegate void DIntNotify(int t);

	public int tick;

	public event DIntNotify OnTick;

	public void Tick()
	{
		if (this.OnTick != null)
		{
			this.OnTick(tick);
		}
		tick++;
	}
}
