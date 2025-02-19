public class DragItemMousePickDoor : DragItemMousePick
{
	private ItemScript_Door mDoor;

	private ItemScript_Door door
	{
		get
		{
			if (mDoor == null)
			{
				mDoor = GetComponent<ItemScript_Door>();
			}
			return mDoor;
		}
	}

	protected override string tipsText
	{
		get
		{
			if (door.IsOpen)
			{
				return base.tipsText + "\n" + PELocalization.GetString(8000127);
			}
			return base.tipsText + "\n" + PELocalization.GetString(8000126);
		}
	}

	protected override void InitCmd(CmdList cmdList)
	{
		base.InitCmd(cmdList);
		if (door.IsOpen)
		{
			cmdList.Add("Shut", ShutDoor);
		}
		else
		{
			cmdList.Add("Open", OpenDoor);
		}
	}

	private void OpenDoor()
	{
		HideItemOpGui();
		door.OpenDoor();
	}

	private void ShutDoor()
	{
		HideItemOpGui();
		door.ShutDoor();
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd())
		{
			if (door.IsOpen)
			{
				ShutDoor();
			}
			else
			{
				OpenDoor();
			}
		}
	}
}
