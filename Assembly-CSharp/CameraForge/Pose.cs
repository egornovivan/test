using UnityEngine;

namespace CameraForge;

public class Pose
{
	public Vector3 position;

	private Vector3 _eulerAngles;

	private Quaternion _rotation;

	public float fov;

	public float nearClip;

	public bool lockCursor;

	public Vector2 cursorPos;

	public float saturate;

	public float motionBlur;

	public static Pose Default
	{
		get
		{
			Pose pose = new Pose();
			pose.position = Vector3.zero;
			pose.eulerAngles = Vector3.zero;
			pose.fov = 60f;
			pose.nearClip = 0.3f;
			pose.lockCursor = false;
			pose.cursorPos = new Vector2(0.5f, 0.5f);
			pose.saturate = 1f;
			pose.motionBlur = 0f;
			return pose;
		}
	}

	public static Pose Zero
	{
		get
		{
			Pose pose = new Pose();
			pose.position = Vector3.zero;
			pose.eulerAngles = Vector3.zero;
			pose.fov = 0f;
			pose.nearClip = 0f;
			pose.lockCursor = false;
			pose.cursorPos = Vector2.zero;
			pose.saturate = 0f;
			pose.motionBlur = 0f;
			return pose;
		}
	}

	public Vector3 eulerAngles
	{
		get
		{
			return _eulerAngles;
		}
		set
		{
			if (_eulerAngles != value)
			{
				_eulerAngles = value;
				_rotation = Quaternion.Euler(_eulerAngles);
			}
		}
	}

	public Quaternion rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			if (_rotation != value)
			{
				_rotation = value;
				_eulerAngles = _rotation.eulerAngles;
			}
		}
	}

	public float yaw
	{
		get
		{
			return _eulerAngles.y;
		}
		set
		{
			_eulerAngles.y = Utils.NormalizeDEG(value);
			_rotation = Quaternion.Euler(_eulerAngles);
		}
	}

	public float pitch
	{
		get
		{
			return 0f - _eulerAngles.x;
		}
		set
		{
			_eulerAngles.x = Utils.NormalizeDEG(0f - value);
			_rotation = Quaternion.Euler(_eulerAngles);
		}
	}

	public float roll
	{
		get
		{
			return _eulerAngles.z;
		}
		set
		{
			_eulerAngles.z = Utils.NormalizeDEG(value);
			_rotation = Quaternion.Euler(_eulerAngles);
		}
	}

	public override string ToString()
	{
		return "Pos: " + position.ToString() + "\r\nRot: " + eulerAngles.ToString() + "\r\nFov: " + fov + "\r\n";
	}
}
