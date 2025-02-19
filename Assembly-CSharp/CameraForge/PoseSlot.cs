namespace CameraForge;

public class PoseSlot
{
	public string name;

	public PoseNode input;

	public Pose value;

	public PoseSlot(string _name)
	{
		name = _name;
		input = null;
		value = Pose.Default;
	}

	public void Calculate()
	{
		if (input != null)
		{
			value = input.Calculate();
		}
	}
}
