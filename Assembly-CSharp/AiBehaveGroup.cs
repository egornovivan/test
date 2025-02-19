public class AiBehaveGroup : AiBehave
{
	private SPGroup mAiGroup;

	public SPGroup aiGroup => mAiGroup;

	public override bool isPause
	{
		set
		{
			base.isPause = value;
			if (value && mAiGroup != null)
			{
				mAiGroup.ClearMoveAndRotation();
			}
		}
	}

	public override bool isGroup => true;

	public override bool isActive => base.isActive && mAiGroup != null;

	public void RegisterSPGroup(SPGroup spGroup)
	{
		mAiGroup = spGroup;
	}
}
