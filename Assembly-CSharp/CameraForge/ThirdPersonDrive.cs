using InControl;
using UnityEngine;

namespace CameraForge;

public class ThirdPersonDrive : ScriptModifier
{
	private float vyaw;

	private float vpitch;

	private Vector3 velUsed = Vector3.zero;

	private float noControlTime;

	private float recalcDistTime;

	private float lockDistAfterClampTime;

	private float fovIncr;

	private float blur;

	public Slot Sens;

	public Slot Damp;

	public Slot FS;

	public Slot Lock;

	public Slot DistMin;

	public Slot DistMax;

	public Slot PitchMin;

	public Slot PitchMax;

	public Slot RollCoef;

	public Slot FovCoef;

	public Slot BlurCoef;

	public Slot OffsetUp;

	public Slot Offset;

	public Slot OffsetDown;

	public override Slot[] slots => new Slot[16]
	{
		Name, Col, Sens, FS, Damp, Lock, DistMin, DistMax, PitchMin, PitchMax,
		RollCoef, FovCoef, BlurCoef, OffsetUp, Offset, OffsetDown
	};

	public override PoseSlot[] poseslots => new PoseSlot[1] { Prev };

	public ThirdPersonDrive()
	{
		Sens = new Slot("Sensitivity");
		Sens.value = 1f;
		Damp = new Slot("Damp Rate");
		Damp.value = 0.3f;
		FS = new Slot("Follow Speed");
		FS.value = 0f;
		Lock = new Slot("Lock Cursor");
		Lock.value = false;
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
		FovCoef = new Slot("Fov Coef");
		FovCoef.value = 20f;
		BlurCoef = new Slot("Blur Coef");
		BlurCoef.value = 0f;
		OffsetUp = new Slot("Offset Up");
		OffsetUp.value = Vector3.zero;
		Offset = new Slot("Offset");
		Offset.value = Vector3.zero;
		OffsetDown = new Slot("Offset Down");
		OffsetDown.value = Vector3.zero;
	}

