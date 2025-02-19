namespace CameraForge;

[Menu("Cone Params", 3)]
public class ConeOutput : OutputNode
{
	public Slot Fov;

	public Slot Near;

	public override Slot[] slots => new Slot[2] { Fov, Near };

	public ConeOutput()
	{
		Fov = new Slot("Fov");
		Near = new Slot("Nearclip");
	}

	public override Pose Output()
	{
		Fov.Calculate();
		Near.Calculate();
		Pose pose = Pose.Default;
		if (modifier != null)
		{
			pose = modifier.Prev.value;
		}
		if (!Fov.value.isNull)
		{
			pose.fov = Fov.value.value_f;
		}
		if (!Near.value.isNull)
		{
			pose.nearClip = Near.value.value_f;
		}
		return pose;
	}
}
