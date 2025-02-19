using UnityEngine;

namespace InControl;

public class UnityGyroAxisSource : InputControlSource
{
	public enum GyroAxis
	{
		X,
		Y
	}

	private int axis;

	private static Quaternion zeroAttitude;

	public UnityGyroAxisSource(GyroAxis axis)
	{
		this.axis = (int)axis;
		Calibrate();
	}

	public float GetValue(InputDevice inputDevice)
	{
		return GetAxis()[axis];
	}

	public bool GetState(InputDevice inputDevice)
	{
		return !Mathf.Approximately(GetValue(inputDevice), 0f);
	}

	private static Quaternion GetAttitude()
	{
		return Quaternion.Inverse(zeroAttitude) * Input.gyro.attitude;
	}

	private static Vector3 GetAxis()
	{
		Vector3 vector = GetAttitude() * Vector3.forward;
		float x = ApplyDeadZone(Mathf.Clamp(vector.x, -1f, 1f));
		float y = ApplyDeadZone(Mathf.Clamp(vector.y, -1f, 1f));
		return new Vector3(x, y);
	}

	private static float ApplyDeadZone(float value)
	{
		return Mathf.InverseLerp(0.05f, 1f, Mathf.Abs(value)) * Mathf.Sign(value);
	}

	public static void Calibrate()
	{
		zeroAttitude = Input.gyro.attitude;
	}
}