	public override Pose Calculate()
	{
		Col.Calculate();
		Sens.Calculate();
		Damp.Calculate();
		FS.Calculate();
		Lock.Calculate();
		DistMax.Calculate();
		DistMin.Calculate();
		PitchMax.Calculate();
		PitchMin.Calculate();
		RollCoef.Calculate();
		FovCoef.Calculate();
		BlurCoef.Calculate();
		OffsetUp.Calculate();
		Offset.Calculate();
		OffsetDown.Calculate();
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
			Vector3 b = controller.executor.GetVar("Driving Velocity").value_v;
			Vector3 vector2 = controller.executor.GetVar("Rigidbody Velocity").value_v;
			float value_f = FS.value.value_f;
			float value_f2 = Sens.value.value_f;
			bool value_b = Lock.value.value_b;
			float t = Mathf.Clamp(Damp.value.value_f, 0.005f, 1f);
			float num2 = Mathf.Clamp(Time.deltaTime, 0.001f, 0.1f);
			bool flag = InputModule.Axis("Mouse Right Button").value_b && !GetBool("Mouse Op GUI");
			float value_f3 = InputModule.Axis("Mouse X").value_f;
			float value_f4 = InputModule.Axis("Mouse Y").value_f;
			float num3 = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickX * 25f * Time.deltaTime));
			float num4 = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickY * 12f * Time.deltaTime));
			float num5 = ((!GetBool("Mouse On Scroll")) ? InputModule.Axis("Mouse ScrollWheel").value_f : 0f);
			bool @bool = GetBool("Inverse X");
			bool bool2 = GetBool("Inverse Y");
			bool bool3 = GetBool("Geometry Clampd");
			float value_f5 = DistMax.value.value_f;
			float value_f6 = DistMin.value.value_f;
			float value_f7 = PitchMax.value.value_f;
			float value_f8 = PitchMin.value.value_f;
			float value_f9 = RollCoef.value.value_f;
			float value_f10 = FovCoef.value.value_f;
			float value_f11 = BlurCoef.value.value_f;
			Vector3 zero = Vector3.zero;
			Vector3 up = Vector3.up;
			if (!(transform2 != null))
			{
				zero = ((!(transform != null)) ? vector : transform.position);
			}
			else
			{
				zero = transform2.position - transform2.up;
				up = transform2.up;
			}
			float num6 = 0f;
			float num7 = 0f;
			float num8 = (0f - num5) * 8f;
			if (Prev.value.lockCursor || flag)
			{
				num6 = Mathf.Clamp(value_f3 * value_f2 * ((!@bool) ? 1f : (-1f)), -200f, 200f) * Time.timeScale;
				num7 = Mathf.Clamp(value_f4 * value_f2 * ((!bool2) ? 1f : (-1f)), -200f, 200f) * Time.timeScale;
			}
			float num9 = 0f;
			float num10 = 0f;
			float num11 = 0f;
			float num12 = 0f;
			num9 = num6;
			num10 = num7;
			num11 = num3 * value_f2 * ((!@bool) ? 1f : (-1f)) * Time.timeScale;
			num12 = num4 * value_f2 * ((!bool2) ? 1f : (-1f)) * Time.timeScale;
			num6 += num11;
			num7 += num12;
			num6 = Mathf.Clamp(num6, -6f * value_f2, 6f * value_f2);
			num7 = Mathf.Clamp(num7, -3f * value_f2, 3f * value_f2);
			float4 += num6;
			float5 += num7;
			float7 += num8;
			float7 = Mathf.Clamp(float7, value_f6, value_f5);
			if (num8 != 0f)
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
				lockDistAfterClampTime -= Time.deltaTime;
			}
			if (bool3)
			{
				lockDistAfterClampTime = 2f;
				controller.executor.SetBool("Geometry Clampd", value: false);
			}
			float6 = float7;
			if (!Prev.value.lockCursor && flag)
			{
				noControlTime = 0f;
			}
			else if (Mathf.Abs(num11) + Mathf.Abs(num12) > 0.2f || Mathf.Abs(num9) + Mathf.Abs(num10) > 8f)
			{
				noControlTime = 0f;
			}
			else
			{
				noControlTime += num2;
			}
			controller.executor.SetFloat("No Rotate Time", noControlTime);
			if (noControlTime > 2f)
			{
				velUsed = Vector3.Lerp(velUsed, b, 0.1f);
			}
			else
			{
				velUsed = Vector3.Lerp(velUsed, Vector3.zero, 0.5f);
			}
			if (float.IsNaN(velUsed.x) || float.IsNaN(velUsed.y) || float.IsNaN(velUsed.z))
			{
				velUsed = Vector3.zero;
			}
			float num13 = Mathf.Clamp01((velUsed.magnitude - 2f) * 0.1f);
			float t2 = Mathf.Clamp01((new Vector2(velUsed.x, velUsed.z).magnitude - 2f) * 0.1f);
			Debug.DrawLine(zero, zero + velUsed, Color.cyan);
			if (num13 > 0.01f)
			{
				Vector3 eulerAngles = Quaternion.LookRotation(velUsed).eulerAngles;
				float y = eulerAngles.y;
				float b2 = 0f - eulerAngles.x - 10f;
				y = Mathf.LerpAngle(float4, y, t2);
				b2 = Mathf.LerpAngle(float5, b2, num13 * 0.02f);
				float4 = Mathf.SmoothDampAngle(float4, y, ref vyaw, 20f / value_f);
				float5 = Mathf.SmoothDampAngle(float5, b2, ref vpitch, 40f / value_f);
			}
			float4 = Mathf.Repeat(float4, 360f);
			float5 = Mathf.Clamp(float5, value_f8, value_f7);
			float6 = Mathf.Clamp(float6, value_f6, value_f5);
			@float = Mathf.LerpAngle(@float, float4, t);
			float2 = Mathf.LerpAngle(float2, float5, t);
			float currentVelocity = controller.executor.GetVar("DistVelocity").value_f;
			if (lockDistAfterClampTime <= 0f)
			{
				num = Mathf.SmoothDamp(num, float6, ref currentVelocity, 0.5f);
			}
			num = Mathf.Clamp(num, value_f6, value_f5);
			controller.executor.SetVar("DistVelocity", currentVelocity);
			Pose value = Prev.value;
			value.eulerAngles = new Vector3(0f - float2, @float, 0f);
			Vector3 vector3 = value.rotation * Vector3.forward;
			Vector3 vector4 = value.rotation * Vector3.right;
			Vector3 zero2 = Vector3.zero;
			zero2 = ((!(float2 < 0f)) ? Vector3.Slerp(Offset.value.value_v, OffsetUp.value.value_v, float2 / value_f7) : Vector3.Slerp(Offset.value.value_v, OffsetDown.value.value_v, float2 / value_f8));
			float num14 = num;
			if (value_f6 == value_f5)
			{
				num14 = value_f6;
			}
			value.position = zero - num14 * vector3 + zero2 * num14;
			value.lockCursor = value_b;
			if (Mathf.Abs(value_f9) > 0.001f)
			{
				Vector3 vector5 = vector3;
				Vector3 rhs = vector4;
				vector5.y = 0f;
				vector5.Normalize();
				rhs.y = 0f;
				float num15 = Vector3.Dot(up, vector5);
				Vector3 vector6 = up - num15 * vector5;
				float num16 = Vector3.Angle(vector6, Vector3.up);
				float t3 = 1f - Mathf.Pow((num16 - 90f) / 90f, 2f);
				num16 = ((!(num16 < 90f)) ? Mathf.Lerp(180f, 90f, t3) : Mathf.Lerp(0f, 90f, t3));
				num16 *= value_f9;
				num16 *= 0f - Mathf.Sign(Vector3.Dot(vector6, rhs));
				float3 = Mathf.Lerp(float3, num16, 0.15f);
			}
			else
			{
				float3 = Mathf.Lerp(float3, 0f, 0.15f);
			}
			value.eulerAngles = new Vector3(0f - float2, @float, float3);
			float num17 = Mathf.InverseLerp(10f, 35f, vector2.magnitude);
			fovIncr = Mathf.Lerp(fovIncr, num17 * value_f10, 0.2f);
			value.fov = Mathf.Clamp(value.fov + fovIncr, 10f, 90f);
			blur = Mathf.Lerp(blur, num17 * value_f11, 0.2f);
			value.motionBlur = Mathf.Clamp(blur, 0f, 0.8f);
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
