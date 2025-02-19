using UnityEngine;

namespace CameraForge;

[Menu("Shoot Params", 2)]
public class ShootOutput : OutputNode
{
	public Slot Fov;

	public Slot Lock;

	public Slot CP;

	public override Slot[] slots => new Slot[3] { Fov, Lock, CP };

	public ShootOutput()
	{
		Fov = new Slot("Fov");
		Lock = new Slot("Lock Cursor");
		CP = new Slot("Cursor Position");
	}

	public override Pose Output()
	{
		Fov.Calculate();
		Lock.Calculate();
		CP.Calculate();
		Pose pose = Pose.Default;
		if (modifier != null)
		{
			pose = modifier.Prev.value;
		}
		if (!Fov.value.isNull)
		{
			pose.fov = Fov.value.value_f;
		}
		if (!Lock.value.isNull)
		{
			pose.lockCursor = Lock.value.value_b;
		}
		if (!CP.value.isNull)
		{
			pose.cursorPos = new Vector2(CP.value.value_v.x, CP.value.value_v.y);
		}
		return pose;
	}
}
