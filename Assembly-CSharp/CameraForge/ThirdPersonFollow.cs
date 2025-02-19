using InControl;
using UnityEngine;

namespace CameraForge;

public class ThirdPersonFollow : ScriptModifier
{
	private float vyaw;

	private float vpitch;

	private Vector3 velUsed = Vector3.zero;

	private float noControlTime;

	private float recalcDistTime;

	private float lockDistAfterClampTime;

	public Slot Sens;

	public Slot Damp;

	public Slot FS;

	public Slot AnimFactor;

	public Slot Lock;

	public Slot DistMin;

	public Slot DistMax;

	public Slot PitchMin;

	public Slot PitchMax;

	public Slot RollCoef;

	public Slot Offset;

	public override Slot[] slots => new Slot[13]
	{
		Name, Col, Sens, FS, AnimFactor, Damp, Lock, DistMin, DistMax, PitchMin,
		PitchMax, RollCoef, Offset
	};

	public override PoseSlot[] poseslots => new PoseSlot[1] { Prev };

	public ThirdPersonFollow()
	{
		Sens = new Slot("Sensitivity");
		Damp = new Slot("Damp Rate");
		FS = new Slot("Follow Speed");
		AnimFactor = new Slot("Animation Factor");
		AnimFactor.value = 0.4f;
		Lock = new Slot("Lock Cursor");
		DistMax = new Slot("Distance Max");
		DistMax.value = 10f;
		DistMin = new Slot("Distance Min");
		DistMin.value = 1.8f;
		PitchMax = new Slot("Pitch Max");
		PitchMax.value = 55f;
		PitchMin = new Slot("Pitch Min");
		PitchMin.value = -70f;
		RollCoef = new Slot("Roll Coef");
		RollCoef.value = 0f;
		Offset = new Slot("Offset");
		Offset.value = Vector3.zero;
	}

