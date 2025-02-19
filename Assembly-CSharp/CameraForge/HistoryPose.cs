using UnityEngine;

namespace CameraForge;

public class HistoryPose : PoseNode
{
	public Slot Index;

	public override PoseSlot[] poseslots => new PoseSlot[0];

	public override Slot[] slots => new Slot[1] { Index };

	public HistoryPose()
	{
		Index = new Slot("Index");
		Index.value = 0;
	}

	public override Pose Calculate()
	{
		Index.Calculate();
		int index = Mathf.RoundToInt(Index.value.value_f);
		if (controller == null)
		{
			return Pose.Default;
		}
		if (controller.executor == null)
		{
			return Pose.Default;
		}
		return controller.executor.GetHistoryPose(index);
	}
}
