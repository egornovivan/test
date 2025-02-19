using InControl;
using UnityEngine;

namespace CameraForge;

public class FreeLook : ScriptModifier
{
	public Slot Sens;

	public Slot Damp;

	public Slot Lock;

	public Slot PitchMin;

	public Slot PitchMax;

	public Slot DistLimit;

	public override Slot[] slots => new Slot[8] { Name, Col, Sens, Damp, Lock, PitchMin, PitchMax, DistLimit };

	public override PoseSlot[] poseslots => new PoseSlot[1] { Prev };

	public FreeLook()
	{
		Sens = new Slot("Sensitivity");
		Damp = new Slot("Damp Rate");
		Lock = new Slot("Lock");
		Lock.value = false;
		PitchMax = new Slot("Pitch Max");
		PitchMax.value = 55f;
		PitchMin = new Slot("Pitch Min");
		PitchMin.value = -70f;
		DistLimit = new Slot("Distance Limit");
		DistLimit.value = 30f;
	}

	public override Pose Calculate()
	{
		Col.Calculate();
		Sens.Calculate();
		Damp.Calculate();
		Lock.Calculate();
		PitchMax.Calculate();
		PitchMin.Calculate();
		DistLimit.Calculate();
		Prev.Calculate();
		if (controller != null && controller.executor != null)
		{
			float @float = GetFloat("Yaw");
			float float2 = GetFloat("Pitch");
			float float3 = GetFloat("YawWanted");
			float float4 = GetFloat("PitchWanted");
			Vector3 vector = CameraController.GetGlobalVar("Default Anchor").value_v;
			Transform transform = CameraController.GetTransform("Anchor");
			Transform transform2 = CameraController.GetTransform("Character");
			float value_f = Sens.value.value_f;
			bool value_b = InputModule.Axis("Mouse Right Button").value_b;
			float value_f2 = InputModule.Axis("Mouse X").value_f;
			float value_f3 = InputModule.Axis("Mouse Y").value_f;
			float num = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickX * 25f * Time.deltaTime));
			float num2 = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickY * 12f * Time.deltaTime));
			float value_f4 = InputModule.Axis("Mouse ScrollWheel").value_f;
			bool @bool = GetBool("Inverse X");
			bool bool2 = GetBool("Inverse Y");
			float value_f5 = PitchMax.value.value_f;
			float value_f6 = PitchMin.value.value_f;
			float t = Mathf.Clamp(Damp.value.value_f, 0.005f, 1f);
			float value_f7 = DistLimit.value.value_f;
			float num3 = Mathf.Clamp(Time.deltaTime, 0.001f, 0.1f);
			Vector3 zero = Vector3.zero;
			zero = ((transform2 != null) ? transform2.position : ((!(transform != null)) ? vector : transform.position));
			float num4 = 0f;
			float num5 = 0f;
			float num6 = value_f4 * 8f;
			if (Prev.value.lockCursor || value_b)
			{
				num4 = Mathf.Clamp(value_f2 * value_f * ((!@bool) ? 1f : (-1f)), -200f, 200f);
				num5 = Mathf.Clamp(value_f3 * value_f * ((!bool2) ? 1f : (-1f)), -200f, 200f);
			}
			float num7 = 0f;
			float num8 = 0f;
			num7 = num * value_f * ((!@bool) ? 1f : (-1f));
			num8 = num2 * value_f * ((!bool2) ? 1f : (-1f));
			num4 += num7;
			num5 += num8;
			float3 += num4;
			float4 += num5;
			float3 = Mathf.Repeat(float3, 360f);
			float4 = Mathf.Clamp(float4, value_f6, value_f5);
			@float = Mathf.LerpAngle(@float, float3, t);
			float2 = Mathf.LerpAngle(float2, float4, t);
			Pose value = Prev.value;
			value.eulerAngles = new Vector3(0f - float2, @float, 0f);
			value.lockCursor = false;
			Vector3 vector2 = value.rotation * Vector3.forward;
			Vector3 vector3 = value.rotation * Vector3.right;
			if (PeInput.Get(PeInput.LogicFunction.MoveForward))
			{
				value.position += vector2 * num3 * 15f;
			}
			if (PeInput.Get(PeInput.LogicFunction.MoveBackward))
			{
				value.position -= vector2 * num3 * 15f;
			}
			if (PeInput.Get(PeInput.LogicFunction.MoveLeft))
			{
				value.position -= vector3 * num3 * 15f;
			}
			if (PeInput.Get(PeInput.LogicFunction.MoveRight))
			{
				value.position += vector3 * num3 * 15f;
			}
			if (num6 != 0f)
			{
				value.position += vector2 * num6;
			}
			Vector3 vector4 = value.position - zero;
			vector4 = Vector3.ClampMagnitude(vector4, value_f7);
			value.position = zero + vector4;
			controller.executor.SetFloat("Yaw", @float);
			controller.executor.SetFloat("Pitch", float2);
			controller.executor.SetFloat("YawWanted", float3);
			controller.executor.SetFloat("PitchWanted", float4);
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