	public override Pose Calculate()
	{
		Col.Calculate();
		Sens.Calculate();
		Damp.Calculate();
		FS.Calculate();
		AnimFactor.Calculate();
		Lock.Calculate();
		DistMax.Calculate();
		DistMin.Calculate();
		PitchMax.Calculate();
		PitchMin.Calculate();
		RollCoef.Calculate();
		Offset.Calculate();
		Prev.Calculate();
		if (controller != null && controller.executor != null)
		{
			float @float = GetFloat("Yaw");
			float float2 = GetFloat("Pitch");
			float float3 = GetFloat("Roll");
			float num = GetFloat("Dist");
			float float4 = GetFloat("YawWanted");
			float float5 = GetFloat("PitchWanted");
			float float6 = GetFloat("DistWanted");
			float float7 = GetFloat("DistLevel");
			Vector3 vector = CameraController.GetGlobalVar("Default Anchor").value_v;
			Transform transform = CameraController.GetTransform("Anchor");
			Transform transform2 = CameraController.GetTransform("Character");
			Transform transform3 = CameraController.GetTransform("Bone Neck M");
			Transform transform4 = CameraController.GetTransform("Bone Neck R");
			bool @bool = GetBool("Is Ragdoll");
			Vector3 b = controller.executor.GetVar("Character Velocity").value_v;
			float num2 = FS.value.value_f;
			float value_f = Sens.value.value_f;
			bool value_b = Lock.value.value_b;
			bool flag = InputModule.Axis("Mouse Right Button").value_b && !GetBool("Mouse Op GUI");
			float value_f2 = InputModule.Axis("Mouse X").value_f;
			float value_f3 = InputModule.Axis("Mouse Y").value_f;
			float num3 = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickX * 25f * Time.deltaTime));
			float num4 = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickY * 12f * Time.deltaTime));
			float num5 = ((!GetBool("Mouse On Scroll")) ? InputModule.Axis("Mouse ScrollWheel").value_f : 0f);
			bool bool2 = GetBool("Inverse X");
			bool bool3 = GetBool("Inverse Y");
			float float8 = GetFloat("Activity Space Size");
			bool bool4 = GetBool("Geometry Clampd");
			float value_f4 = DistMax.value.value_f;
			float value_f5 = DistMin.value.value_f;
			float value_f6 = PitchMax.value.value_f;
			float value_f7 = PitchMin.value.value_f;
			float value_f8 = RollCoef.value.value_f;
			float t = Mathf.Clamp(Damp.value.value_f, 0.005f, 1f);
			float num6 = Mathf.Clamp(Time.deltaTime, 0.001f, 0.1f);
			Vector3 zero = Vector3.zero;
			Vector3 up = Vector3.up;
			zero = ((!(transform != null)) ? vector : transform.position);
			float num7 = 0f;
			float num8 = 0f;
			float num9 = (0f - num5) * 8f;
			if (Prev.value.lockCursor || flag)
			{
				num7 = Mathf.Clamp(value_f2 * value_f * ((!bool2) ? 1f : (-1f)), -200f, 200f) * Time.timeScale;
				num8 = Mathf.Clamp(value_f3 * value_f * ((!bool3) ? 1f : (-1f)), -200f, 200f) * Time.timeScale;
			}
			float num10 = 0f;
			float num11 = 0f;
			float num12 = 0f;
			float num13 = 0f;
			num10 = num7;
			num11 = num8;
			num12 = num3 * value_f * ((!bool2) ? 1f : (-1f)) * Time.timeScale;
			num13 = num4 * value_f * ((!bool3) ? 1f : (-1f)) * Time.timeScale;
			num7 += num12;
			num8 += num13;
			num7 = Mathf.Clamp(num7, -6f * value_f, 6f * value_f);
			num8 = Mathf.Clamp(num8, -3f * value_f, 3f * value_f);
			float4 += num7;
			float5 += num8;
			float7 += num9;
			float7 = Mathf.Clamp(float7, value_f5, value_f4);
			if (num9 != 0f)
			{
				recalcDistTime = 3f;
				lockDistAfterClampTime = 0f;
			}
			if (recalcDistTime > 0f)
			{
				recalcDistTime -= Time.deltaTime;
			}
			if (lockDistAfterClampTime > 0f)
			{
				lockDistAfterClampTime -= num6;
			}
			if (bool4)
			{
				lockDistAfterClampTime = 2f;
				controller.executor.SetBool("Geometry Clampd", value: false);
			}
			float max = Mathf.Clamp(float8 * 2f, value_f5, value_f4);
			float num14 = 1f;
			num14 = Mathf.Clamp(float8 * 0.15f, 0.15f, 1f);
			float6 = float7 * num14;
			if (!Prev.value.lockCursor && flag)
			{
				noControlTime = 0f;
			}
			else if (Mathf.Abs(num12) + Mathf.Abs(num13) > 0.2f || Mathf.Abs(num10) + Mathf.Abs(num11) > 8f)
			{
				noControlTime = 0f;
			}
			else
			{
				noControlTime += num6;
			}
			controller.executor.SetFloat("No Rotate Time", noControlTime);
			if (noControlTime > 1.3f)
			{
				velUsed = Vector3.Lerp(velUsed, b, 0.2f);
			}
			else
			{
				velUsed = Vector3.Lerp(velUsed, Vector3.zero, 0.2f);
			}
			if (float.IsNaN(velUsed.x) || float.IsNaN(velUsed.y) || float.IsNaN(velUsed.z))
			{
				velUsed = Vector3.zero;
			}
			float num15 = Mathf.Clamp01(velUsed.magnitude * 0.2f);
			float t2 = Mathf.Clamp01(new Vector2(velUsed.x, velUsed.z).magnitude * 0.2f);
			Debug.DrawLine(zero, zero + velUsed, Color.cyan);
			if (num15 > 0.01f)
			{
				Vector3 eulerAngles = Quaternion.LookRotation(velUsed).eulerAngles;
				float num16 = eulerAngles.y;
				float b2 = 0f - eulerAngles.x - 10f;
				if (Mathf.DeltaAngle(float4, num16) > 120f || Mathf.DeltaAngle(float4, num16) < -120f)
				{
					num16 = float4;
				}
				num16 = Mathf.LerpAngle(float4, num16, t2);
				b2 = Mathf.LerpAngle(float5, b2, num15);
				Debug.DrawLine(zero, zero + Quaternion.Euler(new Vector3(0f - b2, num16, 0f)) * Vector3.forward, Color.red);
				if (num14 < 0.2f)
				{
					num2 *= 4f;
				}
				else if (num14 < 0.3f)
				{
					num2 *= 3f;
				}
				else if (num14 < 0.4f)
				{
					num2 *= 2.5f;
				}
				else if (num14 < 0.5f)
				{
					num2 *= 2f;
				}
				else if (num14 < 0.6f)
				{
					num2 *= 1.5f;
				}
				float4 = Mathf.SmoothDampAngle(float4, num16, ref vyaw, 20f / num2);
				float5 = Mathf.SmoothDampAngle(float5, b2, ref vpitch, 40f / num2);
			}
			float4 = Mathf.Repeat(float4, 360f);
			float5 = Mathf.Clamp(float5, value_f7, value_f6);
			float6 = Mathf.Clamp(float6, value_f5, max);
			@float = Mathf.LerpAngle(@float, float4, t);
			float2 = Mathf.LerpAngle(float2, float5, t);
			float currentVelocity = controller.executor.GetVar("DistVelocity").value_f;
			if (lockDistAfterClampTime <= 0f)
			{
				num = Mathf.SmoothDamp(num, float6, ref currentVelocity, 0.5f);
			}
			else if (num < value_f5)
			{
				num = Mathf.SmoothDamp(num, value_f5, ref currentVelocity, 0.5f);
			}
			controller.executor.SetVar("DistVelocity", currentVelocity);
			Pose value = Prev.value;
			value.eulerAngles = new Vector3(0f - float2, @float, 0f);
			Vector3 vector2 = value.rotation * Vector3.forward;
			Vector3 vector3 = value.rotation * Vector3.right;
			Vector3 vector4 = value.rotation * Vector3.up;
			Vector3 vector5 = Offset.value.value_v.x * vector3 + Offset.value.value_v.y * vector4 + Offset.value.value_v.z * vector2;
			float num17 = num;
			if (value_f5 == value_f4)
			{
				num17 = value_f5;
			}
			value.position = zero - num17 * vector2;
			value.lockCursor = value_b;
			if (Mathf.Abs(value_f8) > 0.001f)
			{
				Vector3 vector6 = vector2;
				Vector3 rhs = vector3;
				vector6.y = 0f;
				vector6.Normalize();
				rhs.y = 0f;
				float num18 = Vector3.Dot(up, vector6);
				Vector3 vector7 = up - num18 * vector6;
				float num19 = Vector3.Angle(vector7, Vector3.up);
				if (num19 > 90f)
				{
					num19 = 90f;
				}
				num19 = num19 * Mathf.Pow(num19 / 90f, 2f) * value_f8;
				num19 *= 0f - Mathf.Sign(Vector3.Dot(vector7, rhs));
				float3 = Mathf.Lerp(float3, num19, 0.15f);
			}
			else
			{
				float3 = Mathf.Lerp(float3, 0f, 0.15f);
			}
			value.eulerAngles = new Vector3(0f - float2, @float, float3);
			controller.executor.SetFloat("Yaw", @float);
			controller.executor.SetFloat("Pitch", float2);
			controller.executor.SetFloat("Roll", float3);
			controller.executor.SetFloat("Dist", num);
			controller.executor.SetFloat("YawWanted", float4);
			controller.executor.SetFloat("PitchWanted", float5);
			controller.executor.SetFloat("DistWanted", float6);
			controller.executor.SetFloat("DistLevel", float7);
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
