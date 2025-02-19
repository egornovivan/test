using System;

public class DragItemLogicMonsterBeacon : DragItemLogic
{
	public event Action DragItemActivateEvent;

	public override void OnActivate()
	{
		base.OnActivate();
		if (this.DragItemActivateEvent != null)
		{
			this.DragItemActivateEvent();
		}
	}
}
