using WhiteCat;

public class DragItemMousePickCreationSimpleObject : DragItemMousePickCreation
{
	private VCSimpleObjectPart mSimpleObjPart;

	public MousePicker.EPriority overridePriority = MousePicker.EPriority.Level2;

	private VCSimpleObjectPart simpleObjPart
	{
		get
		{
			if (mSimpleObjPart == null)
			{
				mSimpleObjPart = GetComponent<VCSimpleObjectPart>();
			}
			return mSimpleObjPart;
		}
	}

	protected override string tipsText
	{
		get
		{
			CmdList cmdList = simpleObjPart.GetCmdList();
			if (cmdList.count <= 0)
			{
				return string.Empty;
			}
			CmdList.Cmd cmd = cmdList.Get(0);
			if (cmd == null)
			{
				return string.Empty;
			}
			return cmd.name + " - [5CB0FF]E[-]";
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.priority = overridePriority;
	}

	protected override void InitCmd(CmdList cmdList)
	{
		if (simpleObjPart.CanRotateObject())
		{
			cmdList.Add("Turn", Turn90Degree);
		}
		if (simpleObjPart.CanRecycle())
		{
			cmdList.Add("Get", OnGetBtn);
		}
		CmdList cmdList2 = simpleObjPart.GetCmdList();
		for (int i = 0; i < cmdList2.count; i++)
		{
			cmdList.Add(cmdList2.Get(i));
		}
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd())
		{
			CmdList cmdList = simpleObjPart.GetCmdList();
			if (cmdList.count > 0)
			{
				cmdList.Get(0)?.exe();
			}
		}
	}
}
