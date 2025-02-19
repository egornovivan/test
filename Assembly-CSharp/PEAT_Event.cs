public class PEAT_Event : PEAbnormalTrigger
{
	protected bool validEvent { get; set; }

	public override bool Hit()
	{
		if (validEvent)
		{
			validEvent = false;
			return true;
		}
		return base.Hit();
	}

	public void OnEvent()
	{
		validEvent = true;
	}

	public void OnEvent(int value)
	{
		OnEvent();
	}

	public void OnEvent(float value)
	{
		OnEvent();
	}

	public override void Update()
	{
		validEvent = false;
	}
}
