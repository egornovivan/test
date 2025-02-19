using System;
using UnityEngine;

namespace CameraForge;

public class WaterClamp : ScriptModifier
{
	public override Slot[] slots => new Slot[2] { Name, Col };

	public override PoseSlot[] poseslots => new PoseSlot[1] { Prev };

	public override Pose Calculate()
	{
		Col.Calculate();
		Prev.Calculate();
		if (controller != null && controller.executor != null)
		{
			Pose value = Prev.value;
			float num = value.nearClip / Mathf.Cos(value.fov * 0.5f * ((float)Math.PI / 180f)) + 0.01f;
			if (Physics.Raycast(new Ray(value.position, Vector3.down), out var hitInfo, num, 16))
			{
				float num2 = num - hitInfo.distance;
				value.position += Vector3.up * num2;
			}
			if (Physics.Raycast(new Ray(value.position + Vector3.up * num, Vector3.down), out hitInfo, num, 16))
			{
				float num3 = 0f - hitInfo.distance;
				value.position += Vector3.up * num3;
			}
			return value;
		}
		return Pose.Default;
	}

	private bool GetBool(string name)
	{
		return controller.executor.GetVar(name).value_b;
	}

	private float GetFloat(string name)
	{
		return controller.executor.GetVar(name).value_f;
	}

	private Vector3 GetPosition(string name)
	{
		Transform transform = CameraController.GetTransform(name);
		if (transform == null)
		{
			return Vector3.zero;
		}
		return transform.position;
	}
}
