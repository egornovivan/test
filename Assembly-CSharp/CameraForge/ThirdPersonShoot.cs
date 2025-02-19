using System;
using InControl;
using UnityEngine;

namespace CameraForge;

public class ThirdPersonShoot : ScriptModifier
{
	public Slot Sens;

	public Slot Damp;

	public Slot LockCursor;

	public Slot PitchMin;

	public Slot PitchMax;

	public Slot OffsetUp;

	public Slot Offset;

	public Slot OffsetDown;

	public override Slot[] slots => new Slot[10] { Name, Col, Sens, Damp, LockCursor, PitchMin, PitchMax, OffsetUp, Offset, OffsetDown };

	public override PoseSlot[] poseslots => new PoseSlot[1] { Prev };

	public ThirdPersonShoot()
	{
		Sens = new Slot("Sensitivity");
		Damp = new Slot("Damp Rate");
		LockCursor = new Slot("Lock Cursor");
		LockCursor.value = true;
		PitchMax = new Slot("Pitch Max");
		PitchMax.value = 55f;
		PitchMin = new Slot("Pitch Min");
		PitchMin.value = -70f;
		Offset = new Slot("Offset");
		Offset.value = Vector3.zero;
		OffsetUp = new Slot("Offset Up");
		OffsetUp.value = Vector3.zero;
		OffsetDown = new Slot("Offset Down");
		OffsetDown.value = Vector3.zero;
	}

	public override Pose Calculate()
	{
		Col.Calculate();
		Sens.Calculate();
		Damp.Calculate();
		LockCursor.Calculate();
		PitchMax.Calculate();
		PitchMin.Calculate();
		Offset.Calculate();
		OffsetUp.Calculate();
		OffsetDown.Calculate();
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
			Transform transform3 = CameraController.GetTransform("Bone Neck M");
			Transform transform4 = CameraController.GetTransform("Bone Neck R");
			bool @bool = GetBool("Is Ragdoll");
			float value_f = Sens.value.value_f;
			bool value_b = InputModule.Axis("Mouse Right Button").value_b;
			float value_f2 = InputModule.Axis("Mouse X").value_f;
			float value_f3 = InputModule.Axis("Mouse Y").value_f;
			float num = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickX * 25f * Time.deltaTime));
			float num2 = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickY * 12f * Time.deltaTime));
			bool bool2 = GetBool("Inverse X");
			bool bool3 = GetBool("Inverse Y");
			float value_f4 = PitchMax.value.value_f;
			float value_f5 = PitchMin.value.value_f;
			float t = Mathf.Clamp(Damp.value.value_f, 0.005f, 1f);
			Vector3 zero = Vector3.zero;
			zero = ((transform2 != null) ? ((!(transform3 != null) || !(transform4 != null)) ? transform2.position : Vector3.Lerp(transform2.position, ((!@bool) ? transform3 : transform4).position, 0.4f)) : ((!(transform != null)) ? vector : transform.position));
			float num3 = 0f;
			float num4 = 0f;
			if (Prev.value.lockCursor || value_b)
			{
				num3 = Mathf.Clamp(value_f2 * value_f * ((!bool2) ? 1f : (-1f)), -200f, 200f) * Time.timeScale;
				num4 = Mathf.Clamp(value_f3 * value_f * ((!bool3) ? 1f : (-1f)), -200f, 200f) * Time.timeScale;
			}
			float num5 = 0f;
			float num6 = 0f;
			num5 = num * value_f * ((!bool2) ? 1f : (-1f)) * Time.timeScale;
			num6 = num2 * value_f * ((!bool3) ? 1f : (-1f)) * Time.timeScale;
			num3 += num5;
			num4 += num6;
			num3 = Mathf.Clamp(num3, -6f * value_f, 6f * value_f);
			num4 = Mathf.Clamp(num4, -3f * value_f, 3f * value_f);
			float3 += num3;
			float4 += num4;
			float3 = Mathf.Repeat(float3, 360f);
			float4 = Mathf.Clamp(float4, value_f5, value_f4);
			@float = Mathf.LerpAngle(@float, float3, t);
			float2 = Mathf.LerpAngle(float2, float4, t);
			Pose value = Prev.value;
			value.eulerAngles = new Vector3(0f - float2, @float, 0f);
			Vector3 vector2 = value.rotation * Vector3.forward;
			Vector3 vector3 = value.rotation * Vector3.right;
			Vector3 vector4 = value.rotation * Vector3.up;
			Vector3 zero2 = Vector3.zero;
			zero2 = ((!(float2 < 0f)) ? Vector3.Slerp(Offset.value.value_v, OffsetUp.value.value_v, float2 / value_f4) : Vector3.Slerp(Offset.value.value_v, OffsetDown.value.value_v, float2 / value_f5));
			Vector3 vector5 = zero2.x * vector3 + zero2.y * vector4 + zero2.z * vector2;
			float aspect = controller.executor.camera.aspect;
			float num7 = Mathf.Sqrt(1f + aspect * aspect) * Mathf.Tan(value.fov * 0.5f * ((float)Math.PI / 180f));
			LayerMask layerMask = controller.executor.GetVar("Obstacle LayerMask").value_i;
			float num8 = Utils.EvaluateNearclipPlaneRadius(zero, 0.05f, value.nearClip * num7, layerMask);
			value.nearClip = num8 / num7;
			if (Physics.SphereCast(new Ray(zero, vector5.normalized), num8 - 0.01f, out var hitInfo, vector5.magnitude, layerMask, QueryTriggerInteraction.Ignore))
			{
				vector5 = vector5.normalized * hitInfo.distance;
			}
			value.position = zero + vector5;
			value.lockCursor = LockCursor.value.value_b;
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
