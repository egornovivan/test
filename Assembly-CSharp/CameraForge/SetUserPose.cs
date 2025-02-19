namespace CameraForge;

public class SetUserPose : MediaPoseNode
{
	public Slot V;

	public PoseSlot P;

	public override PoseSlot[] poseslots => new PoseSlot[1] { P };

	public override Slot[] slots => new Slot[1] { V };

	public SetUserPose()
	{
		V = new Slot("Var");
		P = new PoseSlot("Pose");
	}

	public override Pose Calculate()
	{
		V.Calculate();
		P.Calculate();
		if (V.value.isNull)
		{
			return P.value;
		}
		string text = V.value.value_str.Trim();
		if (string.IsNullOrEmpty(text))
		{
			return P.value;
		}
		if (controller == null)
		{
			return P.value;
		}
		if (controller.executor == null)
		{
			return P.value;
		}
		controller.executor.SetPose(text, P.value);
		return P.value;
	}
}
