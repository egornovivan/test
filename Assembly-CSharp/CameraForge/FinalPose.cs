namespace CameraForge;

public class FinalPose : PoseNode
{
	public PoseSlot Final;

	public override PoseSlot[] poseslots => new PoseSlot[1] { Final };

	public override Slot[] slots => new Slot[0];

	public FinalPose()
	{
		Final = new PoseSlot("Final");
	}

	public override Pose Calculate()
	{
		Final.Calculate();
		return Final.value;
	}
}
