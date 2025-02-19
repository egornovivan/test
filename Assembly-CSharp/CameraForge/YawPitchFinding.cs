using System;
using UnityEngine;

namespace CameraForge;

public class YawPitchFinding : ScriptModifier
{
	private Vector2 vyp = Vector2.zero;

	private float hideTime;

	private Slot Enabled;

	public override Slot[] slots => new Slot[3] { Name, Col, Enabled };

	public override PoseSlot[] poseslots => new PoseSlot[1] { Prev };

	public YawPitchFinding()
	{
		Enabled = new Slot("Enabled");
		Enabled.value = true;
	}

	public override Pose Calculate()
	{
		Col.Calculate();
		Prev.Calculate();
		Enabled.Calculate();
		if (!Enabled.value.value_b)
		{
			return Prev.value;
		}
		if (controller != null && controller.executor != null)
		{
			float value_f = controller.executor.GetVar("No Rotate Time").value_f;
			if (value_f < 0.3f)
			{
				return Prev.value;
			}
			float value_f2 = controller.executor.GetVar("Dist").value_f;
			LayerMask layerMask = controller.executor.GetVar("Obstacle LayerMask").value_i;
			Pose value = Prev.value;
			Vector3 vector = value.rotation * Vector3.forward;
			Vector3 vector2 = value.position + value_f2 * vector;
			Vector3 vector3 = value.position - vector2;
			float value_f3 = controller.executor.GetVar("Yaw").value_f;
			float value_f4 = controller.executor.GetVar("Pitch").value_f;
			float num = value_f3;
			float num2 = value_f4;
			float num3 = controller.executor.GetVar("NCR").value_f;
			if (num3 < 0.01f)
			{
				num3 = 0.01f;
			}
			float num4 = vector3.magnitude - num3;
			if (num4 < 0f)
			{
				return value;
			}
			if (Physics.SphereCast(new Ray(vector2, vector3.normalized), num3 - 0.01f, num4, layerMask, QueryTriggerInteraction.Ignore))
			{
				hideTime += 0.02f;
				if (hideTime > 0.2f)
				{
					for (float num5 = 5f; num5 < 90.01f; num5 += 5f)
					{
						for (float num6 = 0f; num6 < 180.01f; num6 += 45f)
						{
							num = num5 * Mathf.Cos(num6 * ((float)Math.PI / 180f)) + value_f3;
							num2 = value_f4 - 0.8f * num5 * Mathf.Sin(num6 * ((float)Math.PI / 180f));
							if (!(num2 < -70f))
							{
								Vector3 direction = Quaternion.Euler(0f - num2, num, 0f) * Vector3.back;
								if (!Physics.SphereCast(new Ray(vector2, direction), num3 - 0.01f, num4, layerMask, QueryTriggerInteraction.Ignore))
								{
									Vector2 vector4 = Vector2.SmoothDamp(new Vector2(value_f3, value_f4), new Vector2(num, num2), ref vyp, 0.25f);
									controller.executor.SetFloat("YawWanted", vector4.x);
									controller.executor.SetFloat("PitchWanted", vector4.y);
									return value;
								}
							}
						}
					}
				}
			}
			else
			{
				hideTime = 0f;
			}
			return value;
		}
		return Pose.Default;
	}
}
