using UnityEngine;

namespace CameraForge;

[Menu("Yaw Pitch Roll", 0)]
public class YPROutput : OutputNode
{
	public Slot Yaw;

	public Slot Pitch;

	public Slot Roll;

	public Slot Distance;

	public Slot Target;

	public override Slot[] slots => new Slot[5] { Yaw, Pitch, Roll, Distance, Target };

	public YPROutput()
	{
		Yaw = new Slot("Yaw");
		Pitch = new Slot("Pitch");
		Roll = new Slot("Roll");
		Distance = new Slot("Distance");
		Target = new Slot("Target");
	}

	public override Pose Output()
	{
		Yaw.Calculate();
		Pitch.Calculate();
		Roll.Calculate();
		Distance.Calculate();
		Target.Calculate();
		float y = 0f;
		float num = 0f;
		float z = 0f;
		float num2 = 0f;
		Vector3 vector = Vector3.zero;
		Pose pose = Pose.Default;
		if (modifier != null)
		{
			pose = modifier.Prev.value;
			y = pose.yaw;
			num = pose.pitch;
			z = pose.roll;
			num2 = 0f;
			vector = pose.position;
		}
		if (!Yaw.value.isNull)
		{
			y = Yaw.value.value_f;
		}
		if (!Pitch.value.isNull)
		{
			num = Pitch.value.value_f;
		}
		if (!Roll.value.isNull)
		{
			z = Roll.value.value_f;
		}
		if (!Distance.value.isNull)
		{
			num2 = Distance.value.value_f;
		}
		if (!Target.value.isNull)
		{
			vector = Target.value.value_v;
		}
		pose.eulerAngles = new Vector3(0f - num, y, z);
		Vector3 vector2 = pose.rotation * Vector3.forward;
		pose.position = vector - num2 * vector2;
		return pose;
	}
}
