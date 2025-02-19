namespace CameraForge;

public class GetUserPose : MediaPoseNode
{
	public Slot V;

	public override PoseSlot[] poseslots => new PoseSlot[0];

	public override Slot[] slots => new Slot[1] { V };

	public GetUserPose()
	{
		V = new Slot("Var");
	}

	public override Pose Calculate()
	{
		V.Calculate();
		if (V.value.isNull)
		{
			return Pose.Default;
		}
		string text = V.value.value_str.Trim();
		if (string.IsNullOrEmpty(text))
		{
			return Pose.Default;
		}
		if (controller == null)
		{
			return Pose.Default;
		}
		if (controller.executor == null)
		{
			return Pose.Default;
		}
		return controller.executor.GetPose(text);
	}
}
