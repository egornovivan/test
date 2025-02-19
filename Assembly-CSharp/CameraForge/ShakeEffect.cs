using System;
using UnityEngine;

namespace CameraForge;

public class ShakeEffect : ScriptModifier
{
	private Slot Enabled;

	private Slot YPRScale;

	private Slot YPROmega;

	private Slot YPRPhi;

	private Slot OffsetScale;

	private Slot OffsetOmega;

	private Slot OffsetPhi;

	private Slot Tick;

	private Slot Duration;

	private Slot Falloff;

	public override Slot[] slots => new Slot[12]
	{
		Name, Col, Enabled, YPRScale, YPROmega, YPRPhi, OffsetScale, OffsetOmega, OffsetPhi, Tick,
		Duration, Falloff
	};

	public override PoseSlot[] poseslots => new PoseSlot[1] { Prev };

	public ShakeEffect()
	{
		Enabled = new Slot("Enabled");
		Enabled.value = false;
		YPRScale = new Slot("YPRScale");
		YPRScale.value = Vector3.zero;
		YPROmega = new Slot("YPROmega");
		YPROmega.value = Vector3.one;
		YPRPhi = new Slot("YPRPhi");
		YPRPhi.value = Vector3.zero;
		OffsetScale = new Slot("OffsetScale");
		OffsetScale.value = Vector3.zero;
		OffsetOmega = new Slot("OffsetOmega");
		OffsetOmega.value = Vector3.one;
		OffsetPhi = new Slot("OffsetPhi");
		OffsetPhi.value = Vector3.zero;
		Tick = new Slot("Time");
		Duration = new Slot("Duration");
		Falloff = new Slot("Falloff");
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
		YPRScale.Calculate();
		YPROmega.Calculate();
		YPRPhi.Calculate();
		OffsetScale.Calculate();
		OffsetOmega.Calculate();
		OffsetPhi.Calculate();
		Tick.Calculate();
		Duration.Calculate();
		Falloff.Calculate();
		if (controller != null && controller.executor != null)
		{
			Pose value = Prev.value;
			float value_f = Tick.value.value_f;
			float num = Mathf.Max(0.001f, Duration.value.value_f);
			float p = Mathf.Max(0.001f, Falloff.value.value_f);
			if (value_f < num && value_f >= 0f)
			{
				float num2 = Mathf.Pow(1f - value_f / num, p);
				value.yaw += Mathf.Sin((YPROmega.value.value_v.x * value_f + YPRPhi.value.value_v.x) * 2f * (float)Math.PI) * YPRScale.value.value_v.x * num2;
				value.pitch += Mathf.Sin((YPROmega.value.value_v.y * value_f + YPRPhi.value.value_v.y) * 2f * (float)Math.PI) * YPRScale.value.value_v.y * num2;
				value.roll += Mathf.Sin((YPROmega.value.value_v.z * value_f + YPRPhi.value.value_v.z) * 2f * (float)Math.PI) * YPRScale.value.value_v.z * num2;
				Vector3 vector = value.rotation * Vector3.right;
				Vector3 vector2 = value.rotation * Vector3.up;
				Vector3 vector3 = value.rotation * Vector3.forward;
				Vector3 zero = Vector3.zero;
				zero += vector * Mathf.Sin((OffsetOmega.value.value_v.x * value_f + OffsetPhi.value.value_v.x) * 2f * (float)Math.PI) * OffsetScale.value.value_v.x * num2;
				zero += vector2 * Mathf.Sin((OffsetOmega.value.value_v.y * value_f + OffsetPhi.value.value_v.y) * 2f * (float)Math.PI) * OffsetScale.value.value_v.y * num2;
				zero += vector3 * Mathf.Sin((OffsetOmega.value.value_v.z * value_f + OffsetPhi.value.value_v.z) * 2f * (float)Math.PI) * OffsetScale.value.value_v.z * num2;
				value.position += zero;
				return value;
			}
			return value;
		}
		return Pose.Default;
	}
}
