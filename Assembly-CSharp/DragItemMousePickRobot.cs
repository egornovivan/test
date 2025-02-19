using WhiteCat;

public class DragItemMousePickRobot : DragItemMousePickCreation
{
	private RobotController _controller;

	protected override string tipsText => base.tipsText;

	public void Init(RobotController controller)
	{
		_controller = controller;
	}

	public override bool CanCmd()
	{
		if (!base.CanCmd())
		{
			return false;
		}
		return !_controller.isActive;
	}

	protected override void InitCmd(CmdList cmdList)
	{
		cmdList.Add("Get", OnGetBtn);
	}
}
